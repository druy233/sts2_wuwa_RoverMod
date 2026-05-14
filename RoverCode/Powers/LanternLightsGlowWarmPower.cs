using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class LanternLightsGlowWarmPower : RoverPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != null && player == base.Owner.Player)
        {
            var relic = base.Owner.Player.GetRelic<ObscuraLumis>();
            if (relic != null && relic.EnergyCounter >= 2) {
                await relic.AddToEnergyCounter(-2);
                await CardPileCmd.Draw(choiceContext, 1m, base.Owner.Player);
                await PowerCmd.Apply<VigorPower>(base.Owner, 3m, base.Owner, null);
            }
        }
    }

}
