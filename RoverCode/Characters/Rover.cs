using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Rover.Cards;
using Rover.Charaters;
using Rover.Extensions;
using Rover.Relics;
using Rover.Tools;

namespace Rover.Character;

public class Rover : PlaceholderCharacterModel
{
	public const string CharacterId = "Rover";

	public override string PlaceholderID => "rover";

	public static readonly Color Color = new Color("f2e753");

    public const string energyColorName = "Rover"; // 能量显示的颜色名称

    //角色名称颜色
    public override Color NameColor => new(0.5f, 0.5f, 1f);
    //角色性别
    public override CharacterGender Gender => CharacterGender.Masculine;
    //角色能量计数器文字轮廓
    public override Color EnergyLabelOutlineColor => new Color("#920065");
    // 角色相关对白、气泡、事件发言等文本的颜色
    public override Color DialogueColor => new Color("#f2e753");
    // 地图上该角色绘制连线时使用的颜色
    public override Color MapDrawingColor => new Color("#f2e753");
    //初始血量
    public override int StartingHp => 80;
    //角色模型
    public override string CustomVisualPath => "res://scenes/creature_visuals/rover.tscn";
    //角色能量图标
    public override string CustomEnergyCounterPath => "res://scenes/combat/energy_counters/rover_energy_counter.tscn";
    // 卡牌拖尾场景。
     public override string CustomTrailPath => "res://scenes/vfx/card_trail_rover.tscn";
    //角色头像路径
    public override string CustomIconTexturePath => "top_panel/character_icon_rover.png".CharacterUiPath();
    // 篝火休息场景。
     public override string CustomRestSiteAnimPath => "res://scenes/rest_site/characters/rover_rest_site.tscn";
    // 商店人物场景。
     public override string CustomMerchantAnimPath => "res://scenes/merchant/characters/rover_merchant.tscn";
    // 多人模式角色手臂
     public override string CustomArmPointingTexturePath => "hands/multiplayer_hand_rover_point.png".CharacterUiPath();
    // 多人模式剪刀石头布-石头。
     public override string CustomArmRockTexturePath => "hands/multiplayer_hand_rover_rock.png".CharacterUiPath();
    // 多人模式剪刀石头布-布。
     public override string CustomArmPaperTexturePath => "hands/multiplayer_hand_rover_paper.png".CharacterUiPath();
    // 多人模式剪刀石头布-剪刀。
    public override string CustomArmScissorsTexturePath => "hands/multiplayer_hand_rover_scissors.png".CharacterUiPath();
    // 联机状态下，这个角色的指向线主体颜色
    public override Color RemoteTargetingLineColor => new Color("#f2e753");
    // 联机指向线的外描边颜色
    public override Color RemoteTargetingLineOutline => new Color("#fdf592");     
    // 角色选取背景
    public override string CustomCharacterSelectBg => "res://scenes/screens/char_select/char_select_bg_rover.tscn";
	// 角色选择图标
	public override string CustomCharacterSelectIconPath => "packed/character_select/char_select_rover.png".ImagePath();
	// 角色选择图标-锁定状态
	public override string CustomCharacterSelectLockedIconPath => "packed/character_select/char_select_rover_locked.png".ImagePath();
    // 人物选择过渡动画。
    public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/rover_transition_mat.tres";
    // 地图上的角色标记图标、表情轮盘上的角色头像
    public override string CustomMapMarkerPath => "packed/map/icons/map_marker_rover.png".ImagePath();

    // 攻击音效
    // public override string CustomAttackSfx => null;
    // 施法音效
    // public override string CustomCastSfx => null;
    // 死亡音效
    // public override string CustomDeathSfx => null;
    // 角色选择音效
     public override string CharacterSelectSfx => "";
    // 过渡音效。这个不能删。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_necrobinder";

    public override CardPoolModel CardPool => ModelDb.CardPool<RoverCardPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<RoverRelicPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<RoverPotionPool>();
    //角色初始卡组
    public override IEnumerable<CardModel> StartingDeck => [
		ModelDb.Card<RoverStrike>(),
		ModelDb.Card<RoverStrike>(),
		ModelDb.Card<RoverStrike>(),
		ModelDb.Card<RoverStrike>(),
		ModelDb.Card<RoverDefend>(),
		ModelDb.Card<RoverDefend>(),
		ModelDb.Card<RoverDefend>(),
		ModelDb.Card<RoverDefend>(),
        ModelDb.Card<ResonanceLiberation>(),
        ModelDb.Card<PowerChange>(),
	];
    //初始遗物
    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<ObscuraLumis>()];

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];
}
