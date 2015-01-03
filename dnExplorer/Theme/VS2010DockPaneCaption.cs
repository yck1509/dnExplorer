using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Res = WeifenLuo.WinFormsUI.Docking.Resources;

namespace dnExplorer.Theme {
	internal class VS2010LightDockPaneCaption : DockPaneCaptionBase {
		sealed class InertButton : InertButtonBase {
			Bitmap m_image, m_imageAutoHide;

			public InertButton(VS2010LightDockPaneCaption dockPaneCaption, Bitmap image, Bitmap imageAutoHide) {
				DockPaneCaption = dockPaneCaption;
				m_image = image;
				m_imageAutoHide = imageAutoHide;
				RefreshChanges();
			}

			protected override Color HoverBorderColor {
				get { return VS2010Theme.ARGB(0xFFE5C365); }
			}

			protected override Color HoverBackColor {
				get { return VS2010Theme.ARGB(0xFFFFFCF4); }
			}

			protected override Color PressedBackColor {
				get { return VS2010Theme.ARGB(0xFFFFE8A6); }
			}

			protected override Color HoverForeColor {
				get { return Color.Black; }
			}

			VS2010LightDockPaneCaption DockPaneCaption { get; set; }

			public bool IsAutoHide {
				get { return DockPaneCaption.DockPane.IsAutoHide; }
			}

			public override Bitmap Image {
				get { return IsAutoHide ? m_imageAutoHide : m_image; }
			}

			protected override void OnRefreshChanges() {
				if (DockPaneCaption.DockPane.DockPanel != null) {
					if (DockPaneCaption.TextColor != ForeColor) {
						ForeColor = DockPaneCaption.GlyphColor;
						Invalidate();
					}
				}
			}
		}

		#region consts

		const int _TextGapTop = 3;
		const int _TextGapBottom = 2;
		const int _TextGapLeft = 2;
		const int _TextGapRight = 3;
		const int _ButtonGapTop = 2;
		const int _ButtonGapBottom = 1;
		const int _ButtonGapBetween = 1;
		const int _ButtonGapLeft = 1;
		const int _ButtonGapRight = 2;

		#endregion

		static Bitmap _imageButtonClose;

		static Bitmap ImageButtonClose {
			get {
				if (_imageButtonClose == null)
					_imageButtonClose = Res.DockPane_Close;

				return _imageButtonClose;
			}
		}

		InertButton m_buttonClose;

		InertButton ButtonClose {
			get {
				if (m_buttonClose == null) {
					m_buttonClose = new InertButton(this, ImageButtonClose, ImageButtonClose);
					m_toolTip.SetToolTip(m_buttonClose, ToolTipClose);
					m_buttonClose.Click += Close_Click;
					Controls.Add(m_buttonClose);
				}

				return m_buttonClose;
			}
		}

		static Bitmap _imageButtonAutoHide;

		static Bitmap ImageButtonAutoHide {
			get {
				if (_imageButtonAutoHide == null)
					_imageButtonAutoHide = Res.DockPane_AutoHide;

				return _imageButtonAutoHide;
			}
		}

		static Bitmap _imageButtonDock;

		static Bitmap ImageButtonDock {
			get {
				if (_imageButtonDock == null)
					_imageButtonDock = Res.DockPane_Dock;

				return _imageButtonDock;
			}
		}

		InertButton m_buttonAutoHide;

		InertButton ButtonAutoHide {
			get {
				if (m_buttonAutoHide == null) {
					m_buttonAutoHide = new InertButton(this, ImageButtonDock, ImageButtonAutoHide);
					m_toolTip.SetToolTip(m_buttonAutoHide, ToolTipAutoHide);
					m_buttonAutoHide.Click += AutoHide_Click;
					Controls.Add(m_buttonAutoHide);
				}

				return m_buttonAutoHide;
			}
		}

		static Bitmap _imageButtonOptions;

		static Bitmap ImageButtonOptions {
			get {
				if (_imageButtonOptions == null)
					_imageButtonOptions = Res.DockPane_Option;

				return _imageButtonOptions;
			}
		}

		InertButton m_buttonOptions;

		InertButton ButtonOptions {
			get {
				if (m_buttonOptions == null) {
					m_buttonOptions = new InertButton(this, ImageButtonOptions, ImageButtonOptions);
					m_toolTip.SetToolTip(m_buttonOptions, ToolTipOptions);
					m_buttonOptions.Click += Options_Click;
					Controls.Add(m_buttonOptions);
				}
				return m_buttonOptions;
			}
		}

		IContainer Components { get; set; }

		ToolTip m_toolTip;

		public VS2010LightDockPaneCaption(DockPane pane)
			: base(pane) {
			SuspendLayout();

			Components = new Container();
			m_toolTip = new ToolTip(Components);

			new ActivateHook(pane, this);

			ResumeLayout();
		}

		class ActivateHook : NativeWindow {
			DockPane dockPane;
			VS2010LightDockPaneCaption caption;

			public ActivateHook(DockPane pane, VS2010LightDockPaneCaption caption) {
				dockPane = pane;
				this.caption = caption;
				pane.HandleCreated += (sender, e) => AssignHandle(pane.Handle);
				pane.HandleDestroyed += (sender, e) => ReleaseHandle();

				if (pane.IsHandleCreated)
					AssignHandle(pane.Handle);
			}

			protected override void WndProc(ref Message m) {
				if (m.Msg == 0x0021) {
					if (caption.ButtonClose.ClientRectangle.Contains(caption.ButtonClose.PointToClient(MousePosition)) ||
					    caption.ButtonAutoHide.ClientRectangle.Contains(caption.ButtonAutoHide.PointToClient(MousePosition)))
						m.Result = new IntPtr(3);
					else
						base.WndProc(ref m);
				}
				else
					base.WndProc(ref m);
			}
		}

		protected override void Dispose(bool disposing) {
			if (disposing)
				Components.Dispose();
			base.Dispose(disposing);
		}

		static int TextGapTop {
			get { return _TextGapTop; }
		}

		public Font TextFont {
			get { return DockPane.DockPanel.Font; }
		}

		static int TextGapBottom {
			get { return _TextGapBottom; }
		}

		static int TextGapLeft {
			get { return _TextGapLeft; }
		}

		static int TextGapRight {
			get { return _TextGapRight; }
		}

		static int ButtonGapTop {
			get { return _ButtonGapTop; }
		}

		static int ButtonGapBottom {
			get { return _ButtonGapBottom; }
		}

		static int ButtonGapLeft {
			get { return _ButtonGapLeft; }
		}

		static int ButtonGapRight {
			get { return _ButtonGapRight; }
		}

		static int ButtonGapBetween {
			get { return _ButtonGapBetween; }
		}

		static string _toolTipClose;

		static string ToolTipClose {
			get {
				if (_toolTipClose == null)
					_toolTipClose = Strings.DockPaneCaption_ToolTipClose;
				return _toolTipClose;
			}
		}

		static string _toolTipOptions;

		static string ToolTipOptions {
			get {
				if (_toolTipOptions == null)
					_toolTipOptions = Strings.DockPaneCaption_ToolTipOptions;

				return _toolTipOptions;
			}
		}

		static string _toolTipAutoHide;

		static string ToolTipAutoHide {
			get {
				if (_toolTipAutoHide == null)
					_toolTipAutoHide = Strings.DockPaneCaption_ToolTipAutoHide;
				return _toolTipAutoHide;
			}
		}

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

		Color TextColor {
			get {
				if (DockPane.IsActivated)
					return Color.Black;
				return Color.White;
			}
		}

		Color GlyphColor {
			get {
				if (DockPane.IsActivated)
					return VS2010Theme.ARGB(0xFF75633D);
				return VS2010Theme.ARGB(0xFFCED4DD);
			}
		}

		static TextFormatFlags _textFormat =
			TextFormatFlags.SingleLine |
			TextFormatFlags.EndEllipsis |
			TextFormatFlags.VerticalCenter;

		TextFormatFlags TextFormat {
			get {
				if (RightToLeft == RightToLeft.No)
					return _textFormat;
				return _textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
			}
		}

		protected override int MeasureHeight() {
			int height = TextFont.Height + TextGapTop + TextGapBottom;

			if (height < ButtonClose.Image.Height + ButtonGapTop + ButtonGapBottom)
				height = ButtonClose.Image.Height + ButtonGapTop + ButtonGapBottom;

			return height;
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			DrawCaption(e.Graphics);
		}

		void DrawCaption(Graphics g) {
			if (ClientRectangle.Width == 0 || ClientRectangle.Height == 0)
				return;

			Rectangle rect = ClientRectangle;
			using (var brush = new SolidBrush(DockPane.DockPanel.BackColor))
				g.FillRectangle(brush, rect);

			ColorBlend captionBg;

			if (DockPane.IsActivated)
				captionBg = ActiveBackColorGradientBlend;
			else
				captionBg = InactiveBackColorGradientBlend;

			using (var captionBrush = new LinearGradientBrush(rect, Color.Empty, Color.Empty, LinearGradientMode.Vertical))
			using (var captionPath = RoundedRectangle.Construct(rect, 4, RoundedCorner.TopLeft | RoundedCorner.TopRight)) {
				captionBrush.InterpolationColors = captionBg;
				g.FillPath(captionBrush, captionPath);
			}

			Rectangle rectCaption = rect;

			Rectangle rectCaptionText = rectCaption;
			rectCaptionText.X += TextGapLeft;
			rectCaptionText.Width -= TextGapLeft + TextGapRight;
			rectCaptionText.Width -= ButtonGapLeft + ButtonClose.Width + ButtonGapRight;
			if (ShouldShowAutoHideButton)
				rectCaptionText.Width -= ButtonAutoHide.Width + ButtonGapBetween;
			if (HasTabPageContextMenu)
				rectCaptionText.Width -= ButtonOptions.Width + ButtonGapBetween;
			rectCaptionText.Y += TextGapTop;
			rectCaptionText.Height -= TextGapTop + TextGapBottom;

			Color colorText;
			if (DockPane.IsActivated)
				colorText = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor;
			else
				colorText = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.TextColor;

			TextRenderer.DrawText(g, DockPane.CaptionText, TextFont, DrawHelper.RtlTransform(this, rectCaptionText), colorText,
				TextFormat);

			Rectangle rectDotsStrip = rectCaptionText;
			int textLength = (int)g.MeasureString(DockPane.CaptionText, TextFont).Width + TextGapLeft;
			rectDotsStrip.X += textLength;
			rectDotsStrip.Width -= textLength;
			rectDotsStrip.Height = ClientRectangle.Height;

			Color dotsColor;
			if (DockPane.IsActivated)
				dotsColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.EndColor;
			else
				dotsColor = DockPane.DockPanel.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.EndColor;
		}

		protected override void OnLayout(LayoutEventArgs levent) {
			SetButtonsPosition();
			base.OnLayout(levent);
		}

		protected override void OnRefreshChanges() {
			SetButtons();
			Invalidate();
		}

		bool CloseButtonEnabled {
			get { return (DockPane.ActiveContent != null) ? DockPane.ActiveContent.DockHandler.CloseButton : false; }
		}

		/// <summary>
		///     Determines whether the close button is visible on the content
		/// </summary>
		bool CloseButtonVisible {
			get { return (DockPane.ActiveContent != null) ? DockPane.ActiveContent.DockHandler.CloseButtonVisible : false; }
		}

		bool ShouldShowAutoHideButton {
			get { return !DockPane.IsFloat; }
		}

		void SetButtons() {
			ButtonClose.Enabled = CloseButtonEnabled;
			ButtonClose.Visible = CloseButtonVisible;
			ButtonAutoHide.Visible = ShouldShowAutoHideButton;
			ButtonOptions.Visible = HasTabPageContextMenu;
			ButtonClose.RefreshChanges();
			ButtonAutoHide.RefreshChanges();
			ButtonOptions.RefreshChanges();

			SetButtonsPosition();
		}

		void SetButtonsPosition() {
			// set the size and location for close and auto-hide buttons
			Rectangle rectCaption = ClientRectangle;
			int buttonWidth = ButtonClose.Image.Width;
			int buttonHeight = ButtonClose.Image.Height;
			Size buttonSize = new Size(buttonWidth, buttonHeight);
			int x = rectCaption.X + rectCaption.Width - 1 - ButtonGapRight - m_buttonClose.Width;
			int y = rectCaption.Y + ButtonGapTop;
			Point point = new Point(x, y);
			ButtonClose.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

			// If the close button is not visible draw the auto hide button overtop.
			// Otherwise it is drawn to the left of the close button.
			if (CloseButtonVisible)
				point.Offset(-(buttonWidth + ButtonGapBetween), 0);

			ButtonAutoHide.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
			if (ShouldShowAutoHideButton)
				point.Offset(-(buttonWidth + ButtonGapBetween), 0);
			ButtonOptions.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
		}

		void Close_Click(object sender, EventArgs e) {
			DockPane.CloseActiveContent();
		}

		void AutoHide_Click(object sender, EventArgs e) {
			DockPane.DockState = DockHelper.ToggleAutoHideState(DockPane.DockState);
			if (DockHelper.IsDockStateAutoHide(DockPane.DockState)) {
				DockPane.DockPanel.ActiveAutoHideContent = null;
				DockPane.NestedDockingStatus.NestedPanes.SwitchPaneWithFirstChild(DockPane);
			}
		}

		void Options_Click(object sender, EventArgs e) {
			ShowTabPageContextMenu(PointToClient(MousePosition));
		}

		protected override void OnRightToLeftChanged(EventArgs e) {
			base.OnRightToLeftChanged(e);
			PerformLayout();
		}

		protected override void WndProc(ref Message m) {
			const int WM_LBUTTONDBLCLK = 0x0203;
			if (m.Msg == WM_LBUTTONDBLCLK) {
				if (DockHelper.IsDockStateAutoHide(DockPane.DockState))
					base.WndProc(ref m);
				else
					base.DefWndProc(ref m);
				return;
			}
			base.WndProc(ref m);
		}
	}
}