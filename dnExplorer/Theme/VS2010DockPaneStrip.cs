using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Res = WeifenLuo.WinFormsUI.Docking.Resources;

namespace dnExplorer.Theme {
	internal class VS2010DockPaneStrip : DockPaneStripBase {
		class VS2010Tab : Tab {
			public VS2010Tab(IDockContent content)
				: base(content) {
			}

			public int TabX { get; set; }

			public int TabWidth { get; set; }

			public int MaxWidth { get; set; }

			protected internal bool Flag { get; set; }
		}

		protected override Tab CreateTab(IDockContent content) {
			return new VS2010Tab(content);
		}

		sealed class InertButton : InertButtonBase {
			Bitmap m_image0, m_image1;

			public InertButton(Bitmap image0, Bitmap image1) {
				m_image0 = image0;
				m_image1 = image1;
			}

			int m_imageCategory;

			public int ImageCategory {
				get { return m_imageCategory; }
				set {
					if (m_imageCategory == value)
						return;

					m_imageCategory = value;
					Invalidate();
				}
			}

			protected override Color HoverBorderColor {
				get { return VS2010DockPaneStrip.HoverBorderColor; }
			}

			protected override Color HoverBackColor {
				get { return VS2010DockPaneStrip.HoverBackColor; }
			}

			protected override Color PressedBackColor {
				get { return VS2010DockPaneStrip.PressedBackColor; }
			}

			protected override Color HoverForeColor {
				get { return VS2010DockPaneStrip.HoverForeColor; }
			}

			public override Bitmap Image {
				get { return ImageCategory == 0 ? m_image0 : m_image1; }
			}
		}

		#region Constants

		const int _ToolWindowStripGapTop = 0;
		const int _ToolWindowStripGapBottom = 1;
		const int _ToolWindowStripGapLeft = 0;
		const int _ToolWindowStripGapRight = 0;
		const int _ToolWindowImageHeight = 16;
		const int _ToolWindowImageWidth = 16;
		const int _ToolWindowImageGapTop = 3;
		const int _ToolWindowImageGapBottom = 1;
		const int _ToolWindowImageGapLeft = 2;
		const int _ToolWindowImageGapRight = 0;
		const int _ToolWindowTextGapRight = 3;
		const int _ToolWindowTabSeperatorGapTop = 3;
		const int _ToolWindowTabSeperatorGapBottom = 3;

		const int _DocumentStripGapTop = 0;
		const int _DocumentStripGapBottom = 4;
		const int _DocumentTabMaxWidth = 200;
		const int _DocumentButtonGapTop = 2;
		const int _DocumentButtonGapBottom = 2;
		const int _DocumentButtonGapBetween = 0;
		const int _DocumentButtonGapRight = 3;
		const int _DocumentTabGapTop = 0;
		const int _DocumentTabGapLeft = 0;
		const int _DocumentTabGapRight = 0;
		const int _DocumentIconGapBottom = 4;
		const int _DocumentIconGapLeft = 4;
		const int _DocumentIconGapRight = 2;
		const int _DocumentIconHeight = 16;
		const int _DocumentIconWidth = 16;
		const int _DocumentTextGapRight = 6;

		#endregion

		#region Members

		static Bitmap m_imageButtonClose;
		InertButton m_buttonClose;
		static Bitmap m_imageButtonWindowList;
		static Bitmap m_imageButtonWindowListOverflow;
		InertButton m_buttonWindowList;
		ToolTip m_toolTip;
		Font m_boldFont;
		int m_startDisplayingTab;
		bool m_documentTabsOverflow;
		static string m_toolTipSelect;
		static string m_toolTipClose;
		bool m_closeButtonVisible;

		#endregion

		#region Properties

		Rectangle TabStripRectangle {
			get {
				if (Appearance == DockPane.AppearanceStyle.Document)
					return TabStripRectangle_Document;
				return TabStripRectangle_ToolWindow;
			}
		}

		Rectangle TabStripRectangle_ToolWindow {
			get {
				Rectangle rect = ClientRectangle;
				return new Rectangle(rect.X, rect.Top + ToolWindowStripGapTop, rect.Width,
					rect.Height - ToolWindowStripGapTop - ToolWindowStripGapBottom);
			}
		}

		Rectangle TabStripRectangle_Document {
			get {
				Rectangle rect = ClientRectangle;
				return new Rectangle(rect.X, rect.Top + DocumentStripGapTop, rect.Width,
					rect.Height + DocumentStripGapTop - ToolWindowStripGapBottom);
			}
		}

		Rectangle TabsRectangle {
			get {
				if (Appearance == DockPane.AppearanceStyle.ToolWindow)
					return TabStripRectangle;

				Rectangle rectWindow = TabStripRectangle;
				int x = rectWindow.X;
				int y = rectWindow.Y;
				int width = rectWindow.Width;
				int height = rectWindow.Height;

				x += DocumentTabGapLeft;
				width -= DocumentTabGapLeft +
				         DocumentTabGapRight +
				         DocumentButtonGapRight +
				         ButtonClose.Width +
				         ButtonWindowList.Width +
				         2 * DocumentButtonGapBetween;

				return new Rectangle(x, y, width, height);
			}
		}

		ContextMenuStrip SelectMenu { get; set; }

		static Bitmap ImageButtonClose {
			get {
				if (m_imageButtonClose == null)
					m_imageButtonClose = Res.DockPane_Close;

				return m_imageButtonClose;
			}
		}

		InertButton ButtonClose {
			get {
				if (m_buttonClose == null) {
					m_buttonClose = new InertButton(ImageButtonClose, ImageButtonClose);
					m_toolTip.SetToolTip(m_buttonClose, ToolTipClose);
					m_buttonClose.Click += Close_Click;
					Controls.Add(m_buttonClose);
				}

				return m_buttonClose;
			}
		}

		static Bitmap ImageButtonWindowList {
			get {
				if (m_imageButtonWindowList == null)
					m_imageButtonWindowList = Res.DockPane_Option;

				return m_imageButtonWindowList;
			}
		}

		static Bitmap ImageButtonWindowListOverflow {
			get {
				if (m_imageButtonWindowListOverflow == null)
					m_imageButtonWindowListOverflow = Res.DockPane_OptionOverflow;

				return m_imageButtonWindowListOverflow;
			}
		}

		InertButton ButtonWindowList {
			get {
				if (m_buttonWindowList == null) {
					m_buttonWindowList = new InertButton(ImageButtonWindowList, ImageButtonWindowListOverflow);
					m_toolTip.SetToolTip(m_buttonWindowList, ToolTipSelect);
					m_buttonWindowList.Click += WindowList_Click;
					Controls.Add(m_buttonWindowList);
				}

				return m_buttonWindowList;
			}
		}

		static GraphicsPath GraphicsPath {
			get { return new GraphicsPath(); }
		}

		IContainer Components { get; set; }

		public Font TextFont {
			get { return DockPane.DockPanel.Font; }
		}

		int StartDisplayingTab {
			get { return m_startDisplayingTab; }
			set {
				m_startDisplayingTab = value;
				Invalidate();
			}
		}

		int EndDisplayingTab { get; set; }

		int FirstDisplayingTab { get; set; }

		bool DocumentTabsOverflow {
			set {
				if (m_documentTabsOverflow == value)
					return;

				m_documentTabsOverflow = value;
				if (value)
					ButtonWindowList.ImageCategory = 1;
				else
					ButtonWindowList.ImageCategory = 0;
			}
		}

		#region Customizable Properties

		static int ToolWindowStripGapTop {
			get { return _ToolWindowStripGapTop; }
		}

		static int ToolWindowStripGapBottom {
			get { return _ToolWindowStripGapBottom; }
		}

		static int ToolWindowStripGapLeft {
			get { return _ToolWindowStripGapLeft; }
		}

		static int ToolWindowStripGapRight {
			get { return _ToolWindowStripGapRight; }
		}

		static int ToolWindowImageHeight {
			get { return _ToolWindowImageHeight; }
		}

		static int ToolWindowImageWidth {
			get { return _ToolWindowImageWidth; }
		}

		static int ToolWindowImageGapTop {
			get { return _ToolWindowImageGapTop; }
		}

		static int ToolWindowImageGapBottom {
			get { return _ToolWindowImageGapBottom; }
		}

		static int ToolWindowImageGapLeft {
			get { return _ToolWindowImageGapLeft; }
		}

		static int ToolWindowImageGapRight {
			get { return _ToolWindowImageGapRight; }
		}

		static int ToolWindowTextGapRight {
			get { return _ToolWindowTextGapRight; }
		}

		static int ToolWindowTabSeperatorGapTop {
			get { return _ToolWindowTabSeperatorGapTop; }
		}

		static int ToolWindowTabSeperatorGapBottom {
			get { return _ToolWindowTabSeperatorGapBottom; }
		}

		static string ToolTipClose {
			get {
				if (m_toolTipClose == null)
					m_toolTipClose = Strings.DockPaneStrip_ToolTipClose;
				return m_toolTipClose;
			}
		}

		static string ToolTipSelect {
			get {
				if (m_toolTipSelect == null)
					m_toolTipSelect = Strings.DockPaneStrip_ToolTipWindowList;
				return m_toolTipSelect;
			}
		}

		TextFormatFlags ToolWindowTextFormat {
			get {
				TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
				                             TextFormatFlags.HorizontalCenter |
				                             TextFormatFlags.SingleLine |
				                             TextFormatFlags.VerticalCenter;
				if (RightToLeft == RightToLeft.Yes)
					return textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
				return textFormat;
			}
		}

		static int DocumentStripGapTop {
			get { return _DocumentStripGapTop; }
		}

		static int DocumentStripGapBottom {
			get { return _DocumentStripGapBottom; }
		}

		TextFormatFlags DocumentTextFormat {
			get {
				TextFormatFlags textFormat = TextFormatFlags.EndEllipsis |
				                             TextFormatFlags.SingleLine |
				                             TextFormatFlags.VerticalCenter |
				                             TextFormatFlags.Left;
				if (RightToLeft == RightToLeft.Yes)
					return textFormat | TextFormatFlags.RightToLeft;
				return textFormat;
			}
		}

		static int DocumentTabMaxWidth {
			get { return _DocumentTabMaxWidth; }
		}

		static int DocumentButtonGapTop {
			get { return _DocumentButtonGapTop; }
		}

		static int DocumentButtonGapBottom {
			get { return _DocumentButtonGapBottom; }
		}

		static int DocumentButtonGapBetween {
			get { return _DocumentButtonGapBetween; }
		}

		static int DocumentButtonGapRight {
			get { return _DocumentButtonGapRight; }
		}

		static int DocumentTabGapTop {
			get { return _DocumentTabGapTop; }
		}

		static int DocumentTabGapLeft {
			get { return _DocumentTabGapLeft; }
		}

		static int DocumentTabGapRight {
			get { return _DocumentTabGapRight; }
		}

		static int DocumentIconGapBottom {
			get { return _DocumentIconGapBottom; }
		}

		static int DocumentIconGapLeft {
			get { return _DocumentIconGapLeft; }
		}

		static int DocumentIconGapRight {
			get { return _DocumentIconGapRight; }
		}

		static int DocumentIconWidth {
			get { return _DocumentIconWidth; }
		}

		static int DocumentIconHeight {
			get { return _DocumentIconHeight; }
		}

		static int DocumentTextGapRight {
			get { return _DocumentTextGapRight; }
		}

		Color GlyphColor {
			get { return VS2010Theme.ARGB(0xFFCED4DD); }
		}

		static Color HoverBorderColor {
			get { return VS2010Theme.ARGB(0xFFE5C365); }
		}

		static Color HoverBackColor {
			get { return VS2010Theme.ARGB(0xFFFFFCF4); }
		}

		static Color PressedBackColor {
			get { return VS2010Theme.ARGB(0xFFFFE8A6); }
		}

		static Color HoverForeColor {
			get { return Color.Black; }
		}

		#endregion

		#endregion

		static ColorBlend _activeBackColorGradientBlend;

		static ColorBlend ActiveBackColorGradientBlend {
			get {
				if (_activeBackColorGradientBlend == null) {
					ColorBlend blend = new ColorBlend(4);

					blend.Positions = new[] { 0.0F, 0.5F, 0.5F, 1.0F };
					blend.Colors = new[] {
						VS2010Theme.ARGB(0xFFFFFCF4),
						VS2010Theme.ARGB(0xFFFFF3CD),
						VS2010Theme.ARGB(0xFFFFE8A6),
						VS2010Theme.ARGB(0xFFFFE8A6)
					};
					_activeBackColorGradientBlend = blend;
				}

				return _activeBackColorGradientBlend;
			}
		}

		static ColorBlend _inactiveBackColorGradientBlend;

		static ColorBlend InactiveBackColorGradientBlend {
			get {
				if (_inactiveBackColorGradientBlend == null) {
					ColorBlend blend = new ColorBlend(2);

					blend.Positions = new[] { 0.0F, 1.0F };
					blend.Colors = new[] {
						VS2010Theme.ARGB(0xFF4D6082),
						VS2010Theme.ARGB(0xFF3D5277)
					};
					_inactiveBackColorGradientBlend = blend;
				}

				return _inactiveBackColorGradientBlend;
			}
		}

		static ColorBlend unfocusedBackColorGradientBlend;

		static ColorBlend UnfocusedBackColorGradientBlend {
			get {
				if (unfocusedBackColorGradientBlend == null) {
					ColorBlend blend = new ColorBlend(4);

					blend.Positions = new[] { 0.0F, 0.5F, 0.5F, 1.0F };
					blend.Colors = new[] {
						VS2010Theme.ARGB(0xFFFDFDFD),
						VS2010Theme.ARGB(0xFFD5DAE3),
						VS2010Theme.ARGB(0xFFCED4DF),
						VS2010Theme.ARGB(0xFFCED4DF)
					};
					unfocusedBackColorGradientBlend = blend;
				}

				return unfocusedBackColorGradientBlend;
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

		public VS2010DockPaneStrip(DockPane pane)
			: base(pane) {
			FirstDisplayingTab = 0;
			EndDisplayingTab = 0;
			SetStyle(ControlStyles.ResizeRedraw |
			         ControlStyles.UserPaint |
			         ControlStyles.AllPaintingInWmPaint |
			         ControlStyles.OptimizedDoubleBuffer, true);

			SuspendLayout();

			Components = new Container();
			m_toolTip = new ToolTip(Components);
			SelectMenu = new ContextMenuStrip(Components);

			ResumeLayout();
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				Components.Dispose();
				if (m_boldFont != null) {
					m_boldFont.Dispose();
					m_boldFont = null;
				}
			}
			base.Dispose(disposing);
		}

		protected override int MeasureHeight() {
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				return MeasureHeight_ToolWindow();
			return MeasureHeight_Document();
		}

		int MeasureHeight_ToolWindow() {
			if (DockPane.IsAutoHide || Tabs.Count <= 1)
				return 0;

			int height = Math.Max(TextFont.Height, ToolWindowImageHeight + ToolWindowImageGapTop + ToolWindowImageGapBottom)
			             + ToolWindowStripGapTop + ToolWindowStripGapBottom;

			return height;
		}

		int MeasureHeight_Document() {
			int height = Math.Max(TextFont.Height + DocumentTabGapTop,
				ButtonClose.Height + DocumentButtonGapTop + DocumentButtonGapBottom)
			             + DocumentStripGapBottom + DocumentStripGapTop;

			return height;
		}

		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			Rectangle rect = TabsRectangle;
			e.Graphics.FillRectangle(Brushes.Transparent, rect);

			base.OnPaint(e);
			CalculateTabs();
			if (Appearance == DockPane.AppearanceStyle.Document && DockPane.ActiveContent != null) {
				if (EnsureDocumentTabVisible(DockPane.ActiveContent, false))
					CalculateTabs();
			}

			DrawTabStrip(e.Graphics);
		}

		protected override void OnRefreshChanges() {
			SetInertButtons();
			Invalidate();
		}

		protected override GraphicsPath GetOutline(int index) {
			var ret = new GraphicsPath();
			Rectangle rectTab = RectangleToScreen(GetTabRectangle(index));
			rectTab.Height -= DocumentStripGapBottom;
			ret.AddRectangle(rectTab);

			Rectangle rectPaneClient = DockPane.RectangleToScreen(DockPane.ClientRectangle);
			if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom) {
				ret.AddLine(rectTab.Right, rectTab.Top, rectPaneClient.Right, rectTab.Top);
				ret.AddLine(rectPaneClient.Right, rectTab.Top, rectPaneClient.Right, rectPaneClient.Top);
				ret.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Left, rectPaneClient.Top);
				ret.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Left, rectTab.Top);
				ret.AddLine(rectPaneClient.Left, rectTab.Top, rectTab.Right, rectTab.Top);
			}
			else {
				ret.AddLine(rectTab.Right, rectTab.Bottom, rectPaneClient.Right, rectTab.Bottom);
				ret.AddLine(rectPaneClient.Right, rectTab.Bottom, rectPaneClient.Right, rectPaneClient.Bottom);
				ret.AddLine(rectPaneClient.Right, rectPaneClient.Bottom, rectPaneClient.Left, rectPaneClient.Bottom);
				ret.AddLine(rectPaneClient.Left, rectPaneClient.Bottom, rectPaneClient.Left, rectTab.Bottom);
				ret.AddLine(rectPaneClient.Left, rectTab.Bottom, rectTab.Right, rectTab.Bottom);
			}
			return ret;
		}

		void CalculateTabs() {
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				CalculateTabs_ToolWindow();
			else
				CalculateTabs_Document();
		}

		void CalculateTabs_ToolWindow() {
			if (Tabs.Count <= 1 || DockPane.IsAutoHide)
				return;

			Rectangle rectTabStrip = TabStripRectangle;

			// Calculate tab widths
			int countTabs = Tabs.Count;
			foreach (VS2010Tab tab in Tabs) {
				tab.MaxWidth = GetMaxTabWidth(Tabs.IndexOf(tab));
				tab.Flag = false;
			}

			// Set tab whose max width less than average width
			bool anyWidthWithinAverage = true;
			int totalWidth = rectTabStrip.Width - ToolWindowStripGapLeft - ToolWindowStripGapRight;
			int totalAllocatedWidth = 0;
			int averageWidth = totalWidth / countTabs;
			int remainedTabs = countTabs;
			for (anyWidthWithinAverage = true; anyWidthWithinAverage && remainedTabs > 0;) {
				anyWidthWithinAverage = false;
				foreach (VS2010Tab tab in Tabs) {
					if (tab.Flag)
						continue;

					if (tab.MaxWidth <= averageWidth) {
						tab.Flag = true;
						tab.TabWidth = tab.MaxWidth;
						totalAllocatedWidth += tab.TabWidth;
						anyWidthWithinAverage = true;
						remainedTabs--;
					}
				}
				if (remainedTabs != 0)
					averageWidth = (totalWidth - totalAllocatedWidth) / remainedTabs;
			}

			// If any tab width not set yet, set it to the average width
			if (remainedTabs > 0) {
				int roundUpWidth = (totalWidth - totalAllocatedWidth) - (averageWidth * remainedTabs);
				foreach (VS2010Tab tab in Tabs) {
					if (tab.Flag)
						continue;

					tab.Flag = true;
					if (roundUpWidth > 0) {
						tab.TabWidth = averageWidth + 1;
						roundUpWidth--;
					}
					else
						tab.TabWidth = averageWidth;
				}
			}

			// Set the X position of the tabs
			int x = rectTabStrip.X + ToolWindowStripGapLeft;
			foreach (VS2010Tab tab in Tabs) {
				tab.TabX = x;
				x += tab.TabWidth;
			}
		}

		bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index) {
			bool overflow = false;

			var tab = Tabs[index] as VS2010Tab;
			tab.MaxWidth = GetMaxTabWidth(index);
			int width = Math.Min(tab.MaxWidth, DocumentTabMaxWidth);
			if (x + width < rectTabStrip.Right || index == StartDisplayingTab) {
				tab.TabX = x;
				tab.TabWidth = width;
				EndDisplayingTab = index;
			}
			else {
				tab.TabX = 0;
				tab.TabWidth = 0;
				overflow = true;
			}
			x += width;

			return overflow;
		}

		/// <summary>
		///     Calculate which tabs are displayed and in what order.
		/// </summary>
		void CalculateTabs_Document() {
			if (m_startDisplayingTab >= Tabs.Count)
				m_startDisplayingTab = 0;

			Rectangle rectTabStrip = TabsRectangle;

			int x = rectTabStrip.X; //+ rectTabStrip.Height / 2;
			bool overflow = false;

			// Originally all new documents that were considered overflow
			// (not enough pane strip space to show all tabs) were added to
			// the far left (assuming not right to left) and the tabs on the
			// right were dropped from view. If StartDisplayingTab is not 0
			// then we are dealing with making sure a specific tab is kept in focus.
			if (m_startDisplayingTab > 0) {
				int tempX = x;
				var tab = Tabs[m_startDisplayingTab] as VS2010Tab;
				tab.MaxWidth = GetMaxTabWidth(m_startDisplayingTab);

				// Add the active tab and tabs to the left
				for (int i = StartDisplayingTab; i >= 0; i--)
					CalculateDocumentTab(rectTabStrip, ref tempX, i);

				// Store which tab is the first one displayed so that it
				// will be drawn correctly (without part of the tab cut off)
				FirstDisplayingTab = EndDisplayingTab;

				tempX = x; // Reset X location because we are starting over

				// Start with the first tab displayed - name is a little misleading.
				// Loop through each tab and set its location. If there is not enough
				// room for all of them overflow will be returned.
				for (int i = EndDisplayingTab; i < Tabs.Count; i++)
					overflow = CalculateDocumentTab(rectTabStrip, ref tempX, i);

				// If not all tabs are shown then we have an overflow.
				if (FirstDisplayingTab != 0)
					overflow = true;
			}
			else {
				for (int i = StartDisplayingTab; i < Tabs.Count; i++)
					overflow = CalculateDocumentTab(rectTabStrip, ref x, i);
				for (int i = 0; i < StartDisplayingTab; i++)
					overflow = CalculateDocumentTab(rectTabStrip, ref x, i);

				FirstDisplayingTab = StartDisplayingTab;
			}

			if (!overflow) {
				m_startDisplayingTab = 0;
				FirstDisplayingTab = 0;
				x = rectTabStrip.X; // +rectTabStrip.Height / 2;
				foreach (VS2010Tab tab in Tabs) {
					tab.TabX = x;
					x += tab.TabWidth;
				}
			}
			DocumentTabsOverflow = overflow;
		}

		protected override void EnsureTabVisible(IDockContent content) {
			if (Appearance != DockPane.AppearanceStyle.Document || !Tabs.Contains(content))
				return;

			CalculateTabs();
			EnsureDocumentTabVisible(content, true);
		}

		bool EnsureDocumentTabVisible(IDockContent content, bool repaint) {
			int index = Tabs.IndexOf(content);
			var tab = Tabs[index] as VS2010Tab;
			if (tab.TabWidth != 0)
				return false;

			StartDisplayingTab = index;
			if (repaint)
				Invalidate();

			return true;
		}

		int GetMaxTabWidth(int index) {
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				return GetMaxTabWidth_ToolWindow(index);
			return GetMaxTabWidth_Document(index);
		}

		int GetMaxTabWidth_ToolWindow(int index) {
			IDockContent content = Tabs[index].Content;
			Size sizeString = TextRenderer.MeasureText(content.DockHandler.TabText, TextFont);
			return ToolWindowImageWidth + sizeString.Width + ToolWindowImageGapLeft
			       + ToolWindowImageGapRight + ToolWindowTextGapRight;
		}

		const int TAB_CLOSE_BUTTON_WIDTH = 16;

		int GetMaxTabWidth_Document(int index) {
			IDockContent content = Tabs[index].Content;
			int height = GetTabRectangle_Document((VS2010Tab)Tabs[index]).Height;
			Size sizeText = TextRenderer.MeasureText(content.DockHandler.TabText, Font, new Size(DocumentTabMaxWidth, height),
				DocumentTextFormat);

			int width;
			if (DockPane.DockPanel.ShowDocumentIcon)
				width = sizeText.Width + DocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight + DocumentTextGapRight;
			else
				width = sizeText.Width + DocumentIconGapLeft + DocumentTextGapRight;

			if (content.DockHandler.CloseButtonVisible)
				width += TAB_CLOSE_BUTTON_WIDTH;
			return width;
		}

		void DrawTabStrip(Graphics g) {
			using (var brush = new SolidBrush(DockPane.DockPanel.BackColor))
				g.FillRectangle(brush, ClientRectangle);

			if (Appearance == DockPane.AppearanceStyle.Document)
				DrawTabStrip_Document(g);
			else
				DrawTabStrip_ToolWindow(g);
		}

		void DrawTabStrip_Document(Graphics g) {
			int count = Tabs.Count;
			if (count == 0)
				return;

			Rectangle rectTabStrip = TabStripRectangle;
			rectTabStrip.Height += 1;

			// Draw the tabs
			Rectangle rectTabOnly = TabsRectangle;
			Rectangle rectTab = Rectangle.Empty;
			VS2010Tab tabActive = null;
			g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
			for (int i = 0; i < count; i++) {
				rectTab = GetTabRectangle(i);
				if (Tabs[i].Content == DockPane.ActiveContent) {
					tabActive = Tabs[i] as VS2010Tab;
					continue;
				}
				if (rectTab.IntersectsWith(rectTabOnly))
					DrawTab(g, Tabs[i] as VS2010Tab, rectTab);
			}

			g.SetClip(rectTabStrip);

			using (var brush = new LinearGradientBrush(rectTabStrip, Color.Empty, Color.Empty, LinearGradientMode.Vertical))
			using (
				var roundedRect =
					RoundedRectangle.Construct(new Rectangle(rectTabStrip.Left, rectTabStrip.Bottom - 4, rectTabStrip.Width, 4), 4,
						RoundedCorner.TopLeft | RoundedCorner.TopRight)) {
				if (tabActive != null && DockPane.IsActiveDocumentPane)
					brush.InterpolationColors = DockPane.IsActivated ? ActiveBackColorGradientBlend : UnfocusedBackColorGradientBlend;
				else
					brush.InterpolationColors = InactiveBackColorGradientBlend;
				g.FillPath(brush, roundedRect);
			}

			g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
			if (tabActive != null) {
				rectTab = GetTabRectangle(Tabs.IndexOf(tabActive));
				if (rectTab.IntersectsWith(rectTabOnly)) {
					rectTab.Intersect(rectTabOnly);
					DrawTab(g, tabActive, rectTab);
				}
			}
		}

		void DrawTabStrip_ToolWindow(Graphics g) {
			g.PixelOffsetMode = PixelOffsetMode.None;
			Rectangle rectTabStrip = TabStripRectangle;

			for (int i = 0; i < Tabs.Count; i++)
				DrawTab(g, Tabs[i] as VS2010Tab, GetTabRectangle(i));
		}

		Rectangle GetTabRectangle(int index) {
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				return GetTabRectangle_ToolWindow((VS2010Tab)Tabs[index]);
			return GetTabRectangle_Document((VS2010Tab)Tabs[index]);
		}

		Rectangle GetTabRectangle(VS2010Tab tab) {
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				return GetTabRectangle_ToolWindow(tab);
			return GetTabRectangle_Document(tab);
		}

		Rectangle GetTabRectangle_ToolWindow(VS2010Tab tab) {
			Rectangle rectTabStrip = TabStripRectangle;

			return new Rectangle(tab.TabX, rectTabStrip.Y, tab.TabWidth, rectTabStrip.Height);
		}

		Rectangle GetTabRectangle_Document(VS2010Tab tab) {
			Rectangle rectTabStrip = TabStripRectangle;

			Rectangle rect = new Rectangle();
			rect.X = tab.TabX;
			rect.Width = tab.TabWidth;
			rect.Height = rectTabStrip.Height - DocumentTabGapTop;

			if (DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
				rect.Y = rectTabStrip.Y + DocumentStripGapBottom;
			else
				rect.Y = rectTabStrip.Y + DocumentTabGapTop;

			return rect;
		}

		void DrawTab(Graphics g, VS2010Tab tab, Rectangle rect) {
			if (Appearance == DockPane.AppearanceStyle.ToolWindow)
				DrawTab_ToolWindow(g, tab, rect);
			else
				DrawTab_Document(g, tab, rect);
		}

		void DrawTab_ToolWindow(Graphics g, VS2010Tab tab, Rectangle rect) {
			rect.Width--;
			rect.Y--;

			Rectangle rectIcon = new Rectangle(
				rect.X + ToolWindowImageGapLeft,
				rect.Y + rect.Height - ToolWindowImageGapBottom - ToolWindowImageHeight,
				ToolWindowImageWidth, ToolWindowImageHeight);

			Rectangle rectText = new Rectangle(
				rect.X + ToolWindowImageGapLeft,
				rect.Y,
				rect.Width - ToolWindowImageGapLeft, rect.Height - 1);

			rectText.X += ToolWindowImageWidth + ToolWindowImageGapRight;
			rectText.Width -= ToolWindowImageWidth + ToolWindowImageGapRight;

			Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
			Rectangle rectBack = DrawHelper.RtlTransform(this, rect);
			rectBack.Width += rect.X;
			rectBack.X = 0;

			rectText = DrawHelper.RtlTransform(this, rectText);
			rectIcon = DrawHelper.RtlTransform(this, rectIcon);

			if (DockPane.ActiveContent == tab.Content) {
				using (var roundedRect = RoundedRectangle.Construct(rect, 4, RoundedCorner.BottomLeft | RoundedCorner.BottomRight)) {
					g.FillPath(Brushes.White, roundedRect);
					g.DrawPath(Pens.White, roundedRect);
					TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, Color.Black, DocumentTextFormat);
				}
			}
			else {
				if (tab.Content == DockPane.MouseOverTab) {
					using (var brush = new LinearGradientBrush(rect, Color.Empty, Color.Empty, LinearGradientMode.Vertical))
					using (var pen = new Pen(new SolidBrush(VS2010Theme.ARGB(0xFF9BA7B7)), 1))
					using (var roundedRect = RoundedRectangle.Construct(rect, 4, RoundedCorner.BottomLeft | RoundedCorner.BottomRight)) {
						brush.InterpolationColors = HoverColorBlend;
						g.FillPath(brush, roundedRect);
						g.DrawPath(pen, roundedRect);
					}
				}
				TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, Color.White, DocumentTextFormat);
			}

			if (rectTab.Contains(rectIcon)) {
				using (var icon = new Icon(tab.Content.DockHandler.Icon, rectIcon.Size))
					g.DrawIcon(icon, rectIcon);
			}
		}

		void DrawTab_Document(Graphics g, VS2010Tab tab, Rectangle rect) {
			if (tab.TabWidth == 0)
				return;

			var rectCloseButton = GetCloseButtonRect(rect);
			if (!tab.Content.DockHandler.CloseButtonVisible)
				rectCloseButton = Rectangle.Empty;

			Rectangle rectIcon = new Rectangle(
				rect.X + DocumentIconGapLeft,
				rect.Y + rect.Height - DocumentIconGapBottom - DocumentIconHeight,
				DocumentIconWidth, DocumentIconHeight);

			Rectangle rectText = new Rectangle(
				rect.X + DocumentIconGapLeft,
				rect.Y,
				rect.Width - DocumentIconGapLeft, rect.Height - 1);

			if (DockPane.DockPanel.ShowDocumentIcon) {
				rectText.X += DocumentIconWidth + DocumentIconGapRight;
				rectText.Width -= DocumentIconWidth + DocumentIconGapRight;
			}

			Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
			Rectangle rectBack = DrawHelper.RtlTransform(this, rect);
			rectBack.Width += rect.X;
			rectBack.X = 0;

			rectText = DrawHelper.RtlTransform(this, rectText);
			rectIcon = DrawHelper.RtlTransform(this, rectIcon);

			if (DockPane.ActiveContent == tab.Content) {
				using (var brush = new LinearGradientBrush(rect, Color.Empty, Color.Empty, LinearGradientMode.Vertical))
				using (var roundedRect = RoundedRectangle.Construct(rect, 6, RoundedCorner.TopLeft | RoundedCorner.TopRight)) {
					if (DockPane.IsActiveDocumentPane) {
						brush.InterpolationColors = DockPane.IsActivated ? ActiveBackColorGradientBlend : UnfocusedBackColorGradientBlend;
						g.FillPath(brush, roundedRect);
						TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, Color.Black, DocumentTextFormat);
					}
					else {
						brush.InterpolationColors = InactiveBackColorGradientBlend;
						g.FillPath(brush, roundedRect);
						TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, Color.White, DocumentTextFormat);
					}
					DrawCloseButton(g, ForeColor, rectCloseButton);
				}
			}
			else {
				if (tab.Content == DockPane.MouseOverTab) {
					using (var brush = new LinearGradientBrush(rect, Color.Empty, Color.Empty, LinearGradientMode.Vertical))
					using (var pen = new Pen(new SolidBrush(VS2010Theme.ARGB(0xFF9BA7B7))))
					using (var roundedRect = RoundedRectangle.Construct(rect, 6, RoundedCorner.TopLeft | RoundedCorner.TopRight)) {
						brush.InterpolationColors = HoverColorBlend;
						g.FillPath(brush, roundedRect);
						g.DrawPath(pen, roundedRect);
					}
					DrawCloseButton(g, GlyphColor, rectCloseButton);
				}
				TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectText, Color.White, DocumentTextFormat);
			}

			if (rectTab.Contains(rectIcon) && DockPane.DockPanel.ShowDocumentIcon) {
				using (var icon = new Icon(tab.Content.DockHandler.Icon, rectIcon.Size))
					g.DrawIcon(icon, rectIcon);
			}
		}

		void DrawCloseButton(Graphics g, Color foreground, Rectangle rectCloseButton) {
			if (rectCloseButton.IsEmpty)
				return;
			bool mouseOver = rectCloseButton.Contains(PointToClient(MousePosition));
			bool mouseDown = mouseOver && (MouseButtons & MouseButtons.Left) != 0;
			if (mouseOver && Enabled) {
				using (Pen pen = new Pen(HoverBorderColor))
				using (Brush brush = new SolidBrush(mouseDown ? PressedBackColor : HoverBackColor)) {
					g.PixelOffsetMode = PixelOffsetMode.None;
					var rect = rectCloseButton;
					g.FillRectangle(brush, rect);
					g.DrawRectangle(pen, rect);
					g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				}
			}

			using (var imageAttributes = new ImageAttributes()) {
				var color = mouseOver ? HoverForeColor : foreground;
				var matrix = new ColorMatrix();
				matrix.Matrix40 = color.R / 255f;
				matrix.Matrix41 = color.G / 255f;
				matrix.Matrix42 = color.B / 255f;
				imageAttributes.SetColorMatrix(matrix);

				g.DrawImage(
					ImageButtonClose,
					new Rectangle(
						rectCloseButton.X + (rectCloseButton.Width - ImageButtonClose.Width) / 2,
						rectCloseButton.Y + (rectCloseButton.Height - ImageButtonClose.Height) / 2,
						ImageButtonClose.Width, ImageButtonClose.Height),
					0, 0,
					ImageButtonClose.Width,
					ImageButtonClose.Height,
					GraphicsUnit.Pixel,
					imageAttributes);
			}
		}

		protected override void OnMouseClick(MouseEventArgs e) {
			if (e.Button != MouseButtons.Left || Appearance != DockPane.AppearanceStyle.Document)
				return;

			var indexHit = HitTest();
			if (indexHit != -1 && TabCloseButtonHit(indexHit))
				Tabs[indexHit].Content.DockHandler.Close();
			base.OnMouseClick(e);
		}

		bool noDrag;

		protected override void OnMouseDown(MouseEventArgs e) {
			var indexHit = HitTest();
			if (indexHit != -1 && TabCloseButtonHit(indexHit)) {
				Capture = true;
				noDrag = true;
				return;
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			Capture = false;
			noDrag = false;
			base.OnMouseUp(e);
		}

		bool TabCloseButtonHit(int index) {
			if (!Tabs[index].Content.DockHandler.CloseButtonVisible)
				return false;
			var mousePos = PointToClient(MousePosition);
			var tabRect = GetTabRectangle(index);
			var closeButtonRect = GetCloseButtonRect(tabRect);

			var mouseRect = new Rectangle(mousePos, new Size(1, 1));
			return closeButtonRect.IntersectsWith(mouseRect);
		}

		Rectangle GetCloseButtonRect(Rectangle rectTab) {
			if (Appearance != DockPane.AppearanceStyle.Document) {
				return Rectangle.Empty;
			}

			const int gap = 3;
			const int imageSize = 15;
			return new Rectangle(rectTab.X + rectTab.Width - imageSize - gap - 1, rectTab.Y + gap, imageSize, imageSize);
		}

		void WindowList_Click(object sender, EventArgs e) {
			int x = 0;
			int y = ButtonWindowList.Location.Y + ButtonWindowList.Height;

			SelectMenu.Items.Clear();
			foreach (VS2010Tab tab in Tabs) {
				IDockContent content = tab.Content;

				var bmp = new Bitmap(16, 16);
				using (var icon = new Icon(content.DockHandler.Icon, 16, 16))
				using (var g = Graphics.FromImage(bmp))
					g.DrawIcon(content.DockHandler.Icon, new Rectangle(0, 0, 16, 16));

				ToolStripItem item = SelectMenu.Items.Add(content.DockHandler.TabText, bmp);
				item.Tag = tab.Content;
				item.Click += ContextMenuItem_Click;
			}
			SelectMenu.Show(ButtonWindowList, x, y);
		}

		void ContextMenuItem_Click(object sender, EventArgs e) {
			ToolStripMenuItem item = sender as ToolStripMenuItem;
			if (item != null) {
				IDockContent content = (IDockContent)item.Tag;
				DockPane.ActiveContent = content;
			}
		}

		void SetInertButtons() {
			if (Appearance == DockPane.AppearanceStyle.ToolWindow) {
				if (m_buttonClose != null)
					m_buttonClose.Left = -m_buttonClose.Width;

				if (m_buttonWindowList != null)
					m_buttonWindowList.Left = -m_buttonWindowList.Width;
			}
			else {
				ButtonClose.Enabled = false;
				m_closeButtonVisible = false;
				ButtonClose.Visible = m_closeButtonVisible;
				ButtonClose.RefreshChanges();
				ButtonWindowList.RefreshChanges();
			}
			ButtonClose.ForeColor = ButtonWindowList.ForeColor = GlyphColor;
		}

		protected override void OnLayout(LayoutEventArgs levent) {
			if (Appearance == DockPane.AppearanceStyle.Document) {
				LayoutButtons();
				OnRefreshChanges();
			}

			base.OnLayout(levent);
		}

		void LayoutButtons() {
			Rectangle rectTabStrip = TabStripRectangle;

			// Set position and size of the buttons
			int buttonWidth = ButtonWindowList.Image.Width;
			int buttonHeight = ButtonWindowList.Image.Height;
			Size buttonSize = new Size(buttonWidth, buttonHeight);

			int x = rectTabStrip.X + rectTabStrip.Width - DocumentTabGapLeft
			        - DocumentButtonGapRight - buttonWidth;
			int y = rectTabStrip.Y + DocumentButtonGapTop;
			Point point = new Point(x, y);

			ButtonWindowList.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
		}

		void Close_Click(object sender, EventArgs e) {
			DockPane.CloseActiveContent();
		}

		protected override int HitTest(Point ptMouse) {
			if (!TabsRectangle.Contains(ptMouse))
				return -1;

			foreach (Tab tab in Tabs) {
				var rect = GetTabRectangle((VS2010Tab)tab);
				if (rect.Contains(ptMouse))
					return Tabs.IndexOf(tab);
			}

			return -1;
		}

		Rectangle ActiveClose { get; set; }

		bool SetActiveClose(Rectangle rectangle) {
			if (ActiveClose == rectangle)
				return false;

			ActiveClose = rectangle;
			return true;
		}

		bool SetMouseOverTab(IDockContent content) {
			if (DockPane.MouseOverTab == content)
				return false;

			DockPane.MouseOverTab = content;
			return true;
		}

		protected override void OnMouseHover(EventArgs e) {
			int index = HitTest(PointToClient(MousePosition));
			string toolTip = string.Empty;

			base.OnMouseHover(e);

			if (index != -1) {
				var tab = Tabs[index] as VS2010Tab;
				if (!string.IsNullOrEmpty(tab.Content.DockHandler.ToolTipText))
					toolTip = tab.Content.DockHandler.ToolTipText;
				else if (tab.MaxWidth > tab.TabWidth)
					toolTip = tab.Content.DockHandler.TabText;
			}

			if (m_toolTip.GetToolTip(this) != toolTip) {
				m_toolTip.Active = false;
				m_toolTip.SetToolTip(this, toolTip);
				m_toolTip.Active = true;
			}

			// requires further tracking of mouse hover behavior,
			ResetMouseEventArgs();
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			noDrag &= (MouseButtons & MouseButtons.Left) != 0;
			if (!noDrag)
				base.OnMouseMove(e);

			int index = HitTest(PointToClient(MousePosition));

			bool tabUpdate = false;
			bool buttonUpdate = false;
			if (index != -1) {
				var tab = Tabs[index] as VS2010Tab;
				if (Appearance == DockPane.AppearanceStyle.ToolWindow || Appearance == DockPane.AppearanceStyle.Document) {
					tabUpdate = SetMouseOverTab(tab.Content == DockPane.ActiveContent ? null : tab.Content);
				}

				var mousePos = PointToClient(MousePosition);
				var tabRect = GetTabRectangle(index);
				var closeButtonRect = GetCloseButtonRect(tabRect);
				var mouseRect = new Rectangle(mousePos, new Size(1, 1));
				buttonUpdate = SetActiveClose(closeButtonRect.IntersectsWith(mouseRect) ? closeButtonRect : Rectangle.Empty);
			}
			else {
				tabUpdate = SetMouseOverTab(null);
				buttonUpdate = SetActiveClose(Rectangle.Empty);
			}

			if (tabUpdate || buttonUpdate)
				Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e) {
			var tabUpdate = SetMouseOverTab(null);
			var buttonUpdate = SetActiveClose(Rectangle.Empty);
			if (tabUpdate || buttonUpdate)
				Invalidate();

			base.OnMouseLeave(e);
		}

		protected override void OnRightToLeftChanged(EventArgs e) {
			base.OnRightToLeftChanged(e);
			PerformLayout();
		}
	}
}