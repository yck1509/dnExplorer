using System;
using System.Drawing;
using System.Windows.Forms;

namespace dnExplorer.Trees {
	public class TreeNodeX : TreeNode {
		protected internal virtual void OnCollapse() {
		}

		protected internal virtual void OnExpand() {
		}

		protected internal virtual void DrawIcon(Graphics g, Rectangle bounds) {
		}

		protected internal virtual bool HasIcon {
			get { return false; }
		}

		protected internal virtual void OnMouseClick(TreeNodeMouseClickEventArgs e) {
			if (NodeMouseClick != null) {
				NodeMouseClick(this, e);
			}
		}

		protected internal virtual void OnMouseDoubleClick(TreeNodeMouseClickEventArgs e) {
			if (NodeMouseDoubleClick != null) {
				NodeMouseDoubleClick(this, e);
			}
		}

		protected internal virtual void OnMouseHover(TreeNodeMouseHoverEventArgs e) {
			if (NodeMouseHover != null) {
				NodeMouseHover(this, e);
			}
		}

		public event TreeNodeMouseClickEventHandler NodeMouseClick;
		public event TreeNodeMouseClickEventHandler NodeMouseDoubleClick;
		public event TreeNodeMouseHoverEventHandler NodeMouseHover;
	}
}