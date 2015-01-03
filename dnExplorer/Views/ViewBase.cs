using System;
using System.Windows.Forms;
using dnExplorer.Trees;

namespace dnExplorer.Views {
	public interface IView {
		Control ViewControl { get; }
		object Model { get; set; }

		ContextMenuStrip GetContextMenu();
	}

	public abstract class ViewBase<TModel> : Control, IView where TModel : class, IDataModel {
		TModel model;

		public ViewBase() {
			Dock = DockStyle.Fill;
		}

		public TModel Model {
			get { return model; }
			set {
				if (model != value) {
					model = value;
					OnModelUpdated();
				}
			}
		}

		object IView.Model {
			get { return Model; }
			set { Model = (TModel)value; }
		}

		Control IView.ViewControl {
			get { return this; }
		}

		protected abstract void OnModelUpdated();

		protected internal virtual ContextMenuStrip GetContextMenu() {
			return null;
		}

		ContextMenuStrip IView.GetContextMenu() {
			return GetContextMenu();
		}
	}
}