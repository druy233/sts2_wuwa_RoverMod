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
    private int _lastRoll = -1;

    private int _secondLastRoll = -1;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != null && player == base.Owner.Player)
        {
            if (base.Owner.CombatState == null) return;
            int roll;
            if (_lastRoll == _secondLastRoll && _lastRoll != -1)
            {
                var options = new List<int> { 0, 1, 2 };
                options.Remove(_lastRoll);
                roll = options[base.Owner.CombatState.RunState.Rng.CombatTargets.NextInt(0, options.Count)];
            }
            else
            {
                roll = base.Owner.CombatState.RunState.Rng.CombatTargets.NextInt(0, 3);
            }

            _secondLastRoll = _lastRoll;
            _lastRoll = roll;

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
