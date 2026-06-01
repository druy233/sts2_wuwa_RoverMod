using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class JourneyWell() : RoverCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    private CardModel? _mockGeneratedCard;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel cardModel;
        if (_mockGeneratedCard == null)
        {
            List<CardPoolModel> list = base.Owner.UnlockState.CharacterCardPools.ToList();
            if (list.Count > 1)
            {
                list.Remove(base.Owner.Character.CardPool);
            }
            IEnumerable<CardModel> cards = from c in list.SelectMany((CardPoolModel c) => c.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
                                           where c.Type == CardType.Attack
                                           select c;
            List<CardModel> list2 = CardFactory.GetDistinctForCombat(base.Owner, cards, 3, base.Owner.RunState.Rng.CombatCardGeneration).ToList();
            if (base.IsUpgraded)
            {
                foreach (CardModel item in list2)
                {
                    CardCmd.Upgrade(item);
                }
            }
            cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, list2, base.Owner, canSkip: true);
        }
        else
        {
            cardModel = _mockGeneratedCard;
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(cardModel);
            }
        }
        if (cardModel != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, base.Owner);
        }
    }

    public void MockGeneratedCard(CardModel card)
    {
        AssertMutable();
        _mockGeneratedCard = card;
    }
}
