using BaseLib.Abstracts;
using Godot;
using Rover.Extensions;


namespace Rover.Charaters;

public class RoverPotionPool : CustomPotionPoolModel
{
    public override string EnergyColorName => "Rover";
    public override Color LabOutlineColor => new Color("f2e753");
    public override string BigEnergyIconPath => "ui/combat/energy_counters/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "ui/combat/energy_counters/text_energy.png".ImagePath();
}
