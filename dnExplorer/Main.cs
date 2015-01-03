using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Theme;
using dnExplorer.Views;
using ScintillaNET;
using System.Linq;

namespace dnExplorer {
	public class Main : Form {
		public static readonly string AppName = typeof(InputBox).Assembly.GetName().Name;

		ModuleManager modMgr;
		Panel content;

		public Main() {
			ToolStripManager.Renderer = new VS2010Renderer();
			Scintilla.SetModulePath(IntPtr.Size == 4 ? @"SciLexer.x86.dll" : @"SciLexer.x64.dll");

			Initialize();
		}

		void Initialize() {
			Size = new Size(800, 650);
			Text = "dnExplorer";
			AllowDrop = true;

			var split = new SplitContainer {
				Orientation = Orientation.Vertical,
				Dock = DockStyle.Fill
			};
			Controls.Add(split);

			modMgr = new ModuleManager {
				Dock = DockStyle.Fill
			};
			modMgr.SelectionChanged += OnNodeSelected;
			split.Panel1.Controls.Add(modMgr);

			content = new Panel {
				Dock = DockStyle.Fill
			};
			split.Panel2.Controls.Add(content);

			PerformLayout();
			split.SplitterDistance = 250;
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

		ViewBase currentView;

		void OnNodeSelected(object sender, SelectionChangedEventArgs e) {
			ViewBase newView;
			if (e.Selection == null)
				newView = null;
			else
				newView = ViewLocator.LocateView(e.Selection);

			if (newView != currentView) {
				if (currentView != null) {
					content.Controls.Remove(currentView);
				}
				currentView = newView;
				if (currentView != null) {
					currentView.Model = e.Selection;
					content.Controls.Add(currentView);
				}
			}
			else if (currentView != null)
				currentView.Model = e.Selection;
		}
	}
}