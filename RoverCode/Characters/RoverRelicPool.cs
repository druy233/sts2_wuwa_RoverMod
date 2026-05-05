using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using Rover.Character;
using Rover.Extensions;

namespace Rover.Charaters;

public partial class RoverRelicPool : CustomRelicPoolModel
{
    public override string EnergyColorName => "Rover";
    public override Color LabOutlineColor => new Color("f2e753");
    public override string BigEnergyIconPath => "ui/combat/energy_counters/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "ui/combat/energy_counters/text_energy.png".ImagePath();
}
