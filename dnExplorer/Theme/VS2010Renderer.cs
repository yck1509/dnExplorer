using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace dnExplorer.Theme {
	internal class VS2010Renderer : ToolStripProfessionalRenderer {
		public class VS2010ColorTable : ProfessionalColorTable {
			public static readonly VS2010ColorTable Instance = new VS2010ColorTable();

			static Color ARGB(uint color) {
				return Color.FromArgb((int)color);
			}

			public override Color MenuStripGradientBegin {
				get { return ARGB(0xFFCAD3E2); }
			}

			public override Color MenuStripGradientEnd {
				get { return ARGB(0xFFAEB9CD); }
			}

			public override Color ToolStripGradientBegin {
				get { return ARGB(0xFFBCC7D8); }
			}

			public override Color ToolStripGradientMiddle {
				get { return ARGB(0xFFBCC7D8); }
			}

			public override Color ToolStripGradientEnd {
				get { return ARGB(0xFFBCC7D8); }
			}

			public override Color ToolStripBorder {
				get { return ARGB(0xFFC9D2E1); }
			}

			public override Color ToolStripPanelGradientBegin {
				get { return ARGB(0xFF9CAAC1); }
			}

			public override Color ToolStripPanelGradientEnd {
				get { return ARGB(0xFFA6B3CA); }
			}

			public override Color MenuBorder {
				get { return ARGB(0xFF9BA7B7); }
			}

			public override Color ToolStripDropDownBackground {
				get { return ARGB(0xFFE9ECEE); }
			}

			public Color ToolStripDropDownGradientBegin {
				get { return ARGB(0xFFE8EBED); }
			}

			public Color ToolStripDropDownGradientEnd {
				get { return ARGB(0xFFD1D8E3); }
			}

			public override Color MenuItemSelected {
				get { return ARGB(0xFFFFECB5); }
			}

			public override Color MenuItemSelectedGradientBegin {
				get { return ARGB(0xFFFFFBF0); }
			}

			public override Color MenuItemSelectedGradientEnd {
				get { return ARGB(0xFFFFF3CF); }
			}

			public override Color MenuItemBorder {
				get { return ARGB(0xFFE5C365); }
			}

			public override Color MenuItemPressedGradientBegin {
				get { return ARGB(0xFFE9ECEE); }
			}

			public override Color MenuItemPressedGradientMiddle {
				get { return ARGB(0xFFE9ECEE); }
			}

			public override Color MenuItemPressedGradientEnd {
				get { return ARGB(0xFFE9ECEE); }
			}

			public override Color ImageMarginGradientBegin {
				get { return ARGB(0xFFE9ECEE); }
			}

			public override Color ImageMarginGradientMiddle {
				get { return ARGB(0xFFE9ECEE); }
			}

			public override Color ImageMarginGradientEnd {
				get { return ARGB(0xFFE9ECEE); }
			}

			public override Color GripDark {
				get { return Color.Transparent; }
			}

			public override Color GripLight {
				get { return ARGB(0xFF60728C); }
			}

			public override Color ButtonSelectedGradientBegin {
				get { return ARGB(0xFFFFFBF0); }
			}

			public override Color ButtonSelectedGradientMiddle {
				get { return ARGB(0xFFFFF3CF); }
			}

			public override Color ButtonSelectedGradientEnd {
				get { return ARGB(0xFFFFECB5); }
			}

			public override Color ButtonSelectedBorder {
				get { return ARGB(0xFFE5C365); }
			}

			public override Color ButtonPressedGradientBegin {
				get { return ARGB(0xFFFFE8A6); }
			}

			public override Color ButtonPressedGradientMiddle {
				get { return ARGB(0xFFFFE8A6); }
			}

			public override Color ButtonPressedGradientEnd {
				get { return ARGB(0xFFFFE8A6); }
			}

			public override Color ButtonPressedBorder {
				get { return ARGB(0xFFE5C365); }
			}

			public override Color ButtonCheckedGradientBegin {
				get { return ARGB(0xFFFFEFBB); }
			}

			public override Color ButtonCheckedGradientMiddle {
				get { return ARGB(0xFFFFEFBB); }
			}

			public override Color ButtonCheckedGradientEnd {
				get { return ARGB(0xFFFFEFBB); }
			}

			public override Color ButtonCheckedHighlight {
				get { return ARGB(0xFFFFFCF4); }
			}

			public override Color ButtonCheckedHighlightBorder {
				get { return ARGB(0xFFE5C365); }
			}

			public override Color SeparatorDark {
				get { return ARGB(0xFF8591A2); }
			}

			public override Color SeparatorLight {
				get { return ARGB(0xFFBEC3CB); }
			}
		}

		static VS2010Renderer() {
			typeof(ToolStripDropDownMenu).GetField("DefaultImageMarginWidth", BindingFlags.NonPublic | BindingFlags.Static)
				.SetValue(null, 28);
		}

		public VS2010Renderer()
			: base(VS2010ColorTable.Instance) {
			RoundedEdges = false;
		}

		public new VS2010ColorTable ColorTable {
			get { return (VS2010ColorTable)base.ColorTable; }
		}


		static void RenderGradient(Graphics g, Color begin, Color end, Orientation orientation, Rectangle bound) {
			if (bound.Width == 0 || bound.Height == 0)
				return;

			var o = (orientation == Orientation.Horizontal ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal);

			Brush brush;
			if (begin == end)
				brush = new SolidBrush(begin);
			else
				brush = new LinearGradientBrush(bound, begin, end, o);

			using (brush)
				g.FillRectangle(brush, bound);
		}

		static Rectangle GetBounds(Control control) {
			return new Rectangle(Point.Empty, control.Size);
		}

		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
			ToolStrip toolStrip = e.ToolStrip;
			var bounds = GetBounds(toolStrip);

			Color begin, end;
			if (toolStrip is MenuStrip) {
				begin = ColorTable.MenuStripGradientBegin;
				end = ColorTable.MenuStripGradientEnd;
				toolStrip.Height = 22;
				toolStrip.AutoSize = false;
			}
			else if (toolStrip is ToolStripDropDown) {
				begin = ColorTable.ToolStripDropDownGradientBegin;
				end = ColorTable.ToolStripDropDownGradientEnd;
			}
			else {
				begin = ColorTable.ToolStripGradientBegin;
				end = ColorTable.ToolStripGradientEnd;
			}

			RenderGradient(e.Graphics, begin, end, toolStrip.Orientation, bounds);
		}

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
			if (e.ToolStrip is MenuStrip)
				return;

			if (e.ToolStrip is ToolStripDropDown) {
				using (Pen pen = new Pen(ColorTable.MenuBorder)) {
					var bounds = new Rectangle(Point.Empty, e.ToolStrip.Size);
					bounds.Width--;
					bounds.Height--;
					e.Graphics.DrawRectangle(pen, bounds);
				}
				if (((ToolStripDropDown)e.ToolStrip).OwnerItem is ToolStripMenuItem)
					using (Brush brush = new SolidBrush(ColorTable.ToolStripDropDownBackground)) {
						e.Graphics.FillRectangle(brush, e.ConnectedArea);
					}
			}
			else {
				using (Pen pen = new Pen(ColorTable.ToolStripBorder)) {
					var bounds = GetBounds(e.ToolStrip);
					bounds.Width--;
					bounds.Height--;
					e.Graphics.DrawRectangle(pen, bounds);
				}
			}
		}

		protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e) {
			e.Handled = true;
			RenderGradient(e.Graphics, ColorTable.ToolStripPanelGradientBegin, ColorTable.ToolStripPanelGradientEnd,
				e.ToolStripPanel.Orientation, GetBounds(e.ToolStripPanel));
		}

		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e) {
			var item = (ToolStripMenuItem)e.Item;
			Graphics g = e.Graphics;

			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
			if (item.IsOnDropDown) {
				item.AutoSize = false;
				item.Size = new Size(item.GetPreferredSize(Size.Empty).Width, 24);
				item.TextAlign = ContentAlignment.MiddleRight;
			}
			else {
				item.AutoSize = false;
				item.Size = new Size(item.GetPreferredSize(Size.Empty).Width + 4, 22);
				item.Padding = Padding.Empty;
			}

			Rectangle bounds = new Rectangle(Point.Empty, item.Size);
			if (bounds.Size.IsEmpty)
				return;

			Rectangle selectionRect;
			if (item.IsOnDropDown)
				selectionRect = new Rectangle(bounds.X + 3, bounds.Y + 1, bounds.Width - 6, bounds.Height - 3);
			else
				selectionRect = new Rectangle(bounds.X + 1, bounds.Y + 2, bounds.Width - 2, bounds.Height - 5);

			if (item.Pressed && !item.IsOnDropDown) {
				selectionRect.X--;
				selectionRect.Width++;
				selectionRect.Height += 4;
				using (
					var rect = RoundedRectangle.Construct(selectionRect, 4, RoundedCorner.TopLeft | RoundedCorner.TopRight,
						RoundedEdge.Left | RoundedEdge.Top | RoundedEdge.Right)) {
					using (Brush brush = new SolidBrush(ColorTable.MenuItemPressedGradientMiddle))
						e.Graphics.FillPath(brush, rect);
					using (Pen pen = new Pen(ColorTable.MenuBorder))
						e.Graphics.DrawPath(pen, rect);
				}
			}
			else if (item.Selected) {
				using (var rect = RoundedRectangle.Construct(selectionRect, 4, RoundedCorner.All)) {
					using (Brush brush = new SolidBrush(ColorTable.MenuItemSelected))
						e.Graphics.FillPath(brush, rect);
					var grad = new Rectangle(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height / 2 + 1);
					e.Graphics.SetClip(grad);
					using (
						Brush brush = new LinearGradientBrush(grad, ColorTable.MenuItemSelectedGradientBegin,
							ColorTable.MenuItemSelectedGradientEnd, LinearGradientMode.Vertical))
						e.Graphics.FillPath(brush, rect);
					e.Graphics.ResetClip();
					using (Pen pen = new Pen(ColorTable.MenuItemBorder, 1f))
						e.Graphics.DrawPath(pen, rect);
				}
			}
		}

		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) {
			using (Brush brush = new SolidBrush(ColorTable.ImageMarginGradientMiddle)) {
				var bounds = e.AffectedBounds;
				bounds.Width = 27;
				e.Graphics.FillRectangle(brush, e.AffectedBounds);
			}
			base.OnRenderImageMargin(e);
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
			if (!e.Item.Selected)
				e.TextColor = Color.FromArgb(unchecked((int)0xFF1B293E));
			var correctRect = new Rectangle(Point.Empty, e.Item.Size);
			correctRect.X = e.TextRectangle.X;
			correctRect.Width = e.TextRectangle.Width;
			correctRect.Height--;
			e.TextRectangle = correctRect;
			base.OnRenderItemText(e);
		}

		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e) {
			var item = (ToolStripButton)e.Item;
			Rectangle bounds = new Rectangle(Point.Empty, item.Size);

			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			item.Padding = new Padding(2, 0, 2, 0);

			Rectangle selectionRect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

			if (item.Checked) {
				if (item.Selected) {
					using (Brush brush = new SolidBrush(ColorTable.ButtonCheckedHighlight))
						e.Graphics.FillRectangle(brush, selectionRect);
					using (Pen pen = new Pen(ColorTable.ButtonCheckedHighlightBorder))
						e.Graphics.DrawRectangle(pen, selectionRect);
				}
				else {
					using (Brush brush = new SolidBrush(ColorTable.ButtonCheckedGradientMiddle))
						e.Graphics.FillRectangle(brush, selectionRect);
					using (Pen pen = new Pen(ColorTable.ButtonPressedBorder))
						e.Graphics.DrawRectangle(pen, selectionRect);
				}
			}
			else {
				if (item.Pressed) {
					using (Brush brush = new SolidBrush(ColorTable.ButtonPressedGradientMiddle))
						e.Graphics.FillRectangle(brush, selectionRect);
					using (Pen pen = new Pen(ColorTable.ButtonPressedBorder))
						e.Graphics.DrawRectangle(pen, selectionRect);
				}
				else if (item.Selected) {
					using (Brush brush = new SolidBrush(ColorTable.ButtonSelectedGradientEnd))
						e.Graphics.FillRectangle(brush, selectionRect);
					var grad = new Rectangle(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height / 2 + 1);
					e.Graphics.SetClip(grad);
					using (
						Brush brush = new LinearGradientBrush(grad, ColorTable.ButtonSelectedGradientBegin,
							ColorTable.ButtonSelectedGradientMiddle, LinearGradientMode.Vertical))
						e.Graphics.FillRectangle(brush, selectionRect);
					e.Graphics.ResetClip();
					using (Pen pen = new Pen(ColorTable.ButtonSelectedBorder))
						e.Graphics.DrawRectangle(pen, selectionRect);
				}
			}
		}

		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e) {
			var item = (ToolStripDropDownItem)e.Item;
			Rectangle bounds = new Rectangle(Point.Empty, item.Size);

			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			item.Padding = new Padding(2, 0, 2, 0);

			Rectangle selectionRect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

			if (item.Pressed && item.HasDropDownItems) {
				using (Brush brush = new SolidBrush(ColorTable.ButtonPressedGradientMiddle))
					e.Graphics.FillRectangle(brush, selectionRect);
				using (Pen pen = new Pen(ColorTable.ButtonPressedBorder))
					e.Graphics.DrawRectangle(pen, selectionRect);
			}
			else if (item.Selected) {
				using (Brush brush = new SolidBrush(ColorTable.ButtonSelectedGradientEnd))
					e.Graphics.FillRectangle(brush, selectionRect);
				var grad = new Rectangle(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height / 2 + 1);
				e.Graphics.SetClip(grad);
				using (
					Brush brush = new LinearGradientBrush(grad, ColorTable.ButtonSelectedGradientBegin,
						ColorTable.ButtonSelectedGradientMiddle, LinearGradientMode.Vertical))
					e.Graphics.FillRectangle(brush, selectionRect);
				e.Graphics.ResetClip();
				using (Pen pen = new Pen(ColorTable.ButtonSelectedBorder))
					e.Graphics.DrawRectangle(pen, selectionRect);
			}
		}

		protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e) {
			var item = (ToolStripSplitButton)e.Item;
			Rectangle bounds = new Rectangle(Point.Empty, item.Size);

			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			item.Padding = new Padding(2, 0, 2, 0);

			Rectangle selectionRect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

			if (item.Pressed && item.HasDropDownItems) {
				using (Brush brush = new SolidBrush(ColorTable.ButtonPressedGradientMiddle))
					e.Graphics.FillRectangle(brush, selectionRect);
				using (Pen pen = new Pen(ColorTable.ButtonPressedBorder))
					e.Graphics.DrawRectangle(pen, selectionRect);
			}
			else if (item.Selected) {
				using (Brush brush = new SolidBrush(ColorTable.ButtonSelectedGradientEnd))
					e.Graphics.FillRectangle(brush, selectionRect);
				var grad = new Rectangle(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height / 2 + 1);
				e.Graphics.SetClip(grad);
				using (
					Brush brush = new LinearGradientBrush(grad, ColorTable.ButtonSelectedGradientBegin,
						ColorTable.ButtonSelectedGradientMiddle, LinearGradientMode.Vertical))
					e.Graphics.FillRectangle(brush, selectionRect);
				e.Graphics.ResetClip();
				using (Pen pen = new Pen(ColorTable.ButtonSelectedBorder))
					e.Graphics.DrawRectangle(pen, selectionRect);

				Rectangle buttonBounds = item.ButtonBounds;

				Rectangle buttonRect = new Rectangle(buttonBounds.X, buttonBounds.Y, buttonBounds.Width - 1, buttonBounds.Height - 1);

				if (item.ButtonPressed) {
					using (Brush brush = new SolidBrush(ColorTable.ButtonPressedGradientMiddle))
						e.Graphics.FillRectangle(brush, buttonRect);
					using (Pen pen = new Pen(ColorTable.ButtonPressedBorder))
						e.Graphics.DrawRectangle(pen, buttonRect);
				}
				else {
					using (Pen pen = new Pen(ColorTable.ButtonSelectedBorder))
						e.Graphics.DrawRectangle(pen, buttonRect);
				}
			}
			DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics, item, item.DropDownButtonBounds, item.ForeColor,
				ArrowDirection.Down));
		}

		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
			e.Graphics.SmoothingMode = SmoothingMode.Default;
			base.OnRenderArrow(e);
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
			e.Graphics.DrawImageUnscaledAndClipped(Resources.GetResource<Image>("Icons.check.png"), e.ImageRectangle);
		}

		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
			if (e.Vertical) {
				using (Pen pen = new Pen(ColorTable.SeparatorDark))
					e.Graphics.DrawLine(pen, e.Item.Width / 2, 3, e.Item.Width / 2, e.Item.Height - 4);
			}
			else {
				using (Pen pen = new Pen(ColorTable.SeparatorLight)) {
					int l = 0, r = e.Item.Width - 4;
					if (e.Item.GetCurrentParent() is ToolStripDropDown) {
						var p = e.Item.GetCurrentParent().Padding;
						l = p.Left - 5;
						r = e.Item.Width - p.Right - 2;
					}
					e.Graphics.DrawLine(pen, l, e.Item.Height / 2, r, e.Item.Height / 2);
				}
			}
		}
	}
}