using System;
using dnExplorer.Controls;
using dnExplorer.Models;

namespace dnExplorer.Views {
	public class PEImageView : ViewBase {
		GridView view;

		public PEImageView() {
			view = new GridView();
			view.AddColumn(new GridView.Column("Name", true, 150));
			view.AddColumn(new GridView.Column("Value", false, 450));
			Controls.Add(view);
		}

		protected override void OnModelUpdated() {
			var model = (PEImageModel)Model;
			view.Clear();
			if (model != null) {
				view.AddRow("Location", new RawString(model.Image.FileName ?? "<Unknown>"));
				view.AddRow("Machine", model.Image.ImageNTHeaders.FileHeader.Machine);
				view.AddRow("Characteristics", model.Image.ImageNTHeaders.FileHeader.Characteristics);
				view.AddRow("AddressOfEntryPoint", model.Image.ImageNTHeaders.OptionalHeader.AddressOfEntryPoint);
				view.AddRow("CheckSum", model.Image.ImageNTHeaders.OptionalHeader.CheckSum);
				view.AddRow("Subsystem", model.Image.ImageNTHeaders.OptionalHeader.Subsystem);
				view.AddRow("DllCharacteristics", model.Image.ImageNTHeaders.OptionalHeader.DllCharacteristics);
				view.AddRow("NumberOfRvaAndSizes", model.Image.ImageNTHeaders.OptionalHeader.NumberOfRvaAndSizes);
			}
		}
	}
}