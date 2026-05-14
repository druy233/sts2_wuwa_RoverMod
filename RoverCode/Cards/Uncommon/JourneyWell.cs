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
        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
        //bool hasNecrobinder = base.Owner.RunState.Players.Any((Player p) => p.Character.CardPool is NecrobinderCardPool);
        bool isSinglePlayer = base.Owner.RunState.Players.Count <= 1;
        List<CardModel> options = (from card in (from _ in CardFactory.FilterForCombat(ModelDb.AllCards)
                                                 where _.Type == CardType.Attack && _.Id != base.Id 
                                                 && _.Rarity != CardRarity.Token 
                                                 //&& (hasNecrobinder || !(_.Pool is NecrobinderCardPool)) 
                                                 && (!isSinglePlayer || _.MultiplayerConstraint != CardMultiplayerConstraint.MultiplayerOnly) 
                                                 && (isSinglePlayer || _.MultiplayerConstraint != CardMultiplayerConstraint.SingleplayerOnly)
                                                 && _.Pool != base.Owner.Character.CardPool
                                                 orderby base.Owner.RunState.Rng.Niche.NextInt()
                                                 select _).Take(3)
                                   select base.CombatState?.CreateCard(card, base.Owner)).ToList();
        CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, base.Owner, false);
        if (cardModel != null)
        {
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(cardModel, CardPreviewStyle.HorizontalLayout);
            }
            await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
        }
    }

    protected override void OnUpgrade()
    {

    }
}
