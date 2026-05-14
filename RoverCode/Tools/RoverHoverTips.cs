using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rover.Tools;

public class RoverHoverTips
{
    public static IHoverTip Charge => new HoverTip(
        new LocString("relics", "ROVER_CHARGE_TITLE"),
        new LocString("relics", "ROVER_CHARGE_DESC"));

}
