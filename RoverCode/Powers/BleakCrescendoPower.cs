using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rover.Cards;
using Rover.Relics;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class BleakCrescendoPower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner) return;

        var relic = cardPlay.Card.Owner.GetRelic<ObscuraLumis>();
        if (cardPlay.Card.Type == CardType.Attack && relic != null && StanceHelper.IsInStance<HavocPower>(base.Owner))
        {
            await relic.AddToEnergyCounter(base.Amount);
        }
        await Task.CompletedTask;
    }
}
