using System;
using System.Collections.Generic;

namespace dnExplorer.Trees {
	public class TreeNavigator {
		Action<DataTreeNodeX> onNavigated;
		Dictionary<Type, Func<object, NavigationState>> navPath;

		TreeNavigator() {
			navPath = new Dictionary<Type, Func<object, NavigationState>>();
		}

		public TreeNavigator Path<TModel>(Func<TModel, NavigationState> predicate) where TModel : IDataModel {
			Func<object, NavigationState> internalPredicate = arg => {
				if (arg is TModel)
					return predicate((TModel)arg);
				return NavigationState.Next;
			};

			navPath[typeof(TModel)] = internalPredicate;
			return this;
		}

		public TreeNavigator Handler(Action<DataTreeNodeX> onNavigated) {
			this.onNavigated = onNavigated;
			return this;
		}

		public static TreeNavigator Create() {
			return new TreeNavigator();
		}

		public void Goto(TreeNodeX node) {
			if (node.TreeView == null)
				throw new ArgumentException("node");
			Goto((TreeViewX)node.TreeView);
		}

		public void Goto(IDataModel model) {
			if (model.Node == null || model.Node.TreeView == null)
				throw new ArgumentException("model");
			Goto((TreeViewX)model.Node.TreeView);
		}

		public void Goto(TreeViewX treeView) {
			foreach (var child in treeView.Nodes)
				NavigateChild(child as DataTreeNodeX,
					node => {
						node.TreeView.SelectedNode = node;
						if (onNavigated != null)
							onNavigated(node);
					}, SearchNodeGoto);
		}

		public void Find<T>(TreeNodeX node, Action<T> handler) where T : IDataModel {
			if (node.TreeView == null)
				throw new ArgumentException("node");
			Find((TreeViewX)node.TreeView, handler);
		}

		public void Find<T>(IDataModel model, Action<T> handler) where T : IDataModel {
			if (model.Node == null || model.Node.TreeView == null)
				throw new ArgumentException("model");
			Find((TreeViewX)model.Node.TreeView, handler);
		}

		public void Find<T>(TreeViewX treeView, Action<T> handler) where T : IDataModel {
			foreach (var child in treeView.Nodes)
				NavigateChild(child as DataTreeNodeX,
					node => { handler((T)node.Model); }, SearchNodeFind);
		}

		void NavigateChild(DataTreeNodeX node, Action<DataTreeNodeX> done, Action<DataTreeNodeX, Action<DataTreeNodeX>> search) {
			if (node == null || node.TreeView == null)
				return;

			Func<object, NavigationState> predicate;
			if (!navPath.TryGetValue(node.Model.GetType(), out predicate))
				return;

			switch (predicate(node.Model)) {
				case NavigationState.Next:
					return;
				case NavigationState.In:
					break;
				case NavigationState.Done:
					node.TreeView.Invoke(new Action(() => {
						done(node);
					}));
					return;
			}

			if (node.Model is LazyModel) {
				var op = ((LazyModel)node.Model).Load();
				if (op != null) {
					op.Completed += (sender, e) => search(node, done);
					op.Begin();
				}
				else
					search(node, done);
			}
			else {
				search(node, done);
			}
		}

		void SearchNodeGoto(DataTreeNodeX node, Action<DataTreeNodeX> done) {
			if (node.TreeView != null) {
				if (node.TreeView.InvokeRequired)
					node.TreeView.Invoke(new Action(() => node.Expand()));
				else
					node.Expand();
			}

			foreach (var child in node.Nodes)
				NavigateChild(child as DataTreeNodeX, done, SearchNodeGoto);
		}

		void SearchNodeFind(DataTreeNodeX node, Action<DataTreeNodeX> done) {
			foreach (var child in node.Nodes)
				NavigateChild(child as DataTreeNodeX, done, SearchNodeFind);
		}
	}

	public enum NavigationState {
		Next,
		In,
		Done
	}
}