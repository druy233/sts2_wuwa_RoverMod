using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Rover.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Relics;

public class StarofSolaris : RoverRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool IsAllowedInShops => false;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(325)];

    public override bool IsAllowed(IRunState runState)
    {
        return RelicModel.IsBeforeAct3TreasureChest(runState);
    }

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainGold(base.DynamicVars.Gold.BaseValue, base.Owner);
    }
}
