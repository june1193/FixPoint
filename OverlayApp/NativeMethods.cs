using System;
using System.Runtime.InteropServices;

namespace OverlayApp
{
	public static class NativeMethods
	{
		public const int WS_EX_LAYERED = 0x00080000;
		public const int WS_EX_TRANSPARENT = 0x00000020;
		public const int WS_EX_TOOLWINDOW = 0x00000080;

		public const int GWL_EXSTYLE = -20;

		public const int WM_HOTKEY = 0x0312;

		[Flags]
		public enum Modifiers
		{
			None = 0x0000,
			Alt = 0x0001,
			Ctrl = 0x0002,
			Shift = 0x0004,
			Win = 0x0008
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		public const int LWA_COLORKEY = 0x00000001;
		public const int LWA_ALPHA = 0x00000002;

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool DestroyIcon(IntPtr hIcon);
	}
}


