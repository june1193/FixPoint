using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using OverlayApp.Models;

namespace OverlayApp
{
		public sealed class TrayAppContext : ApplicationContext
	{
		private readonly OverlaySettings _settings;
		private readonly OverlayForm _edgesOverlay;
		private readonly OverlayForm _centerOverlay;
		private readonly OverlayForm _cornersOverlay;
		private readonly NotifyIcon _tray;
		private Icon? _appIcon;
		private IntPtr _iconHandle = IntPtr.Zero;
		private readonly HotkeyMessageWindow _msgWnd;
		private const int HotkeyId = 0x3001;

		public TrayAppContext()
		{
			_settings = OverlaySettings.Load();
			_edgesOverlay = new OverlayForm(_settings, OverlayLayer.Edges);
			_centerOverlay = new OverlayForm(_settings, OverlayLayer.Center);
			_cornersOverlay = new OverlayForm(_settings, OverlayLayer.Corners);
			// Start hidden; user applies from settings first

			_msgWnd = new HotkeyMessageWindow(this);
			var _ = _msgWnd.Handle; // ensure handle created
			_edgesOverlay.AddOwnedForm(_msgWnd);
			_centerOverlay.AddOwnedForm(_msgWnd);
			_cornersOverlay.AddOwnedForm(_msgWnd);

			_appIcon = CreateVerticalRectIcon(_settings.EdgeColor);
			_tray = new NotifyIcon
			{
				Icon = _appIcon,
				Text = "FixPoint",
				Visible = true,
				ContextMenuStrip = BuildContextMenu()
			};

			RegisterHotkey();

			// Show settings dialog initially (overlay appears only when '적용' is clicked)
			ShowSettingsDialog();

				Application.ApplicationExit += (_, __) => OnExitCleanup();
		}

		private ContextMenuStrip BuildContextMenu()
		{
			var menu = new ContextMenuStrip();
			var toggle = new ToolStripMenuItem("표시 / 숨김", null, (_, __) => ToggleOverlay());
			var settings = new ToolStripMenuItem("설정 열기", null, (_, __) => ShowSettingsDialog());
			var exit = new ToolStripMenuItem("프로그램 종료", null, (_, __) => ExitThread());
			menu.Items.Add(toggle);
			menu.Items.Add(settings);
			menu.Items.Add(new ToolStripSeparator());
			menu.Items.Add(exit);
			return menu;
		}

		private void ToggleOverlay()
		{
				if (_edgesOverlay.Visible || _centerOverlay.Visible)
				{
					_edgesOverlay.Hide();
					_centerOverlay.Hide();
					_cornersOverlay.Hide();
				}
			else
				{
					ApplyAndShowOverlays();
				}
		}

		private void ShowSettingsDialog()
		{
			using var dlg = new SettingsForm(_settings);
			dlg.ApplyRequested += (_, __) =>
			{
				_settings.Save();
				_edgesOverlay.ApplySettings();
				_centerOverlay.ApplySettings();
				_cornersOverlay.ApplySettings();
				ReRegisterHotkey();
				ApplyAndShowOverlays();
			};
			dlg.ShowDialog();
		}

		protected override void ExitThreadCore()
		{
			OnExitCleanup();
			base.ExitThreadCore();
		}

		private void OnExitCleanup()
		{
			try
			{
				_settings.Save();
			}
			catch { /* ignore */ }
			UnregisterHotkey();
			_tray.Visible = false;
			_edgesOverlay.Close();
			_centerOverlay.Close();
			_cornersOverlay.Close();
			_tray.Dispose();
			try { _appIcon?.Dispose(); } catch { }
			if (_iconHandle != IntPtr.Zero) { try { NativeMethods.DestroyIcon(_iconHandle); } catch { } }
			_edgesOverlay.Dispose();
			_centerOverlay.Dispose();
			_cornersOverlay.Dispose();
		}

		private Icon CreateVerticalRectIcon(Color color)
		{
			// Create a simple 32x32 icon with a tall vertical rectangle
			using var bmp = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(bmp))
			{
				g.Clear(Color.Transparent);
				using var brush = new SolidBrush(color);
				// centered vertical bar: width 10px, height 28px
				int barWidth = 10;
				int barHeight = 28;
				int x = (32 - barWidth) / 2;
				int y = (32 - barHeight) / 2;
				g.FillRectangle(brush, new Rectangle(x, y, barWidth, barHeight));
			}
			_iconHandle = bmp.GetHicon();
			return Icon.FromHandle(_iconHandle);
		}

		private void RegisterHotkey()
		{
			UnregisterHotkey();
			if (_settings.Hotkey != Keys.None)
			{
				NativeMethods.RegisterHotKey(
					_msgWnd.Handle,
					HotkeyId,
					0,
					(uint)_settings.Hotkey);
			}
		}

		private void ReRegisterHotkey()
		{
			RegisterHotkey();
		}

		private void UnregisterHotkey()
		{
			try { NativeMethods.UnregisterHotKey(_msgWnd.Handle, HotkeyId); } catch { /* ignore */ }
		}

		private sealed class HotkeyMessageWindow : Form
		{
			private readonly TrayAppContext _ctx;

			public HotkeyMessageWindow(TrayAppContext ctx)
			{
				_ctx = ctx;
				ShowInTaskbar = false;
				Opacity = 0;
				FormBorderStyle = FormBorderStyle.FixedToolWindow;
				Size = new Size(0, 0);
				StartPosition = FormStartPosition.Manual;
				Location = new Point(-2000, -2000);
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == NativeMethods.WM_HOTKEY)
				{
					_ctx.ToggleOverlay();
					return;
				}
				base.WndProc(ref m);
			}
		}

		private void ApplyAndShowOverlays()
		{
			if (_settings.ShowEdges)
			{
				_edgesOverlay.ApplySettings();
				_edgesOverlay.Show();
			}
			else
			{
				_edgesOverlay.Hide();
			}

			if (_settings.ShowCenter)
			{
				_centerOverlay.ApplySettings();
				_centerOverlay.Show();
			}
			else
			{
				_centerOverlay.Hide();
			}

			if (_settings.ShowCorners)
			{
				_cornersOverlay.ApplySettings();
				_cornersOverlay.Show();
			}
			else
			{
				_cornersOverlay.Hide();
			}
		}
	}
}


