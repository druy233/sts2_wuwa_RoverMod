using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rover.Powers;


namespace Rover.Cards;

public class TheEnd() : RoverCard(6,
    CardType.Attack, CardRarity.Rare,
    TargetType.AllEnemies)
{
    private int _timesPlayedThisCombat;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(40m, ValueProp.Move)];
    private int TimesPlayedThisCombat
    {
        get
        {
            return _timesPlayedThisCombat;
        }
        set
        {
            AssertMutable();
            _timesPlayedThisCombat = value;
        }
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
            .WithAttackerAnim("Cast", 0.7f).WithAttackerFx(null, null, "energy_cannon.wav")
            .BeforeDamage(async delegate
            {
                List<Creature> enemies = base.CombatState.Enemies.Where((Creature e) => e.IsAlive).ToList();

                // 获取玩家和敌人的视觉节点
                NCreature sourceNode = NCombatRoom.Instance?.GetCreatureNode(base.Owner.Creature);
                NCreature targetNode = NCombatRoom.Instance?.GetCreatureNode(enemies.Last());
                if (sourceNode == null || targetNode == null) return;
                // 自定义起始位置：玩家位置向上偏移400像素
                Vector2 customStart = sourceNode.GlobalPosition + new Vector2(0, -400);
                Vector2 targetPos = targetNode.GlobalPosition;

                NHyperbeamVfx nHyperbeamVfx = NHyperbeamVfx.Create(customStart, targetPos);
                if (nHyperbeamVfx != null)
                {
                    ((Node)(object)NCombatRoom.Instance?.CombatVfxContainer).AddChildSafely((Node?)(object)nHyperbeamVfx);
                    await Cmd.Wait(1.03f);
                }
                foreach (Creature item in enemies)
                {
                    NHyperbeamImpactVfx nHyperbeamImpactVfx = NHyperbeamImpactVfx.Create(base.Owner.Creature, item);
                    if (nHyperbeamImpactVfx != null)
                    {
                        ((Node)(object)NCombatRoom.Instance?.CombatVfxContainer).AddChildSafely((Node?)(object)nHyperbeamImpactVfx);
                    }
                }
            })
            .Execute(choiceContext);
        TimesPlayedThisCombat++;
        base.EnergyCost.AddThisCombat(-2);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(20m);
    }
}
