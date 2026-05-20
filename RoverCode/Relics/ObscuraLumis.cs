using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Cards;
using Rover.Powers;

namespace Rover.Relics;


public class ObscuraLumis : RoverRelic
{
    public int unlockBurst = 18;// 解锁共鸣解放所需的最大能量数

    private int _turnCounter; // 回合计数器

    private int _energytCounter; // 能量使用计数器

    private int _intentChangeCount;// 意图替换次数

    private int _stunChance; // 眩晕敌人概率

    private int _turnGetValue; // 每回合获得的充能

    private readonly int _MaxCharge = 36;// 最大能量储存

    public override RelicModel? GetUpgradeReplacement()
    {// 先古升级：你漂哥已经是最高级了
        return this;
    }

    // 自定义变量参数
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] 
    {
        new DamageVar("DefaultPowerDamage", 6m, ValueProp.Unpowered),
        new DamageVar("BombDamage", 8m, ValueProp.Unpowered),
        new DamageVar("RocketDamage", 50m, ValueProp.Move),
        new HealVar("Sleep", 3m),
        new EnergyVar(1)
    };
    public String TurnCounter => _turnCounter.ToString(); // 回合计数器（显示用）

    // 使用 SavedSpireField 持久化存储复活使用状态
    private static readonly SavedSpireField<ObscuraLumis, bool> ReviveUsedField =
    new(() => false, "Rover_ReviveUsed");
    // 使用 SavedSpireField 存储已吸收过的怪物 ID 列表
    private static readonly SavedSpireField<ObscuraLumis, string> _absorbedMonsterEntriesField =
    new(() => "", "Rover_AbsorbedMonsterEntries");
    // 用于 UI 展示
    public IReadOnlyList<ModelId> AbsorbedMonsterIds =>
    AbsorbedMonsterEntries.Select(entry => new ModelId("MONSTER", entry)).ToList();




    // 共鸣解放充能计数器
    public int EnergyCounter => this._energytCounter;
    public async Task AddToEnergyCounter(int amount)
    {// 增加计数器
        if (amount == 0) return;

        int newValue = this._energytCounter + amount;
        if (newValue < 0) newValue = 0;
        if (newValue > _MaxCharge) newValue = _MaxCharge;
        this._energytCounter = newValue;
        Flash();
        InvokeDisplayAmountChanged();  // 刷新 UI 显示
        await Task.CompletedTask;
    }
    public async Task SetEnergyCounter(int value)
    {
        if (value < 0) return;
        this._energytCounter = value;
        Flash();
        InvokeDisplayAmountChanged();
    }
    public override int DisplayAmount
    {
        get
        {
            return this._energytCounter;
        }
    }
    public int TurnGetValue => this._turnGetValue;
    public int GaSunlockBurst
    {
        get => unlockBurst;
        set => unlockBurst = value;
    }


    public override bool ShowCounter => true;// 显示计数器

    // 遗物稀有度（起始遗物）
    public override RelicRarity Rarity => RelicRarity.Starter;
    // 使用 SavedSpireField 持久化存储怪物 ID
    private static readonly SavedSpireField<ObscuraLumis, ModelId> StoredMonsterIdField =
        new(() => ModelId.none, "Rover_ObscuraLumis_MonsterId");
    // 存储怪物能力
    protected ModelId StoredMonsterId
    {
        get => StoredMonsterIdField.Get(this) ?? ModelId.none;
        set => StoredMonsterIdField.Set(this, value);
    }
    // 是否已存储力量
    public bool HasStoredPower => StoredMonsterId.Entry != "NONE";
    // 已吸收过的怪物 ID 列表
    public IReadOnlyList<string> AbsorbedMonsterEntries
    {
        get
        {
            string str = _absorbedMonsterEntriesField.Get(this);
            if (string.IsNullOrEmpty(str))
                return Array.Empty<string>();
            return str.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }
    }
    private void SetAbsorbedMonsterEntries(IEnumerable<string> entries)
    {
        string str = string.Join(",", entries);
        _absorbedMonsterEntriesField.Set(this, str);
    }
    // 对外方法
    public void StoreMonsterId(ModelId monsterId)
    {
        var entries = AbsorbedMonsterEntries.ToList();
        string entry = monsterId.Entry;
        if (!entries.Contains(entry))
        {
            entries.Add(entry);
            SetAbsorbedMonsterEntries(entries);
        }
        StoredMonsterId = monsterId;
        Flash();
        Log.Info($"晦明终端已存储怪物: {monsterId.Entry}");
        Log.Info("怪物本地化名称:" + GetMonsterLocalizedName(monsterId));
        InvokeDisplayAmountChanged();
    }
    // 切换到吸收过的怪物能力
    public void SwitchToMonster(ModelId monsterId)
    {
        if (AbsorbedMonsterEntries.Contains(monsterId.Entry))
        {
            StoredMonsterId = monsterId;
            Flash();
            InvokeDisplayAmountChanged();
        }
    }
    // 当前激活的怪物能力 ID
    public ModelId CurrentMonsterId => StoredMonsterId;
    // 获取怪物本地化名称
    public string GetMonsterLocalizedName(ModelId monsterId)
    {
        // 怪物名称的本地化键为 "MONSTER.{Entry}.name"
        if (monsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_FRONT") || monsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_MIDDLE") || monsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_BACK"))
    {
        var loc_decimillipede_segment = LocString.GetIfExists("monsters", "DECIMILLIPEDE_SEGMENT.name");
        return loc_decimillipede_segment?.GetRawText() ?? "残杀千足虫";
    }
    var loc = LocString.GetIfExists("monsters", monsterId.Entry + ".name");
    return loc?.GetRawText() ?? monsterId.Entry;
    }

    // 怪物能力描述
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (StoredMonsterId.Entry.Equals("NONE"))
                yield break;

            if (HasStoredPower)
            {
                // 获取怪物本地化名称
                var monsterName = GetMonsterLocalizedName(StoredMonsterId);
                // 动态生成能力描述的本地化键
                string powerDescKey = $"ROVER_POWER_DESC_{StoredMonsterId.Entry}";
                var powerDesc = LocString.GetIfExists("relics", powerDescKey) ?? new LocString("relics", "ROVER_DEFAULT_POWER_DESC");

                var title = new LocString("relics", "ROVER_OBSCURA_LUMIS_STORED_TITLE");
                title.Add("monster", monsterName);
                yield return new HoverTip(title, powerDesc);

                switch (StoredMonsterId.Entry)
                {
                    case "TWIG_SLIME_M":
                        yield return HoverTipFactory.FromPower<SlimedPower>();
                        break;
                    case "LEAF_SLIME_M":
                        yield return HoverTipFactory.FromPower<SlimedPower>();
                        break;
                    case "TWIG_SLIME_S":
                        yield return HoverTipFactory.FromPower<SlimedPower>();
                        break;
                    case "LEAF_SLIME_S":
                        yield return HoverTipFactory.FromPower<SlimedPower>();
                        break;
                    case "SLIMED_BERSERKER":
                        yield return HoverTipFactory.FromPower<SlimedPower>();
                        break;
                    case "SLITHERING_STRANGLER":
                        yield return HoverTipFactory.FromPower<ConstrictPowerCopy>();
                        break;
                    case "CEREMONIAL_BEAST":
                        yield return HoverTipFactory.FromPower<PlowPowerCopy>();
                        break;
                    case "THE_INSATIABLE":
                        yield return HoverTipFactory.FromPower<SandpitPowerCopy>();
                        break;
                    case "AXEBOT":
                        yield return HoverTipFactory.FromPower<StockPowerCopy>();
                        break;
                    case "OWL_MAGISTRATE":
                        yield return HoverTipFactory.FromPower<SoarPowerCopy>();
                        break;
                    case "SCROLL_OF_BITING":
                        yield return HoverTipFactory.FromPower<PaperCutsPowerCopy>();
                        break;
                    case "MECHA_KNIGHT":
                        yield return HoverTipFactory.FromPower<BurnPower>();
                        break;
                    case "EXOSKELETON":
                        yield return HoverTipFactory.FromPower<HardToKillPowerCopy>();
                        break;
                    case "TUNNELER":
                        yield return HoverTipFactory.FromPower<BurrowedPowerCopy>();
                        break;
                    case "THIEVING_HOPPER":
                        yield return HoverTipFactory.FromPower<FlutterPowerCopy>();
                        break;
                    case "FOSSIL_STALKER":
                        yield return HoverTipFactory.FromPower<SuckPower>();
                        break;
                    case "BYRDONIS":
                        yield return HoverTipFactory.FromPower<TerritorialPower>();
                        break;
                    case "SKULKING_COLONY":
                        yield return HoverTipFactory.FromPower<HardenedShellPower>();
                        break;
                    case "PHANTASMAL_GARDENER":
                        yield return HoverTipFactory.FromPower<SkittishPower>();
                        break;
                }
            }
        }
    }

    public override async Task BeforeCombatStart()// 战斗开始时事件
    {
        // 放入一张“为我所用+”
        var card = base.Owner.Creature.CombatState?.CreateCard<WorkForMe>(base.Owner);
        if (card == null) return;
        CardCmd.Upgrade(card);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);

    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (StoredMonsterId.Entry.Equals("SOUL_NEXUS"))
        {
            if (dealer == Owner?.Creature && cardSource != null)
            {
                return 6m;
            }
        }
        return 0m;
    }

    public override bool ShouldDie(Creature creature)
    {
        if (StoredMonsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_FRONT") || StoredMonsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_MIDDLE")
            || StoredMonsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_BACK"))
        {
            // 只对遗物持有者生效，且尚未使用过复活
            if (creature == base.Owner.Creature && !ReviveUsedField.Get(this))
            {
                // 阻止死亡
                return false;
            }
        }
        return true;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (ReviveUsedField.Get(this)) return;

        string monsterEntry = StoredMonsterId.Entry;
        // 判断是否为残杀千足虫的任何一段
        if (monsterEntry.Equals("DECIMILLIPEDE_SEGMENT_FRONT") ||monsterEntry.Equals("DECIMILLIPEDE_SEGMENT_MIDDLE") ||monsterEntry.Equals("DECIMILLIPEDE_SEGMENT_BACK"))
        {
            ReviveUsedField.Set(this, true);
            int healAmount = Math.Max(creature.MaxHp / 2, 1);
            await CreatureCmd.Heal(creature, healAmount);

            // 获取当前已吸收的怪物条目列表
            var entries = AbsorbedMonsterEntries.ToList();
            // 定义残杀千足虫的三个可能条目
            var centipedeEntries = new[]
            {
            "DECIMILLIPEDE_SEGMENT_FRONT",
            "DECIMILLIPEDE_SEGMENT_MIDDLE",
            "DECIMILLIPEDE_SEGMENT_BACK"};

            bool changed = false;
            foreach (var centipedeEntry in centipedeEntries)
            {
                if (entries.Contains(centipedeEntry))
                {
                    entries.Remove(centipedeEntry);
                    changed = true;
                }
            }
            if (changed) SetAbsorbedMonsterEntries(entries);

            // 清空当前激活的能力
            StoredMonsterId = ModelId.none;
            Flash();
            InvokeDisplayAmountChanged();
        }
    }

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {// 修改最大能量
        if (player == base.Owner)
        {
            if (StoredMonsterId.Entry.Equals("INFESTED_PRISM"))
            {
                return amount + base.DynamicVars.Energy.BaseValue;
            }
            return amount;
        }
        return amount;
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {// 卡牌打出后
        if (cardPlay.Card.Owner != Owner) return;// 只处理自己打出的卡牌

        if (StoredMonsterId.Entry.Equals("GREMLIN_MERC"))
        {
            if (cardPlay.Card.Type == CardType.Attack)
            {
                await PlayerCmd.GainGold(5, Owner);
            }
        }
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // 只处理伤害来源不为空、且目标是自己
        if (dealer == null) return;
        if (target != Owner.Creature) return;

        // 针对特定怪物的效果
        if (StoredMonsterId.Entry.Equals("ENTOMANCER"))
        {
            // 每次受伤增加 2% 眩晕概率
            _stunChance += 2;
            if (_stunChance > 20) _stunChance = 20;   // 最大不超过 20%
            Log.Info($"当前眩晕概率为：{_stunChance}%");

            // 投掷 1~100 随机数
            int roll = base.Owner.RunState.Rng.CombatTargets.NextInt(1, 101);
            if (roll <= _stunChance)
            {
                await CreatureCmd.Stun(dealer);
                Flash();
                _stunChance = 0;   // 触发后重置概率
            }
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room) // 战斗结束时事件
    {// 每场战斗结束时重置计数器
        _turnCounter = 0;
        _intentChangeCount = 0;
        await this.SetEnergyCounter(0);
        InvokeDisplayAmountChanged();
        await Task.CompletedTask;
    }

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {// 能量消耗时事件
        if (card.Owner != base.Owner) return;// 只处理自己打出的卡牌

        int lastValue =  this._energytCounter + amount;
        this._energytCounter = Math.Min(lastValue, _MaxCharge);
        this._turnGetValue += amount;
        InvokeDisplayAmountChanged();
        await Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {// 玩家回合开始时
        if (player != base.Owner) return;// 只处理自己的回合
        _turnCounter++;// 回合计数器
        _turnGetValue = 0;

        float roll = base.Owner.RunState.Rng.CombatTargets.NextFloat();// 随机一个数
        var enemies = player.Creature.CombatState?.HittableEnemies;// 获取可供攻击的敌人

        if (StoredMonsterId.Entry.Equals("EYE_WITH_TEETH") || StoredMonsterId.Entry.Equals("NOISEBOT"))
        {// 3%概率眩晕随机敌人
            if (roll < 0.03f)
            {
                if (enemies?.Count == 0) return;
                Flash();
                // 随机选择一个敌人
                int index = base.Owner.RunState.Rng.CombatTargets.NextInt(0, enemies?.Count ?? 0); 
                var target = enemies?[index];
                if (target == null) return;
                await CreatureCmd.Stun(target);
            }
        }

        if (StoredMonsterId.Entry.Equals("HAUNTED_SHIP"))
        {// 5%概率眩晕随机敌人
            if (roll < 0.05f)
            {
                if (enemies?.Count == 0) return;
                Flash();
                // 随机选择一个敌人
                int index = base.Owner.RunState.Rng.CombatTargets.NextInt(0, enemies?.Count ?? 0);
                var target = enemies?[index];
                if (target == null) return;
                await CreatureCmd.Stun(target);
            }
        }

        if (StoredMonsterId.Entry.Equals("CROSSBOW_RUBY_RAIDER"))
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, new BlockVar(5, ValueProp.Unpowered), null);
        }

        if (StoredMonsterId.Entry.Equals("GUARDBOT"))
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, new BlockVar(8, ValueProp.Unpowered), null);
        }

        if (StoredMonsterId.Entry.Equals("PHROG_PARASITE"))
        {
            Flash();
            int index = base.Owner.RunState.Rng.CombatTargets.NextInt(0, enemies?.Count ?? 0);
            var target = enemies?[index];
            if (target == null) return;
            await CreatureCmd.Damage(choiceContext, target, 9m, ValueProp.Unpowered, player.Creature);
        }

        if (StoredMonsterId.Entry.Equals("WRIGGLER"))
        {
            Flash();
            int index = base.Owner.RunState.Rng.CombatTargets.NextInt(0, enemies?.Count ?? 0);
            var target = enemies?[index];
            if (target == null) return;
            await CreatureCmd.Damage(choiceContext, target, 3m, ValueProp.Unpowered, player.Creature);
        }

        if (StoredMonsterId.Entry.Equals("BOWLBUG_EGG"))
        {
            Flash();
            await CreatureCmd.GainBlock(base.Owner.Creature, 5m, ValueProp.Unpowered, null);
        }

        if (StoredMonsterId.Entry.Equals("CHOMPER"))
        {
            if (roll < 0.03f)
            {
                if (enemies?.Count == 0) return;
                Flash();
                // 随机选择一个敌人
                int index = base.Owner.RunState.Rng.CombatTargets.NextInt(0, enemies?.Count ?? 0);
                var target = enemies?[index];
                if (target == null) return;
                await CreatureCmd.Stun(target);
            }
        }

        if(_turnCounter % 3  == 0)
        {
            if (StoredMonsterId.Entry.Equals("KIN_PRIEST"))
            {
                Flash();
                if (enemies == null) return;
                await CreatureCmd.Damage(choiceContext, enemies, 9m, ValueProp.Unpowered, player.Creature);
            }

            if (StoredMonsterId.Entry.Equals("TERROR_EEL"))
            {
                Flash();
                await PowerCmd.Apply<VigorPower>(base.Owner.Creature, 6m, base.Owner.Creature, null);
            }
        }

        if(_turnCounter % 5  == 0)
        {
            if (StoredMonsterId.Entry.Equals("SOUL_FYSH"))
            {
                Flash();
                if (enemies == null) return;
                await PowerCmd.Apply<IntangiblePower>(base.Owner.Creature, 1m ,base.Owner.Creature, null);
                await CreatureCmd.Damage(choiceContext, enemies, 12m, ValueProp.Unblockable, player.Creature);
            }
        }
    }
    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {// 某一方回合开始前
        if (side == CombatSide.Enemy)// 敌人回合开始时
        {
            if (StoredMonsterId.Entry.Equals("HUNTER_KILLER"))
            {
                Flash();
                await PowerCmd.Apply<StrengthPower>(combatState.HittableEnemies, -1m, base.Owner.Creature, null);
                await PowerCmd.Apply<DexterityPower>(combatState.HittableEnemies, -1m, base.Owner.Creature, null);
            }

            if (StoredMonsterId.Entry.Equals("KNOWLEDGE_DEMON"))
            {
                Flash();
                foreach (var enemy in combatState.HittableEnemies)
                {
                    // 随机选择 0、1、2 对应三种减益
                    int roll = base.Owner.RunState.Rng.CombatTargets.NextInt(0, 4); // 0，1，2，3
                    PowerModel? debuff = null;
                    decimal amount;

                    switch (roll)
                    {
                        case 0:
                            debuff = ModelDb.Power<WeakPower>(); // 虚弱
                            amount = 1m;
                            await PowerCmd.Apply(debuff.ToMutable(), enemy, amount, Owner.Creature, null);
                            break;
                        case 1:
                            debuff = ModelDb.Power<DemisePower>(); // 消亡
                            amount = 6m;
                            await PowerCmd.Apply(debuff.ToMutable(), enemy, amount, Owner.Creature, null);
                            break;
                        case 2:
                            debuff = ModelDb.Power<FrailPower>(); // 脆弱
                            amount = 1m;
                            await PowerCmd.Apply(debuff.ToMutable(), enemy, amount, Owner.Creature, null);
                            break;
                        case 3:
                            debuff = ModelDb.Power<StrengthPower>(); // 力量-1
                            amount = -1m;
                            await PowerCmd.Apply(debuff.ToMutable(), enemy, amount, Owner.Creature, null);
                            break;
                    }
                }
            }

            if (StoredMonsterId.Entry.Equals("GLOBE_HEAD"))
            {
                foreach (var enemy in combatState.HittableEnemies)
                {
                    var monster = enemy.Monster;
                    if (monster == null) continue;

                    // 检查当前意图是否为技能
                    bool hasPower = monster.NextMove.Intents.Any(i => i is BuffIntent);
                    if (!hasPower) continue;

                    Flash();
                    await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), enemy, 12m, ValueProp.Unblockable, Owner.Creature);
                }
            }
        }

        if (side != CombatSide.Player) return; // 玩家回合开始时
        if (CombatSide.Player != base.Owner.Creature.Side) return; // 只处理遗物持有者的回合

        if (StoredMonsterId.Entry.Equals("LIVING_FOG"))
        {
            foreach (var enemy in combatState.HittableEnemies)
            {
                if (_intentChangeCount >= 1) break;

                var monster = enemy.Monster;
                if (monster == null) continue;

                // 检查当前意图是否为技能
                bool hasSkill = monster.NextMove.Intents.Any(i => i is DefendIntent || i is DebuffIntent 
                || i is StatusIntent || i is HealIntent || i is SummonIntent || i is EscapeIntent);
                if (!hasSkill) continue;

                // 修复 FollowUpStateId，确保下一回合能恢复
                string? followUpStateId = monster.NextMove.FollowUpStateId;
                if (string.IsNullOrEmpty(followUpStateId)) followUpStateId = monster.NextMove.Id;
                if (string.IsNullOrEmpty(followUpStateId)) followUpStateId = monster.MoveStateMachine?.States.Keys.FirstOrDefault();

                int damage = 6; // 可自定义伤害
                var attackIntent = new SingleAttackIntent(damage);

                async Task PerformAttack(IReadOnlyList<Creature> _)
                {
                    var combatState = enemy.CombatState;
                    if (combatState == null) return;

                    foreach (var player in combatState.Players)
                    {
                        if (player.Creature.IsDead) continue;
                        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), player.Creature, damage, ValueProp.Move, enemy);
                    }
                }

                var forcedAttack = new MoveState("forced_attack", PerformAttack, attackIntent)
                {
                    FollowUpStateId = followUpStateId
                };

                monster.SetMoveImmediate(forcedAttack, forceTransition: true);
                _intentChangeCount++;
                Flash();
            }
        }

        if (StoredMonsterId.Entry.Equals("KNOWLEDGE_DEMON"))
        {
            Flash();
            // 随机选择 0、1、2 对应三种增益
            int roll = base.Owner.RunState.Rng.CombatTargets.NextInt(0, 3); // 0,1,2
            PowerModel? debuff = null;

            switch (roll)
            {
                case 0:
                    debuff = ModelDb.Power<StrengthPower>(); // 力量
                    await PowerCmd.Apply(debuff.ToMutable(), base.Owner.Creature, 1m, Owner.Creature, null);
                    break;
                case 1:
                    debuff = ModelDb.Power<DexterityPower>(); // 敏捷
                    await PowerCmd.Apply(debuff.ToMutable(), base.Owner.Creature, 1m, Owner.Creature, null);
                    break;
                case 2:
                    await CreatureCmd.Heal(base.Owner.Creature, 3m); // 生命恢复
                    break;
            }
        }

        if (StoredMonsterId.Entry.Equals("STABBOT"))
        {
            Flash();
            await PowerCmd.Apply<FrailPower>(combatState.HittableEnemies, 1m, base.Owner.Creature, null);
        }

        if (_turnCounter > 5)
        {
            if (StoredMonsterId.Entry.Equals("TEST_SUBJECT"))
            {
                await PowerCmd.Apply<NemesisPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
            }
        }

        if (_turnCounter % 2 == 0)
        {
            if (StoredMonsterId.Entry.Equals("THE_LOST"))
            {
                Flash();
                var enemies = combatState.HittableEnemies;
                int index = base.Owner.RunState.Rng.CombatTargets.NextInt(0, enemies.Count);
                var target = enemies[index];
                await PowerCmd.Apply<StrengthPower>(target, -2m, base.Owner.Creature, null);
                await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, 2m, base.Owner.Creature, null);
            }

            if (StoredMonsterId.Entry.Equals("THE_FORGOTTEN"))
            {
                Flash();
                var enemies = combatState.HittableEnemies;
                int index = base.Owner.RunState.Rng.CombatTargets.NextInt(0, enemies.Count);
                var target = enemies[index];
                await PowerCmd.Apply<DexterityPower>(target, -2m, base.Owner.Creature, null);
                await PowerCmd.Apply<DexterityPower>(base.Owner.Creature, 2m, base.Owner.Creature, null);
            }

        }

        if (_turnCounter % 3 == 0)
        {
            if (StoredMonsterId.Entry.Equals("VINE_SHAMBLER"))
            {
                Flash();
                await PowerCmd.Apply<ShacklingPotionPower>(combatState.HittableEnemies, 3m, Owner.Creature, null);
            }

            if (StoredMonsterId.Entry.Equals("KIN_PRIEST"))
            {
                Flash();
                await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, 2m, base.Owner.Creature, null);
                await PowerCmd.Apply<FrailPower>(combatState.HittableEnemies, 1m, base.Owner.Creature, null);
                await PowerCmd.Apply<WeakPower>(combatState.HittableEnemies, 1m, base.Owner.Creature, null);
            }

        }
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {// 回合结束后

    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {// 某回合开始前事件
        // 只在第一回合开始时触发怪物能力(一次性怪物能力)
        if (side == base.Owner.Creature.Side && combatState.RoundNumber <= 1 && CombatSide.Player == base.Owner.Creature.Side)
        {
            if (StoredMonsterId.Entry.Equals("NONE") || StoredMonsterId.Entry.Equals("VINE_SHAMBLER") || StoredMonsterId.Entry.Equals("EYE_WITH_TEETH")
                || StoredMonsterId.Entry.Equals("PHROG_PARASITE") || StoredMonsterId.Entry.Equals("WRIGGLER")
                || StoredMonsterId.Entry.Equals("KIN_PRIEST") || StoredMonsterId.Entry.Equals("CROSSBOW_RUBY_RAIDER")
                || StoredMonsterId.Entry.Equals("LIVING_FOG") || StoredMonsterId.Entry.Equals("GREMLIN_MERC")
                || StoredMonsterId.Entry.Equals("TERROR_EEL") || StoredMonsterId.Entry.Equals("SOUL_FYSH")
                || StoredMonsterId.Entry.Equals("HUNTER_KILLER") || StoredMonsterId.Entry.Equals("BOWLBUG_EGG")
                || StoredMonsterId.Entry.Equals("ENTOMANCER") || StoredMonsterId.Entry.Equals("INFESTED_PRISM")
                || StoredMonsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_FRONT") || StoredMonsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_MIDDLE")
                || StoredMonsterId.Entry.Equals("DECIMILLIPEDE_SEGMENT_BACK") || StoredMonsterId.Entry.Equals("KNOWLEDGE_DEMON")
                || StoredMonsterId.Entry.Equals("THE_LOST") || StoredMonsterId.Entry.Equals("THE_FORGOTTEN")
                || StoredMonsterId.Entry.Equals("GLOBE_HEAD") || StoredMonsterId.Entry.Equals("NOISEBOT")
                || StoredMonsterId.Entry.Equals("GUARDBOT") || StoredMonsterId.Entry.Equals("STABBOT")
                || StoredMonsterId.Entry.Equals("SOUL_NEXUS"))
                return;
            Flash();
            // 匹配有能力的怪物id
            switch (StoredMonsterId.Entry)
            {
                case "SHRINKER_BEETLE":
                    await PowerCmd.Apply<ShrinkPower>(combatState.HittableEnemies, 3m, base.Owner.Creature, null);
                    break;
                case "TWIG_SLIME_M":
                    await PowerCmd.Apply<SlimedPower>(combatState.HittableEnemies, 3m, base.Owner.Creature, null);
                    break;
                case "LEAF_SLIME_M":
                    await PowerCmd.Apply<SlimedPower>(combatState.HittableEnemies, 3m, base.Owner.Creature, null);
                    break;
                case "TWIG_SLIME_S":
                    await PowerCmd.Apply<SlimedPower>(combatState.HittableEnemies, 1m, base.Owner.Creature, null);
                    break;
                case "LEAF_SLIME_S":
                    await PowerCmd.Apply<SlimedPower>(combatState.HittableEnemies, 1m, base.Owner.Creature, null);
                    break;
                case "INKLET":
                    await PowerCmd.Apply<SlipperyPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "SLITHERING_STRANGLER":
                    await PowerCmd.Apply<ConstrictPowerCopy>(combatState.HittableEnemies, 3m, base.Owner.Creature, null);
                    break;
                case "FLYCONID":
                    await PowerCmd.Apply<FrailPower>(combatState.HittableEnemies, 2m, base.Owner.Creature, null);
                    await PowerCmd.Apply<VulnerablePower>(combatState.HittableEnemies, 2m, base.Owner.Creature, null);
                    break;
                case "TRACKER_RUBY_RAIDER":
                    await PowerCmd.Apply<FrailPower>(combatState.HittableEnemies, 2m, base.Owner.Creature, null);
                    break;
                case "CUBEX_CONSTRUCT":
                    await PowerCmd.Apply<ArtifactPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "BYRDONIS":
                    await PowerCmd.Apply<TerritorialPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "BYGONE_EFFIGY":
                    await PowerCmd.Apply<SlowPower>(combatState.HittableEnemies, 1m, base.Owner.Creature, null);
                    break;
                case "VANTOM":
                    await PowerCmd.Apply<SlipperyPower>(base.Owner.Creature, 3m, base.Owner.Creature, null);
                    break;
                case "TOADPOLE":
                    await PowerCmd.Apply<ThornsPower>(base.Owner.Creature, 2m, base.Owner.Creature, null);
                    break;
                case "CEREMONIAL_BEAST":
                    int threshold = (int)(Owner.Creature.MaxHp * 0.6);
                    await PowerCmd.Apply<PlowPowerCopy>(base.Owner.Creature, threshold, base.Owner.Creature, null);
                    break;
                case "CALCIFIED_CULTIST":
                    await PowerCmd.Apply<RitualPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "DAMP_CULTIST":
                    await PowerCmd.Apply<RitualPower>(base.Owner.Creature, 2m, base.Owner.Creature, null);
                    break;
                case "CORPSE_SLUG":
                    List<CardModel> cardsToExhaust = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, new CardSelectorPrefs(new LocString("relics", "ROVER_CORPSE_SLUG"), 0, 10), null, this)).ToList();
                    if (cardsToExhaust.Count > 0)
                    {
                        foreach (var card in cardsToExhaust)
                        {
                            await CardCmd.Exhaust(choiceContext, card, causedByEthereal: false, skipVisuals: false);
                            await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, 1m, Owner.Creature, null);
                        }
                    }
                    break;
                case "GAS_BOMB":
                    (await PowerCmd.Apply<TheBombPower>(base.Owner.Creature, 1m, base.Owner.Creature, null))?.SetDamage(base.DynamicVars["BombDamage"].BaseValue);
                    break;
                case "FOSSIL_STALKER":
                    await PowerCmd.Apply<SuckPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "TWO_TAILED_RAT":
                    await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, (DamageVar)base.DynamicVars["DefaultPowerDamage"], base.Owner.Creature);
                    await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, (DamageVar)base.DynamicVars["DefaultPowerDamage"], base.Owner.Creature);
                    await PowerCmd.Apply<FrailPower>(combatState.HittableEnemies, 1m, base.Owner.Creature, null);
                    break;
                case "SEWER_CLAM":
                    await PowerCmd.Apply<PlatingPower>(base.Owner.Creature, 8m, base.Owner.Creature, null);
                    break;
                case "HAUNTED_SHIP":
                    await PowerCmd.Apply<WeakPower>(combatState.HittableEnemies, 1m, base.Owner.Creature, null);
                    break;
                case "SKULKING_COLONY":
                    await PowerCmd.Apply<HardenedShellPower>(base.Owner.Creature, 20m, base.Owner.Creature, null);
                    break;
                case "PHANTASMAL_GARDENER":
                    await PowerCmd.Apply<SkittishPower>(base.Owner.Creature, 8m, base.Owner.Creature, null);
                    break;
                case "LAGAVULIN_MATRIARCH":
                    await PowerCmd.Apply<PlatingPower>(base.Owner.Creature, 4m, base.Owner.Creature, null);
                    await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["Sleep"].IntValue); 
                    break;
                case "EXOSKELETON":
                    await PowerCmd.Apply<HardToKillPowerCopy>(base.Owner.Creature, 15m, base.Owner.Creature, null);
                    break;
                case "TUNNELER":
                    await PowerCmd.Apply<BurrowedPowerCopy>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "THIEVING_HOPPER":
                    await PowerCmd.Apply<FlutterPowerCopy>(base.Owner.Creature, 3m, base.Owner.Creature, null);
                    break;
                case "LOUSE_PROGENITOR":
                    await PowerCmd.Apply<CurlUpPower>(base.Owner.Creature, 12m, base.Owner.Creature, null);
                    break;
                case "MYTE":
                    await PowerCmd.Apply<PoisonPower>(combatState.HittableEnemies, 5m, base.Owner.Creature, null);
                    break;
                case "CHOMPER":
                    await PowerCmd.Apply<ArtifactPower>(base.Owner.Creature, 2m, base.Owner.Creature, null);
                    break;
                case "SLUMBERING_BEETLE":
                    await PowerCmd.Apply<PlatingPower>(base.Owner.Creature, 4m, base.Owner.Creature, null);
                    await CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars["Sleep"].IntValue);
                    break;
                case "SPINY_TOAD":
                    await PowerCmd.Apply<ThornsPower>(base.Owner.Creature, 5m, base.Owner.Creature, null);
                    break;
                case "THE_INSATIABLE":
                    await PowerCmd.Apply<SandpitPowerCopy>(base.Owner.Creature, 5m, base.Owner.Creature, null);
                    break;
                case "ROCKET":
                    (await PowerCmd.Apply<TheBombPower>(base.Owner.Creature, 3m, base.Owner.Creature, null))?.SetDamage(base.DynamicVars["RocketDamage"].BaseValue);
                    break;
                case "DEVOTED_SCULPTOR":
                    await PowerCmd.Apply<RitualPower>(base.Owner.Creature, 3m, base.Owner.Creature, null);
                    break;
                case "LIVING_SHIELD":
                    await PowerCmd.Apply<BeaconOfHopePower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "AXEBOT":
                    await PowerCmd.Apply<StockPowerCopy>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "FROG_KNIGHT":
                    await PowerCmd.Apply<PlatingPower>(base.Owner.Creature, 8m, base.Owner.Creature, null);
                    break;
                case "SLIMED_BERSERKER":
                    await PowerCmd.Apply<SlimedPower>(combatState.HittableEnemies, 8m, base.Owner.Creature, null);
                    break;
                case "OWL_MAGISTRATE":
                    await PowerCmd.Apply<SoarPowerCopy>(base.Owner.Creature, 2m, base.Owner.Creature, null);
                    break;
                case "SCROLL_OF_BITING":
                    await PowerCmd.Apply<PaperCutsPowerCopy>(combatState.HittableEnemies, 3m, base.Owner.Creature, null);
                    break;
                case "ZAPBOT":
                    await PowerCmd.Apply<HighVoltagePower>(combatState.HittableEnemies, 3m, base.Owner.Creature, null);
                    break;
                case "PUNCH_CONSTRUCT":
                    await CreatureCmd.GainBlock(base.Owner.Creature, 10m, ValueProp.Unpowered, null);
                    await PowerCmd.Apply<ArtifactPower>(base.Owner.Creature, 1m, base.Owner.Creature, null);
                    break;
                case "MECHA_KNIGHT":
                    await PowerCmd.Apply<ArtifactPower>(base.Owner.Creature, 2m, base.Owner.Creature, null);
                    await PowerCmd.Apply<BurnPower>(combatState.HittableEnemies, 8m, base.Owner.Creature, null);
                    break;
                case "SPECTRAL_KNIGHT":
                    var drawPileCards = PileType.Draw.GetPile(Owner).Cards;
                    if (drawPileCards.Count > 0)
                    {
                        int maxSelect = Math.Min(5, drawPileCards.Count);
                        int minSelect = Math.Min(0, drawPileCards.Count);
                        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, minSelect, maxSelect)
                        {
                            Cancelable = false
                        };
                        var selectedCards = await CardSelectCmd.FromSimpleGrid(new BlockingPlayerChoiceContext(), drawPileCards, Owner, prefs);
                        foreach (var card in selectedCards)
                        {
                            await CardCmd.Exhaust(choiceContext, card, causedByEthereal: false, skipVisuals: false);
                        }
                    }
                    break;
                case "MAGI_KNIGHT":
                    var upgradableCards = PileType.Draw.GetPile(Owner).Cards.Where(c => c.IsUpgradable).ToList();
                    if (upgradableCards.Count > 0)
                    {
                        int maxSelect = Math.Min(5, upgradableCards.Count);
                        int minSelect = Math.Min(0, upgradableCards.Count);
                        var prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, minSelect, maxSelect)
                        {
                            Cancelable = false
                        };
                        var selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, upgradableCards, Owner, prefs);
                        CardCmd.Upgrade(selectedCards, CardPreviewStyle.GridLayout);
                    }
                    break;
                case "DOORMAKER":
                    var drawPileCards_doormaker = PileType.Draw.GetPile(Owner).Cards;
                    if (drawPileCards_doormaker.Count > 0)
                    {
                        int maxSelect = Math.Min(10, drawPileCards_doormaker.Count);
                        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, maxSelect, maxSelect)
                        {
                            Cancelable = false
                        };
                        var selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, drawPileCards_doormaker, Owner, prefs);
                        foreach (var card in selectedCards)
                        {
                            await CardCmd.Exhaust(choiceContext, card, causedByEthereal: false, skipVisuals: false);
                        }
                    }
                    break;
                case "QUEEN":
                    await PowerCmd.Apply<VulnerablePower>(combatState.HittableEnemies, 99, base.Owner.Creature, null);
                    await PowerCmd.Apply<WeakPower>(combatState.HittableEnemies, 99, base.Owner.Creature, null);
                    await PowerCmd.Apply<FrailPower>(combatState.HittableEnemies, 99, base.Owner.Creature, null);
                    break;
                default:
                    await CreatureCmd.Damage(choiceContext, combatState.HittableEnemies, (DamageVar)base.DynamicVars["DefaultPowerDamage"], base.Owner.Creature);
                    break;
            }
        }
    }
}