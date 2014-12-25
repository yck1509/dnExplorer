using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Nodes;
using dnExplorer.Trees;
using dnExplorer.Views;
using dnlib.DotNet.MD;
using dnlib.PE;

namespace dnExplorer {
	public class Main : Form {
		TreeViewX treeView;
		Panel content;

		public Main() {
			Initialize();
		}

		void Initialize() {
			Size = new Size(600, 450);
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
				treeView.Nodes.Add(new ModuleModel(MetaDataCreator.CreateMetaData(new PEImage(module))).ToNode());
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

			SuspendLayout();

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

			ResumeLayout();
		}
	}
}