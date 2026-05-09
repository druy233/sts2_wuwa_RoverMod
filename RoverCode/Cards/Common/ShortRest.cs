using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class ShortRest() : RoverCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("RoverNum", 2m),new BlockVar(6m, ValueProp.Move)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            await relic.AddToEnergyCounter((int)base.DynamicVars["RoverNum"].BaseValue);
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RoverNum"].UpgradeValueBy(1m);
    }
}
