using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OverlayApp.Models
{
	public enum EdgeShape
	{
		Rectangle,
		Triangle,
		Semicircle
	}

	public enum CenterShape
	{
		Square,
		Crosshair,
		Circle
	}

	public sealed class OverlaySettings
	{
		public EdgeShape EdgeShape { get; set; } = EdgeShape.Rectangle;
		public CenterShape CenterShape { get; set; } = CenterShape.Square;
		public string EdgeColorHtml { get; set; } = ColorTranslator.ToHtml(Color.Yellow);
		public string CenterColorHtml { get; set; } = ColorTranslator.ToHtml(Color.Yellow);
		public string CornerColorHtml { get; set; } = ColorTranslator.ToHtml(Color.Yellow);
		public int EdgeThickness { get; set; } = 6;
		public int CenterThickness { get; set; } = 4;
		public int CornerThickness { get; set; } = 6;
		public int EdgeOpacityPercent { get; set; } = 90; // 10-100
		public int CenterOpacityPercent { get; set; } = 90; // 10-100
		public int CornerOpacityPercent { get; set; } = 90; // 10-100
		public Keys Hotkey { get; set; } = Keys.None;
		public bool ShowCenter { get; set; } = true;
		public bool ShowEdges { get; set; } = true;
		public bool ShowCorners { get; set; } = false;

		[JsonIgnore]
		public Color EdgeColor
		{
			get => ColorTranslator.FromHtml(EdgeColorHtml);
			set => EdgeColorHtml = ColorTranslator.ToHtml(value);
		}

		[JsonIgnore]
		public Color CenterColor
		{
			get => ColorTranslator.FromHtml(CenterColorHtml);
			set => CenterColorHtml = ColorTranslator.ToHtml(value);
		}

		[JsonIgnore]
		public Color CornerColor
		{
			get => ColorTranslator.FromHtml(CornerColorHtml);
			set => CornerColorHtml = ColorTranslator.ToHtml(value);
		}

		public static string GetConfigPath()
		{
			var dir = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"MotionOverlay");
			Directory.CreateDirectory(dir);
			return Path.Combine(dir, "settings.json");
		}

		public static OverlaySettings CreateDefaults()
		{
			return new OverlaySettings
			{
				EdgeShape = EdgeShape.Rectangle,
				CenterShape = CenterShape.Square,
				EdgeColorHtml = ColorTranslator.ToHtml(Color.Yellow),
				CenterColorHtml = ColorTranslator.ToHtml(Color.Yellow),
				CornerColorHtml = ColorTranslator.ToHtml(Color.Yellow),
				EdgeThickness = 6,
				CenterThickness = 4,
				CornerThickness = 6,
				EdgeOpacityPercent = 90,
				CenterOpacityPercent = 90,
				CornerOpacityPercent = 90,
				Hotkey = Keys.None,
				ShowCenter = true,
				ShowEdges = true,
				ShowCorners = false
			};
		}

		public static OverlaySettings Load()
		{
			try
			{
				var path = GetConfigPath();
				if (!File.Exists(path))
					return CreateDefaults();
				var json = File.ReadAllText(path);
				var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				return JsonSerializer.Deserialize<OverlaySettings>(json, options) ?? CreateDefaults();
			}
			catch
			{
				return CreateDefaults();
			}
		}

		public void Save()
		{
			var path = GetConfigPath();
			var options = new JsonSerializerOptions { WriteIndented = true };
			File.WriteAllText(path, JsonSerializer.Serialize(this, options));
		}
	}
}


