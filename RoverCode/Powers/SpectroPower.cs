using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class SpectroPower : RoverPower
{
    private Node2D? _vfx;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override bool IsVisibleInternal => false;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _vfx = StanceVfxHelper.SpawnOnCreature(base.Owner, StanceVfxHelper.CreateSpectroVfx());
        await Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == base.Owner)
        {
            return -1m;
        }
        return 0m;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        StanceVfxHelper.Remove(ref _vfx);
        await Task.CompletedTask;
    }

}
