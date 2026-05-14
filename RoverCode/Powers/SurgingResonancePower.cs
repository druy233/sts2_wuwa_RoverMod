using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Rover.Cards;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class SurgingResonancePower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != base.Owner) return;

        if (cardPlay.Card.Type == CardType.Attack && StanceHelper.IsInStance<HavocPower>(base.Owner))
        {
            decimal amount = (base.Owner.MaxHp - base.Owner.CurrentHp) * 0.1m;// 回复10%失去的生命值
            await CreatureCmd.Heal(base.Owner, amount);
        }
        await Task.CompletedTask;
    }
}
