using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Powers;

public class UnboundFlowPower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (StanceHelper.IsInStance<AeroPower>(base.Owner) && player == base.Owner.Player)
        {
            await PowerCmd.Apply<VulnerablePower>(base.CombatState.HittableEnemies, base.Amount, base.Owner, null);
            await PowerCmd.Apply<WeakPower>(base.CombatState.HittableEnemies, base.Amount, base.Owner, null);
        }
    }
}
