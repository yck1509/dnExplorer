using System;
using dnExplorer.Controls;
using dnExplorer.Nodes;

namespace dnExplorer.Views {
	public class PESectionsView : ViewBase {
		GridView view;

		public PESectionsView() {
			view = new GridView();
			view.AddColumn(new GridView.Column("Name", false, 90));
			view.AddColumn(new GridView.Column("Virtual Size", false, 90));
			view.AddColumn(new GridView.Column("Virtual Address", false, 90));
			view.AddColumn(new GridView.Column("Raw Size", false, 90));
			view.AddColumn(new GridView.Column("Raw Address", false, 90));
			view.AddColumn(new GridView.Column("Reloc", false, 90));
			view.AddColumn(new GridView.Column("Linenums.", false, 90));
			view.AddColumn(new GridView.Column("Reloc No.", false, 90));
			view.AddColumn(new GridView.Column("Linenums. No.", false, 90));
			view.AddColumn(new GridView.Column("Characteristics", false, 90));
			Controls.Add(view);
		}

		protected override void OnModelUpdated() {
			var model = (PESectionsModel)Model;
			view.Clear();
			if (model != null) {
				foreach (var section in model.Image.ImageSectionHeaders)
					view.AddRow(
						section.DisplayName,
						section.VirtualSize,
						section.VirtualAddress,
						section.SizeOfRawData,
						section.PointerToRawData,
						section.PointerToRelocations,
						section.PointerToLinenumbers,
						section.NumberOfRelocations,
						section.NumberOfRelocations,
						section.Characteristics);
			}
			view.LayoutGrid();
		}
	}
}