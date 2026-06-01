using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Rover.Cards;

namespace Rover.Powers;

public class GrapplePower : RoverPower
{
    private bool _shouldIgnoreNextInstance;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public PowerModel InternallyAppliedPower => ModelDb.Power<DexterityPower>();

    protected virtual bool IsPositive => true;

    public void IgnoreNextInstance()
    {
        _shouldIgnoreNextInstance = true;
    }

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
        }
        else
        {
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);
        }
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (!(amount == (decimal)base.Amount) && power == this)
        {
            if (_shouldIgnoreNextInstance)
            {
                _shouldIgnoreNextInstance = false;
            }
            else
            {
                await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner, amount, applier, cardSource, silent: true);
            }
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            Flash();
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner, -base.Amount, base.Owner, null);
        }
    }
}
