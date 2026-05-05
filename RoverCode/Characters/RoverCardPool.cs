using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using Rover.Extensions;

namespace Rover.Character;

public class RoverCardPool : CustomCardPoolModel
{
    public override string Title => Rover.CharacterId; //This is not a display name.
    public override string BigEnergyIconPath => "ui/combat/energy_counters/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "ui/combat/energy_counters/text_energy.png".ImagePath();

    /* These HSV values will determine the color of your card back.
	They are applied as a shader onto an already colored image,
	so it may take some experimentation to find a color you like.
	Generally they should be values between 0 and 1. */
    public override float H => 0.142f;
    public override float S => 1f;
    public override float V => 1f;

    //Alternatively, leave these values at 1 and provide a custom frame image.
    //public override Texture2D CustomFrame(CustomCardModel card)
    //{
    //    //This will attempt to load Oddmelt/images/cards/frame.png
    //    return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    //}

    //Color of small card icons
    public override Color DeckEntryCardColor => new("#f2e753");
    public override Color EnergyOutlineColor => new("651565");

    public override bool IsColorless => false;
}
