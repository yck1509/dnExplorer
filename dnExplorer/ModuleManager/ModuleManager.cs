using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using dnExplorer.Models;
using dnExplorer.Trees;
using dnlib.DotNet;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer {
	public class ModuleManager : DockContent {
		InfoPanel infos;
		TreeViewX treeView;
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

			treeView = new TreeViewX(app) {
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.None
			};
			treeView.AfterSelect += OnNodeSelected;
			treeView.KeyDown += OnTreeViewKeyDown;
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

		void OnTreeViewKeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Delete) {
				var selectedNode = treeView.SelectedNode as DataTreeNodeX;
				if (selectedNode != null && selectedNode.Model is dnModuleModel)
					RemoveModule(((dnModuleModel)selectedNode.Model).Module);
			}
		}

		public Task LoadModule(string path) {
			if (treeView.InvokeRequired) {
				return (Task)treeView.Invoke(new Func<string, Task>(LoadModule), path);
			}

			return Task.Factory.StartNew(() => {
				var module = new dnModule(path, new ModuleContext(resolver));
				if (module.ModuleDef != null) {
					ModuleDefMD cached;
					if (!resolver.AddModuleDef(module.ModuleDef, out cached))
						return null;
				}
				return module;
			}).ContinueWith(task => {
				if (task.Result != null)
					treeView.Nodes.Add(new dnModuleModel(task.Result).ToNode());
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public void RemoveModule(dnModule module) {
			if (treeView.InvokeRequired) {
				treeView.Invoke(new Action<dnModule>(RemoveModule), module);
			}
			if (module.ModuleDef != null) {
				if (!resolver.RemoveModuleDef(module.ModuleDef))
					return;
			}
			foreach (DataTreeNodeX node in treeView.Nodes) {
				if (node != null && node.Model is dnModuleModel) {
					var nodeModule = ((dnModuleModel)node.Model).Module;
					if (nodeModule == module) {
						treeView.Nodes.Remove(node);
						OnNodeSelected(treeView, new TreeViewEventArgs(treeView.SelectedNode));
					}
				}
			}
		}

		public IEnumerable<ModuleDefMD> GetModules() {
			return new List<ModuleDefMD>(resolver.Modules);
		}

		public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

		public NavigationHistory History {
			get { return history; }
		}

		public IAssemblyResolver Resolver {
			get { return resolver; }
		}
	}

	public class SelectionChangedEventArgs : EventArgs {
		public IDataModel Selection { get; private set; }

		public SelectionChangedEventArgs(IDataModel selection) {
			Selection = selection;
		}
	}
}