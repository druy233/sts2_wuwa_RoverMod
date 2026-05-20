using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using Rover.Relics;
using Rover.Tools; // 请根据你实际的命名空间调整

namespace Rover.RoverCode.Tools;

public partial class RoverAbilitySwitchButton : Control
{
	private TextureRect? _icon;
	private RoverAbilitySwitchPanel? _panel = null;
	private Color _originalModulate = Colors.White;

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("icon");
		if (_icon != null)
			_originalModulate = _icon.Modulate;

		// 连接鼠标事件
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		GuiInput += OnGuiInput;
	}

	private void OnMouseEntered()
	{
		// 简单高亮：提高图标亮度（或改变颜色）
		if (_icon != null)
			_icon.Modulate = Colors.LightYellow;
	}

	private void OnMouseExited()
	{
		// 恢复图标颜色
		if (_icon != null)
			_icon.Modulate = _originalModulate;
	}

	private void OnGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
		{
			// 播放点击音效
			RoverAudioHelper.PlayOneShot("res://debug_audio/rover_ui_click.wav");
			OpenPanel();
		}
	}

	private async void OpenPanel()
	{
		if (CombatManager.Instance.IsInProgress)
		{
			ShowFloatingText("ABILITY_SWITCH_NOT_IN_COMBAT");
			return;
		}
		// 如果已有面板，先销毁
		if (_panel != null && IsInstanceValid(_panel))
		{
			_panel.QueueFree();
			_panel = null;
		}

		var relic = GetRelic();
		if (relic == null)
		{
			GD.Print("未找到晦明终端遗物");
			return;
		}

		var panelScene = PreloadManager.Cache.GetScene("res://scenes/ui/rover_ability_switch_panel.tscn");
		if (panelScene == null)
		{
			GD.PrintErr("无法加载能力切换面板场景");
			return;
		}

		_panel = panelScene.Instantiate<RoverAbilitySwitchPanel>();
		if (_panel == null) return;

		// 添加到地图屏幕
		NMapScreen.Instance?.AddChild(_panel);
		_panel.ShowPanel(relic);
	}

	private static ObscuraLumis GetRelic()
	{
		var me = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState()?.Players);
		return me?.GetRelic<ObscuraLumis>();
	}

	private void ShowFloatingText(string locKey)
	{
		var locString = new LocString("relics", locKey);
		string message = locString.GetFormattedText();
		// 获取合适的容器（优先地图屏幕，否则全局 UI）
		Control container = null;
		if (NMapScreen.Instance != null && NMapScreen.Instance.Visible)
			container = NMapScreen.Instance;
		else if (NRun.Instance?.GlobalUi != null)
			container = NRun.Instance.GlobalUi;

		if (container == null) return;

		var label = new Label();
		label.Text = message;
		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.VerticalAlignment = VerticalAlignment.Center;
		label.Modulate = Colors.Gold;
		label.AddThemeFontSizeOverride("font_size", 95);
		label.AddThemeConstantOverride("outline_size", 5);          // 添加描边
		label.AddThemeColorOverride("font_outline_color", Colors.Black); // 描边颜色黑色

		// 获取当前视口大小（确保坐标正确）
		var viewportSize = GetViewport().GetVisibleRect().Size;
		label.Position = new Vector2(viewportSize.X / 2 - 450, viewportSize.Y / 2 - 300);
		label.Size = new Vector2(300, 40);

		container.AddChild(label);

		// 淡出动画并自动销毁
		var tween = label.CreateTween();
		tween.TweenProperty(label, "modulate:a", 0f, 0.8f).SetDelay(0.65f);
		tween.TweenCallback(Callable.From(() => label.QueueFree()));
	}
}
