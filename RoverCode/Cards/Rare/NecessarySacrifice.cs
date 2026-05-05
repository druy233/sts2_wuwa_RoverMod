using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class NecessarySacrifice() : RoverCard(0,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new[] { new DamageVar(14m, ValueProp.Move) };
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState).Execute(choiceContext);
        // 升级操作
        List<CardModel> upgradableCards = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c.IsUpgradable).ToList();
        if (upgradableCards.Count > 0)
        {
            await Cmd.Wait(0.5f);
            CardModel cardModel = base.Owner.RunState.Rng.Niche.NextItem(upgradableCards);
            base.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(base.Owner.NetId).UpgradedCards.Add(cardModel.Id);
            cardModel.UpgradeInternal();
            cardModel.FinalizeUpgradeInternal();
            ((Node)(object)NRun.Instance?.GlobalUi.CardPreviewContainer).AddChildSafely((Node?)(object)NCardSmithVfx.Create(new CardModel[1] { cardModel }));
        }
        var permanentDeck = base.Owner.Deck;
        var originalCard = permanentDeck?.Cards.FirstOrDefault(c => c.Id.Equals(this.Id));
        if (originalCard != null)
        {
            await CardPileCmd.RemoveFromDeck(originalCard);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
