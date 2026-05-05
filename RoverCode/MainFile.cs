using BaseLib.Utils;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Cards;

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
    public const string ModId = "rover"; //At the moment, this is used only for the Logger and harmony names.

    public static Logger Logger { get; } =
        new(ModId, LogType.Generic);

    public static void Init()
    {
        var harmony = new Harmony(ModId);

        harmony.PatchAll();

        ScriptManagerBridge.LookupScriptsInAssembly(typeof(MainFile).Assembly);
        Log.Info("Mod initialized!");
    }
}