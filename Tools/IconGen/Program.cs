using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace IconGen;

class Program
{
	static int Main(string[] args)
	{
		try
		{
			if (args.Length < 1)
			{
				Console.Error.WriteLine("Usage: IconGen <outputIcoPath>");
				return 2;
			}
			var outPath = args[0];
			var dir = Path.GetDirectoryName(outPath);
			if (!string.IsNullOrEmpty(dir))
			{
				Directory.CreateDirectory(dir);
			}

			using var bmp = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(bmp))
			{
				g.Clear(Color.Transparent);
				using var brush = new SolidBrush(Color.Yellow);
				int barWidth = 10;
				int barHeight = 28;
				int x = (32 - barWidth) / 2;
				int y = (32 - barHeight) / 2;
				g.FillRectangle(brush, new Rectangle(x, y, barWidth, barHeight));
				// small border for clarity
				using var pen = new Pen(Color.FromArgb(200, 0, 0, 0), 1);
				g.DrawRectangle(pen, new Rectangle(x, y, barWidth - 1, barHeight - 1));
			}

			var hIcon = bmp.GetHicon();
			try
			{
				using var icon = Icon.FromHandle(hIcon);
				using var fs = File.Create(outPath);
				icon.Save(fs);
			}
			finally
			{
				// Free native HICON
				NativeMethods.DestroyIcon(hIcon);
			}

			Console.WriteLine($"Icon generated: {Path.GetFullPath(outPath)}");
			return 0;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex);
			return 1;
		}
	}

	private static class NativeMethods
	{
		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
		public static extern bool DestroyIcon(IntPtr hIcon);
	}
}


