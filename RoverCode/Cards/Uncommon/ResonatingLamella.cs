using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rover.Powers;


namespace Rover.Cards;

public class ResonatingLamella() : RoverCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SpectroPower>(),HoverTipFactory.Static(StaticHoverTip.Block)];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("RoverNum", 5m)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<ResonatingLamellaPower>(base.Owner.Creature, base.DynamicVars["RoverNum"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RoverNum"].UpgradeValueBy(3m);
    }
}
