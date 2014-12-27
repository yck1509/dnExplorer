using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Nodes;

namespace dnExplorer.Views {
	public class PEDDView : ViewBase {
		GridView view;

		public PEDDView() {
			view = new GridView();
			view.AddColumn(new GridView.Column("Directory", true, 200));
			view.AddColumn(new GridView.Column("RVA", false));
			view.AddColumn(new GridView.Column("Size", false));
			view.AddColumn(new GridView.Column("Section", false));
			Controls.Add(view);
		}

		static readonly string[] DirectoryNames = {
			"Export Table",
			"Import Table",
			"Resource Table",
			"Exception Table",
			"Certificate Table",
			"Base Relocation Table",
			"Debug",
			"Copyright",
			"Global Ptr",
			"TLS Table",
			"Load Config Table",
			"Bound Import",
			"Import Address Table",
			"Delay Import Descriptor",
			"CLI Header",
			"Reserved"
		};

		protected override void OnModelUpdated() {
			var model = (PEDDModel)Model;
			view.Clear();
			if (model != null) {
				for (int i = 0; i < 0x10; i++) {
					var dir = model.Image.ImageNTHeaders.OptionalHeader.DataDirectories[i];
					var section = model.Image.ToImageSectionHeader(dir.VirtualAddress);

					GridView.Cell sectionCell;
					if (dir.VirtualAddress != 0 && section == null)
						sectionCell = new GridView.Cell("Invalid", back: ControlPaint.Light(Color.Red));
					else if (section != null)
						sectionCell = new GridView.Cell(section.DisplayName, back: SystemColors.ControlLight);
					else
						sectionCell = new GridView.Cell("", back: SystemColors.ControlLight);

					view.AddRow(DirectoryNames[i], dir.VirtualAddress, dir.Size, sectionCell);
				}
			}
		}
	}
}