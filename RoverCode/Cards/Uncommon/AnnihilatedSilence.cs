using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class AnnihilatedSilence() : RoverCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(4m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("TotalHits").WithMultiplier(static (card, target) =>
        {
            var owner = (card as CardModel)?.Owner;
            if (owner == null) return 0;
            var hand = PileType.Hand.GetPile(owner);
            int attackCount = hand.Cards.Count(c => c.Type == CardType.Attack);
            return attackCount;
        })
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(base.CombatState, "base.CombatState");
        int hitCount = (int)((CalculatedVar)DynamicVars["TotalHits"]).Calculate(cardPlay.Target);
        int damage = (int)DynamicVars.Damage.BaseValue;
        for (int i = 0; i < hitCount; i++)
        {
            await DamageCmd.Attack(damage).FromCard(this).TargetingAllOpponents(base.CombatState).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
