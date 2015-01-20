using System;
using System.Windows.Forms;
using dnExplorer.Models;

namespace dnExplorer.Views {
	public class MultipleAnalysesView : ViewBase<MultipleAnalysesModel> {
		protected override void OnModelUpdated() {
		}

		ContextMenuStrip ctxMenu;

		protected internal override ContextMenuStrip GetContextMenu() {
			if (ctxMenu != null)
				return ctxMenu;

			ctxMenu = new ContextMenuStrip();

			var gotoDef = new ToolStripMenuItem("Go To Definition");
			gotoDef.Click += GotoDef;
			ctxMenu.Items.Add(gotoDef);

			return ctxMenu;
		}

		void GotoDef(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<MultipleAnalysesModel>();
			App.Modules.NavigateTarget(model.Item);
		}
	}
}