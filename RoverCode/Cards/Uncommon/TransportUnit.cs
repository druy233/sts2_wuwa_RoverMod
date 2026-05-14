using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class TransportUnit() : RoverCard(1,
    CardType.Skill, CardRarity.Uncommon, 
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("RoverNum", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawPile = PileType.Draw.GetPile(base.Owner);
        var drawCards = drawPile.Cards.ToList();
        if (drawCards.Count == 0) return;

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, base.DynamicVars["RoverNum"].IntValue);
        var selected = await CardSelectCmd.FromSimpleGrid(choiceContext, drawCards, base.Owner, prefs);

        foreach (var card in selected)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RoverNum"].UpgradeValueBy(1m);
    }
}
