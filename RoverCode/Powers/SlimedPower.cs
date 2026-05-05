using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace Rover.Powers;

public class SlimedPower : RoverPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool IsInstanced => false;
    public override int DisplayAmount => base.Amount;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只对持有者自己造成的伤害生效
        if (dealer != Owner) return 0m;
        // 不受力量类增益影响的伤害（如固定伤害）不减免
        if (props.HasFlag(ValueProp.Unpowered)) return 0m;
        // 每层降低 1 点伤害
        return -base.Amount;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side) return;

        await PowerCmd.ModifyAmount(this, -1m, null, null);
        InvokeDisplayAmountChanged();

        // 层数归零则移除
        if (base.Amount <= 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}
