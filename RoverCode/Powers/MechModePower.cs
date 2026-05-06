using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;

namespace Rover.Powers;

public class MechModePower : RoverPower
{
    private bool _isProcessing = false;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == base.Owner && amount > 0)
        {
            return 0.3m;
        }
        if (dealer == base.Owner && target != base.Owner && amount > 0 && cardSource != null)
        {
            return 3m;
        }
        return 1m;
    }
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        try
        {
            if (player.Creature != base.Owner) return;
            var relic = player.GetRelic<ObscuraLumis>();
            if (relic != null && relic.EnergyCounter >= base.Amount)
            {
                await relic.AddToEnergyCounter(-base.Amount);
            }
            else
            {
                await PowerCmd.Remove(this);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }
    public override async Task AfterRemoved(Creature oldOwner)
    {
        await PowerCmd.Remove<NoBlockPower>(oldOwner);
        await PowerCmd.Remove<CantChangeStancePower>(oldOwner);
    }
}
