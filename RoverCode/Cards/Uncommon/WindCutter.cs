using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Powers;
using Rover.Tools;

namespace Rover.Cards;

internal class WindCutter() : RoverCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<AeroPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<WeakPower>(2m),
        new RepeatVar(3),
        new DamageVar(4m, ValueProp.Move)];
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this)
            .WithHitFx("vfx/vfx_attack_slash", null, "attack.wav")
            .WithHitCount((int)base.DynamicVars.Repeat.BaseValue)
            .Targeting(cardPlay.Target).Execute(context);
        if (StanceHelper.IsInStance<AeroPower>(base.Owner.Creature))
        {
            await PowerCmd.Apply<WeakPower>(cardPlay.Target, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Repeat.UpgradeValueBy(1);
        base.DynamicVars.Weak.UpgradeValueBy(1m);
    }
}
