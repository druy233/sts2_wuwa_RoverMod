using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Powers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rover.Powers;

public class BurrowedPowerCopy : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldClearBlock(Creature creature)
    {
        if (base.Owner != creature)
        {
            return true;
        }
        return false;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 如果是持有者受到伤害
        if (target == Owner)
            return 1.5m;
        // 如果是持有者造成伤害
        if (dealer == Owner && cardSource != null)
            return 2m;
        return 1m;
    }

    // 护盾被击破时触发
    public override async Task AfterBlockBroken(Creature creature)
    {
        if (creature != Owner) return;

        // 获取战斗状态和持有者的玩家对象
        var combatState = Owner.CombatState;
        var player = Owner.Player;
        if (combatState != null && player != null)
        {
            var statusCards = new CardPileAddResult[5];
            for (int i = 0; i < 5; i++)
            {
                CardModel card = combatState.CreateCard<Dazed>(player);
                statusCards[i] = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, addedByPlayer: false, CardPilePosition.Top);
            }
            CardCmd.PreviewCardPileAdd(statusCards);
            await Cmd.Wait(0.5f);
        }
        await PowerCmd.Remove(this);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await CreatureCmd.LoseBlock(oldOwner, 999999999m);
    }
}