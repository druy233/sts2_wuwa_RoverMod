using BaseLib.Utils;
using Godot;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using Rover.Relics;
using Rover.RoverCode.Tools;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace rover.RoverCode;

/**
 * Ideas
 * 
 * Self Bind
 * 
 * Bind effect - square texture based on model size, lines random generated (amount equal to bind amount)
 * shader of transparency of line based on average of point spread of the model
 * colored
 * Mod Author MC-druy
 * Bind... rename? Necrobinder kinda overlaps.
 * */


[ModInitializer("Init")]
public class MainFile
{
    public const string ModId = "rover";
    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Init()
    {
        var harmony = new Harmony(ModId);
        harmony.PatchAll();
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(MainFile).Assembly);
        PreloadManager.Cache.GetScene("res://scenes/ui/rover_ability_switch_panel.tscn");
        Log.Info("Mod initialized!");
    }
}

[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen._Ready))]
static class NMapScreenReadyPatch
{
    static void Postfix(NMapScreen __instance)
    {
        // 已经添加过则不再重复
        if (__instance.GetNodeOrNull<RoverAbilitySwitchButton>("AbilitySwitchBtn") != null) return;
        if (!RunManager.Instance.IsInProgress) return;

        // 获取本地玩家
        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null) return;
        var me = LocalContext.GetMe(runState.Players);
        if (me == null) return;

        // 只对漂泊者角色显示按钮
        if (me.Character.Id.Entry != "ROVER-ROVER") return;

        var scene = PreloadManager.Cache.GetScene("res://scenes/ui/rover_ability_switch_button.tscn");
        if (scene == null) return;
        var btn = scene.Instantiate<RoverAbilitySwitchButton>();
        btn.Name = "AbilitySwitchBtn";
        __instance.AddChild(btn);
    }
}