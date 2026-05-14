using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using Rover.Charaters;
using Rover.Extensions;

namespace Rover.Potions;

[Pool(typeof(RoverPotionPool))]
public abstract class RoverPotion : CustomPotionModel
{
    public override string CustomPackedImagePath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
            return ResourceLoader.Exists(path) ? path : "potion.png".PotionImagePath();
        }
    }
    public override string CustomPackedOutlinePath
    {
        get
        {
            var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
            return ResourceLoader.Exists(path) ? path : "potion.png".PotionOutlineImagePath();
        }
    }
}
