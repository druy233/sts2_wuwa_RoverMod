using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using Rover.Extensions;

namespace Rover.Character;

public class RoverCardPool : CustomCardPoolModel
{
    public override string Title => Rover.CharacterId;
    public override string BigEnergyIconPath => "ui/combat/energy_counters/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "ui/combat/energy_counters/text_energy.png".ImagePath();

    /* 卡背颜色（取值在0 ~ 1之间） */
    public override float H => 0.142f;
    public override float S => 1f;
    public override float V => 1f;

    // 将以上三个数值设为1，然后在下方代码中放入你自己的卡背图片路径，就可以使用自定义卡背了。
    //public override Texture2D CustomFrame(CustomCardModel card)
    //{
    //    //自定义卡背图片路径。
    //    return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    //}

    // 卡牌图标颜色
    public override Color DeckEntryCardColor => new("#f2e753");
    public override Color EnergyOutlineColor => new("651565");

    public override bool IsColorless => false;
}
