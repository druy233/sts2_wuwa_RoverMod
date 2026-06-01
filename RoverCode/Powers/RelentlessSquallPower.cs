using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class RelentlessSquallPower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner) return;

        if (cardPlay.Card.Type != CardType.Attack) return;

        await CardCmd.Exhaust(choiceContext, cardPlay.Card, causedByEthereal: false);

        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, base.Amount, base.Owner, cardPlay.Card);
    }
}
