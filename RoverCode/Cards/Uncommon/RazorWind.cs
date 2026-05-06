using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;


namespace Rover.Cards;

public class RazorWind() : RoverCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override bool HasEnergyCostX => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
    protected override bool ShouldGlowGoldInternal => base.Owner.GetRelic<ObscuraLumis>().EnergyCounter >= 12;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            int attackX = ResolveEnergyXValue();
            if (relic.EnergyCounter >= 12)
            {
                attackX *= 2;
                await relic.SetEnergyCounter(0);
            }
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(attackX).FromCard(this)
                .WithHitFx("vfx/vfx_attack_slash", null , "attack.wav")
                .Targeting(cardPlay.Target).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
