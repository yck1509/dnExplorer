using System;
using System.Windows.Forms;
using dnExplorer.Models;

namespace dnExplorer.Views {
	public class AnalysisView : ViewBase<AnalysisModel> {
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

			ctxMenu.Opening += (sender, e) => {
				var model = sender.GetContextMenuModel<AnalysisModel>();
				if (model.IsRoot)
					e.Cancel = true;
			};

			return ctxMenu;
		}

		void GotoDef(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<AnalysisModel>();
			App.Modules.NavigateTarget(model.Item);
		}
	}
}