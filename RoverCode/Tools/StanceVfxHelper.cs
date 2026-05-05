using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using static Godot.CanvasItemMaterial;
using static Godot.CpuParticles2D;
using static Godot.Node;

namespace Rover.Tools;

internal class StanceVfxHelper
{
    private static Texture2D? _exhaustTexture;
    private static Texture2D? _glowSparkTexture;
    private static Texture2D? _calmOrbTexture;

    // 将特效挂载到生物身上
    public static Node2D? SpawnOnCreature(Creature owner, Node2D vfx)
    {
        NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(owner);
        if (nCreature == null)
        {
            ((Node)vfx).QueueFree();
            return null;
        }
        ((Node)nCreature.Visuals).AddChild((Node)(object)vfx, false, (InternalMode)0);
        return vfx;
    }

    // 安全移除特效
    public static void Remove(ref Node2D? vfx)
    {
        if (vfx != null && GodotObject.IsInstanceValid((GodotObject)(object)vfx))
        {
            ((Node)vfx).QueueFree();
        }
        vfx = null;
    }

    // 创建衍射粒子特效
    public static Node2D CreateSpectroVfx()
    {
        Node2D val = new Node2D
        {
            Name = "SpectroVfx"
        };

        CpuParticles2D fog = CreateFogParticles(8, 3.0, new Vector2(30f, 80f), 2f, 3f,
            new Color(0.804f, 0.718f, 0.137f, 1f), 0.15f, 40f);
        ((Node2D)fog).Position = new Vector2(0f, -120f);
        ((Node)val).AddChild((Node)(object)fog, false, (InternalMode)0);

        CpuParticles2D orb = CreateParticles(10, 0.8, new Vector2(60f, 80f), new Vector2(-1f, -0.3f),
            30f, 50f, 160f, 0.3f, 0.6f,
            new Color(0.804f, 0.718f, 0.137f, 0.35f),
            new Color(0.804f, 0.718f, 0.137f, 0f),
            2f, 8f,
            "res://images/vfx/stance/calm_orb.png");
        ((Node2D)orb).Position = new Vector2(60f, -100f);
        ((Node)val).AddChild((Node)(object)orb, false, (InternalMode)0);

        return val;
    }

    // 创建湮灭粒子特效
    public static Node2D CreateHavocVfx()
    {
        Node2D val = new Node2D
        {
            Name = "HavocVfx"
        };

        CpuParticles2D fog = CreateFogParticles(10, 3.0, new Vector2(35f, 80f), 2f, 3f,
            new Color(0.188f, 0.071f, 0.459f, 1f), 0.15f, 20f);   // #5b25d4 = (0.357, 0.145, 0.831)
        ((Node2D)fog).Position = new Vector2(0f, -110f);
        ((Node)val).AddChild((Node)(object)fog, false, (InternalMode)0);

        CpuParticles2D spark = CreateParticles(16, 1.5, new Vector2(100f, 30f), new Vector2(0f, -1f),
            25f, 70f, 130f, 0.15f, 0.45f,
            new Color(0.357f, 0.145f, 0.831f, 0.4f), 
            new Color(0.188f, 0.071f, 0.459f, 0f), 2f, 8f);
        ((Node2D)spark).Position = new Vector2(0f, -85f);
        ((Node)val).AddChild((Node)(object)spark, false, (InternalMode)0);

        return val;
    }

    // 创建气动粒子特效
    public static Node2D CreateAeroVfx()
    {
        Node2D val = new Node2D
        {
            Name = "AeroVfx"
        };

        CpuParticles2D fog = CreateFogParticles(10, 3.0, new Vector2(40f, 100f), 2f, 2.7f,
            new Color(0.086f, 0.486f, 0.435f, 1f), 0.2f, 120f);   // 颜色、角速度修改
        ((Node2D)fog).Position = new Vector2(0f, -130f);
        ((Node)val).AddChild((Node)(object)fog, false, (InternalMode)0);

        CpuParticles2D spark = CreateParticles(20, 1.5, new Vector2(35f, 60f), new Vector2(0f, -1f),
            360f, 30f, 100f, 0.15f, 0.5f,
            new Color(0.086f, 0.486f, 0.435f, 0.4f), new Color(0.086f, 0.486f, 0.435f, 0f), 8f, 16f);
        ((Node2D)spark).Position = new Vector2(0f, -130f);
        ((Node)val).AddChild((Node)(object)spark, false, (InternalMode)0);

        return val;
    }

    // 带缓存的纹理加载
    private static Texture2D? LoadCached(ref Texture2D? cache, string path)
    {
        if (cache != null)
            return cache;
        cache = TextureHelper.LoadTexture(path);
        return cache;
    }

    // 创建雾气粒子（用于姿态背景）
    private static CpuParticles2D CreateFogParticles(int amount, double lifetime, Vector2 emissionExtents,
        float scaleMin, float scaleMax, Color peakColor, float peakAlpha, float angularVelocity)
    {
        CpuParticles2D val = new CpuParticles2D();
        val.Emitting = true;
        val.Amount = amount;
        val.Lifetime = lifetime;
        val.OneShot = false;
        val.Preprocess = lifetime;
        val.Randomness = 1f;
        val.LifetimeRandomness = 0.3;
        val.LocalCoords = true;

        Texture2D tex = LoadCached(ref _exhaustTexture, "res://images/vfx/stance/exhaust_l.png");
        if (tex != null)
            val.Texture = tex;

        CanvasItemMaterial mat = new CanvasItemMaterial();
        mat.BlendMode = (BlendModeEnum)1; // 混合模式：相加
        ((CanvasItem)val).Material = (Material)(object)mat;

        val.EmissionShape = (EmissionShapeEnum)3; // 矩形
        val.EmissionRectExtents = emissionExtents;
        val.Direction = new Vector2(0f, -1f);
        val.Spread = 30f;
        val.Gravity = Vector2.Zero;
        val.InitialVelocityMin = 2f;
        val.InitialVelocityMax = 8f;
        val.ScaleAmountMin = scaleMin;
        val.ScaleAmountMax = scaleMax;
        val.AngularVelocityMin = 0f - angularVelocity;
        val.AngularVelocityMax = angularVelocity;

        Gradient grad = new Gradient();
        grad.Offsets = new float[3] { 0f, 0.3f, 1f };
        grad.Colors = new Color[3]
        {
            new Color(peakColor.R, peakColor.G, peakColor.B, 0f),
            new Color(peakColor.R, peakColor.G, peakColor.B, peakAlpha),
            new Color(peakColor.R, peakColor.G, peakColor.B, 0f)
        };
        val.ColorRamp = grad;
        return val;
    }

    // 创建普通粒子（光点/火花）
    private static CpuParticles2D CreateParticles(int amount, double lifetime, Vector2 emissionExtents,
        Vector2 direction, float spread, float velocityMin, float velocityMax, float scaleMin, float scaleMax,
        Color colorStart, Color colorEnd, float velMin, float velMax, string? texturePath = null)
    {
        CpuParticles2D val = new CpuParticles2D();
        val.Emitting = true;
        val.Amount = amount;
        val.Lifetime = lifetime;
        val.OneShot = false;
        val.Preprocess = lifetime;
        val.Randomness = 1f;
        val.LifetimeRandomness = 0.4;
        val.InitialVelocityMin = velMin;
        val.InitialVelocityMax = velMax;
        val.LocalCoords = true;

        Texture2D tex;
        if (texturePath == null)
            tex = LoadCached(ref _glowSparkTexture, "res://images/vfx/stance/glow_spark.png");
        else
            tex = LoadCached(ref _calmOrbTexture, texturePath);
        if (tex != null)
            val.Texture = tex;

        CanvasItemMaterial mat = new CanvasItemMaterial();
        mat.BlendMode = (BlendModeEnum)1; // 相加混合
        ((CanvasItem)val).Material = (Material)(object)mat;

        val.EmissionShape = (EmissionShapeEnum)3;
        val.EmissionRectExtents = emissionExtents;
        val.Direction = direction;
        val.Spread = spread;
        val.Gravity = Vector2.Zero;
        val.InitialVelocityMin = velocityMin;
        val.InitialVelocityMax = velocityMax;
        val.ScaleAmountMin = scaleMin;
        val.ScaleAmountMax = scaleMax;

        Gradient grad = new Gradient();
        grad.SetColor(0, colorStart);
        grad.SetColor(1, colorEnd);
        val.ColorRamp = grad;
        return val;
    }

}
