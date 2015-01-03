using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace dnExplorer.Theme {
	internal class VS2010AutoHideStrip : AutoHideStripBase {
		class VS2010Tab : Tab {
			internal VS2010Tab(IDockContent content)
				: base(content) {
				TabWidth = 0;
				TabX = 0;
			}

			public int TabX { get; set; }

			public int TabWidth { get; set; }

			public bool IsMouseOver { get; set; }
		}

		const int _ImageHeight = 16;
		const int _ImageWidth = 16;
		const int _ImageGapTop = 2;
		const int _ImageGapLeft = 4;
		const int _ImageGapRight = 2;
		const int _ImageGapBottom = 2;
		const int _TextGapLeft = 2;
		const int _TextGapRight = 2;
		const int _TabGapTop = 4;
		const int _TabGapBottom = 2;
		const int _TabGapLeft = 4;
		const int _TabGapBetween = 10;

		#region Customizable Properties

		public Font TextFont {
			get { return DockPanel.Font; }
		}

		static StringFormat _stringFormatTabHorizontal;

		StringFormat StringFormatTabHorizontal {
			get {
				if (_stringFormatTabHorizontal == null) {
					_stringFormatTabHorizontal = new StringFormat();
					_stringFormatTabHorizontal.Alignment = StringAlignment.Near;
					_stringFormatTabHorizontal.LineAlignment = StringAlignment.Center;
					_stringFormatTabHorizontal.FormatFlags = StringFormatFlags.NoWrap;
					_stringFormatTabHorizontal.Trimming = StringTrimming.None;
				}

				if (RightToLeft == RightToLeft.Yes)
					_stringFormatTabHorizontal.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				else
					_stringFormatTabHorizontal.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;

				return _stringFormatTabHorizontal;
			}
		}

		static StringFormat _stringFormatTabVertical;

		StringFormat StringFormatTabVertical {
			get {
				if (_stringFormatTabVertical == null) {
					_stringFormatTabVertical = new StringFormat();
					_stringFormatTabVertical.Alignment = StringAlignment.Near;
					_stringFormatTabVertical.LineAlignment = StringAlignment.Center;
					_stringFormatTabVertical.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.DirectionVertical;
					_stringFormatTabVertical.Trimming = StringTrimming.None;
				}
				if (RightToLeft == RightToLeft.Yes)
					_stringFormatTabVertical.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				else
					_stringFormatTabVertical.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;

				return _stringFormatTabVertical;
			}
		}

		static int ImageHeight {
			get { return _ImageHeight; }
		}

		static int ImageWidth {
			get { return _ImageWidth; }
		}

		static int ImageGapTop {
			get { return _ImageGapTop; }
		}

		static int ImageGapLeft {
			get { return _ImageGapLeft; }
		}

		static int ImageGapRight {
			get { return _ImageGapRight; }
		}

		static int ImageGapBottom {
			get { return _ImageGapBottom; }
		}

		static int TextGapLeft {
			get { return _TextGapLeft; }
		}

		static int TextGapRight {
			get { return _TextGapRight; }
		}

		static int TabGapTop {
			get { return _TabGapTop; }
		}

		static int TabGapBottom {
			get { return _TabGapBottom; }
		}

		static int TabGapLeft {
			get { return _TabGapLeft; }
		}

		static int TabGapBetween {
			get { return _TabGapBetween; }
		}

		static ColorBlend inactiveColorBlend;

		static ColorBlend InactiveColorBlend {
			get {
				if (inactiveColorBlend == null) {
					ColorBlend blend = new ColorBlend(2);

					blend.Positions = new[] { 0.0F, 1.0F };
					blend.Colors = new[] {
						VS2010Theme.ARGB(0xFF4D6082),
						VS2010Theme.ARGB(0xFF3D5277)
					};
					inactiveColorBlend = blend;
				}

				return inactiveColorBlend;
			}
		}

		static ColorBlend hoverColorBlend;

		static ColorBlend HoverColorBlend {
			get {
				if (hoverColorBlend == null) {
					ColorBlend blend = new ColorBlend(2);

					blend.Positions = new[] { 0.0F, 1.0F };
					blend.Colors = new[] {
						VS2010Theme.ARGB(0xFF707776),
						VS2010Theme.ARGB(0xFF4B5C74)
					};
					hoverColorBlend = blend;
				}

				return hoverColorBlend;
			}
		}

		#endregion

		static Matrix _matrixIdentity = new Matrix();

		static Matrix MatrixIdentity {
			get { return _matrixIdentity; }
		}

		static DockState[] _dockStates;

		static DockState[] DockStates {
			get {
				if (_dockStates == null) {
					_dockStates = new DockState[4];
					_dockStates[0] = DockState.DockLeftAutoHide;
					_dockStates[1] = DockState.DockRightAutoHide;
					_dockStates[2] = DockState.DockTopAutoHide;
					_dockStates[3] = DockState.DockBottomAutoHide;
				}
				return _dockStates;
			}
		}

		public VS2010AutoHideStrip(DockPanel panel)
			: base(panel) {
			SetStyle(ControlStyles.ResizeRedraw |
			         ControlStyles.UserPaint |
			         ControlStyles.AllPaintingInWmPaint |
			         ControlStyles.OptimizedDoubleBuffer |
			         ControlStyles.SupportsTransparentBackColor, true);
		}

		protected override void OnPaint(PaintEventArgs e) {
			Graphics g = e.Graphics;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.None;
			g.FillRectangle(Brushes.Transparent, ClientRectangle);
			DrawTabStrip(g);
		}

		protected override void OnLayout(LayoutEventArgs levent) {
			CalculateTabs();
			base.OnLayout(levent);
		}

		void DrawTabStrip(Graphics g) {
			DrawTabStrip(g, DockState.DockTopAutoHide);
			DrawTabStrip(g, DockState.DockBottomAutoHide);
			DrawTabStrip(g, DockState.DockLeftAutoHide);
			DrawTabStrip(g, DockState.DockRightAutoHide);
		}

		void DrawTabStrip(Graphics g, DockState dockState) {
			Rectangle rectTabStrip = GetLogicalTabStripRectangle(dockState);

			if (rectTabStrip.IsEmpty)
				return;

			Matrix matrixIdentity = g.Transform;
			if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide) {
				Matrix matrixRotated = new Matrix();
				matrixRotated.RotateAt(90, new PointF(rectTabStrip.X + (float)rectTabStrip.Height / 2,
					rectTabStrip.Y + (float)rectTabStrip.Height / 2));
				g.Transform = matrixRotated;
			}

			foreach (Pane pane in GetPanes(dockState)) {
				foreach (VS2010Tab tab in pane.AutoHideTabs)
					DrawTab(g, tab);
			}
			g.Transform = matrixIdentity;
		}

		void CalculateTabs() {
			CalculateTabs(DockState.DockTopAutoHide);
			CalculateTabs(DockState.DockBottomAutoHide);
			CalculateTabs(DockState.DockLeftAutoHide);
			CalculateTabs(DockState.DockRightAutoHide);
		}

		void CalculateTabs(DockState dockState) {
			Rectangle rectTabStrip = GetLogicalTabStripRectangle(dockState);

			int imageHeight = rectTabStrip.Height - ImageGapTop - ImageGapBottom;
			int imageWidth = ImageWidth;
			if (imageHeight > ImageHeight)
				imageWidth = ImageWidth * (imageHeight / ImageHeight);

			int x = TabGapLeft + rectTabStrip.X;
			foreach (Pane pane in GetPanes(dockState)) {
				foreach (VS2010Tab tab in pane.AutoHideTabs) {
					int width = imageWidth + ImageGapLeft + ImageGapRight +
					            TextRenderer.MeasureText(tab.Content.DockHandler.TabText, TextFont).Width +
					            TextGapLeft + TextGapRight;
					tab.TabX = x;
					tab.TabWidth = width;
					x += width;
				}

				x += TabGapBetween;
			}
		}

		Rectangle RtlTransform(Rectangle rect, DockState dockState) {
			Rectangle rectTransformed;
			if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
				rectTransformed = rect;
			else
				rectTransformed = DrawHelper.RtlTransform(this, rect);

			return rectTransformed;
		}

		GraphicsPath GetTabOutline(VS2010Tab tab, bool transformed, bool rtlTransform) {
			DockState dockState = tab.Content.DockHandler.DockState;
			Rectangle rectTab = GetTabRectangle(tab, transformed);
			if (rtlTransform)
				rectTab = RtlTransform(rectTab, dockState);

			var path = new GraphicsPath();
			path.AddRectangle(rectTab);

			return path;
		}

		void DrawTab(Graphics g, VS2010Tab tab) {
			Rectangle rectTabOrigin = GetTabRectangle(tab);
			if (rectTabOrigin.IsEmpty)
				return;

			rectTabOrigin.X++;
			rectTabOrigin.Width -= 2;

			DockState dockState = tab.Content.DockHandler.DockState;
			IDockContent content = tab.Content;

			var corner = RoundedCorner.TopLeft | RoundedCorner.TopRight;
			if (dockState == DockState.DockRightAutoHide || dockState == DockState.DockTopAutoHide)
				corner = RoundedCorner.BottomLeft | RoundedCorner.BottomRight;

			using (var brush = new LinearGradientBrush(rectTabOrigin, Color.Empty, Color.Empty, LinearGradientMode.Vertical))
			using (var roundedRect = RoundedRectangle.Construct(rectTabOrigin, 6, corner)) {
				if (tab.IsMouseOver) {
					brush.InterpolationColors = HoverColorBlend;
					g.FillPath(brush, roundedRect);
					using (var pen = new Pen(new SolidBrush(VS2010Theme.ARGB(0xFF9BA7B7))))
						g.DrawPath(pen, roundedRect);
				}
				else {
					brush.InterpolationColors = InactiveColorBlend;
					g.FillPath(brush, roundedRect);
				}
			}

			//Set no rotate for drawing icon and text
			Matrix matrixRotate = g.Transform;
			g.Transform = MatrixIdentity;

			// Draw the icon
			Rectangle rectImage = rectTabOrigin;
			rectImage.X += ImageGapLeft;
			rectImage.Y += ImageGapTop;
			int imageHeight = rectTabOrigin.Height - ImageGapTop - ImageGapBottom;
			int imageWidth = ImageWidth;
			if (imageHeight > ImageHeight)
				imageWidth = ImageWidth * (imageHeight / ImageHeight);
			rectImage.Height = imageHeight;
			rectImage.Width = imageWidth;
			rectImage = GetTransformedRectangle(dockState, rectImage);

			if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide) {
				// The DockState is DockLeftAutoHide or DockRightAutoHide, so rotate the image 90 degrees to the right. 
				Rectangle rectTransform = RtlTransform(rectImage, dockState);
				Point[] rotationPoints = {
					new Point(rectTransform.X + rectTransform.Width, rectTransform.Y),
					new Point(rectTransform.X + rectTransform.Width, rectTransform.Y + rectTransform.Height),
					new Point(rectTransform.X, rectTransform.Y)
				};

				using (Icon rotatedIcon = new Icon(((Form)content).Icon, 16, 16)) {
					g.DrawImage(rotatedIcon.ToBitmap(), rotationPoints);
				}
			}
			else {
				// Draw the icon normally without any rotation.

				using (var icon = new Icon(((Form)content).Icon, rectImage.Size))
					g.DrawIcon(icon, RtlTransform(rectImage, dockState));
			}

			// Draw the text
			Rectangle rectText = rectTabOrigin;
			rectText.X += ImageGapLeft + imageWidth + ImageGapRight + TextGapLeft;
			rectText.Width -= ImageGapLeft + imageWidth + ImageGapRight + TextGapLeft;
			rectText = RtlTransform(GetTransformedRectangle(dockState, rectText), dockState);

			Color textColor = DockPanel.Skin.AutoHideStripSkin.TabGradient.TextColor;

			if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
				g.DrawString(content.DockHandler.TabText, TextFont, new SolidBrush(textColor), rectText, StringFormatTabVertical);
			else
				g.DrawString(content.DockHandler.TabText, TextFont, new SolidBrush(textColor), rectText, StringFormatTabHorizontal);

			// Set rotate back
			g.Transform = matrixRotate;
		}

		Rectangle GetLogicalTabStripRectangle(DockState dockState) {
			return GetLogicalTabStripRectangle(dockState, false);
		}

		Rectangle GetLogicalTabStripRectangle(DockState dockState, bool transformed) {
			if (!DockHelper.IsDockStateAutoHide(dockState))
				return Rectangle.Empty;

			int leftPanes = GetPanes(DockState.DockLeftAutoHide).Count;
			int rightPanes = GetPanes(DockState.DockRightAutoHide).Count;
			int topPanes = GetPanes(DockState.DockTopAutoHide).Count;
			int bottomPanes = GetPanes(DockState.DockBottomAutoHide).Count;

			int x, y, width, height;

			height = MeasureHeight();
			if (dockState == DockState.DockLeftAutoHide && leftPanes > 0) {
				x = 0;
				y = (topPanes == 0) ? 0 : height;
				width = Height - (topPanes == 0 ? 0 : height) - (bottomPanes == 0 ? 0 : height);
			}
			else if (dockState == DockState.DockRightAutoHide && rightPanes > 0) {
				x = Width - height;
				if (leftPanes != 0 && x < height)
					x = height;
				y = (topPanes == 0) ? 0 : height;
				width = Height - (topPanes == 0 ? 0 : height) - (bottomPanes == 0 ? 0 : height);
			}
			else if (dockState == DockState.DockTopAutoHide && topPanes > 0) {
				x = leftPanes == 0 ? 0 : height;
				y = 0;
				width = Width - (leftPanes == 0 ? 0 : height) - (rightPanes == 0 ? 0 : height);
			}
			else if (dockState == DockState.DockBottomAutoHide && bottomPanes > 0) {
				x = leftPanes == 0 ? 0 : height;
				y = Height - height;
				if (topPanes != 0 && y < height)
					y = height;
				width = Width - (leftPanes == 0 ? 0 : height) - (rightPanes == 0 ? 0 : height);
			}
			else
				return Rectangle.Empty;

			if (width == 0 || height == 0) {
				return Rectangle.Empty;
			}

			var rect = new Rectangle(x, y, width, height);
			return transformed ? GetTransformedRectangle(dockState, rect) : rect;
		}

		Rectangle GetTabRectangle(VS2010Tab tab) {
			return GetTabRectangle(tab, false);
		}

		Rectangle GetTabRectangle(VS2010Tab tab, bool transformed) {
			DockState dockState = tab.Content.DockHandler.DockState;
			Rectangle rectTabStrip = GetLogicalTabStripRectangle(dockState);

			if (rectTabStrip.IsEmpty)
				return Rectangle.Empty;

			int x = tab.TabX;
			int y = rectTabStrip.Y +
			        (dockState == DockState.DockTopAutoHide || dockState == DockState.DockRightAutoHide
				        ? 0
				        : TabGapTop);
			int width = tab.TabWidth;
			int height = rectTabStrip.Height - TabGapTop;

			if (!transformed)
				return new Rectangle(x, y, width, height);
			return GetTransformedRectangle(dockState, new Rectangle(x, y, width, height));
		}

		Rectangle GetTransformedRectangle(DockState dockState, Rectangle rect) {
			if (dockState != DockState.DockLeftAutoHide && dockState != DockState.DockRightAutoHide)
				return rect;

			PointF[] pts = new PointF[1];
			// the center of the rectangle
			pts[0].X = rect.X + (float)rect.Width / 2;
			pts[0].Y = rect.Y + (float)rect.Height / 2;
			Rectangle rectTabStrip = GetLogicalTabStripRectangle(dockState);
			using (var matrix = new Matrix()) {
				matrix.RotateAt(90, new PointF(rectTabStrip.X + (float)rectTabStrip.Height / 2,
					rectTabStrip.Y + (float)rectTabStrip.Height / 2));
				matrix.TransformPoints(pts);
			}

			return new Rectangle((int)(pts[0].X - (float)rect.Height / 2 + .5F),
				(int)(pts[0].Y - (float)rect.Width / 2 + .5F),
				rect.Height, rect.Width);
		}

		protected override IDockContent HitTest(Point ptMouse) {
			Tab tab = TabHitTest(ptMouse);

			if (tab != null)
				return tab.Content;
			return null;
		}

		protected Tab TabHitTest(Point ptMouse) {
			foreach (DockState state in DockStates) {
				Rectangle rectTabStrip = GetLogicalTabStripRectangle(state, true);
				if (!rectTabStrip.Contains(ptMouse))
					continue;

				foreach (Pane pane in GetPanes(state)) {
					foreach (VS2010Tab tab in pane.AutoHideTabs) {
						GraphicsPath path = GetTabOutline(tab, true, true);
						if (path.IsVisible(ptMouse))
							return tab;
					}
				}
			}

			return null;
		}

		VS2010Tab lastSelectedTab;

		protected override void OnMouseMove(MouseEventArgs e) {
			var tab = (VS2010Tab)TabHitTest(PointToClient(MousePosition));
			if (tab != null) {
				tab.IsMouseOver = true;
				Invalidate();
			}

			if (lastSelectedTab != tab) {
				if (lastSelectedTab != null)
					lastSelectedTab.IsMouseOver = false;
				lastSelectedTab = tab;
			}

			base.OnMouseMove(e);
		}

		protected override void OnMouseLeave(EventArgs e) {
			base.OnMouseLeave(e);

			if (lastSelectedTab != null)
				lastSelectedTab.IsMouseOver = false;
			Invalidate();
		}

		protected override int MeasureHeight() {
			return Math.Max(ImageGapBottom +
			                ImageGapTop + ImageHeight,
				TextFont.Height) + TabGapTop + TabGapBottom;
		}

		protected override void OnRefreshChanges() {
			CalculateTabs();
			Invalidate();
		}

		protected override Tab CreateTab(IDockContent content) {
			return new VS2010Tab(content);
		}
	}
}