using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class PlowPowerCopy : RoverPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Stun),
        HoverTipFactory.FromPower<StrengthPower>()
    };

    public override bool ShouldScaleInMultiplayer => true;

    public override async Task AfterSideTurnStartLate(CombatSide side, CombatState combatState)
    {
        if (side == CombatSide.Player)
        {
        await PowerCmd.Apply<StrengthPower>(base.Owner, 3m, base.Owner, null);
        }
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 当受到伤害的是这个力量的持有者，且伤害未被格挡，且当前生命值 ≤ 最大生命值的 60% 时触发
        if (target == base.Owner && result.UnblockedDamage > 0 && target.CurrentHp <= base.Amount)
        {
            if (target.HasPower<StrengthPower>())
            {
                await PowerCmd.Remove<StrengthPower>(target);
            }

            var combatState = target.CombatState;
            var player = Owner.Player;
            if (combatState != null && player != null)
            {
                // 准备存储添加结果的数组
                var statusCards = new CardPileAddResult[5];
                for (int i = 0; i < 5; i++)
                {
                    CardModel card = combatState.CreateCard<Dazed>(player);
                    // 添加卡牌到抽牌堆顶部
                    statusCards[i] = await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, addedByPlayer: false, CardPilePosition.Top);
                }
                // 播放所有卡牌添加的预览特效（包括拖尾）
                CardCmd.PreviewCardPileAdd(statusCards);
                await Cmd.Wait(0.5f); // 等待特效播放
            }

            await PowerCmd.Remove(this);
        }
    }
}
