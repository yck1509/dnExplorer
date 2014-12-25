using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace dnExplorer.Trees {
	public abstract class DataModel : IDataModel {
		DataTreeNodeX node;
		ContextMenuStrip ctxMenu;
		Color color;
		string text;

		public DataModel() {
			Children = new BatchObservableCollection<IDataModel>();
		}

		public virtual void OnCollapse() {
		}

		protected void OnCollapseRequested() {
			if (CollapseRequested != null) {
				CollapseRequested(this, EventArgs.Empty);
			}
		}

		public virtual void OnExpand() {
		}

		protected void OnExpandRequested() {
			if (ExpandRequested != null) {
				ExpandRequested(this, EventArgs.Empty);
			}
		}

		protected void OnPropertyChanged(string property) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public abstract void Refresh(bool refreshChildren);

		public virtual void DrawIcon(Graphics g, Rectangle bound) {
		}

		public virtual bool HasIcon {
			get { return false; }
		}

		public BatchObservableCollection<IDataModel> Children { get; private set; }

		public DataTreeNodeX Node {
			get { return node; }
			set {
				if (node != value) {
					node = value;
					OnPropertyChanged("Node");
				}
			}
		}

		public ContextMenuStrip ContextMenu {
			get { return ctxMenu; }
			set {
				if (ctxMenu != value) {
					ctxMenu = value;
					OnPropertyChanged("ContextMenu");
				}
			}
		}

		public Color ForeColor {
			get { return color; }
			set {
				if (color != value) {
					color = value;
					OnPropertyChanged("ForeColor");
				}
			}
		}

		public string Text {
			get { return text; }
			set {
				if (text != value) {
					text = value;
					OnPropertyChanged("Text");
				}
			}
		}

		public event EventHandler CollapseRequested;
		public event EventHandler ExpandRequested;
		public event PropertyChangedEventHandler PropertyChanged;
	}
}