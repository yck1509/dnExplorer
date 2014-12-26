using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using dnlib.IO;
using dnlib.PE;

namespace dnExplorer.Controls {
	public class GridView : Panel {
		public struct Column {
			public readonly string Header;
			public readonly bool IsLabel;
			public readonly int Width;

			public Column(string header, bool isLabel, int width = 150) {
				Header = header;
				IsLabel = isLabel;
				Width = width;
			}
		}

		public int RowHeight { get; set; }

		List<Column> cols = new List<Column>();
		List<object[]> rows = new List<object[]>();

		public GridView() {
			RowHeight = 25;
			BackColor = SystemColors.AppWorkspace;
			AutoScroll = true;
			Font = new Font("Segoe UI", 9);
			Dock = DockStyle.Fill;
		}

		public void AddColumn(Column col) {
			cols.Add(col);
		}

		public void AddRow(params object[] row) {
			rows.Add(row);
		}

		public void Clear() {
			rows.Clear();
		}

		class CellHeader : Label {
			public CellHeader(string text, Column col, int rowHeight) {
				BackColor = SystemColors.Control;
				Text = text;
				AutoSize = false;
				Size = new Size(col.Width, rowHeight);
			}

			protected override void OnPaint(PaintEventArgs e) {
				ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
					SystemColors.ButtonHighlight, 1, ButtonBorderStyle.Solid,
					SystemColors.ButtonHighlight, 1, ButtonBorderStyle.Solid,
					SystemColors.ControlDarkDark, 1, ButtonBorderStyle.Solid,
					SystemColors.ControlDarkDark, 1, ButtonBorderStyle.Solid);
				e.Graphics.DrawLine(SystemPens.ButtonFace, 0, ClientRectangle.Bottom, 0, ClientRectangle.Bottom);

				var bounds = ClientRectangle;
				bounds.Inflate(-2, -2);
				TextRenderer.DrawText(e.Graphics, Text, Font, bounds,
					ForeColor, BackColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
			}
		}

		class CellText : Label {
			TextBox txtBox;

			public CellText(string text, Column col, int rowHeight) {
				BackColor = SystemColors.ControlLight;
				Text = text;
				AutoSize = false;
				Size = new Size(col.Width, rowHeight);

				txtBox = new TextBox {
					BorderStyle = BorderStyle.None,
					ReadOnly = true,
					Cursor = Cursors.Arrow,
					BackColor = SystemColors.ControlLight
				};
				txtBox.Text = text;
				Controls.Add(txtBox);
				var height = txtBox.GetPreferredSize(Size.Empty).Height;
				txtBox.SetBounds(4, (rowHeight - height) / 2, col.Width - 8, height);
			}

			protected override void OnPaint(PaintEventArgs e) {
				ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
					SystemColors.Window, 1, ButtonBorderStyle.None,
					SystemColors.Window, 1, ButtonBorderStyle.None,
					SystemColors.ActiveBorder, 1, ButtonBorderStyle.Solid,
					SystemColors.ActiveBorder, 1, ButtonBorderStyle.Solid);
			}

			protected override void OnMouseDown(MouseEventArgs e) {
				base.OnMouseDown(e);
				txtBox.Focus();
			}
		}

		static string ToString(object value) {
			if (value == null)
				return "<NULL>";
			if (value is RVA || value is FileOffset)
				return ((Enum)value).ToString("X");
			if (value is Enum)
				return ((Enum)value).ToString();
			if (value is ulong)
				return ((ulong)value).ToString("X16");
			if (value is uint)
				return ((uint)value).ToString("X8");
			if (value is ushort)
				return ((ushort)value).ToString("X4");
			if (value is byte)
				return ((byte)value).ToString("X2");
			return value.ToString();
		}

		public void LayoutGrid() {
			const int PAD_X = 1;
			const int PAD_Y = 1;

			int currentX = PAD_X, currentY = PAD_Y;

			var ctrls = new List<Control>();
			for (int x = 0; x < cols.Count; x++) {
				var col = cols[x];
				ctrls.Add(new CellHeader(col.Header, col, RowHeight) {
					Left = currentX,
					Top = currentY
				});
				currentX += col.Width;
			}

			currentY += RowHeight;
			for (int y = 0; y < rows.Count; y++) {
				var row = rows[y];
				currentX = PAD_X;
				for (int x = 0; x < cols.Count; x++) {
					var col = cols[x];

					Control cell;
					if (col.IsLabel)
						cell = new CellHeader(ToString(row[x]), col, RowHeight);
					else
						cell = new CellText(ToString(row[x]), col, RowHeight);

					cell.Left = currentX;
					cell.Top = currentY;
					ctrls.Add(cell);

					currentX += col.Width;
				}
				currentY += RowHeight;
			}

			SuspendLayout();
			Controls.Clear();
			Controls.AddRange(ctrls.ToArray());
			ResumeLayout();
		}
	}
}