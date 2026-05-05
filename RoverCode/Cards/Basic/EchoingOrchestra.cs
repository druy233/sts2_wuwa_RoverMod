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
    public override List<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(24m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        RoverAudioHelper.PlayOneShot("res://debug_audio/echoing_orchestra.wav");
        await CommonActions.CardAttack(this, cardPlay.Target).Execute(choiceContext);
        await CreatureCmd.Stun(cardPlay.Target);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(12m);
    }
}
