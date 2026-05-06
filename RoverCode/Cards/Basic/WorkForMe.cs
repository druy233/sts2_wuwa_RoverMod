using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;

namespace Rover.Cards;

public class WorkForMe() : RoverCard(3,
    CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(21m, ValueProp.Move)];// 21 点伤害
    public override List<CardKeyword> CanonicalKeywords => [
        CardKeyword.Retain, // 保留
        CardKeyword.Exhaust // 消耗
        ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            Log.Error("捕捉怪物出现问题！");
            return;
        }
        bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
        AttackCommand attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
        .FromCard(this).Targeting(cardPlay.Target)
        .Execute(choiceContext);
        // 检查目标是否死亡，并且是由本次伤害造成的死亡
        if (shouldTriggerFatal && attackCommand.Results.Any((DamageResult r) => r.WasTargetKilled))
        {
            await HandleKill(choiceContext, cardPlay.Target);
        }
    }
    private async Task HandleKill(PlayerChoiceContext choiceContext, Creature killedCreature)
    {
        var relic = Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            relic.StoreMonsterId(killedCreature.ModelId);// 调用遗物中的方法存储击杀的怪物ID
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(0m);
    }
}
