using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Rover.Powers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class SkyfallSeverance() : RoverCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<AeroPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<VulnerablePower>(2),
        new PowerVar<WeakPower>(2)];
        
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)   
    {
        if (cardPlay.Target == null) return;
        bool hasStrength = cardPlay.Target.HasPower<StrengthPower>();
        if (base.Owner.Creature.HasPower<AeroPower>())
        {
            base.DynamicVars.Vulnerable.BaseValue *= 2;
            base.DynamicVars.Weak.BaseValue *= 2;
        }
        if (hasStrength)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
        }
    }
        
    protected override void OnUpgrade()   
    {
        base.DynamicVars.Vulnerable.UpgradeValueBy(1);
        base.DynamicVars.Weak.UpgradeValueBy(1);
    }
}
