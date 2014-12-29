using System;
using System.Windows.Forms;
using dnExplorer.Nodes;
using dnlib.DotNet;

namespace dnExplorer.Views {
	public class MDRowView : ViewBase {
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

			var copyToken = new ToolStripMenuItem("Copy Token");
			copyToken.Click += CopyToken;
			ctxMenu.Items.Add(copyToken);

			var copyTokenRaw = new ToolStripMenuItem("Copy Raw Token");
			copyTokenRaw.Click += CopyTokenRaw;
			ctxMenu.Items.Add(copyTokenRaw);

			ctxMenu.Items.Add(new ToolStripSeparator());
			ctxMenu.Opening += (sender, e) => {
				var model = sender.GetContextMenuModel<MDRowModel>();
				var view = (MDTableHeapView)ViewLocator.LocateView(model.Parent.Parent);
				ToolStripManager.Merge(view.GetContextMenu(), (ContextMenuStrip)sender);
			};
			ctxMenu.Closed += (sender, e) => {
				var model = sender.GetContextMenuModel<MDRowModel>();
				var view = (MDTableHeapView)ViewLocator.LocateView(model.Parent.Parent);
				ToolStripManager.RevertMerge((ContextMenuStrip)sender, view.GetContextMenu());
			};

			return ctxMenu;
		}

		static void ShowHex(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MDRowModel>();
			var view = (MDTableHeapView)ViewLocator.LocateView(model.Parent.Parent);

			var offset = model.Parent.MDTable.StartOffset - model.Parent.Tables.StartOffset;
			offset += (model.Rid - 1) * model.Parent.MDTable.RowSize;
			var size = model.Parent.MDTable.RowSize;
			view.SelectHexRange((uint)offset, (uint)(offset + size - 1));
		}

		static void CopyToken(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MDRowModel>();
			var token = new MDToken(model.Parent.MDTable.Table, model.Rid);
			Clipboard.SetText(token.ToDescription());
		}

		static void CopyTokenRaw(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MDRowModel>();
			var token = new MDToken(model.Parent.MDTable.Table, model.Rid);
			Clipboard.SetText(token.Raw.ToString("X8"));
		}
	}
}