using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Cards;
using Rover.Powers;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class Havoc() : RoverCard(-1,
    CardType.Attack, CardRarity.Basic, 
    TargetType.AllEnemies)
{
    public override List<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move),new PowerVar<VulnerablePower>(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { 
        HoverTipFactory.FromPower<HavocPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState == null) return;
        await CommonActions.CardAttack(this, cardPlay.Target).Execute(choiceContext);
        await StanceHelper.EnterHavoc(base.Owner, this);
        await PowerCmd.Apply<VulnerablePower>(base.CombatState.HittableEnemies, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(5m);
        base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
    }
}
