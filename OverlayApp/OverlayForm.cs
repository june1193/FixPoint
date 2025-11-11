using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using OverlayApp.Models;

namespace OverlayApp
{
	public enum OverlayLayer
	{
		Edges,
		Center,
		Corners
	}

	public sealed class OverlayForm : Form
	{
		private readonly OverlaySettings _settings;
		private readonly Color _transparentKey = Color.Lime; // fully transparent
		private readonly OverlayLayer _layer;

		public OverlayForm(OverlaySettings settings, OverlayLayer layer)
		{
			_settings = settings;
			_layer = layer;

			FormBorderStyle = FormBorderStyle.None;
			TopMost = true;
			ShowInTaskbar = false;
			BackColor = _transparentKey;
			TransparencyKey = _transparentKey;
			Opacity = 1.0; // set properly in ApplySettings
			DoubleBuffered = true;
			StartPosition = FormStartPosition.Manual;
			Bounds = Screen.PrimaryScreen?.Bounds ?? SystemInformation.VirtualScreen; // default: primary monitor only
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			// Extended styles: layered + transparent + toolwindow (no Alt-Tab)
			var ex = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
			ex |= NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW;
			NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, ex);

			// Apply only color key; per-window opacity will be handled by Form.Opacity
			var colorKey = (uint)ColorTranslator.ToWin32(_transparentKey);
			NativeMethods.SetLayeredWindowAttributes(Handle, colorKey, 0, NativeMethods.LWA_COLORKEY);
		}

		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.ExStyle |= NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_TOOLWINDOW;
				return cp;
			}
		}

		public void ApplySettings()
		{
			if (_layer == OverlayLayer.Edges)
				Opacity = Math.Clamp(_settings.EdgeOpacityPercent / 100.0, 0.1, 1.0);
			else if (_layer == OverlayLayer.Center)
				Opacity = Math.Clamp(_settings.CenterOpacityPercent / 100.0, 0.1, 1.0);
			else
				Opacity = Math.Clamp(_settings.CornerOpacityPercent / 100.0, 0.1, 1.0);
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.SmoothingMode = SmoothingMode.None;
			DrawOverlay(e.Graphics);
		}

		private void DrawOverlay(Graphics g)
		{
			var edgeColor = _settings.EdgeColor;
			var centerColor = _settings.CenterColor;
			var edgeThickness = Math.Max(1, _settings.EdgeThickness);
			var centerThickness = Math.Max(1, _settings.CenterThickness);
			var cornerThickness = Math.Max(1, _settings.CornerThickness);
			
			var center = new Point(ClientRectangle.Left + ClientRectangle.Width / 2,
				ClientRectangle.Top + ClientRectangle.Height / 2);

			if (_layer == OverlayLayer.Edges && _settings.ShowEdges)
			{
				if (_settings.EdgeShape == EdgeShape.Rectangle)
					DrawEdgeRectangles(g, edgeColor, edgeThickness);
				else if (_settings.EdgeShape == EdgeShape.Triangle)
					DrawEdgeTriangles(g, edgeColor, edgeThickness);
				else if (_settings.EdgeShape == EdgeShape.Semicircle)
					DrawEdgeSemicircles(g, edgeColor, edgeThickness);
			}
			if (_layer == OverlayLayer.Center && _settings.ShowCenter)
			{
				switch (_settings.CenterShape)
				{
					case CenterShape.Square:
						DrawCenterSquare(g, centerColor, centerThickness, center);
						break;
					case CenterShape.Crosshair:
						using (var pen = new Pen(centerColor, centerThickness) { Alignment = PenAlignment.Center })
						{
							var span = Math.Max(10, centerThickness * 6);
							DrawCrosshair(g, pen, center, span);
						}
						break;
					case CenterShape.Circle:
						var diameter = Math.Max(10, centerThickness * 5);
						DrawCenterCircle(g, centerColor, diameter, center);
						break;
				}
			}

			if (_layer == OverlayLayer.Corners && _settings.ShowCorners)
			{
				DrawCornerSquares(g, _settings.CornerColor, cornerThickness);
			}
		}

		private static void DrawCrosshair(Graphics g, Pen pen, Point center, int size)
		{
			var half = size / 2;
			g.DrawLine(pen, center.X - half, center.Y, center.X + half, center.Y);
			g.DrawLine(pen, center.X, center.Y - half, center.X, center.Y + half);
		}

		// Removed unused generic shape outline helpers to keep codebase minimal

		private void DrawSideTabs(Graphics g, Color color, int thickness)
		{
			// Draw four filled rectangles at screen midpoints (top/bottom/left/right),
			// like small "post-it" tabs flush to the very edge.
			int longLen = Math.Max(10, thickness * 6);  // length along normal axis
			int shortLen = Math.Max(8, thickness * 4); // length along tangent axis

			using var brush = new SolidBrush(color);

			// Top tab (vertical)
			var topX = Width / 2 - shortLen / 2;
			var topY = 0;
			g.FillRectangle(brush, new Rectangle(topX, topY, shortLen, longLen));

			// Bottom tab (vertical)
			var botX = Width / 2 - shortLen / 2;
			var botY = Height - longLen;
			g.FillRectangle(brush, new Rectangle(botX, botY, shortLen, longLen));

			// Left tab (horizontal)
			var leftX = 0;
			var leftY = Height / 2 - shortLen / 2;
			g.FillRectangle(brush, new Rectangle(leftX, leftY, longLen, shortLen));

			// Right tab (horizontal)
			var rightX = Width - longLen;
			var rightY = Height / 2 - shortLen / 2;
			g.FillRectangle(brush, new Rectangle(rightX, rightY, longLen, shortLen));
		}

		private void DrawEdgeRectangles(Graphics g, Color color, int thickness)
		{
			DrawSideTabs(g, color, thickness);
		}

		private void DrawEdgeTriangles(Graphics g, Color color, int thickness)
		{
			int baseLen = Math.Max(10, thickness * 6);   // base width
			int height = Math.Max(10, thickness * 8);    // height toward center
			using var brush = new SolidBrush(color);

			// Top (pointing down)
			var topMid = new Point(Width / 2, height);
			var topP1 = new Point(topMid.X - baseLen / 2, 0);
			var topP2 = new Point(topMid.X + baseLen / 2, 0);
			var topP3 = new Point(topMid.X, height);
			g.FillPolygon(brush, new[] { topP1, topP2, topP3 });

			// Bottom (pointing up)
			var botMidY = Height - height - 1;
			var botP1 = new Point(Width / 2 - baseLen / 2, Height - 1);
			var botP2 = new Point(Width / 2 + baseLen / 2, Height - 1);
			var botP3 = new Point(Width / 2, botMidY);
			g.FillPolygon(brush, new[] { botP1, botP2, botP3 });

			// Left (pointing right)
			var leftMidX = height;
			var leftP1 = new Point(0, Height / 2 - baseLen / 2);
			var leftP2 = new Point(0, Height / 2 + baseLen / 2);
			var leftP3 = new Point(leftMidX, Height / 2);
			g.FillPolygon(brush, new[] { leftP1, leftP2, leftP3 });

			// Right (pointing left)
			var rightMidX = Width - height - 1;
			var rightP1 = new Point(Width - 1, Height / 2 - baseLen / 2);
			var rightP2 = new Point(Width - 1, Height / 2 + baseLen / 2);
			var rightP3 = new Point(rightMidX, Height / 2);
			g.FillPolygon(brush, new[] { rightP1, rightP2, rightP3 });
		}

		private void DrawEdgeSemicircles(Graphics g, Color color, int thickness)
		{
			int radius = Math.Max(8, thickness * 5);
			int diameter = radius * 2;
			using var brush = new SolidBrush(color);

			// Draw full circles whose centers lie exactly on each edge.
			// The window clipping will keep only the inner halves, guaranteeing flush edges with no gap.
			// Top (center on top edge, bulging downward)
			var topRect = new Rectangle(Width / 2 - radius, -radius, diameter, diameter);
			g.FillEllipse(brush, topRect);

			// Bottom (center on bottom edge, bulging upward)
			var bottomRect = new Rectangle(Width / 2 - radius, Height - radius, diameter, diameter);
			g.FillEllipse(brush, bottomRect);

			// Left (center on left edge, bulging right)
			var leftRect = new Rectangle(-radius, Height / 2 - radius, diameter, diameter);
			g.FillEllipse(brush, leftRect);

			// Right (center on right edge, bulging left)
			var rightRect = new Rectangle(Width - radius, Height / 2 - radius, diameter, diameter);
			g.FillEllipse(brush, rightRect);
		}

		private void DrawCenterSquare(Graphics g, Color color, int thickness, Point center)
		{
			int side = Math.Max(10, thickness * 5);
			using var brush = new SolidBrush(color);
			var rect = new Rectangle(center.X - side / 2, center.Y - side / 2, side, side);
			g.FillRectangle(brush, rect);
		}

		private void DrawCenterTriangle(Graphics g, Color color, int thickness, Point center)
		{
			int baseLen = Math.Max(10, thickness * 6);
			int height = Math.Max(10, thickness * 7);
			using var brush = new SolidBrush(color);
			var p1 = new Point(center.X - baseLen / 2, center.Y + height / 2);
			var p2 = new Point(center.X + baseLen / 2, center.Y + height / 2);
			var p3 = new Point(center.X, center.Y - height / 2);
			g.FillPolygon(brush, new[] { p1, p2, p3 });
		}

		private void DrawCenterCircle(Graphics g, Color color, int diameter, Point center)
		{
			using var brush = new SolidBrush(color);
			var rect = new Rectangle(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
			g.FillEllipse(brush, rect);
		}

		private void DrawCornerSquares(Graphics g, Color color, int thickness)
		{
			int side = Math.Max(10, thickness * 6);
			using var brush = new SolidBrush(color);

			// Top-left
			g.FillRectangle(brush, new Rectangle(0, 0, side, side));
			// Top-right
			g.FillRectangle(brush, new Rectangle(Width - side, 0, side, side));
			// Bottom-left
			g.FillRectangle(brush, new Rectangle(0, Height - side, side, side));
			// Bottom-right
			g.FillRectangle(brush, new Rectangle(Width - side, Height - side, side, side));
		}
	}
}


