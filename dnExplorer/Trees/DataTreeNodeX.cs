using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace dnExplorer.Trees {
	public class DataTreeNodeX : TreeNodeX {
		public DataTreeNodeX(IDataModel model) {
			Model = model;
		}

		IDataModel model;

		public IDataModel Model {
			get { return model; }
			set { SetModel(value); }
		}

		void CollapseRequested(object sender, EventArgs e) {
			if (TreeView != null && TreeView.InvokeRequired) {
				TreeView.Invoke(new EventHandler(CollapseRequested), sender, e);
				return;
			}
			Collapse();
		}

		void ExpandRequested(object sender, EventArgs e) {
			if (TreeView != null && TreeView.InvokeRequired) {
				TreeView.Invoke(new EventHandler(ExpandRequested), sender, e);
				return;
			}
			Expand();
		}

		public void Invalidate() {
			if (model is IDataModel) {
				model.Refresh(false);
			}
			foreach (object obj2 in Nodes) {
				if (obj2 is DataTreeNodeX) {
					(obj2 as DataTreeNodeX).Invalidate();
				}
			}
		}

		protected internal override void OnCollapse() {
			model.OnCollapse();
		}

		protected internal override void OnExpand() {
			model.OnExpand();
		}

		protected internal override void DrawIcon(Graphics g, Rectangle bounds) {
			model.DrawIcon(g, bounds);
		}

		protected internal override bool HasIcon {
			get { return model.HasIcon; }
		}

		void SetModel(IDataModel value) {
			if (model != null) {
				model.Node = null;
				model.PropertyChanged -= OnModelPropertyChanged;
				model.ExpandRequested -= ExpandRequested;
				model.CollapseRequested -= CollapseRequested;
				model.Children.CollectionChanged -= OnChildrenUpdated;
			}
			model = value;
			model.Node = this;
			model.Refresh(true);

			model.PropertyChanged += OnModelPropertyChanged;
			model.ExpandRequested += ExpandRequested;
			model.CollapseRequested += CollapseRequested;
			model.Children.CollectionChanged += OnChildrenUpdated;

			Text = model.Text;
			ForeColor = model.ForeColor;
			ContextMenuStrip = model.ContextMenu;
			foreach (var child in model.Children) {
				var childNode = new DataTreeNodeX(child);
				Nodes.Add(childNode);
			}
			ImageIndex = model.HasIcon ? 0 : -1;
		}

		void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (TreeView != null && TreeView.InvokeRequired) {
				TreeView.Invoke(new PropertyChangedEventHandler(OnModelPropertyChanged), sender, e);
				return;
			}

			if (e.PropertyName == "Text") {
				Text = model.Text;
			}
			else if (e.PropertyName == "ForeColor") {
				ForeColor = model.ForeColor;
			}
			else if (e.PropertyName == "ContextMenu") {
				ContextMenuStrip = model.ContextMenu;
			}
		}

		void OnChildrenUpdated(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action == NotifyCollectionChangedAction.Reset) {
				DoReset();
				return;
			}

			if (TreeView != null && TreeView.InvokeRequired) {
				TreeView.Invoke(new NotifyCollectionChangedEventHandler(OnChildrenUpdated), sender, e);
				return;
			}

			switch (e.Action) {
				case NotifyCollectionChangedAction.Add: {
					Nodes.Insert(e.NewStartingIndex, model.Children[e.NewStartingIndex].ToNode());
					break;
				}

				case NotifyCollectionChangedAction.Remove: {
					Nodes.RemoveAt(e.NewStartingIndex);
					break;
				}

				case NotifyCollectionChangedAction.Move: {
					var child = (DataTreeNodeX)Nodes[e.OldStartingIndex];
					Nodes.RemoveAt(e.OldStartingIndex);
					Nodes.Insert(e.NewStartingIndex, child);
					break;
				}

				case NotifyCollectionChangedAction.Replace: {
					((DataTreeNodeX)Nodes[e.NewStartingIndex]).Model = model.Children[e.NewStartingIndex];
					break;
				}
			}
		}

		void DoReset() {
			var newNodes = new TreeNode[model.Children.Count];
			for (int i = 0; i < newNodes.Length; i++)
				newNodes[i] = model.Children[i].ToNode();

			if (TreeView != null && TreeView.InvokeRequired) {
				TreeView.Invoke(new Action<TreeNode[]>(DoReset), new object[] { newNodes });
			}
			else
				DoReset(newNodes);
		}

		void DoReset(TreeNode[] nodes) {
			Nodes.Clear();
			((TreeViewX)TreeView).updating = true;
			try {
				Nodes.AddRange(nodes);
			}
			finally {
				((TreeViewX)TreeView).updating = false;
			}
		}
	}
}