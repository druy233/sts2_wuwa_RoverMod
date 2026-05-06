using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class IndelibleMemoryPower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;

        var hand = player.PlayerCombatState?.Hand?.Cards?.ToList();
        if (hand == null || hand.Count == 0) return;

        var eligible = hand.Where(c => !c.Keywords.Contains(CardKeyword.Retain)).ToList();
        if (eligible.Count == 0) return;

        var card = eligible[base.CombatState.RunState.Rng.CombatTargets.NextInt(0, eligible.Count)];
        CardCmd.ApplyKeyword(card, CardKeyword.Retain);
    }
}
