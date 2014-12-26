using System;
using dnExplorer.Controls;
using dnExplorer.Nodes;

namespace dnExplorer.Views {
	public class PECLIView : ViewBase {
		GridView view;

		public PECLIView() {
			view = new GridView();
			view.AddColumn(new GridView.Column("Field", true, 200));
			view.AddColumn(new GridView.Column("Value", false, 200));
			Controls.Add(view);
		}

		protected override void OnModelUpdated() {
			var model = (PECLIModel)Model;
			view.Clear();
			if (model != null) {
				view.AddRow("CB", model.CLIHeader.CB);
				view.AddRow("MajorRuntimeVersion", model.CLIHeader.MajorRuntimeVersion);
				view.AddRow("MinorRuntimeVersion", model.CLIHeader.MinorRuntimeVersion);

				view.AddRow("MetaData RVA", model.CLIHeader.MetaData.VirtualAddress);
				view.AddRow("MetaData Size", model.CLIHeader.MetaData.Size);

				view.AddRow("Flags", model.CLIHeader.Flags);
				view.AddRow("Entry Point", model.CLIHeader.EntryPointToken_or_RVA);

				view.AddRow("Resources RVA", model.CLIHeader.Resources.VirtualAddress);
				view.AddRow("Resources Size", model.CLIHeader.Resources.Size);

				view.AddRow("StrongNameSignature RVA", model.CLIHeader.StrongNameSignature.VirtualAddress);
				view.AddRow("StrongNameSignature Size", model.CLIHeader.StrongNameSignature.Size);

				view.AddRow("CodeManagerTable RVA", model.CLIHeader.CodeManagerTable.VirtualAddress);
				view.AddRow("CodeManagerTable Size", model.CLIHeader.CodeManagerTable.Size);

				view.AddRow("VTableFixups RVA", model.CLIHeader.VTableFixups.VirtualAddress);
				view.AddRow("VTableFixups Size", model.CLIHeader.VTableFixups.Size);

				view.AddRow("ExportAddressTableJumps RVA", model.CLIHeader.ExportAddressTableJumps.VirtualAddress);
				view.AddRow("ExportAddressTableJumps Size", model.CLIHeader.ExportAddressTableJumps.Size);

				view.AddRow("ManagedNativeHeader RVA", model.CLIHeader.ManagedNativeHeader.VirtualAddress);
				view.AddRow("ManagedNativeHeader Size", model.CLIHeader.ManagedNativeHeader.Size);
			}
		}
	}
}