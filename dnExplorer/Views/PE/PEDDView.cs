using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Models;

namespace dnExplorer.Views {
	public class PEDDView : ViewBase<PEDDModel> {
		GridView view;
		ContextMenuStrip ctxMenu;

		public PEDDView() {
			view = new GridView();
			view.AddColumn(new GridView.Column("Directory", true, 200));
			view.AddColumn(new GridView.Column("RVA", false));
			view.AddColumn(new GridView.Column("Size", false));
			view.AddColumn(new GridView.Column("Section", false));
			Controls.Add(view);

			ctxMenu = new ContextMenuStrip();
			var nav = new ToolStripMenuItem("Show in Raw Data");
			nav.Click += OnShowData;
			ctxMenu.Items.Add(nav);
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
			view.Clear();
			if (Model != null) {
				for (int i = 0; i < 0x10; i++) {
					var dir = Model.Image.ImageNTHeaders.OptionalHeader.DataDirectories[i];
					var section = Model.Image.ToImageSectionHeader(dir.VirtualAddress);

					GridView.Cell sectionCell;
					if (dir.VirtualAddress != 0 && section == null)
						sectionCell = new GridView.Cell("Invalid", back: ControlPaint.Light(Color.Red));
					else if (section != null)
						sectionCell = new GridView.Cell(section.DisplayName, back: SystemColors.ControlLight);
					else
						sectionCell = new GridView.Cell("", back: SystemColors.ControlLight);

					view.AddRow(DirectoryNames[i], dir.VirtualAddress, dir.Size, sectionCell, ctxMenu);
				}
			}
		}

		void OnShowData(object sender, EventArgs e) {
			var row = view.SelectedCells[0].RowIndex;
			var dd = Model.Image.ImageNTHeaders.OptionalHeader.DataDirectories[row - 1];

			var section = Model.Image.ToImageSectionHeader(dd.VirtualAddress);
			if (section == null) {
				MessageBox.Show("Invalid address.", App.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var offset = (long)Model.Image.ToFileOffset(dd.VirtualAddress);
			ViewUtils.ShowRawData(App, Model, Model.Image, offset, offset + dd.Size - 1);
		}
	}
}