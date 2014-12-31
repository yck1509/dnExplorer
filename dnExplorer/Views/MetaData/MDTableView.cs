using System;
using System.Windows.Forms;
using dnExplorer.Models;

namespace dnExplorer.Views {
	public class MDTableView : ViewBase {
		protected override void OnModelUpdated() {
		}

		static ContextMenuStrip ctxMenu;

		protected internal override ContextMenuStrip GetContextMenu() {
			if (ctxMenu != null)
				return ctxMenu;

			ctxMenu = new ContextMenuStrip();

			var showHex = new ToolStripMenuItem("Show in Hex View");
			showHex.Click += ShowHex;
			ctxMenu.Items.Add(showHex);

			ctxMenu.Items.Add(new ToolStripSeparator());
			ctxMenu.Opening += (sender, e) => {
				var model = sender.GetContextMenuModel<MDTableModel>();
				var view = (MDTableHeapView)ViewLocator.LocateView(model.Parent);
				ToolStripManager.Merge(view.GetContextMenu(), (ContextMenuStrip)sender);
			};
			ctxMenu.Closed += (sender, e) => {
				var model = sender.GetContextMenuModel<MDTableModel>();
				var view = (MDTableHeapView)ViewLocator.LocateView(model.Parent);
				ToolStripManager.RevertMerge((ContextMenuStrip)sender, view.GetContextMenu());
			};

			return ctxMenu;
		}

		static void ShowHex(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MDTableModel>();
			var view = (MDTableHeapView)ViewLocator.LocateView(model.Parent);

			var offset = model.MDTable.StartOffset - model.Tables.StartOffset;
			var size = model.MDTable.Rows * model.MDTable.RowSize;
			view.SelectHexRange((uint)offset, (uint)(offset + size - 1));
		}
	}
}