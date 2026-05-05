using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Powers;
using Rover.Relics;
using Rover.Tools;
using System.Transactions;

namespace Rover.Cards;

public class ResonanceLiberation() : RoverCard(0,
    CardType.Attack, CardRarity.Basic,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(30, ValueProp.Move)];
    public override List<CardKeyword> CanonicalKeywords => [CardKeyword.Innate,CardKeyword.Retain,CardKeyword.Eternal];
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (IsUpgraded)
            {
                var upgradedEchoingOrchestra = (EchoingOrchestra)ModelDb.Card<EchoingOrchestra>().MutableClone();
                CardCmd.Upgrade(upgradedEchoingOrchestra, CardPreviewStyle.None);

                var upgradedDeadeningAbyss = (DeadeningAbyss)ModelDb.Card<DeadeningAbyss>().MutableClone();
                CardCmd.Upgrade(upgradedDeadeningAbyss, CardPreviewStyle.None);

                var upgradedOmegaStorm = (OmegaStorm)ModelDb.Card<OmegaStorm>().MutableClone();
                CardCmd.Upgrade(upgradedOmegaStorm, CardPreviewStyle.None);

                yield return HoverTipFactory.FromCard(upgradedEchoingOrchestra);
                yield return HoverTipFactory.FromCard(upgradedDeadeningAbyss);
                yield return HoverTipFactory.FromCard(upgradedOmegaStorm);
            }
            else
            {
                yield return HoverTipFactory.FromCard<EchoingOrchestra>();
                yield return HoverTipFactory.FromCard<DeadeningAbyss>();
                yield return HoverTipFactory.FromCard<OmegaStorm>();
            }
        }
    }
    // 是否可以打出
    protected override bool IsPlayable
    {
        get 
        {
            var relic = base.Owner.GetRelic<ObscuraLumis>();
            if (relic != null && relic.EnergyCounter >= ObscuraLumis.unlockBurst)
                return true;
            return false;
        }
    }
    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (card != this) return;
        // 手牌未满时移回
        var hand = CardPile.Get(PileType.Hand, base.Owner);
        if (hand != null && hand.Cards.Count < CardPile.maxCardsInHand)
        {
            await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Top);
        }
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType pileType, AbstractModel? source)
    {
        // 只处理自身
        if (card != this) return;
        // 进入手牌时不处理（防止循环）
        if (pileType == PileType.Hand) return;
        // 获取手牌（战斗中才存在）
        var hand = CardPile.Get(PileType.Hand, base.Owner);
        // 手牌数量已达到上限（10张），无法再加入
        if (hand != null && hand.Cards.Count >= CardPile.maxCardsInHand) return;
        // 消耗牌堆在上面处理
        if (pileType == PileType.Exhaust) return;
        await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Top);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(base.Owner.Creature.CombatState, "base.Owner.Creature.CombatState");
        // 获取当前战斗状态
        CombatState combatState = base.Owner.Creature.CombatState;

        // 通过 CombatState 创建卡牌（自动关联 Owner 和 CombatState）
        CardModel cardA = combatState.CreateCard(ModelDb.Card<EchoingOrchestra>(), base.Owner);
        CardModel cardB = combatState.CreateCard(ModelDb.Card<DeadeningAbyss>(), base.Owner);
        CardModel cardC = combatState.CreateCard(ModelDb.Card<OmegaStorm>(), base.Owner);

        List<CardModel> cards = new List<CardModel> { cardA, cardB, cardC };

        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
        }

        switch (StanceHelper.GetCurrentStance(base.Owner.Creature))
        {
            case var type when type == typeof(SpectroPower):
                await CardCmd.AutoPlay(choiceContext, cardA, target: null, AutoPlayType.Default);
                break;
            case var type when type == typeof(HavocPower):
                await CardCmd.AutoPlay(choiceContext, cardB, target: null, AutoPlayType.Default);
                break;
            case var type when type == typeof(AeroPower):
                await CardCmd.AutoPlay(choiceContext, cardC, target: null, AutoPlayType.Default);
                break;
            case null:
                await CommonActions.CardAttack(this, cardPlay.Target).Execute(choiceContext);
                break;
        }
        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            await relic.SetEnergyCounter(0);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10m);
    }
}
