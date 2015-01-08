using System;
using System.Linq;
using System.Windows.Forms;
using dnExplorer.Models;
using dnlib.DotNet;
using dnlib.DotNet.MD;

namespace dnExplorer.Views {
	public class MDRowView : ViewBase<MDRowModel> {
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

			var showBrowser = new ToolStripMenuItem("Show in Browser");
			showBrowser.Click += ShowBrowser;
			ctxMenu.Items.Add(showBrowser);

			ctxMenu.Items.Add(new ToolStripSeparator());

			var copyToken = new ToolStripMenuItem("Copy Token");
			copyToken.Click += CopyToken;
			ctxMenu.Items.Add(copyToken);

			var copyTokenRaw = new ToolStripMenuItem("Copy Raw Token");
			copyTokenRaw.Click += CopyTokenRaw;
			ctxMenu.Items.Add(copyTokenRaw);

			ctxMenu.Items.Add(new ToolStripSeparator());
			ctxMenu.Opening += (sender, e) => {
				var model = sender.GetContextMenuModel<MDRowModel>();
				var view = (MDTableHeapView)ViewLocator.LocateViews(model.Parent.Parent).Single();
				ToolStripManager.Merge(view.GetContextMenu(), (ContextMenuStrip)sender);

				var canShowBrowser = GetBrowserToken(model) != null;
				canShowBrowser &= model.Parent.Parent.Module.ModuleDef != null;
				showBrowser.Visible = showBrowser.Enabled = canShowBrowser;
			};
			ctxMenu.Closed += (sender, e) => {
				var model = sender.GetContextMenuModel<MDRowModel>();
				var view = (MDTableHeapView)ViewLocator.LocateViews(model.Parent.Parent).Single();
				ToolStripManager.RevertMerge((ContextMenuStrip)sender, view.GetContextMenu());
			};

			return ctxMenu;
		}

		static void ShowHex(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MDRowModel>();
			var view = (MDTableHeapView)ViewLocator.LocateViews(model.Parent.Parent).Single();

			var offset = model.Parent.MDTable.StartOffset - model.Parent.Tables.StartOffset;
			offset += (model.Rid - 1) * model.Parent.MDTable.RowSize;
			var size = model.Parent.MDTable.RowSize;
			view.SelectHexRange((uint)offset, (uint)(offset + size - 1));
		}

		static MDToken? GetBrowserToken(MDRowModel model) {
			var table = model.Parent.MDTable.Table;
			switch (table) {
				case Table.TypeDef:
				case Table.Method:
				case Table.Field:
				case Table.Property:
				case Table.Event:
					return new MDToken(table, model.Rid);
				default:
					return null;
			}
		}

		static void ShowBrowser(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MDRowModel>();
			var token = GetBrowserToken(model);
			var module = model.Parent.Parent.Module.ModuleDef;
			var item = module.ResolveToken(token.Value.Raw);
			if (item == null)
				return;
			ViewUtils.ShowMember(model.Parent.Parent, (IMemberDef)item);
		}

		static void CopyToken(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MDRowModel>();
			var token = new MDToken(model.Parent.MDTable.Table, model.Rid);
			Clipboard.SetText(token.ToDescription());
		}

		static void CopyTokenRaw(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MDRowModel>();
			var token = new MDToken(model.Parent.MDTable.Table, model.Rid);
			Clipboard.SetText(token.Raw.ToString("x8"));
		}
	}
}