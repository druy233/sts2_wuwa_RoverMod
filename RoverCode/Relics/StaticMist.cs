using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Relics;

public class StaticMist : RoverRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            relic.GaSunlockBurst = 12;
        }
    }

}
