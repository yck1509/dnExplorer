using System;
using System.Windows.Forms;
using dnExplorer.Models;
using dnExplorer.Trees;
using dnlib.DotNet;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer {
	public class ModuleManager : DockContent {
		InfoPanel infos;
		ModuleTreeView treeView;
		NavigationHistory history = new NavigationHistory();
		ModuleResolver resolver;

		public ModuleManager(IApp app) {
			Text = "Modules";
			Height = 600;
			CloseButtonVisible = false;
			CloseButton = false;

			var split = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			Controls.Add(split);

			treeView = new ModuleTreeView(app, this) {
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

			history.Navigated += (sender, e) => {
				navCount++;
				try {
					treeView.SelectedNode = history.Current.Node;
				}
				finally {
					navCount--;
				}
			};
			resolver = new ModuleResolver(this);
		}

		int navCount;

		void OnNodeSelected(object sender, TreeViewEventArgs e) {
			var newNode = e.Node as DataTreeNodeX;
			IDataModel newModel = null;
			if (newNode != null)
				newModel = newNode.Model;

			if (newModel is IHasInfo)
				infos.SetInfo((IHasInfo)newModel);
			else
				infos.Clear();

			if (newNode != null && navCount == 0)
				History.Record(newNode);

			if (SelectionChanged != null)
				SelectionChanged(this, new SelectionChangedEventArgs(newModel));
		}

		public void LoadModule(string path) {
			if (treeView.InvokeRequired) {
				treeView.Invoke(new Action<string>(LoadModule), path);
				return;
			}

			var module = new dnModule(path, new ModuleContext(resolver));
			if (module.ModuleDef != null) {
				ModuleDefMD cached;
				if (!resolver.AddModuleDef(module.ModuleDef, out cached))
					return;
			}
			treeView.Nodes.Add(new dnModuleModel(module).ToNode());
		}

		public void RemoveModule(dnModule module) {
			if (treeView.InvokeRequired) {
				treeView.Invoke(new Action<dnModule>(RemoveModule), module);
			}
		}

		public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

		public NavigationHistory History {
			get { return history; }
		}
	}

	internal class ModuleTreeView : TreeViewX {
		public ModuleTreeView(IApp app, ModuleManager manager)
			: base(app) {
			Manager = manager;
		}

		public ModuleManager Manager { get; private set; }
	}

	public class SelectionChangedEventArgs : EventArgs {
		public IDataModel Selection { get; private set; }

		public SelectionChangedEventArgs(IDataModel selection) {
			Selection = selection;
		}
	}
}