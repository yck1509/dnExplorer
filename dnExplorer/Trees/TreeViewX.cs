﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace dnExplorer.Trees {
	public class TreeViewX : TreeView {
		Control scratch;

		public TreeViewX() {
			HotTracking = true;
			HideSelection = false;
			ShowLines = false;
			BorderStyle = BorderStyle.FixedSingle;
			DrawMode = TreeViewDrawMode.OwnerDrawAll;
			ImageList = new ImageList();
			ImageList.Images.Add(new Bitmap(16, 16));

			scratch = new Control();
			scratch.CreateControl();

			Font = new Font("Segoe UI", 9);
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
		}

		[DllImport("user32.dll")]
		static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		protected override void CreateHandle() {
			CreateHandle();
			PostMessage(Handle, 0x1100 + 44, (IntPtr)4, (IntPtr)4);
		}

		protected override void OnAfterCollapse(TreeViewEventArgs e) {
			var node = e.Node as TreeNodeX;
			if (node != null) {
				node.OnCollapse();
			}
			OnAfterCollapse(e);
		}

		protected override void OnBeforeExpand(TreeViewCancelEventArgs e) {
			var node = e.Node as TreeNodeX;
			if (node != null) {
				node.OnExpand();
			}
			OnBeforeExpand(e);
		}

		protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e) {
			var node = e.Node as TreeNodeX;
			if (node != null) {
				node.OnMouseClick(e);
			}
			OnNodeMouseClick(e);
		}

		protected override void OnNodeMouseDoubleClick(TreeNodeMouseClickEventArgs e) {
			var node = e.Node as TreeNodeX;
			if (node != null) {
				node.OnMouseDoubleClick(e);
			}
			OnNodeMouseDoubleClick(e);
		}

		protected override void OnNodeMouseHover(TreeNodeMouseHoverEventArgs e) {
			var node = e.Node as TreeNodeX;
			if (node != null) {
				node.OnMouseHover(e);
			}
			OnNodeMouseHover(e);
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			OnMouseDown(e);
			switch (e.Button) {
				case MouseButtons.Left:
				case MouseButtons.Right:
				case MouseButtons.Middle:
					var targetNode = GetNodeAt(e.X, e.Y);
					if (targetNode != null)
						SelectedNode = targetNode;
					break;
			}
		}

		protected override void OnDrawNode(DrawTreeNodeEventArgs e) {
			if (e.Bounds.IsEmpty)
				return;
			CustomDrawNode(e);
		}

		void CustomDrawNode(DrawTreeNodeEventArgs e) {
			using (var brush = new SolidBrush(BackColor))
				e.Graphics.FillRectangle(brush, e.Bounds);
			DrawToggle(e);
			DrawText(e);
		}

		void DrawToggle(DrawTreeNodeEventArgs e) {
			if (!ShowPlusMinus)
				return;

			var bounds = e.Node.Bounds;
			bounds.X = (e.Bounds.X + ((e.Node.Level + 1) * Indent)) - 15;
			bounds.Y += (bounds.Height - 8) / 2;
			bounds.Width = bounds.Height = 8;
			if (e.Node.Nodes.Count != 0) {
				e.Graphics.FillRectangle(new SolidBrush(BackColor),
					new Rectangle(bounds.X - 1, bounds.Y - 1, bounds.Width + 3, bounds.Height + 3));
				e.Graphics.DrawRectangle(Pens.DimGray, bounds);
				e.Graphics.DrawLine(Pens.Black, new Point(2 + bounds.X, 4 + bounds.Y), new Point(6 + bounds.X, 4 + bounds.Y));
				if (!e.Node.IsExpanded) {
					e.Graphics.DrawLine(Pens.Black, new Point(4 + bounds.X, 2 + bounds.Y), new Point(4 + bounds.X, 6 + bounds.Y));
				}
			}
		}

		void DrawText(DrawTreeNodeEventArgs e) {
			var node = (TreeNodeX)e.Node;
			var state = e.State;
			var bounds = e.Bounds;
			bounds.Offset((e.Node.Level + 1) * Indent, 0);

			var hot = (state & TreeNodeStates.Hot) != 0;
			var sel = (state & TreeNodeStates.Selected) != 0;

			var font = node.NodeFont ?? node.TreeView.Font;
			Color foreColor;
			Brush backColor;
			if (sel) {
				foreColor = SystemColors.HighlightText;
			}
			else if (node.ForeColor.IsEmpty && hot) {
				foreColor = SystemColors.HotTrack;
			}
			else {
				foreColor = node.ForeColor;
			}

			if (sel) {
				backColor = SystemBrushes.Highlight;
			}
			else {
				backColor = new SolidBrush(e.Node.BackColor);
			}

			if (hot)
				font = new Font(font, FontStyle.Underline);

			if (node.HasIcon) {
				Rectangle iconBounds = new Rectangle(bounds.X + 2, bounds.Y + (bounds.Height - 16) / 2, 16, 16);
				e.Graphics.SetClip(iconBounds);
				node.DrawIcon(e.Graphics, iconBounds);
				e.Graphics.ResetClip();
				bounds.Offset(20, 0);
			}

			bounds.Width = TextRenderer.MeasureText(node.Text, font).Width;
			e.Graphics.FillRectangle(backColor, bounds);
			bounds.Offset(0, 1);
			try {
				TextRenderer.DrawText(e.Graphics, node.Text, font, bounds, foreColor, TextFormatFlags.Default);
			}
			finally {
				if (hot)
					font.Dispose();
			}
		}

		int updating = 0;
		bool updateRedraw = false;

		internal void EnterUpdate() {
			updating++;
			updateRedraw = true;
			Invalidate();
		}

		internal void LeaveUpdate() {
			updating--;
			updateRedraw = true;
			Invalidate();
		}

		protected override void WndProc(ref Message m) {
			const int WM_NOTIFY = 0x4e;
			const int TTN_GETDISPINFOA = -520;
			const int TTN_GETDISPINFOW = -530;

			if (m.Msg == WM_NOTIFY) {
				// Disable tooltips
				// code = NMHDR.code
				int code = Marshal.ReadInt32(m.LParam + IntPtr.Size * 2);
				if (code == TTN_GETDISPINFOA || code == TTN_GETDISPINFOW)
					return;
			}
			else if (m.Msg == 0x204e) {
				int code = Marshal.ReadInt32(m.LParam + IntPtr.Size * 2);
				if (code == -12) {
					if (updating > 0 && !updateRedraw)
						return;

					var nodeHnd = Marshal.ReadIntPtr(m.LParam + IntPtr.Size * 5 + 16);
					var node = TreeNode.FromHandle(this, nodeHnd);
					if (node != null && !node.IsVisible)
						return;
				}
			}

			if (m.Msg == 20)
				return;

			WndProc(ref m);
			if (m.Msg == 15 && updateRedraw)
				updateRedraw = false;
		}
	}
}