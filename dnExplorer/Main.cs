using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Models;
using dnExplorer.Theme;
using dnExplorer.Trees;
using dnExplorer.Views;
using ScintillaNET;

namespace dnExplorer {
	public class Main : Form {
		public static readonly string AppName = typeof(InputBox).Assembly.GetName().Name;

		InfoPanel infos;
		TreeViewX treeView;
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

			var split2 = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			split.Panel1.Controls.Add(split2);

			treeView = new TreeViewX {
				Dock = DockStyle.Fill
			};
			treeView.AfterSelect += OnNodeSelected;
			split2.Panel1.Controls.Add(treeView);

			infos = new InfoPanel {
				Dock = DockStyle.Fill
			};
			split2.Panel2.Controls.Add(infos);

			content = new Panel {
				Dock = DockStyle.Fill
			};
			split.Panel2.Controls.Add(content);

			PerformLayout();
			split.SplitterDistance = 250;
			split2.SplitterDistance = split2.Height - 150;
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
				treeView.Nodes.Add(new dnModuleModel(new dnModule(module)).ToNode());
			}
		}

		ViewBase currentView;

		void OnNodeSelected(object sender, TreeViewEventArgs e) {
			var newNode = e.Node as DataTreeNodeX;
			ViewBase newView;
			if (newNode == null)
				newView = null;
			else
				newView = ViewLocator.LocateView(newNode.Model);

			if (newView != currentView) {
				if (currentView != null) {
					content.Controls.Remove(currentView);
					currentView.Model = null;
				}
				currentView = newView;
				if (currentView != null) {
					currentView.Model = newNode.Model;
					content.Controls.Add(currentView);
				}
			}
			else if (currentView != null)
				currentView.Model = newNode.Model;

			if (newNode.Model is IHasInfo)
				infos.SetInfo((IHasInfo)newNode.Model);
			else
				infos.Clear();
		}
	}
}