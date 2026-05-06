using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rover.Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Relics;

public class CrystalSolvent : RoverRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool IsAllowedInShops => false;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            await relic.AddToEnergyCounter(3);
        }
    }

}
