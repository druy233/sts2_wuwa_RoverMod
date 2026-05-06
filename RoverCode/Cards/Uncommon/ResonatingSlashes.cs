using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Powers;
using Rover.Tools;


namespace Rover.Cards;

public class ResonatingSlashes() : RoverCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SpectroPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(7m, ValueProp.Move),
        new RepeatVar(2)];

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == this)
        {
            var piles = new[] { PileType.Draw, PileType.Discard, PileType.Exhaust };
            var cards = piles.SelectMany(p => p.GetPile(base.Owner).Cards)
                .OfType<ResonatingEchoes>()
                .ToList();
            await CardPileCmd.Add(cards, PileType.Hand);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        if (StanceHelper.IsInStance<SpectroPower>(base.Owner.Creature))
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).
                WithHitCount(base.DynamicVars.Repeat.IntValue).FromCard(this)
                .Targeting(cardPlay.Target).Execute(choiceContext);
        }
        else
        {
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this)
                .Targeting(cardPlay.Target).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
