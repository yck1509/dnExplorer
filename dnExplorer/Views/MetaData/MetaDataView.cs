using System;
using System.Drawing;
using System.Windows.Forms;
using dnExplorer.Controls;
using dnExplorer.Nodes;

namespace dnExplorer.Views {
	public class MetaDataView : ViewBase {
		GridView hdrGridView;
		GridView strGridView;
		HexViewer hexView;

		public MetaDataView() {
			var split1 = new SplitContainer {
				Orientation = Orientation.Horizontal,
				Dock = DockStyle.Fill
			};
			Controls.Add(split1);

			var split2 = new SplitContainer {
				Orientation = Orientation.Vertical,
				Dock = DockStyle.Fill
			};
			split1.Panel1.Controls.Add(split2);

			hdrGridView = new GridView();
			hdrGridView.AddColumn(new GridView.Column("Field", true, 120));
			hdrGridView.AddColumn(new GridView.Column("Offset", true));
			hdrGridView.AddColumn(new GridView.Column("Value", false));
			split2.Panel1.Controls.Add(hdrGridView);

			strGridView = new GridView();
			strGridView.AddColumn(new GridView.Column("Offset", false));
			strGridView.AddColumn(new GridView.Column("Size", false));
			strGridView.AddColumn(new GridView.Column("Name", false));
			strGridView.SelectionChanged += OnStreamSelectionChanged;
			split2.Panel2.Controls.Add(strGridView);

			hexView = new HexViewer();
			split1.Panel2.Controls.Add(hexView);

			PerformLayout();

			split2.SplitterDistance = split2.Width / 2;
		}

		void OnStreamSelectionChanged(object sender, EventArgs e) {
			if (strGridView.Rows.Count < 1)
				return;

			object offset, size, name;
			if (strGridView.SelectedCells.Count == 0)
				offset = size = name = "";
			else {
				var model = (MetaDataModel)Model;
				var cell = strGridView.SelectedCells[0];
				if (cell.RowIndex <= 1 || cell.RowIndex - 2 >= model.MetaData.MetaDataHeader.Streams)
					offset = size = name = "";
				else {
					var hdr = model.MetaData.MetaDataHeader.StreamHeaders[cell.RowIndex - 2];
					uint hdrOffset = (uint)hdr.StartOffset;
					offset = hdrOffset + 0;
					size = hdrOffset + 4;
					name = hdrOffset + 8;
				}
			}

			strGridView[0, 1].Value = offset;
			strGridView[1, 1].Value = size;
			strGridView[2, 1].Value = name;
		}

		Color nextHLColor;

		void AddHighLight(MetaDataModel model, ref uint offset, uint increment) {
			var viewOffset = offset - (uint)model.MetaData.MetaDataHeader.StartOffset;
			offset += increment;

			nextHLColor = (nextHLColor == Color.Red ? Color.Blue : Color.Red);
			hexView.AddHighLight(new HexViewer.HighLight(nextHLColor, viewOffset, viewOffset + increment));
		}

		void AddHighLight(MetaDataModel model, uint offset, uint endOffset) {
			var relBase = (uint)model.MetaData.MetaDataHeader.StartOffset;
			var start = offset - relBase;
			var end = endOffset - relBase;

			nextHLColor = (nextHLColor == Color.Orange ? Color.Green : Color.Orange);
			hexView.AddHighLight(new HexViewer.HighLight(nextHLColor, start, end));
		}

		void PopulateMDHeader(MetaDataModel model) {
			hexView.ClearHighLight();
			nextHLColor = Color.Empty;

			uint offset = (uint)model.MetaData.MetaDataHeader.StartOffset;

			hdrGridView.AddRow("Signature", offset, model.MetaData.MetaDataHeader.Signature);
			AddHighLight(model, ref offset, 4);

			hdrGridView.AddRow("MajorVersion", offset, model.MetaData.MetaDataHeader.MajorVersion);
			AddHighLight(model, ref offset, 2);

			hdrGridView.AddRow("MinorVersion", offset, model.MetaData.MetaDataHeader.MinorVersion);
			AddHighLight(model, ref offset, 2);

			hdrGridView.AddRow("Reserved", offset, model.MetaData.MetaDataHeader.Reserved1);
			AddHighLight(model, ref offset, 4);

			hdrGridView.AddRow("VersionLength", offset, model.MetaData.MetaDataHeader.StringLength);
			AddHighLight(model, ref offset, 4);

			hdrGridView.AddRow("VersionString", offset, model.MetaData.MetaDataHeader.VersionString);
			AddHighLight(model, ref offset, model.MetaData.MetaDataHeader.StringLength);

			hdrGridView.AddRow("Flags", offset, model.MetaData.MetaDataHeader.Flags);
			AddHighLight(model, ref offset, 1);

			hdrGridView.AddRow("Reserved", offset, model.MetaData.MetaDataHeader.Reserved2);
			AddHighLight(model, ref offset, 1);

			hdrGridView.AddRow("NumberOfStreams", offset, model.MetaData.MetaDataHeader.Streams);
			AddHighLight(model, ref offset, 2);
		}

		void PopulateStreamHeader(MetaDataModel model) {
			strGridView.AddRow(new GridView.Cell("", true), new GridView.Cell("", true), new GridView.Cell("", true));

			nextHLColor = Color.Empty;
			foreach (var stream in model.MetaData.MetaDataHeader.StreamHeaders) {
				strGridView.AddRow(stream.Offset, stream.StreamSize, stream.Name);
				var offset = (uint)stream.StartOffset;
				AddHighLight(model, offset, offset + 8);
				AddHighLight(model, offset + 8, (uint)stream.EndOffset);
			}
		}

		protected override void OnModelUpdated() {
			var model = (MetaDataModel)Model;
			hdrGridView.Clear();
			strGridView.Clear();
			if (model != null) {
				PopulateMDHeader(model);
				PopulateStreamHeader(model);
				hexView.Stream = model.MetaData.PEImage.CreateStream(model.MetaData.MetaDataHeader);
			}
			else
				hexView.Stream = null;
		}
	}
}