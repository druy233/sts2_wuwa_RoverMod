using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class HoverdroidShooterPower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack) return;
        if (cardPlay.Card.TargetType == TargetType.AllEnemies)
        {
            var enemies = base.CombatState?.Enemies.Where(e => e.IsAlive);
            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    await CreatureCmd.Damage(choiceContext, enemy, base.Amount, ValueProp.Unpowered, base.Owner, null);
                }
            }
            return;
        }

        if (cardPlay.Target != null && cardPlay.Target.IsAlive)
        {
            await CreatureCmd.Damage(choiceContext, cardPlay.Target, base.Amount, ValueProp.Unpowered, base.Owner, null);
        }
    }
}
