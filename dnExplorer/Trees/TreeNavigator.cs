using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dnExplorer.Trees {
	public class TreeNavigator {
		Func<object, NavigationState> rootPredicate;
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

		public void Navigate(TreeNodeX node) {
			if (node.TreeView == null)
				throw new ArgumentException("node");
			Navigate((TreeViewX)node.TreeView);
		}

		public void Navigate(IDataModel model) {
			if (model.Node == null || model.Node.TreeView == null)
				throw new ArgumentException("model");
			Navigate((TreeViewX)model.Node.TreeView);
		}

		public void Navigate(TreeViewX treeView) {
			foreach (var child in treeView.Nodes)
				NavigateChild(child as DataTreeNodeX);
		}

		void NavigateChild(DataTreeNodeX node) {
			if (node == null)
				return;

			Func<object, NavigationState> predicate = rootPredicate;
			if (!navPath.TryGetValue(node.Model.GetType(), out predicate))
				return;

			switch (predicate(node.Model)) {
				case NavigationState.Next:
					return;
				case NavigationState.In:
					break;
				case NavigationState.Done:
					node.TreeView.SelectedNode = node;
					if (onNavigated != null)
						onNavigated(node);
					return;
			}

			if (node.Model is LazyModel) {
				var task = ((LazyModel)node.Model).LoadImmediate();
				if (task == null)
					SearchNodeChildren(node);
				else
					task.ContinueWith(t => SearchNodeChildren(node),
						TaskScheduler.FromCurrentSynchronizationContext());
			}
			else {
				SearchNodeChildren(node);
			}
		}

		void SearchNodeChildren(DataTreeNodeX node) {
			node.Expand();

			foreach (var child in node.Nodes)
				NavigateChild(child as DataTreeNodeX);
		}
	}

	public enum NavigationState {
		Next,
		In,
		Done
	}
}