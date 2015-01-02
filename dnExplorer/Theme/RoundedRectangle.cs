using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace dnExplorer.Theme {
	[Flags]
	internal enum RoundedCorner {
		TopLeft = 1,
		TopRight = 2,
		BottomLeft = 4,
		BottomRight = 8,
		All = TopLeft | TopRight | BottomLeft | BottomRight
	}

	[Flags]
	internal enum RoundedEdge {
		Left = 1,
		Right = 2,
		Top = 4,
		Bottom = 8,
		All = Left | Right | Top | Bottom
	}

	internal static class RoundedRectangle {
		static GraphicsPath Round(GraphicsPath path) {
			return
				new GraphicsPath(
					path.PathPoints.Select(pt => new PointF((float)Math.Round(pt.X), (float)Math.Round(pt.Y))).ToArray(),
					path.PathTypes, path.FillMode);
		}

		public static GraphicsPath Construct(Rectangle bounds, int radius, RoundedCorner corners,
			RoundedEdge edges = RoundedEdge.All) {
			var path = new GraphicsPath();

			if ((corners & RoundedCorner.TopLeft) != 0)
				path.AddArc(bounds.Left, bounds.Top, radius, radius, 180, 90);

			if ((edges & RoundedEdge.Top) != 0) {
				int left = (corners & RoundedCorner.TopLeft) != 0 ? bounds.Left + radius : bounds.Left;
				int right = (corners & RoundedCorner.TopRight) != 0 ? bounds.Right - radius : bounds.Right;
				path.AddLine(left, bounds.Top, right, bounds.Top);
			}

			if ((corners & RoundedCorner.TopRight) != 0)
				path.AddArc(bounds.Right - radius, bounds.Top, radius, radius, 270, 90);

			if ((edges & RoundedEdge.Right) != 0) {
				int top = (corners & RoundedCorner.TopRight) != 0 ? bounds.Top + radius : bounds.Top;
				int bottom = (corners & RoundedCorner.BottomRight) != 0 ? bounds.Bottom - radius : bounds.Bottom;
				path.AddLine(bounds.Right, top, bounds.Right, bottom);
			}

			if ((corners & RoundedCorner.BottomRight) != 0)
				path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);

			if ((edges & RoundedEdge.Bottom) != 0) {
				int left = (corners & RoundedCorner.BottomLeft) != 0 ? bounds.Left + radius : bounds.Left;
				int right = (corners & RoundedCorner.BottomRight) != 0 ? bounds.Right - radius : bounds.Right;
				path.AddLine(right, bounds.Bottom, left, bounds.Bottom);
			}

			if ((corners & RoundedCorner.BottomLeft) != 0)
				path.AddArc(bounds.Left, bounds.Bottom - radius, radius, radius, 90, 90);

			if ((edges & RoundedEdge.Left) != 0) {
				int top = (corners & RoundedCorner.TopLeft) != 0 ? bounds.Top + radius : bounds.Top;
				int bottom = (corners & RoundedCorner.BottomLeft) != 0 ? bounds.Bottom - radius : bounds.Bottom;
				path.AddLine(bounds.Left, bottom, bounds.Left, top);
			}

			path.CloseFigure();
			return Round(path);
		}
	}
}