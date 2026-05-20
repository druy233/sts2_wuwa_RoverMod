using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Powers;

namespace Rover.Cards;

public class EchoesOfWanderlust() : RoverCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [base.EnergyHoverTip];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("RoverNum", 1m),new EnergyVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<EchoesOfWanderlustPower>(base.Owner.Creature, base.DynamicVars["RoverNum"].BaseValue, base.Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        base.DynamicVars["RoverNum"].UpgradeValueBy(1);
        base.DynamicVars.Energy.UpgradeValueBy(1);
    }
}
