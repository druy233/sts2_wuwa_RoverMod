using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;

namespace Rover.Powers;

public class MechModePower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == base.Owner && target != base.Owner && cardSource != null) // 自己造成伤害
        {
            decimal healthPercent = (decimal)base.Owner.CurrentHp / base.Owner.MaxHp;
            if (healthPercent > 0.5m)
            {
                return 2m;
            }
        }
        else if (target == base.Owner) // 自己受到伤害
        {
            decimal healthPercent = (decimal)base.Owner.CurrentHp / base.Owner.MaxHp;
            if (healthPercent <= 0.5m)
            {
                return 0.5m;
            }
        }
        return 1m;
    }
}
