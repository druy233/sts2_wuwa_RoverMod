using System.Collections.Generic;
using Godot;
using static Godot.ResourceLoader;

namespace Rover.Tools;

internal static class TextureHelper
{
    private static readonly Dictionary<string, Texture2D?> TextureCache = new Dictionary<string, Texture2D?>();

    private static readonly HashSet<string> NegativeCache = new HashSet<string>();

    public static Texture2D? LoadTexture(string path)
    {
        if (NegativeCache.Contains(path))
        {
            return null;
        }
        if (TextureCache.TryGetValue(path, out Texture2D? value))
        {
            if (value != null && GodotObject.IsInstanceValid((GodotObject)(object)value))
            {
                return value;
            }
            TextureCache.Remove(path);
        }
        Texture2D? val = null;
        try
        {
            if (path.StartsWith("res://"))
            {
                if (ResourceLoader.Exists(path, ""))
                {
                    val = ResourceLoader.Load<Texture2D>(path, (string?)null, (CacheMode)1);
                }
            }
            else
            {
                Image val2 = Image.LoadFromFile(path);
                if (val2.GetWidth() > 0 && val2.GetHeight() > 0)
                {
                    val = (Texture2D)(object)ImageTexture.CreateFromImage(val2);
                }
            }
        }
        catch
        {
            val = null;
        }
        if (val != null)
        {
            TextureCache[path] = val;
        }
        else
        {
            NegativeCache.Add(path);
        }
        return val;
    }
}