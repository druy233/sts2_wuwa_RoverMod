using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using Rover.Relics;
using Rover.Tools;

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

		// 添加到全局 UI（确保在任何场景都能显示）
		var container = NRun.Instance?.GlobalUi;
		if (container != null)
			container.AddChild(_panel);
		else
			GetTree().Root.AddChild(_panel); // 后备方案

		_panel.ShowPanel(relic);
	}

	private static ObscuraLumis GetRelic()
	{
		var me = LocalContext.GetMe(RunManager.Instance.DebugOnlyGetState()?.Players);
		return me?.GetRelic<ObscuraLumis>();
	}
}
