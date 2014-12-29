using System;
using System.ComponentModel;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Nodes;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.PE;

namespace dnExplorer.Views {
	public class PECLIView : ViewBase {
		GridView view;

		ContextMenuStrip showDataMenu;
		ContextMenuStrip showEntryMenu;

		public PECLIView() {
			view = new GridView();
			view.AddColumn(new GridView.Column("Field", true, 200));
			view.AddColumn(new GridView.Column("Value", false, 200));
			Controls.Add(view);

			showDataMenu = new ContextMenuStrip();
			var item = new ToolStripMenuItem("Show in Raw Data");
			item.Click += OnShowData;
			showDataMenu.Items.Add(item);

			showEntryMenu = new ContextMenuStrip();
			item = new ToolStripMenuItem("Show in MD Tables");
			item.Click += OnShowEntry;
			showEntryMenu.Opening += showEntryMenu_Opening;
			showEntryMenu.Items.Add(item);
		}

		protected override void OnModelUpdated() {
			var model = (PECLIModel)Model;
			view.Clear();
			if (model != null) {
				view.AddRow("CB", model.CLIHeader.CB);
				view.AddRow("MajorRuntimeVersion", model.CLIHeader.MajorRuntimeVersion);
				view.AddRow("MinorRuntimeVersion", model.CLIHeader.MinorRuntimeVersion);

				view.AddRow("MetaData RVA", model.CLIHeader.MetaData.VirtualAddress, showDataMenu);
				view.AddRow("MetaData Size", model.CLIHeader.MetaData.Size, showDataMenu);

				view.AddRow("Flags", model.CLIHeader.Flags);
				view.AddRow("Entry Point", model.CLIHeader.EntryPointToken_or_RVA, showEntryMenu);

				view.AddRow("Resources RVA", model.CLIHeader.Resources.VirtualAddress, showDataMenu);
				view.AddRow("Resources Size", model.CLIHeader.Resources.Size, showDataMenu);

				view.AddRow("StrongNameSignature RVA", model.CLIHeader.StrongNameSignature.VirtualAddress, showDataMenu);
				view.AddRow("StrongNameSignature Size", model.CLIHeader.StrongNameSignature.Size, showDataMenu);

				view.AddRow("CodeManagerTable RVA", model.CLIHeader.CodeManagerTable.VirtualAddress, showDataMenu);
				view.AddRow("CodeManagerTable Size", model.CLIHeader.CodeManagerTable.Size, showDataMenu);

				view.AddRow("VTableFixups RVA", model.CLIHeader.VTableFixups.VirtualAddress, showDataMenu);
				view.AddRow("VTableFixups Size", model.CLIHeader.VTableFixups.Size, showDataMenu);

				view.AddRow("ExportAddressTableJumps RVA", model.CLIHeader.ExportAddressTableJumps.VirtualAddress, showDataMenu);
				view.AddRow("ExportAddressTableJumps Size", model.CLIHeader.ExportAddressTableJumps.Size, showDataMenu);

				view.AddRow("ManagedNativeHeader RVA", model.CLIHeader.ManagedNativeHeader.VirtualAddress, showDataMenu);
				view.AddRow("ManagedNativeHeader Size", model.CLIHeader.ManagedNativeHeader.Size, showDataMenu);
			}
		}

		void OnShowData(object sender, EventArgs e) {
			var model = (PECLIModel)Model;
			var cell = view.SelectedCells[0];

			RVA rva;
			uint size;
			if (((string)view[0, cell.RowIndex].Value).EndsWith("RVA")) {
				rva = (RVA)cell.Value;
				size = (uint)view[cell.ColumnIndex, cell.RowIndex + 1].Value;
			}
			else {
				rva = (RVA)view[cell.ColumnIndex, cell.RowIndex - 1].Value;
				size = (uint)cell.Value;
			}

			var section = model.Image.ToImageSectionHeader(rva);
			if (section == null) {
				MessageBox.Show("Invalid address.", Main.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var offset = (long)model.Image.ToFileOffset(rva);
			ViewUtils.ShowRawData(Model, model.Image, offset, offset + size - 1);
		}

		void showEntryMenu_Opening(object sender, CancelEventArgs e) {
			showEntryMenu.Items[0].Enabled = (((PECLIModel)Model).CLIHeader.Flags & ComImageFlags.NativeEntryPoint) == 0;
		}

		void OnShowEntry(object sender, EventArgs e) {
			var model = (PECLIModel)Model;
			var cell = view.SelectedCells[0];
			var token = new MDToken((uint)cell.Value);

			ViewUtils.ShowToken(Model, model.Image, token);
		}
	}
}