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

public class UmbraEclipse() : RoverCard(0,
    CardType.Attack, CardRarity.Rare,
    TargetType.RandomEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(4m, ValueProp.Move),
        new RepeatVar(3),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("TotalHits").WithMultiplier(static (card, _) =>
        {
            var owner = (card as CardModel)?.Owner;
            var relic = owner?.GetRelic<ObscuraLumis>();
            int repeatBase = (int)((card as CardModel)?.DynamicVars.Repeat?.BaseValue ?? 0);
            int extra = relic?.TurnGetValue ?? 0;
            return repeatBase + extra;
        })
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(base.CombatState, "base.CombatState");
        int hitCount = (int)((CalculatedVar)DynamicVars["TotalHits"]).Calculate(null);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this)
            .TargetingRandomOpponents(base.CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
