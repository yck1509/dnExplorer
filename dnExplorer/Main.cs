using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using dnExplorer.Analysis;
using dnExplorer.Language;
using dnExplorer.Theme;
using dnExplorer.Trees;
using dnExplorer.Views;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer {
	public class Main : Form, IApp {
		ToolStripPanel toolStripPanel;
		ToolStrip mainStrip;

		ToolStripButton backBtn;
		ToolStripButton forwardBtn;

		public Main() {
			AppName = typeof(IApp).Assembly.GetName().Name;
			Views = new ViewLocator(this);

			ToolStripManager.Renderer = new VS2010Renderer();

			Initialize();
		}

		void Initialize() {
			ClientSize = new Size(800, 600);
			Text = AppName;
			AllowDrop = true;
			IsMdiContainer = true;

			DockArea = new DockPanel {
				Dock = DockStyle.Fill,
				Theme = new VS2010Theme(),
				DockLeftPortion = 0.35,
				DockBottomPortion = 0.3,
				DocumentStyle = DocumentStyle.DockingMdi,
				ShowDocumentIcon = true,
				AllowEndUserDocking = false,
				AllowEndUserNestedDocking = false
			};
			Controls.Add(DockArea);

			Languages = new LanguageManager();
			Analyzer = new Analyzer(this);

			Modules = new ModuleManager(this);
			Modules.SelectionChanged += OnNodeSelected;
			Modules.History.Navigated += (sender, e) => UpdateHistoryButtons();

			Modules.Show(DockArea, DockState.Document); // Reduce flickering when first show document later
			Modules.Show(DockArea, DockState.DockLeft);

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
			backBtn.Click += (sender, e) => Modules.History.GoBack();
			mainStrip.Items.Add(backBtn);

			forwardBtn = new ToolStripButton(Resources.GetResource<Image>("Icons.forward.png")) {
				ToolTipText = "Go Forward",
				Enabled = false
			};
			forwardBtn.Click += (sender, e) => Modules.History.GoForward();
			mainStrip.Items.Add(forwardBtn);


			mainStrip.Items.Add(new ToolStripSeparator());
			var langCombo = new ToolStripComboBox();
			mainStrip.Items.Add(langCombo);

			langCombo.ComboBox.DataSource = Languages.Languages;
			Languages.PropertyChanged += (sender, e) => langCombo.ComboBox.SelectedItem = Languages.ActiveLanguage;
			Shown += (sender, e) => {
				langCombo.ComboBox.SelectedItem = Languages.ActiveLanguage;
				langCombo.ComboBox.SelectedValueChanged +=
					(s, ee) => Languages.ActiveLanguage = (ILanguage)langCombo.ComboBox.SelectedItem;
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
				Modules.LoadModule(module);
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
			else if (!e.Selection.ShowViews)
				return;
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
					Modules.Activate();
				}
				newActualViews.Add(view, content);
			}

			DockArea.SuspendLayout(true);

			IDockContent activeView = null;
			if (currentViews.Count > 0)
				activeView = currentViews.Values.First().PanelPane.ActiveContent;
			if (!newActualViews.Values.Contains(activeView))
				activeView = null;

			foreach (var view in newActualViews) {
				view.Key.Model = e.Selection;
				view.Value.Show(DockArea, DockState.Document);
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
			Modules.Activate();
			DockArea.ResumeLayout(true, true);
		}

		void UpdateHistoryButtons() {
			backBtn.Enabled = Modules.History.CanGoBack;
			forwardBtn.Enabled = Modules.History.CanGoForward;
		}


		public string AppName { get; private set; }
		public ViewLocator Views { get; private set; }

		public DockPanel DockArea { get; set; }

		public ModuleManager Modules { get; private set; }

		public LanguageManager Languages { get; private set; }

		public Analyzer Analyzer { get; private set; }
	}
}