using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Tools;

namespace Rover.Powers;

public class AeroPower : RoverPower
{
    private Node2D? _vfx;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _vfx = StanceVfxHelper.SpawnOnCreature(base.Owner, StanceVfxHelper.CreateAeroVfx());
        await Task.CompletedTask;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        StanceVfxHelper.Remove(ref _vfx);
        await Task.CompletedTask;
    }
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == base.Owner)
        {
            var roll = base.CombatState.RunState.Rng.CombatTargets.NextFloat();
            if (roll < 0.5f)
            {
                await CreatureCmd.Heal(base.Owner, 1m);
            }
        }
    }

}
