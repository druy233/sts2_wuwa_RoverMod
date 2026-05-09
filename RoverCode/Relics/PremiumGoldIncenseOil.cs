using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Rover.Relics;

public class PremiumGoldIncenseOil : RoverRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

    public override async Task BeforeCombatStart()
    {
        await PowerCmd.Apply<VigorPower>(base.Owner.Creature, 9m, base.Owner.Creature, null);
    }

}
