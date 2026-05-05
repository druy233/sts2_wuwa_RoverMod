using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class UmbraEclipse() : RoverCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.RandomEnemy)
{
    protected int _repeat;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5m, ValueProp.Move),
        new RepeatVar(3),
        new DynamicVar("RoverNum", 0)];

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (base.CombatState == null) return; 
        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            _repeat = relic.TurnGetValue;
            base.DynamicVars["RoverNum"].BaseValue = _repeat;
        }
    }
    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (base.CombatState == null) return;
        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            _repeat = relic.TurnGetValue;
            base.DynamicVars["RoverNum"].BaseValue = _repeat;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int repeatCount = (int)(base.DynamicVars.Repeat.BaseValue + base.DynamicVars["RoverNum"].BaseValue);
        ArgumentNullException.ThrowIfNull(base.CombatState, "base.CombatState");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(repeatCount).FromCard(this)
                .TargetingRandomOpponents(base.CombatState).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
