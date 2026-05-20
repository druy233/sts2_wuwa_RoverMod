using Godot;
using MegaCrit.Sts2.Core.Extensions;
using Rover.Relics;
using Rover.Tools;

namespace Rover.RoverCode.Tools;

public partial class RoverAbilitySwitchPanel : Control
{
    private ItemList? _itemList;
    private ObscuraLumis? _relic;
    private TextureButton? _closeBtn;   // 改为 TextureButton

    public override void _Ready()
    {
        _itemList = GetNode<ItemList>("%ItemList");
        _closeBtn = GetNode<TextureButton>("%CloseBtn");  // 获取 TextureButton

        if (_itemList == null || _closeBtn == null)
        {
            GD.PrintErr("RoverAbilitySwitchPanel: 关键节点缺失！");
            QueueFree();
            return;
        }

        _closeBtn.Pressed += () => HidePanel();
        _itemList.ItemSelected += OnItemSelected;

        var background = GetNode<ColorRect>("Background");
        background.GuiInput += (InputEvent e) =>
        {
            if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                HidePanel();
        };
    }

    public void ShowPanel(ObscuraLumis relic)
    {
        _relic = relic;
        _itemList.Clear();
        foreach (var id in relic.AbsorbedMonsterIds)
        {
            string name = relic.GetMonsterLocalizedName(id);
            _itemList.AddItem(name);
        }

        // 高亮当前激活的能力
        int currentIndex = -1;
        for (int i = 0; i < relic.AbsorbedMonsterIds.Count; i++)
        {
            if (relic.AbsorbedMonsterIds[i] == relic.CurrentMonsterId)
            {
                currentIndex = i;
                break;
            }
        }
        if (currentIndex >= 0)
            _itemList.Select(currentIndex);

        Visible = true;
    }

    public void HidePanel(bool playSound = true)
    {
        if (playSound)
            RoverAudioHelper.PlayOneShot("res://debug_audio/rover_close.wav");
        Visible = false;
        CallDeferred("queue_free");
    }

    private async void OnItemSelected(long index)
    {
        if (_relic == null || !Visible) return;
        var selectedId = _relic.AbsorbedMonsterIds[(int)index];
        _relic.SwitchToMonster(selectedId);
        RoverAudioHelper.PlayOneShot("res://debug_audio/rover_ability_set.wav");
        HidePanel(false);
        await Task.CompletedTask;
    }
}