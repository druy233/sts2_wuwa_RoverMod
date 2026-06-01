using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using System;

namespace Rover.Powers;

public class SandpitPowerCopy : RoverPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => base.Amount;

    private bool _hasIncreasedThisTurn = false;

    // 玩家回合开始时，层数 -1
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            _hasIncreasedThisTurn = false;
            // 减少 1 层
            await PowerCmd.ModifyAmount(choiceContext, this, -1m, null, null);

            InvokeDisplayAmountChanged();
            // 层数归零强制杀死随机敌人，然后移除自身
            if (base.Amount <= 0)
            {
                await ExecuteKillAndRemove();
            }
        }
    }

    // 受到伤害时，延长一回合
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return;
        if (_hasIncreasedThisTurn) return;
        if (result.UnblockedDamage <= 0) return;

        _hasIncreasedThisTurn = true;

        await PowerCmd.ModifyAmount(choiceContext, this, 1m, null, null);
        InvokeDisplayAmountChanged();
    }

    // 执行斩杀并移除自身
    private async Task ExecuteKillAndRemove()
    {
        var combatState = Owner.CombatState;
        if (combatState == null) return;

        var enemies = combatState.HittableEnemies;
        if (enemies.Count == 0) return;

        if (base.Owner.CombatState == null) return;
        var target = enemies[base.Owner.CombatState.RunState.Rng.CombatTargets.NextInt(0, enemies.Count)];

        await CreatureCmd.Kill(target, force: true);

        await PowerCmd.Remove(this);
    }
}
