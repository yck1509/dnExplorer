using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using dnExplorer.Theme;
using dnExplorer.Views;
using ScintillaNET;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer {
	public class Main : Form {
		public static readonly string AppName = typeof(InputBox).Assembly.GetName().Name;

		ModuleManager modMgr;
		DockPanel dockPanel;

		public Main() {
			ToolStripManager.Renderer = new VS2010Renderer();
			Scintilla.SetModulePath(IntPtr.Size == 4 ? @"SciLexer.x86.dll" : @"SciLexer.x64.dll");

			Initialize();
		}

		void Initialize() {
			ClientSize = new Size(800, 600);
			Text = "dnExplorer";
			AllowDrop = true;
			IsMdiContainer = true;

			modMgr = new ModuleManager();
			modMgr.SelectionChanged += OnNodeSelected;

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
			modMgr.Show(dockPanel, DockState.DockLeft);

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
				modMgr.LoadModule(module);
			}
		}

		Dictionary<IView, DockContent> currentViews = new Dictionary<IView, DockContent>();

		void OnNodeSelected(object sender, SelectionChangedEventArgs e) {
			IView[] newViews;
			if (e.Selection == null)
				newViews = new IView[0];
			else
				newViews = ViewLocator.LocateViews(e.Selection).ToArray();

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

					if (string.IsNullOrEmpty(view.ViewControl.Text))
						content.Text = e.Selection.Node.Text;
					else
						content.Text = view.ViewControl.Text;

					if (view.Icon != null)
						content.Icon = view.Icon;
					else {
						var bmp = new Bitmap(16, 16);
						using (var g = Graphics.FromImage(bmp))
							e.Selection.Node.DrawIcon(g, new Rectangle(0, 0, 16, 16));
						content.Icon = IconCreator.CreateIcon(bmp, 16);
					}

					content.Controls.Add(view.ViewControl);
				}
				newActualViews.Add(view, content);
			}

			dockPanel.SuspendLayout(true);

			foreach (var view in newActualViews) {
				view.Key.Model = e.Selection;
				view.Value.Show(dockPanel, DockState.Document);
			}

			foreach (var prevView in currentViews) {
				if (!newActualViews.ContainsKey(prevView.Key))
					prevView.Value.Hide();
			}
			currentViews = newActualViews;

			modMgr.Activate();
			dockPanel.ResumeLayout(true, true);
		}
	}
}