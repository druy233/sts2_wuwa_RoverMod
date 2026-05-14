using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;


namespace Rover.Cards;

public class TidalHeritage() : RoverCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int num = ResolveEnergyXValue();
        if (base.IsUpgraded)
        {
            num++;
        }
        List<CardModel> list = CardFactory.GetForCombat(base.Owner, from c in base.Owner.Character.CardPool.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
                                                                           where c.Type == CardType.Attack
                                                                           select c, num, base.Owner.RunState.Rng.CombatCardGeneration).ToList();
        foreach (CardModel item in list)
        {
            item.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(item, PileType.Hand, addedByPlayer: true);
        }
    }
}
