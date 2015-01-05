using System;
using System.Windows.Forms;
using dnExplorer.Models;
using dnExplorer.Trees;
using dnlib.DotNet;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer {
	internal class ModuleManager : DockContent {
		InfoPanel infos;
		TreeViewX treeView;

		public ModuleManager() {
			Text = "Modules";
			Height = 600;
			CloseButtonVisible = false;
			CloseButton = false;

			var split = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			Controls.Add(split);

			treeView = new TreeViewX {
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.None
			};
			treeView.AfterSelect += OnNodeSelected;
			split.Panel1.Controls.Add(treeView);

			infos = new InfoPanel {
				Dock = DockStyle.Fill
			};
			split.Panel2.Controls.Add(infos);

			split.SplitterDistance = 400;
			Icon = null;
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
			treeView.Nodes.Add(new dnModuleModel(new dnModule(path, new ModuleContext())).ToNode());
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