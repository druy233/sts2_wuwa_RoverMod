using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Rover.Tools;


namespace Rover.Powers;

public class AftertunePower : RoverPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != null && StanceHelper.IsInStance<SpectroPower>(base.Owner) && base.Owner.Player == player)
        {
            await PowerCmd.Apply<StrengthPower>(base.CombatState.HittableEnemies, -1m, base.Owner, null);
        }
    }
}
