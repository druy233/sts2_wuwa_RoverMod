using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using System.Threading.Tasks;

namespace Rover.Powers;

public class PaperCutsPowerCopy : RoverPower
{
    private bool _isProcessing; // 防止递归的标志
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool IsInstanced => false;
    public override int DisplayAmount => base.Amount;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return;
        if (_isProcessing) return;            // 防止无限循环
        if (dealer == null) return;          // 不是攻击造成的伤害不触发
        if (base.Amount <= 0) return;        // 伤害值为0时不执行

        _isProcessing = true;

        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), Owner, base.Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, dealer);

        _isProcessing = false;
    }
}