using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Rover.Powers;

namespace Rover.Cards;

public class PowerChange() : RoverCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.Self)
{
    public CardModel GetTranscendenceTransformedCard()
    {
        return this;
    }
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<SpectroPower>();
            yield return HoverTipFactory.FromPower<HavocPower>();
            yield return HoverTipFactory.FromPower<AeroPower>();
            if (IsUpgraded)
            {
                var upgradedSpectro = (Spectro)ModelDb.Card<Spectro>().MutableClone();
                CardCmd.Upgrade(upgradedSpectro, CardPreviewStyle.None);

                var upgradedHavoc = (Havoc)ModelDb.Card<Havoc>().MutableClone();
                CardCmd.Upgrade(upgradedHavoc, CardPreviewStyle.None);

                var upgradedAero = (Aero)ModelDb.Card<Aero>().MutableClone();
                CardCmd.Upgrade(upgradedAero, CardPreviewStyle.None);

                yield return HoverTipFactory.FromCard(upgradedSpectro);
                yield return HoverTipFactory.FromCard(upgradedHavoc);
                yield return HoverTipFactory.FromCard(upgradedAero);
            }
            else
            {
                yield return HoverTipFactory.FromCard<Spectro>();
                yield return HoverTipFactory.FromCard<Havoc>();
                yield return HoverTipFactory.FromCard<Aero>();
            }
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.Owner.Creature.CombatState == null) return;
        // 获取当前战斗状态
        ICombatState combatState = base.Owner.Creature.CombatState;

        // 通过 CombatState 创建卡牌（自动关联 Owner 和 CombatState）
        CardModel cardA = combatState.CreateCard(ModelDb.Card<Spectro>(), base.Owner);
        CardModel cardB = combatState.CreateCard(ModelDb.Card<Havoc>(), base.Owner);
        CardModel cardC = combatState.CreateCard(ModelDb.Card<Aero>(), base.Owner);

        List <CardModel> cards = new List<CardModel> { cardA, cardB, cardC };

        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
        }

        CardModel chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, base.Owner, canSkip: true);

        if (chosenCard == null) return;

        await CardCmd.AutoPlay(choiceContext, chosenCard, target: null, AutoPlayType.Default);
    }
    protected override void OnUpgrade()
    {

    }
}
