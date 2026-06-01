using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Rover.Powers;
using Rover.Relics;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rover.Cards;

public class StormsEcho() : RoverCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [RoverHoverTips.Charge];
    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar("RoverNum", 2m)
    };

    protected override bool IsPlayable
    {
        get
        {
            var relic = base.Owner.GetRelic<ObscuraLumis>();
            if (relic != null && relic.EnergyCounter >= 9)
                return true;
            return false;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取手牌
        var discard = PileType.Discard.GetPile(base.Owner);
        if (!discard.Cards.Any()) return;

        var prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1)
        {
            Cancelable = false
        };
        var chosen = (await CardSelectCmd.FromSimpleGrid(choiceContext, discard.Cards, base.Owner, prefs)).FirstOrDefault();
        if (chosen == null) return;

        int extraTimes = base.DynamicVars["RoverNum"].IntValue;
        await PowerCmd.Apply<MultiCastExtraPower>(choiceContext, base.Owner.Creature, extraTimes - 1, base.Owner.Creature, this);
        await CardCmd.AutoPlay(choiceContext, chosen, null);

        if (chosen.Pile?.Type != PileType.Exhaust)
        {
            await CardCmd.Exhaust(choiceContext, chosen);
        }

        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            await relic.AddToEnergyCounter(-9);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["RoverNum"].UpgradeValueBy(1m);
    }
}
