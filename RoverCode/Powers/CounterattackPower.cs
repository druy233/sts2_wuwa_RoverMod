using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Tools;

namespace Rover.Powers;

public class CounterattackPower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || dealer == base.Owner || cardSource != null)
        {
            return amount;
        }
        if (dealer != null && dealer.IsAlive)
        {
            int damage = Math.Max(1, (int)(dealer.CurrentHp * 0.05m));
            
            CreatureCmd.Damage(new BlockingPlayerChoiceContext(), dealer, damage, ValueProp.Unblockable, base.Owner, null);
        }
        return 0m;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        RoverAudioHelper.PlayOneShot("res://debug_audio/counterattack.wav");
        await PowerCmd.Decrement(this);
    }
}
