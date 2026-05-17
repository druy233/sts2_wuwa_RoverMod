using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Tools;

namespace Rover.Powers;

public class CounterattackPower : RoverPower
{
    private bool _shouldCounterattack = false; // 本次受伤是否需要反击

    private bool _isCounterattacking = false;  // 防止反击伤害递归

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == base.Owner && dealer != null && dealer != base.Owner && dealer.Monster != null && cardSource == null && amount > 0m)
        {
            _shouldCounterattack = true;
            return 0m;
        }
        return amount;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (_shouldCounterattack && target == base.Owner && dealer != null && dealer.Monster != null && dealer.IsAlive)
        {
            _shouldCounterattack = false;
            if (_isCounterattacking) return;
            _isCounterattacking = true;

            int damage = Math.Max(6, (int)(dealer.MaxHp * 0.05m));
            RoverAudioHelper.PlayOneShot("res://debug_audio/counterattack.wav");
            await CreatureCmd.Damage(choiceContext, dealer, damage, ValueProp.Unpowered | ValueProp.SkipHurtAnim, base.Owner, null);

            _isCounterattacking = false;
            await PowerCmd.Decrement(this);
        }
    }
}
