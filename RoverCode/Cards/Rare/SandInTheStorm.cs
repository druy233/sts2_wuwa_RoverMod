using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Relics;
using Rover.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Cards;

public class SandInTheStorm() : RoverCard(0,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [RoverHoverTips.Charge];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("RoverNum", 1),
        new DamageVar(8m, ValueProp.Move)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        var relic = base.Owner.GetRelic<ObscuraLumis>();
        if (relic != null)
        {
            await relic.AddToEnergyCounter((int)base.DynamicVars["RoverNum"].BaseValue);
        }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        CardModel card = CreateClone();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, base.Owner), 1f);
    }
    protected override void OnUpgrade()
    {
        base.DynamicVars["RoverNum"].UpgradeValueBy(1);
        base.DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
