using System;
using System.ComponentModel;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Models;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.PE;

namespace dnExplorer.Views {
	public class PECLIView : ViewBase<PECLIModel> {
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
			view.Clear();
			if (Model != null) {
				view.AddRow("CB", Model.CLIHeader.CB);
				view.AddRow("MajorRuntimeVersion", Model.CLIHeader.MajorRuntimeVersion);
				view.AddRow("MinorRuntimeVersion", Model.CLIHeader.MinorRuntimeVersion);

				view.AddRow("MetaData RVA", Model.CLIHeader.MetaData.VirtualAddress, showDataMenu);
				view.AddRow("MetaData Size", Model.CLIHeader.MetaData.Size, showDataMenu);

				view.AddRow("Flags", Model.CLIHeader.Flags);
				view.AddRow("Entry Point", Model.CLIHeader.EntryPointToken_or_RVA, showEntryMenu);

				view.AddRow("Resources RVA", Model.CLIHeader.Resources.VirtualAddress, showDataMenu);
				view.AddRow("Resources Size", Model.CLIHeader.Resources.Size, showDataMenu);

				view.AddRow("StrongNameSignature RVA", Model.CLIHeader.StrongNameSignature.VirtualAddress, showDataMenu);
				view.AddRow("StrongNameSignature Size", Model.CLIHeader.StrongNameSignature.Size, showDataMenu);

				view.AddRow("CodeManagerTable RVA", Model.CLIHeader.CodeManagerTable.VirtualAddress, showDataMenu);
				view.AddRow("CodeManagerTable Size", Model.CLIHeader.CodeManagerTable.Size, showDataMenu);

				view.AddRow("VTableFixups RVA", Model.CLIHeader.VTableFixups.VirtualAddress, showDataMenu);
				view.AddRow("VTableFixups Size", Model.CLIHeader.VTableFixups.Size, showDataMenu);

				view.AddRow("ExportAddressTableJumps RVA", Model.CLIHeader.ExportAddressTableJumps.VirtualAddress, showDataMenu);
				view.AddRow("ExportAddressTableJumps Size", Model.CLIHeader.ExportAddressTableJumps.Size, showDataMenu);

				view.AddRow("ManagedNativeHeader RVA", Model.CLIHeader.ManagedNativeHeader.VirtualAddress, showDataMenu);
				view.AddRow("ManagedNativeHeader Size", Model.CLIHeader.ManagedNativeHeader.Size, showDataMenu);
			}
		}

		void OnShowData(object sender, EventArgs e) {
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

			var section = Model.Image.ToImageSectionHeader(rva);
			if (section == null) {
				MessageBox.Show("Invalid address.", App.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var offset = (long)Model.Image.ToFileOffset(rva);
			ViewUtils.ShowRawData(App, Model, Model.Image, offset, offset + size - 1);
		}

		void showEntryMenu_Opening(object sender, CancelEventArgs e) {
			showEntryMenu.Items[0].Enabled = (Model.CLIHeader.Flags & ComImageFlags.NativeEntryPoint) == 0;
		}

		void OnShowEntry(object sender, EventArgs e) {
			var cell = view.SelectedCells[0];
			var token = new MDToken((uint)cell.Value);

			ViewUtils.ShowToken(App, Model, Model.Image, token);
		}
	}
}