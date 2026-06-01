using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Rover.Relics;

namespace Rover.RoverCode.Actions
{
    public class SwitchMonsterAction : GameAction
    {
        public ulong PlayerNetId { get; set; }
        public string MonsterEntry { get; set; }

        public override ulong OwnerId => PlayerNetId;
        public override GameActionType ActionType => GameActionType.Any;

        public override INetAction ToNetAction()
        {
            return new NetSwitchMonsterAction { PlayerNetId = PlayerNetId, MonsterEntry = MonsterEntry };
        }

        protected override async Task ExecuteAction()
        {
            Log.Info($"SwitchMonsterAction.ExecuteAction方法寻找玩家ID={PlayerNetId}, 怪物名称为={MonsterEntry}");
            var runState = RunManager.Instance.DebugOnlyGetState();
            if (runState == null) return;
            var player = runState.Players.FirstOrDefault(p => p.NetId == PlayerNetId);
            if (player == null) return;
            var relic = player.GetRelic<ObscuraLumis>();
            if (relic == null) return;
            relic.SwitchToMonsterInternal(new ModelId("MONSTER", MonsterEntry));
        }
    }
}