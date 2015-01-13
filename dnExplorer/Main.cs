using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using dnExplorer.Language;
using dnExplorer.Theme;
using dnExplorer.Trees;
using dnExplorer.Views;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer {
	public class Main : Form, IApp {
		DockPanel dockPanel;
		ToolStripPanel toolStripPanel;
		ToolStrip mainStrip;

		ToolStripButton backBtn;
		ToolStripButton forwardBtn;

		public Main() {
			AppName = typeof(IApp).Assembly.GetName().Name;
			Views = new ViewLocator(this);

			ToolStripManager.Renderer = new VS2010Renderer();

			Initialize();

			Module.LoadModule(@"E:\Source\Public\Confuser2\Confuser.Test\RenamingTest\TestC\bin\Debug\TestC.exe");
			Module.LoadModule(@"E:\Source\Public\Confuser2\Confuser.Test\RenamingTest\TestC\bin\Debug\dnlib.dll");
		}

		void Initialize() {
			ClientSize = new Size(800, 600);
			Text = AppName;
			AllowDrop = true;
			IsMdiContainer = true;

			Language = new LanguageManager();

			Module = new ModuleManager(this);
			Module.SelectionChanged += OnNodeSelected;
			Module.History.Navigated += (sender, e) => UpdateHistoryButtons();

			dockPanel = new DockPanel {
				Dock = DockStyle.Fill,
				Theme = new VS2010Theme(),
				DockLeftPortion = 0.4,
				DockBottomPortion = 0.4,
				DocumentStyle = DocumentStyle.DockingMdi,
				ShowDocumentIcon = true,
				AllowEndUserDocking = false,
				AllowEndUserNestedDocking = false
			};
			Controls.Add(dockPanel);
			Module.Show(dockPanel, DockState.DockLeft);

			toolStripPanel = new ToolStripPanel {
				Dock = DockStyle.Top
			};
			Controls.Add(toolStripPanel);

			mainStrip = new ToolStrip();
			toolStripPanel.Join(mainStrip);

			backBtn = new ToolStripButton(Resources.GetResource<Image>("Icons.back.png")) {
				ToolTipText = "Go Back",
				Enabled = false
			};
			backBtn.Click += (sender, e) => Module.History.GoBack();
			mainStrip.Items.Add(backBtn);

			forwardBtn = new ToolStripButton(Resources.GetResource<Image>("Icons.forward.png")) {
				ToolTipText = "Go Forward",
				Enabled = false
			};
			forwardBtn.Click += (sender, e) => Module.History.GoForward();
			mainStrip.Items.Add(forwardBtn);


			mainStrip.Items.Add(new ToolStripSeparator());
			var langCombo = new ToolStripComboBox();
			mainStrip.Items.Add(langCombo);

			langCombo.ComboBox.DataSource = Language.Languages;
			Language.PropertyChanged += (sender, e) => langCombo.ComboBox.SelectedItem = Language.ActiveLanguage;
			Shown += (sender, e) => {
				langCombo.ComboBox.SelectedItem = Language.ActiveLanguage;
				langCombo.ComboBox.SelectedValueChanged +=
					(s, ee) => Language.ActiveLanguage = (ILanguage)langCombo.ComboBox.SelectedItem;
			};

			langCombo.ComboBox.DisplayMember = "Name";
			langCombo.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

			PerformLayout();
		}

		protected override void OnDragOver(DragEventArgs drgevent) {
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop)) {
				drgevent.Effect = DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll;
			}
			base.OnDragOver(drgevent);
		}

		protected override void OnDragDrop(DragEventArgs drgevent) {
			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop)) {
				LoadModules(drgevent.Data.GetData(DataFormats.FileDrop) as string[]);
			}
		}

		void LoadModules(string[] files) {
			foreach (var module in files) {
				Module.LoadModule(module);
			}
		}

		Dictionary<IView, DockContent> currentViews = new Dictionary<IView, DockContent>();

		void UpdateViewContainer(DataTreeNodeX node, IView view, DockContent container) {
			if (string.IsNullOrEmpty(view.ViewControl.Text))
				container.Text = node.Text;
			else
				container.Text = view.ViewControl.Text;

			if (view.Icon != null)
				container.Icon = view.Icon;
			else {
				var bmp = new Bitmap(16, 16);
				using (var g = Graphics.FromImage(bmp))
					node.DrawIcon(g, new Rectangle(0, 0, 16, 16));
				container.Icon = IconCreator.CreateIcon(bmp, 16);
			}
		}

		void OnNodeSelected(object sender, SelectionChangedEventArgs e) {
			UpdateHistoryButtons();

			IView[] newViews;
			if (e.Selection == null)
				newViews = new IView[0];
			else
				newViews = Views.LocateViews(e.Selection).ToArray();

			var newActualViews = new Dictionary<IView, DockContent>();
			foreach (var view in newViews) {
				DockContent content;
				if (!currentViews.TryGetValue(view, out content)) {
					content = new DockContent {
						HideOnClose = true,
						CloseButtonVisible = false,
						CloseButton = false
					};
					content.DockHandler.ActivateOnShow = false;
					view.ViewControl.Dock = DockStyle.Fill;
					UpdateViewContainer(e.Selection.Node, view, content);

					content.Controls.Add(view.ViewControl);
				}
				newActualViews.Add(view, content);
			}

			dockPanel.SuspendLayout(true);

			IDockContent activeView = null;
			if (currentViews.Count > 0)
				activeView = currentViews.Values.First().PanelPane.ActiveContent;
			if (!newActualViews.Values.Contains(activeView))
				activeView = null;

			foreach (var view in newActualViews) {
				view.Key.Model = e.Selection;
				view.Value.Show(dockPanel, DockState.Document);
				if (activeView == null)
					activeView = view.Value;
			}

			foreach (var prevView in currentViews) {
				if (!newActualViews.ContainsKey(prevView.Key)) {
					prevView.Value.Hide();
					prevView.Key.Model = null;
				}
				else
					UpdateViewContainer(e.Selection.Node, prevView.Key, prevView.Value);
			}
			currentViews = newActualViews;

			if (activeView != null)
				activeView.DockHandler.PanelPane.ActiveContent = activeView;
			Module.Activate();
			dockPanel.ResumeLayout(true, true);
		}

		void UpdateHistoryButtons() {
			backBtn.Enabled = Module.History.CanGoBack;
			forwardBtn.Enabled = Module.History.CanGoForward;
		}


		public string AppName { get; private set; }
		public ViewLocator Views { get; private set; }

		public ModuleManager Module { get; set; }

		public LanguageManager Language { get; set; }
	}
}