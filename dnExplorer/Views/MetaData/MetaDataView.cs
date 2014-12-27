using System;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Nodes;

namespace dnExplorer.Views {
	public class MetaDataView : ViewBase {
		GridView gridView;
		HexViewer hexView;

		public MetaDataView() {
			var split = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			Controls.Add(split);

			gridView = new GridView();
			gridView.AddColumn(new GridView.Column("Field", true, 120));
			gridView.AddColumn(new GridView.Column("Offset", true));
			gridView.AddColumn(new GridView.Column("Value", false));
			split.Panel1.Controls.Add(gridView);

			hexView = new HexViewer();
			split.Panel2.Controls.Add(hexView);

			PerformLayout();
		}

		protected override void OnModelUpdated() {
			var model = (MetaDataModel)Model;
			gridView.Clear();
			if (model != null) {
				uint offset = (uint)model.MetaData.MetaDataHeader.StartOffset;

				gridView.AddRow("Signature", offset, model.MetaData.MetaDataHeader.Signature);
				offset += 4;

				gridView.AddRow("MajorVersion", offset, model.MetaData.MetaDataHeader.MajorVersion);
				offset += 2;

				gridView.AddRow("MinorVersion", offset, model.MetaData.MetaDataHeader.MinorVersion);
				offset += 2;

				gridView.AddRow("Reserved", offset, model.MetaData.MetaDataHeader.Reserved1);
				offset += 4;

				gridView.AddRow("VersionLength", offset, model.MetaData.MetaDataHeader.StringLength);
				offset += 4;

				gridView.AddRow("VersionString", offset, model.MetaData.MetaDataHeader.VersionString);
				offset += model.MetaData.MetaDataHeader.StringLength;

				gridView.AddRow("Flags", offset, model.MetaData.MetaDataHeader.Flags);
				offset++;

				gridView.AddRow("Reserved", offset, model.MetaData.MetaDataHeader.Reserved2);
				offset++;

				gridView.AddRow("NumberOfStreams", offset, model.MetaData.MetaDataHeader.Streams);
				offset += 2;

				hexView.Stream = model.MetaData.PEImage.CreateStream(model.MetaData.MetaDataHeader);
			}
		}
	}
}