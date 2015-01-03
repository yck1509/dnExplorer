using System;
using dnExplorer.Controls;
using dnExplorer.Models;

namespace dnExplorer.Views {
	public class PEImageView : ViewBase<PEImageModel> {
		GridView view;

		public PEImageView() {
			view = new GridView();
			view.AddColumn(new GridView.Column("Name", true, 150));
			view.AddColumn(new GridView.Column("Value", false, 450));
			Controls.Add(view);
		}

		protected override void OnModelUpdated() {
			view.Clear();
			if (Model != null) {
				view.AddRow("Location", new RawString(Model.Image.FileName ?? "<Unknown>"));
				view.AddRow("Machine", Model.Image.ImageNTHeaders.FileHeader.Machine);
				view.AddRow("Characteristics", Model.Image.ImageNTHeaders.FileHeader.Characteristics);
				view.AddRow("AddressOfEntryPoint", Model.Image.ImageNTHeaders.OptionalHeader.AddressOfEntryPoint);
				view.AddRow("CheckSum", Model.Image.ImageNTHeaders.OptionalHeader.CheckSum);
				view.AddRow("Subsystem", Model.Image.ImageNTHeaders.OptionalHeader.Subsystem);
				view.AddRow("DllCharacteristics", Model.Image.ImageNTHeaders.OptionalHeader.DllCharacteristics);
				view.AddRow("NumberOfRvaAndSizes", Model.Image.ImageNTHeaders.OptionalHeader.NumberOfRvaAndSizes);
			}
		}
	}
}