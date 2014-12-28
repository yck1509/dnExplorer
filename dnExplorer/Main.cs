using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Nodes;
using dnExplorer.Trees;
using dnExplorer.Views;

namespace dnExplorer {
	public class Main : Form {
		public static readonly string AppName = typeof(InputBox).Assembly.GetName().Name;

		TreeViewX treeView;
		Panel content;

		public Main() {
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

			treeView = new TreeViewX {
				Dock = DockStyle.Fill
			};
			treeView.AfterSelect += OnNodeSelected;
			split.Panel1.Controls.Add(treeView);

			content = new Panel {
				Dock = DockStyle.Fill
			};
			split.Panel2.Controls.Add(content);

			PerformLayout();
			split.SplitterDistance = 200;
			treeView.BeginUpdate();
			treeView.EndUpdate();
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
				treeView.Nodes.Add(new ModuleModel(new dnModule(module)).ToNode());
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
		}
	}
}