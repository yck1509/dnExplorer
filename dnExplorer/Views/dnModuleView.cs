using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Models;
using dnlib.DotNet;
using dnlib.DotNet.MD;

namespace dnExplorer.Views {
	public class dnModuleView : ViewBase<dnModuleModel> {
		public dnModuleView() {
			var label = new Label {
				Text = "This page is intentionally left blank\r\nI'm thinking of what to put here :P",
				TextAlign = ContentAlignment.MiddleCenter,
				Dock = DockStyle.Fill,
				AutoSize = false
			};
			Controls.Add(label);
		}

		protected override void OnModelUpdated() {
		}

		ContextMenuStrip ctxMenu;

		protected internal override ContextMenuStrip GetContextMenu() {
			if (ctxMenu != null)
				return ctxMenu;

			ctxMenu = new ContextMenuStrip();

			var gotoEntry = new ToolStripMenuItem("Go To Entry Point");
			gotoEntry.Click += GotoEntryPoint;
			ctxMenu.Items.Add(gotoEntry);

			var gotoGlobal = new ToolStripMenuItem("Go To Global Type");
			gotoGlobal.Click += GotoGlobalType;
			ctxMenu.Items.Add(gotoGlobal);

			ctxMenu.Items.Add(new ToolStripSeparator());

			var remove = new ToolStripMenuItem("Remove", Resources.GetResource<Image>("Icons.delete.png"));
			remove.Click += Remove;
			ctxMenu.Items.Add(remove);

			ctxMenu.Opening += (sender, e) => {
				var model = sender.GetContextMenuModel<dnModuleModel>();

				bool hasMD = model.Module.MetaData != null;
				if (hasMD) {
					gotoEntry.Enabled = true;

					var flags = model.Module.MetaData.ImageCor20Header.Flags;
					if ((flags & ComImageFlags.NativeEntryPoint) != 0)
						gotoEntry.Enabled = false;

					var entryPoint = model.Module.MetaData.ImageCor20Header.EntryPointToken_or_RVA;
					if (entryPoint == 0)
						gotoEntry.Enabled = false;

					gotoGlobal.Enabled = model.Module.MetaData.TablesStream.TypeDefTable.Rows > 0;
				}
				else {
					gotoEntry.Enabled = false;
					gotoGlobal.Enabled = false;
				}
			};

			return ctxMenu;
		}

		void GotoEntryPoint(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<dnModuleModel>();
			if (model.Module.ModuleDef != null && model.Module.ModuleDef.EntryPoint != null) {
				ViewUtils.ShowMember(model, model.Module.ModuleDef.EntryPoint);
			}
			else {
				var token = new MDToken(model.Module.MetaData.ImageCor20Header.EntryPointToken_or_RVA);
				ViewUtils.ShowToken(App, model, model.Module.Image, token);
			}
		}

		void GotoGlobalType(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<dnModuleModel>();
			if (model.Module.ModuleDef != null && model.Module.ModuleDef.GlobalType != null) {
				ViewUtils.ShowMember(model, model.Module.ModuleDef.GlobalType);
			}
			else {
				var token = new MDToken(Table.TypeDef, 1);
				ViewUtils.ShowToken(App, model, model.Module.Image, token);
			}
		}

		void Remove(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<dnModuleModel>();
			App.Modules.RemoveModule(model.Module);
		}
	}
}