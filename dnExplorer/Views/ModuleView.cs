using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Models;
using dnlib.DotNet;
using dnlib.DotNet.MD;

namespace dnExplorer.Views {
	public class ModuleView : ViewBase {
		public ModuleView() {
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

		static ContextMenuStrip ctxMenu;

		protected internal override ContextMenuStrip GetContextMenu() {
			if (ctxMenu != null)
				return ctxMenu;

			ctxMenu = new ContextMenuStrip();

			var gotoEntryMD = new ToolStripMenuItem("Go To Entry Point");
			gotoEntryMD.Click += GotoEntryPoint;
			ctxMenu.Items.Add(gotoEntryMD);

			ctxMenu.Items.Add(new ToolStripSeparator());

			var remove = new ToolStripMenuItem("Remove", Resources.GetResource<Image>("Icons.delete.png"));
			remove.Click += Remove;
			ctxMenu.Items.Add(remove);

			ctxMenu.Opening += (sender, e) => {
				var model = sender.GetContextMenuModel<dnModuleModel>();

				bool canGoEntry = model.Module.MetaData != null;
				if (canGoEntry) {
					var flags = model.Module.MetaData.ImageCor20Header.Flags;
					if ((flags & ComImageFlags.NativeEntryPoint) != 0)
						canGoEntry = false;

					var entryPoint = model.Module.MetaData.ImageCor20Header.EntryPointToken_or_RVA;
					if (entryPoint == 0)
						canGoEntry = false;
				}
				gotoEntryMD.Enabled = canGoEntry;
			};

			return ctxMenu;
		}

		static void GotoEntryPoint(object sender, EventArgs e) {
			var model = sender.GetContextMenuModel<dnModuleModel>();
			var token = new MDToken(model.Module.MetaData.ImageCor20Header.EntryPointToken_or_RVA);
			ViewUtils.ShowToken(model, model.Module.Image, token);
		}

		static void Remove(object sender, EventArgs e) {
			var node = sender.GetContextMenuModel<dnModuleModel>().Node;
			node.TreeView.Nodes.Remove(node);
		}
	}
}