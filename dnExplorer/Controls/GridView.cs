using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using dnlib.IO;
using dnlib.PE;

namespace dnExplorer.Controls {
	public class GridView : DataGridView {
		public struct Column {
			public readonly string Header;
			public readonly bool IsLabel;
			public readonly int Width;

			public Column(string header, bool isLabel, int width = 100) {
				Header = header;
				IsLabel = isLabel;
				Width = width;
			}
		}

		public struct Cell {
			public readonly object Value;
			public readonly bool? IsLabel;
			public readonly Color? BackColor;
			public readonly Color? ForeColor;

			public Cell(object value, bool? isLabel = null, Color? back = null, Color? fore = null) {
				Value = value;
				IsLabel = isLabel;
				BackColor = back;
				ForeColor = fore;
			}
		}

		class GridViewTextBox : DataGridViewTextBoxCell {
			protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle,
				TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context) {
				return FormatValue(value);
			}

			protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
				DataGridViewElementStates cellState, object value, object formattedValue, string errorText,
				DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
				DataGridViewPaintParts paintParts) {
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle,
					advancedBorderStyle, paintParts);
				if (DataGridView.SelectedCells.Count > 0 && DataGridView.SelectedCells[0] == this) {
					var bounds = cellBounds;
					bounds.Width--;
					bounds.Height--;
					ControlPaint.DrawBorder(graphics, bounds, SystemColors.ControlText, ButtonBorderStyle.Solid);
				}
			}
		}

		class GridViewHeader : DataGridViewCell {
			public override Type FormattedValueType {
				get { return typeof(string); }
			}

			protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle,
				TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context) {
				return FormatValue(value);
			}

			protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
				DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText,
				DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
				DataGridViewPaintParts paintParts) {
				graphics.FillRectangle(new SolidBrush(SystemColors.Control), cellBounds);
				if (DataGridView.SelectedCells.Count > 0 &&
				    (DataGridView.SelectedCells[0].ColumnIndex == ColumnIndex ||
				     DataGridView.SelectedCells[0].RowIndex == RowIndex)) {
					ControlPaint.DrawBorder(graphics, cellBounds,
						SystemColors.ControlDarkDark, 1, ButtonBorderStyle.Solid,
						SystemColors.ControlDarkDark, 1, ButtonBorderStyle.Solid,
						SystemColors.ButtonFace, 1, ButtonBorderStyle.Solid,
						SystemColors.ButtonFace, 1, ButtonBorderStyle.Solid);
				}
				else {
					ControlPaint.DrawBorder(graphics, cellBounds,
						SystemColors.ButtonHighlight, 1, ButtonBorderStyle.Solid,
						SystemColors.ButtonHighlight, 1, ButtonBorderStyle.Solid,
						SystemColors.ControlDarkDark, 1, ButtonBorderStyle.Solid,
						SystemColors.ControlDarkDark, 1, ButtonBorderStyle.Solid);

					graphics.DrawLine(SystemPens.ButtonFace,
						cellBounds.Left, cellBounds.Bottom,
						cellBounds.Left, cellBounds.Bottom);
				}

				var bounds = cellBounds;
				bounds.Inflate(-2, -2);
				TextRenderer.DrawText(graphics, (string)formattedValue, cellStyle.Font, bounds,
					cellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
			}
		}

		public int RowHeight { get; set; }

		List<Column> cols = new List<Column>();

		public GridView() {
			RowHeight = 25;
			Font = new Font("Segoe UI", 9);
			Dock = DockStyle.Fill;

			AllowUserToAddRows = false;
			AllowUserToDeleteRows = false;
			AllowUserToOrderColumns = false;
			AllowUserToResizeRows = false;
			RowHeadersVisible = false;
			ColumnHeadersVisible = false;
			MultiSelect = false;

			DefaultCellStyle.SelectionBackColor = DefaultCellStyle.BackColor;
			DefaultCellStyle.SelectionForeColor = DefaultCellStyle.ForeColor;
			GridColor = SystemColors.ActiveBorder;

			EditingPanel.BorderStyle = BorderStyle.FixedSingle;
			DoubleBuffered = true;
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
		}

		static string FormatValue(object value) {
			if (value == null)
				return "<NULL>";
			if (value is RVA || value is FileOffset)
				return ((Enum)value).ToString("X");
			if (value is Enum)
				return string.Format("({0:X}) {0}", value);
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

		protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e) {
			if (e.Control is TextBox)
				((TextBox)e.Control).ReadOnly = true;
			base.OnEditingControlShowing(e);
		}

		public void AddColumn(Column col) {
			cols.Add(col);
			var dvCol = new DataGridViewColumn();
			if (col.IsLabel)
				dvCol.CellTemplate = new GridViewHeader();
			else
				dvCol.CellTemplate = new GridViewTextBox();
			dvCol.Width = col.Width;
			Columns.Add(dvCol);
		}

		void AddHeaderRow() {
			var dvRow = new DataGridViewRow { Height = RowHeight };
			for (int i = 0; i < cols.Count; i++) {
				dvRow.Cells.Add(new GridViewHeader { Value = cols[i].Header });
			}
			Rows.Add(dvRow);
		}

		public void AddRow(params object[] row) {
			if (Rows.Count == 0)
				AddHeaderRow();

			var dvRow = new DataGridViewRow { Height = RowHeight };
			ContextMenuStrip ctxMenu = null;
			foreach (var value in row) {
				if (value is ContextMenuStrip)
					ctxMenu = (ContextMenuStrip)value;
			}

			for (int i = 0; i < cols.Count; i++) {
				var value = row[i];

				DataGridViewCell cell;
				if (value is Cell) {
					var rawCell = (Cell)value;

					if (rawCell.IsLabel == null)
						cell = (DataGridViewCell)Columns[i].CellTemplate.Clone();
					else if (rawCell.IsLabel.Value)
						cell = new GridViewHeader();
					else
						cell = new GridViewTextBox();

					cell.Value = rawCell.Value;
					if (rawCell.BackColor != null || rawCell.ForeColor != null) {
						var style = cell.Style;
						if (rawCell.BackColor != null)
							style.BackColor = style.SelectionBackColor = rawCell.BackColor.Value;
						if (rawCell.ForeColor != null)
							style.ForeColor = style.SelectionForeColor = rawCell.ForeColor.Value;
						cell.Style = style;
					}
				}
				else {
					cell = (DataGridViewCell)Columns[i].CellTemplate.Clone();
					cell.Value = value;
				}

				if (ctxMenu != null && cell is GridViewTextBox)
					cell.ContextMenuStrip = ctxMenu;
				dvRow.Cells.Add(cell);
			}
			Rows.Add(dvRow);
		}

		public void Clear() {
			Rows.Clear();
		}

		protected override void OnCellMouseDoubleClick(DataGridViewCellMouseEventArgs e) {
			base.OnCellMouseDoubleClick(e);
			if (this[e.ColumnIndex, e.RowIndex] is GridViewTextBox)
				BeginEdit(true);
		}

		bool isMouseDown;
		bool inMouseDown;

		protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e) {
			if (this[e.ColumnIndex, e.RowIndex] is GridViewTextBox && e.Button == MouseButtons.Left &&
			    EditingControl == null) {
				isMouseDown = true;
				Capture = true;
			}
			inMouseDown = true;
			this[e.ColumnIndex, e.RowIndex].Selected = true;
			base.OnCellMouseDown(e);
			inMouseDown = false;
		}

		protected override void OnCellMouseMove(DataGridViewCellMouseEventArgs e) {
			base.OnCellMouseMove(e);
			if (isMouseDown) {
				var row = e.RowIndex;
				var col = e.ColumnIndex;
				if (SelectedCells.Count > 0 && this[col, row] == SelectedCells[0])
					return;

				if (this[col, row] is GridViewTextBox)
					this[col, row].Selected = true;
				else if (this[SelectedCells[0].ColumnIndex, row] is GridViewTextBox)
					this[SelectedCells[0].ColumnIndex, row].Selected = true;
				else if (this[col, SelectedCells[0].RowIndex] is GridViewTextBox)
					this[col, SelectedCells[0].RowIndex].Selected = true;
			}
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left) {
				isMouseDown = false;
				Capture = false;
			}
		}

		protected override void OnScroll(ScrollEventArgs e) {
			base.OnScroll(e);
			Invalidate();
		}

		DataGridViewCell prevSelectedCell;
		DataGridViewCell prevCurrentCell;

		DataGridViewCell SearchCorrectCell(DataGridViewCell newCell, DataGridViewCell prevCell) {
			if (newCell.RowIndex != 0) {
				// Header row
				int row = newCell.RowIndex, col = newCell.ColumnIndex;

				int direction = 1;
				if (prevCell != null &&
				    (row < prevCell.RowIndex ||
				     (row == prevCell.RowIndex && col < prevCell.ColumnIndex)))
					direction = -1;

				while (row > 0 && row < Rows.Count) {
					if (this[col, row] is GridViewHeader) {
						col += direction;
					}
					else
						return this[col, row];

					if (col < 0) {
						col = cols.Count - 1;
						row--;
					}
					else if (col >= cols.Count) {
						col = 0;
						row++;
					}
				}
			}
			if (prevCell != null && prevCell.DataGridView == this)
				return prevCell;
			return null;
		}

		protected override void SetSelectedCellCore(int columnIndex, int rowIndex, bool selected) {
			if (selected) {
				var newCell = this[columnIndex, rowIndex];
				if (newCell is GridViewHeader) {
					if (inMouseDown) {
						if (SelectedCells.Count > 0)
							base.SetSelectedCellCore(SelectedCells[0].ColumnIndex, SelectedCells[0].RowIndex, false);
						return;
					}
					var correctCell = SearchCorrectCell(newCell, prevSelectedCell);
					if (correctCell != null) {
						newCell = correctCell;
						columnIndex = correctCell.ColumnIndex;
						rowIndex = correctCell.RowIndex;
					}
					else
						return;
				}
				prevSelectedCell = newCell;
			}

			base.SetSelectedCellCore(columnIndex, rowIndex, selected);
		}

		protected override bool SetCurrentCellAddressCore(int columnIndex, int rowIndex, bool setAnchorCellAddress,
			bool validateCurrentCell, bool throughMouseClick) {
			if (columnIndex >= 0 && rowIndex >= 0) {
				var newCell = this[columnIndex, rowIndex];
				if (newCell is GridViewHeader) {
					if (throughMouseClick) {
						return base.SetCurrentCellAddressCore(-1, -1, setAnchorCellAddress, validateCurrentCell, throughMouseClick);
					}

					var correctCell = SearchCorrectCell(newCell, prevCurrentCell);
					if (correctCell != null) {
						newCell = correctCell;
						columnIndex = correctCell.ColumnIndex;
						rowIndex = correctCell.RowIndex;
					}
					else
						return false;
				}
			}

			var value = base.SetCurrentCellAddressCore(columnIndex, rowIndex, setAnchorCellAddress, validateCurrentCell,
				throughMouseClick);
			if (value && columnIndex >= 0 && rowIndex >= 0)
				prevCurrentCell = this[columnIndex, rowIndex];
			return value;
		}

		protected override void OnSelectionChanged(EventArgs e) {
			base.OnSelectionChanged(e);
			Invalidate();
		}
	}
}