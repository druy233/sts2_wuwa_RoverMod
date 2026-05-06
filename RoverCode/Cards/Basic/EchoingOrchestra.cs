using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Tools;

namespace Rover.Cards;

public class EchoingOrchestra() : RoverCard(-1,
    CardType.Attack, CardRarity.Basic, 
    TargetType.AllEnemies)
{
    public override bool GainsBlock => true;
    public override List<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar (24m, ValueProp.Move),
        new DamageVar(24m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState == null) return;
        RoverAudioHelper.PlayOneShot("res://debug_audio/echoing_orchestra.wav");
        await Cmd.Wait(0.8f);
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this)
            .TargetingAllOpponents(base.CombatState).Execute(choiceContext);
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(12m);
        DynamicVars.Block.UpgradeValueBy(12m);
    }
}
