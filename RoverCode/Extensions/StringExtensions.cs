using System.IO;  // 如果不再使用 Path.Join，可以移除这个 using

namespace Rover.Extensions;

public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return $"res://images/{path}";
    }

    public static string CardImagePath(this string path)
    {
        return $"res://images/packed/card_portraits/rover/{path}";
    }

    public static string BigCardImagePath(this string path)
    {
        return $"res://images/packed/card_portraits/rover/{path}";
    }

    public static string PowerImagePath(this string path)
    {
        return $"res://images/powers/{path}";
    }

    public static string BigPowerImagePath(this string path)
    {
        return $"res://images/powers/{path}";
    }

    public static string RelicImagePath(this string path)
    {
        return $"res://images/relics/{path}";
    }

    public static string RelicOutlineImagePath(this string path)
    {
        return $"res://images/relics/outline/{path}";
    }

    public static string BigRelicImagePath(this string path)
    {
        return $"res://images/relics/{path}";
    }

    public static string CharacterUiPath(this string path)
    {
        return $"res://images/ui/{path}";
    }
    public static string PotionImagePath(this string path)
    {
        return $"res://images/potions/{path}";
    }
}