using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Cards;
using Rover.Powers;

namespace Rover.Tools;

public static class StanceHelper
{
    // 进入衍射属性
    public static async Task EnterSpectro(Player owner, CardModel? source)
    {
        RoverAudioHelper.PlayOneShot("res://debug_audio/enter_spectro.wav");
        await ChangeStance<SpectroPower>(owner, source);
    }

    // 进入湮灭属性
    public static async Task EnterHavoc(Player owner, CardModel? source)
    {
        RoverAudioHelper.PlayOneShot("res://debug_audio/enter_havoc.wav");
        await ChangeStance<HavocPower>(owner, source);
    }

    // 进入气动属性
    public static async Task EnterAero(Player owner, CardModel? source)
    {
        RoverAudioHelper.PlayOneShot("res://debug_audio/enter_aero.wav");
        await ChangeStance<AeroPower>(owner, source);
    }

    // 退出当前属性
    public static async Task ExitStance(Player owner)
    {
        Type? currentStance = GetCurrentStance(owner.Creature);
        if (currentStance != null)
        {
            await RemoveAllStances(owner.Creature);
            await OnStanceChanged(owner, currentStance, null);
        }
    }

    // 检查是否处于指定属性
    public static bool IsInStance<T>(Creature creature) where T : PowerModel
    {
        return creature.HasPower<T>();
    }

    // 通用属性切换逻辑
    private static async Task ChangeStance<T>(Player owner, CardModel? source) where T : PowerModel
    {
        if (owner.Creature.HasPower<CannotChangeStancePower>())
            return;

        Type targetStance = typeof(T);
        Type? currentStance = GetCurrentStance(owner.Creature);
        if (currentStance == targetStance)
            return;

        await RemoveAllStances(owner.Creature);
        await PowerCmd.Apply<T>(owner.Creature, 1m, owner.Creature, source);
        await OnStanceChanged(owner, currentStance, targetStance);
    }

    // 获取当前属性类型
    public static Type? GetCurrentStance(Creature creature)
    {
        if (creature.HasPower<SpectroPower>()) return typeof(SpectroPower);
        if (creature.HasPower<HavocPower>()) return typeof(HavocPower);
        if (creature.HasPower<AeroPower>()) return typeof(AeroPower);
        return null;
    }

    // 移除所有属性
    private static async Task RemoveAllStances(Creature creature)
    {
        await PowerCmd.Remove(creature.GetPower<SpectroPower>());
        await PowerCmd.Remove(creature.GetPower<HavocPower>());
        await PowerCmd.Remove(creature.GetPower<AeroPower>());
    }

    // 属性改变后的回调
    private static async Task OnStanceChanged(Player owner, Type? oldStance, Type? newStance)
    {
        // SwordCombo的方法
        var piles = new[] { PileType.Draw, PileType.Discard, PileType.Exhaust };
        var list = piles.SelectMany(p => p.GetPile(owner).Cards)
            .OfType<SwordCombo>()
            .ToList();

        foreach (CardModel item in list)
        {
            await CardPileCmd.Add(item, PileType.Hand);
        }

        // EchoesOfWanderlust的方法
        if (owner.Creature.HasPower<EchoesOfWanderlustPower>())
        {
            await PlayerCmd.GainEnergy(owner.Creature.GetPowerAmount<EchoesOfWanderlustPower>(), owner);
        }

        // BoundlessWinds的方法
        if (owner.Creature.HasPower<BoundlessWindsPower>())
        {
            await CardPileCmd.Draw(new BlockingPlayerChoiceContext(), owner.Creature.GetPowerAmount<BoundlessWindsPower>(), owner);
        }

        // CycleOfWind的方法
        if (owner.Creature.HasPower<CycleOfWindPower>())
        {
            await CreatureCmd.GainBlock(owner.Creature, owner.Creature.GetPowerAmount<CycleOfWindPower>(), ValueProp.Unpowered, null);
        }
    }

}
