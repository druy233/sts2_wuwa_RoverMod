using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class HavocPower : RoverPower
{
    private Node2D? _vfx;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner && cardSource != null && cardSource.Type != CardType.Status && target != base.Owner)
            return 1.5m;
        return 1m;
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _vfx = StanceVfxHelper.SpawnOnCreature(base.Owner, StanceVfxHelper.CreateHavocVfx());
        await Task.CompletedTask;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        StanceVfxHelper.Remove(ref _vfx);
        await Task.CompletedTask;
    }

}
