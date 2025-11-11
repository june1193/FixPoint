using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OverlayApp.Models;

namespace OverlayApp
{
	public sealed class SettingsForm : Form
	{
		private readonly OverlaySettings _settings;

		private ComboBox _edgeShape = null!;
		private ComboBox _centerShape = null!;
		private ComboBox _edgeColorCmb = null!;
		private TrackBar _edgeThickness = null!;
		private TrackBar _edgeOpacity = null!;
		private ComboBox _centerColorCmb = null!;
		private TrackBar _centerThickness = null!;
		private TrackBar _centerOpacity = null!;
		private CheckBox _showCorners = null!;
		private ComboBox _cornerShape = null!;
		private ComboBox _cornerColorCmb = null!;
		private TrackBar _cornerThickness = null!;
		private TrackBar _cornerOpacity = null!;
		private ComboBox _hotkey = null!;
		private CheckBox _showCenter = null!;
		private CheckBox _showEdges = null!;
		private Button _reset = null!;
		private Button _apply = null!;

		public event EventHandler? ApplyRequested;

		private Label _centerThicknessVal = null!;
		private Label _centerOpacityVal = null!;
		private Label _edgeThicknessVal = null!;
		private Label _edgeOpacityVal = null!;
		private Label _cornerThicknessVal = null!;
		private Label _cornerOpacityVal = null!;

		public SettingsForm(OverlaySettings settings)
		{
			_settings = settings;
			InitializeComponent();
			LoadFromSettings();
		}

		private void InitializeComponent()
		{
			Text = "FixPoint(3D 멀미 방지 오버레이)";
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			StartPosition = FormStartPosition.CenterScreen;
			ClientSize = new Size(560, 720);

			// Top section: Center settings
			var lblCenterHeader = new Label { Text = "중앙 도형 설정", AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold), Location = new Point(16, 16) };
			_showCenter = new CheckBox { Text = "중앙 도형 표시", AutoSize = true, Location = new Point(420, 14) };
			var lblCenterShape = new Label { Text = "중앙 모양", AutoSize = true, Location = new Point(16, 50) };
			_centerShape = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 46), Width = 160 };
			_centerShape.Items.AddRange(Enum.GetNames(typeof(CenterShape)));
			var lblCenterColor = new Label { Text = "중앙 색상", AutoSize = true, Location = new Point(16, 88) };
			_centerColorCmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 84), Width = 160 };
			PopulateColorCombo(_centerColorCmb);
			var lblCenterThickness = new Label { Text = "중앙 크기", AutoSize = true, Location = new Point(16, 126) };
			_centerThickness = new TrackBar { Minimum = 1, Maximum = 30, TickFrequency = 1, Location = new Point(120, 120), Width = 380 };
			_centerThicknessVal = new Label { AutoSize = true, Location = new Point(508, 126) };
			var lblCenterOpacity = new Label { Text = "중앙 불투명도(%)", AutoSize = true, Location = new Point(16, 166) };
			_centerOpacity = new TrackBar { Minimum = 10, Maximum = 100, TickFrequency = 10, Location = new Point(160, 160), Width = 340 };
			_centerOpacityVal = new Label { AutoSize = true, Location = new Point(508, 166) };

			// Separator line
			var sep = new Label { BorderStyle = BorderStyle.Fixed3D, AutoSize = false, Location = new Point(16, 204), Width = 528, Height = 2 };

			// Bottom section: Edge settings
			var lblEdgeHeader = new Label { Text = "상하좌우 도형 설정", AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold), Location = new Point(16, 220) };
			_showEdges = new CheckBox { Text = "상하좌우 도형 표시", AutoSize = true, Location = new Point(420, 218) };
			var lblEdgeShape = new Label { Text = "상하좌우 모양", AutoSize = true, Location = new Point(16, 254) };
			_edgeShape = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 250), Width = 160 };
			_edgeShape.Items.AddRange(Enum.GetNames(typeof(EdgeShape)));
			var lblEdgeColor = new Label { Text = "상하좌우 색상", AutoSize = true, Location = new Point(16, 292) };
			_edgeColorCmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 288), Width = 160 };
			PopulateColorCombo(_edgeColorCmb);
			var lblEdgeThickness = new Label { Text = "상하좌우 크기", AutoSize = true, Location = new Point(16, 330) };
			_edgeThickness = new TrackBar { Minimum = 1, Maximum = 30, TickFrequency = 1, Location = new Point(120, 324), Width = 380 };
			_edgeThicknessVal = new Label { AutoSize = true, Location = new Point(508, 330) };
			var lblEdgeOpacity = new Label { Text = "상하좌우 불투명도(%)", AutoSize = true, Location = new Point(16, 370) };
			_edgeOpacity = new TrackBar { Minimum = 10, Maximum = 100, TickFrequency = 10, Location = new Point(160, 364), Width = 340 };
			_edgeOpacityVal = new Label { AutoSize = true, Location = new Point(508, 370) };

			// Second separator
			var sep2 = new Label { BorderStyle = BorderStyle.Fixed3D, AutoSize = false, Location = new Point(16, 408), Width = 528, Height = 2 };

			// Corner section
			var lblCornerHeader = new Label { Text = "꼭짓점 도형 설정", AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold), Location = new Point(16, 424) };
			_showCorners = new CheckBox { Text = "꼭짓점 도형 표시", AutoSize = true, Location = new Point(420, 422) };
			var lblCornerShape = new Label { Text = "꼭짓점 모양", AutoSize = true, Location = new Point(16, 458) };
			_cornerShape = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 454), Width = 160 };
			_cornerShape.Items.AddRange(new object[] { "Square" });
			_cornerShape.SelectedIndex = 0;
			var lblCornerColor = new Label { Text = "꼭짓점 색상", AutoSize = true, Location = new Point(16, 496) };
			_cornerColorCmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 492), Width = 160 };
			PopulateColorCombo(_cornerColorCmb);
			var lblCornerThickness = new Label { Text = "꼭짓점 크기", AutoSize = true, Location = new Point(16, 534) };
			_cornerThickness = new TrackBar { Minimum = 1, Maximum = 30, TickFrequency = 1, Location = new Point(120, 528), Width = 380 };
			_cornerThicknessVal = new Label { AutoSize = true, Location = new Point(508, 534) };
			var lblCornerOpacity = new Label { Text = "꼭짓점 불투명도(%)", AutoSize = true, Location = new Point(16, 574) };
			_cornerOpacity = new TrackBar { Minimum = 10, Maximum = 100, TickFrequency = 10, Location = new Point(160, 568), Width = 340 };
			_cornerOpacityVal = new Label { AutoSize = true, Location = new Point(508, 574) };

			// Hotkey and action buttons
			var lblHotkey = new Label { Text = "단축키", AutoSize = true, Location = new Point(16, 620) };
			_hotkey = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(120, 616), Width = 120 };
			PopulateHotkeyCombo(_hotkey);

			_apply = new Button { Text = "적용", Location = new Point(454, 678), Width = 90 };
			_reset = new Button { Text = "설정 초기화", Location = new Point(16, 678), Width = 100 };

			_apply.Click += (_, __) => { SaveToSettings(); ApplyRequested?.Invoke(this, EventArgs.Empty); };
			_reset.Click += (_, __) => ResetToDefaults();

			_centerThickness.ValueChanged += (_, __) => _centerThicknessVal.Text = $"{_centerThickness.Value}px";
			_edgeThickness.ValueChanged += (_, __) => _edgeThicknessVal.Text = $"{_edgeThickness.Value}px";
			_centerOpacity.ValueChanged += (_, __) => _centerOpacityVal.Text = $"{_centerOpacity.Value}%";
			_edgeOpacity.ValueChanged += (_, __) => _edgeOpacityVal.Text = $"{_edgeOpacity.Value}%";
			_cornerThickness.ValueChanged += (_, __) => _cornerThicknessVal.Text = $"{_cornerThickness.Value}px";
			_cornerOpacity.ValueChanged += (_, __) => _cornerOpacityVal.Text = $"{_cornerOpacity.Value}%";

			Controls.AddRange(new Control[]
			{
				lblCenterHeader, _showCenter,
				lblCenterShape, _centerShape,
				lblCenterColor, _centerColorCmb,
				lblCenterThickness, _centerThickness, _centerThicknessVal,
				lblCenterOpacity, _centerOpacity, _centerOpacityVal,
				sep,
				lblEdgeHeader, _showEdges,
				lblEdgeShape, _edgeShape,
				lblEdgeColor, _edgeColorCmb,
				lblEdgeThickness, _edgeThickness, _edgeThicknessVal,
				lblEdgeOpacity, _edgeOpacity, _edgeOpacityVal,
				sep2,
				lblCornerHeader, _showCorners,
				lblCornerShape, _cornerShape,
				lblCornerColor, _cornerColorCmb,
				lblCornerThickness, _cornerThickness, _cornerThicknessVal,
				lblCornerOpacity, _cornerOpacity, _cornerOpacityVal,
				lblHotkey, _hotkey,
				_apply, _reset
			});
		}

		private void LoadFromSettings()
		{
			_edgeShape.SelectedItem = _settings.EdgeShape.ToString();
			_centerShape.SelectedItem = _settings.CenterShape.ToString();
			if (_cornerShape.Items.Count > 0 && _cornerShape.SelectedIndex < 0) _cornerShape.SelectedIndex = 0;
			SetSelectedColor(_edgeColorCmb, _settings.EdgeColor);
			SetSelectedColor(_centerColorCmb, _settings.CenterColor);
			_edgeThickness.Value = Math.Clamp(_settings.EdgeThickness, _edgeThickness.Minimum, _edgeThickness.Maximum);
			_centerThickness.Value = Math.Clamp(_settings.CenterThickness, _centerThickness.Minimum, _centerThickness.Maximum);
			_edgeOpacity.Value = Math.Clamp(_settings.EdgeOpacityPercent, _edgeOpacity.Minimum, _edgeOpacity.Maximum);
			_centerOpacity.Value = Math.Clamp(_settings.CenterOpacityPercent, _centerOpacity.Minimum, _centerOpacity.Maximum);
			_centerThicknessVal.Text = $"{_centerThickness.Value}px";
			_edgeThicknessVal.Text = $"{_edgeThickness.Value}px";
			_centerOpacityVal.Text = $"{_centerOpacity.Value}%";
			_edgeOpacityVal.Text = $"{_edgeOpacity.Value}%";
			SetSelectedColor(_cornerColorCmb, _settings.CornerColor);
			_cornerThickness.Value = Math.Clamp(_settings.CornerThickness, _cornerThickness.Minimum, _cornerThickness.Maximum);
			_cornerOpacity.Value = Math.Clamp(_settings.CornerOpacityPercent, _cornerOpacity.Minimum, _cornerOpacity.Maximum);
			_cornerThicknessVal.Text = $"{_cornerThickness.Value}px";
			_cornerOpacityVal.Text = $"{_cornerOpacity.Value}%";
			SetSelectedHotkey(_hotkey, _settings.Hotkey);
			_showCenter.Checked = _settings.ShowCenter;
			_showEdges.Checked = _settings.ShowEdges;
			_showCorners.Checked = _settings.ShowCorners;
		}

		private void SaveToSettings()
		{
			if (_edgeShape.SelectedItem is string es && Enum.TryParse<EdgeShape>(es, out var edge))
				_settings.EdgeShape = edge;
			if (_centerShape.SelectedItem is string cs && Enum.TryParse<CenterShape>(cs, out var center))
				_settings.CenterShape = center;
			_settings.EdgeColor = GetSelectedColor(_edgeColorCmb);
			_settings.CenterColor = GetSelectedColor(_centerColorCmb);
			_settings.CornerColor = GetSelectedColor(_cornerColorCmb);
			_settings.EdgeThickness = _edgeThickness.Value;
			_settings.CenterThickness = _centerThickness.Value;
			_settings.CornerThickness = _cornerThickness.Value;
			_settings.EdgeOpacityPercent = _edgeOpacity.Value;
			_settings.CenterOpacityPercent = _centerOpacity.Value;
			_settings.CornerOpacityPercent = _cornerOpacity.Value;
			_settings.Hotkey = GetSelectedHotkey(_hotkey);
			_settings.ShowCenter = _showCenter.Checked;
			_settings.ShowEdges = _showEdges.Checked;
			_settings.ShowCorners = _showCorners.Checked;
		}

		private void ResetToDefaults()
		{
			var d = OverlaySettings.CreateDefaults();
			_edgeShape.SelectedItem = d.EdgeShape.ToString();
			_centerShape.SelectedItem = d.CenterShape.ToString();
			if (_cornerShape.Items.Count > 0) _cornerShape.SelectedIndex = 0;
			SetSelectedColor(_edgeColorCmb, d.EdgeColor);
			SetSelectedColor(_centerColorCmb, d.CenterColor);
			SetSelectedColor(_cornerColorCmb, d.CornerColor);
			_edgeThickness.Value = Math.Clamp(d.EdgeThickness, _edgeThickness.Minimum, _edgeThickness.Maximum);
			_centerThickness.Value = Math.Clamp(d.CenterThickness, _centerThickness.Minimum, _centerThickness.Maximum);
			_cornerThickness.Value = Math.Clamp(d.CornerThickness, _cornerThickness.Minimum, _cornerThickness.Maximum);
			_edgeOpacity.Value = Math.Clamp(d.EdgeOpacityPercent, _edgeOpacity.Minimum, _edgeOpacity.Maximum);
			_centerOpacity.Value = Math.Clamp(d.CenterOpacityPercent, _centerOpacity.Minimum, _centerOpacity.Maximum);
			_cornerOpacity.Value = Math.Clamp(d.CornerOpacityPercent, _cornerOpacity.Minimum, _cornerOpacity.Maximum);
			SetSelectedHotkey(_hotkey, d.Hotkey);
			_showCenter.Checked = d.ShowCenter;
			_showEdges.Checked = d.ShowEdges;
			_showCorners.Checked = d.ShowCorners;
			_centerThicknessVal.Text = $"{_centerThickness.Value}px";
			_edgeThicknessVal.Text = $"{_edgeThickness.Value}px";
			_centerOpacityVal.Text = $"{_centerOpacity.Value}%";
			_edgeOpacityVal.Text = $"{_edgeOpacity.Value}%";
			_cornerThicknessVal.Text = $"{_cornerThickness.Value}px";
			_cornerOpacityVal.Text = $"{_cornerOpacity.Value}%";
		}

		private sealed class ColorOption
		{
			public string Name { get; }
			public Color Color { get; }
			public ColorOption(string name, Color color) { Name = name; Color = color; }
			public override string ToString() { return Name; }
		}
		private sealed class KeyOption
		{
			public string Name { get; }
			public Keys Key { get; }
			public KeyOption(string name, Keys key) { Name = name; Key = key; }
			public override string ToString() { return Name; }
		}

		private void PopulateColorCombo(ComboBox combo)
		{
			combo.Items.Clear();
			combo.Items.Add(new ColorOption("빨강", Color.Red));
			combo.Items.Add(new ColorOption("파랑", Color.Blue));
			combo.Items.Add(new ColorOption("초록", Color.Green));
			combo.Items.Add(new ColorOption("노랑", Color.Yellow));
			combo.Items.Add(new ColorOption("검정", Color.Black));
			combo.Items.Add(new ColorOption("흰색", Color.White));
		}

		private void SetSelectedColor(ComboBox combo, Color color)
		{
			for (int i = 0; i < combo.Items.Count; i++)
			{
				if (combo.Items[i] is ColorOption opt && opt.Color.ToArgb() == color.ToArgb())
				{
					combo.SelectedIndex = i;
					return;
				}
			}
			combo.SelectedIndex = 3; // default Yellow
		}

		private Color GetSelectedColor(ComboBox combo)
		{
			return combo.SelectedItem is ColorOption opt ? opt.Color : Color.Yellow;
		}

		private void PopulateHotkeyCombo(ComboBox combo)
		{
			combo.Items.Clear();
			combo.Items.Add(new KeyOption("없음", Keys.None));
			combo.Items.Add(new KeyOption("F1", Keys.F1));
			combo.Items.Add(new KeyOption("F2", Keys.F2));
			combo.Items.Add(new KeyOption("F3", Keys.F3));
			combo.Items.Add(new KeyOption("F4", Keys.F4));
			combo.Items.Add(new KeyOption("F5", Keys.F5));
			combo.Items.Add(new KeyOption("F6", Keys.F6));
			combo.Items.Add(new KeyOption("F7", Keys.F7));
			combo.Items.Add(new KeyOption("F8", Keys.F8));
			combo.Items.Add(new KeyOption("F9", Keys.F9));
			combo.Items.Add(new KeyOption("F10", Keys.F10));
			combo.Items.Add(new KeyOption("F11", Keys.F11));
			combo.Items.Add(new KeyOption("F12", Keys.F12));
		}

		private void SetSelectedHotkey(ComboBox combo, Keys key)
		{
			for (int i = 0; i < combo.Items.Count; i++)
			{
				if (combo.Items[i] is KeyOption opt && opt.Key == key)
				{
					combo.SelectedIndex = i;
					return;
				}
			}
			combo.SelectedIndex = 0; // 없음
		}

		private Keys GetSelectedHotkey(ComboBox combo)
		{
			return combo.SelectedItem is KeyOption opt ? opt.Key : Keys.None;
		}
	}
}


