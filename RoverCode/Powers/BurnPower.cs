using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rover.Powers;

public class BurnPower : RoverPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private decimal _currentRoundMultiplier = 1m;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            // 生成 0~100 随机数，决定倍率
            int roll = base.CombatState.RunState.Rng.CombatTargets.NextInt(0, 101);
            if (roll <= 20) _currentRoundMultiplier = 0.5m;
            else if (roll <= 40) _currentRoundMultiplier = 0.6m;
            else if (roll <= 60) _currentRoundMultiplier = 0.7m;
            else if (roll <= 80) _currentRoundMultiplier = 0.8m;
            else _currentRoundMultiplier = 0.9m;
        }
        await Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == base.Owner.Side)
        {
            await CreatureCmd.Damage(choiceContext, base.Owner, base.Amount, ValueProp.Unpowered, null, null);
        }
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != base.Owner) return 1m;
        return _currentRoundMultiplier;
    }

}

