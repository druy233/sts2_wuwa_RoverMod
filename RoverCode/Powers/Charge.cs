using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;

namespace Rover.Powers;
public class Charge : RoverPower
{
    private bool _isProcessing = false;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (_isProcessing) return;
        _isProcessing = true;
        try
        {
            if (player.Creature != base.Owner) return;
            var relic = player.GetRelic<ObscuraLumis>();
            if (relic != null)
            {
                await relic.AddToEnergyCounter(base.Amount);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }
}
