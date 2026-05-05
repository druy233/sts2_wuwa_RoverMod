using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class OdysseyOfBeginningsPower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != null && player == base.Owner.Player)
        {
            int roll = Rng.Chaotic.NextInt(0, 3);
            switch (roll)
            {
                case 0:
                    await StanceHelper.EnterSpectro(player, null);
                    break;
                case 1:
                    await StanceHelper.EnterHavoc(player, null);
                    break;
                case 2:
                    await StanceHelper.EnterAero(player, null);
                    break;
            }
        }
    }
}
