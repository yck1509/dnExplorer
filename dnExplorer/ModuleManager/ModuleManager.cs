using System;
using System.Windows.Forms;
using dnExplorer.Models;
using dnExplorer.Trees;

namespace dnExplorer {
	internal class ModuleManager : Control {
		InfoPanel infos;
		TreeViewX treeView;

		public ModuleManager() {
			var split = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			Controls.Add(split);

			treeView = new TreeViewX {
				Dock = DockStyle.Fill
			};
			treeView.AfterSelect += OnNodeSelected;
			split.Panel1.Controls.Add(treeView);

			infos = new InfoPanel {
				Dock = DockStyle.Fill
			};
			split.Panel2.Controls.Add(infos);

			bool layouted = false;
			split.Layout += (sender, e) => {
				if (layouted)
					return;
				if (split.Height > 150)
					split.SplitterDistance = split.Height - 150;
				else
					split.SplitterDistance = split.Height;
				layouted = true;
			};
		}

		void OnNodeSelected(object sender, TreeViewEventArgs e) {
			var newNode = e.Node as DataTreeNodeX;
			IDataModel newModel = null;
			if (newNode != null)
				newModel = newNode.Model;

			if (newModel is IHasInfo)
				infos.SetInfo((IHasInfo)newModel);
			else
				infos.Clear();

			if (SelectionChanged != null)
				SelectionChanged(this, new SelectionChangedEventArgs(newModel));
		}

		public void LoadModule(string path) {
			treeView.Nodes.Add(new dnModuleModel(new dnModule(path)).ToNode());
		}

		public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
	}

	internal class SelectionChangedEventArgs : EventArgs {
		public IDataModel Selection { get; private set; }

		public SelectionChangedEventArgs(IDataModel selection) {
			Selection = selection;
		}
	}
}