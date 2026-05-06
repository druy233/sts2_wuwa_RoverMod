using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Powers;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

internal class WindCutter() : RoverCard(2,
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
        if (StanceHelper.IsInStance<HavocPower>(base.Owner.Creature))
        {
            await CardPileCmd.Draw(context, base.DynamicVars.Cards.BaseValue, base.Owner);
        }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).WithHitCount((int)base.DynamicVars.Repeat.BaseValue).Targeting(cardPlay.Target).Execute(context);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Weak.UpgradeValueBy(1m);
        base.EnergyCost.UpgradeBy(-1);
    }
}
