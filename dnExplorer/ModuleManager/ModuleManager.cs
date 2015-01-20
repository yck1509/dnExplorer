using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using dnExplorer.Models;
using dnExplorer.Trees;
using dnExplorer.Views;
using dnlib.DotNet;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer {
	public class ModuleManager : DockContent {
		IApp app;
		InfoPanel infos;
		TreeViewX treeView;
		NavigationHistory history = new NavigationHistory();
		ModuleResolver resolver;

		public ModuleManager(IApp app) {
			this.app = app;

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
					if (history.Current.Model.Node != null &&
					    history.Current.Model.Node.TreeView != null)
						treeView.SelectedNode = history.Current.Model.Node;
					else
						NavigateModel(history.Current.Model);
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

			if (newNode != null && navCount == 0 && newNode.Model != null && newNode.Model.RecordHistory)
				History.Record(newNode.Model);

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

		void NavigateModel(IDataModel model) {
			if (model is ObjModel)
				ViewUtils.ShowMember(treeView, (IMemberDef)((ObjModel)model).Definition);
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

		public void NavigateTarget(object target) {
			if (!InvokeRequired) {
				// Resolving must done in non-UI thread to avoid deadlock.
				Task.Factory.StartNew(() => NavigateTarget(target));
				return;
			}

			if (target is IMemberDef)
				ViewUtils.ShowMember(treeView, (IMemberDef)target);
			else if (target is IAssembly) {
				var assemblyRef = (IAssembly)target;
				var assemblyDef = resolver.Resolve(assemblyRef, null);
				if (assemblyDef != null)
					ViewUtils.ShowModule(treeView, assemblyDef.ManifestModule);
				else
					Error("Failed to resolve '" + assemblyRef.FullName + "'.");
			}
			else if (target is MemberRef) {
				var memberRef = (MemberRef)target;
				var memberDef = (IMemberDef)memberRef.Resolve();
				if (memberDef != null)
					ViewUtils.ShowMember(treeView, memberDef);
				else
					Error("Failed to resolve '" + memberRef.FullName + "'.");
			}
			else if (target is TypeRef) {
				var typeRef = (TypeRef)target;
				var typeDef = typeRef.Resolve();
				if (typeDef != null)
					ViewUtils.ShowMember(treeView, typeDef);
				else
					Error("Failed to resolve '" + typeRef.FullName + "'.");
			}
			else if (target is MethodSpec) {
				var methodRef = ((MethodSpec)target).Method;
				var methodDef = methodRef.ResolveMethodDef();
				if (methodDef != null)
					ViewUtils.ShowMember(treeView, methodDef);
				else
					Error("Failed to resolve '" + methodRef.FullName + "'.");
			}
			else
				Error("Unsupported navigation target '" + target.GetType().FullName + "'.");
		}

		void Error(string msg) {
			if (InvokeRequired) {
				Invoke(new Action<string>(Error), msg);
				return;
			}
			MessageBox.Show(msg, app.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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