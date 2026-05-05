using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace Rover.Powers;

public sealed class SoarPowerCopy : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;   // 使用层数作为剩余次数
    public override bool IsInstanced => true;
    public override int DisplayAmount => base.Amount;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 受到伤害减半（仅当伤害来源不是自己，避免自伤减半）
        if (target == Owner && dealer != Owner)
            return 0.5m;

        // 造成双倍伤害：攻击牌 + 还有剩余次数
        if (dealer == Owner && cardSource?.Type == CardType.Attack && base.Amount > 0)
            return 2m;

        return 1m;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Attack) return;
        if (base.Amount <= 0) return;

        // 减少一层（剩余次数减一）
        await PowerCmd.ModifyAmount(this, -1m, null, null);
        InvokeDisplayAmountChanged();

        if (base.Amount <= 0)
            await PowerCmd.Remove(this);
    }
}