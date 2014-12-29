using System;
using System.Windows.Forms;
using dnExplorer.Trees;

namespace dnExplorer.Views {
	public abstract class ViewBase : Control {
		IDataModel model;

		public ViewBase() {
			Dock = DockStyle.Fill;
		}

		public IDataModel Model {
			get { return model; }
			set {
				if (model != value) {
					model = value;
					OnModelUpdated();
				}
			}
		}

		protected abstract void OnModelUpdated();

		protected internal virtual ContextMenuStrip GetContextMenu() {
			return null;
		}
	}
}