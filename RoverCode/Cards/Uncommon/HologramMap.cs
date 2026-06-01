using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class HologramMap() : RoverCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(base.Owner).Cards
            .Where(c => c != this)
            .ToList();
        if (hand.Count == 0) return;

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
        var selected = await CardSelectCmd.FromHand(choiceContext, base.Owner, prefs, null, this);
        if (selected.Count() == 0) return;

        CardModel targetCard = selected.ElementAt(0);
        int currentCost = targetCard.EnergyCost.GetAmountToSpend();
        int newCost = Math.Max(0, currentCost - 1);
        targetCard.EnergyCost.SetThisCombat(newCost);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
