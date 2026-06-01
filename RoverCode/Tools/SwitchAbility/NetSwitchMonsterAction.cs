using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace Rover.RoverCode.Actions;

public class NetSwitchMonsterAction : INetAction
{
    public ulong PlayerNetId { get; set; }
    public string MonsterEntry { get; set; }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(PlayerNetId);
        writer.WriteString(MonsterEntry);
    }

    public void Deserialize(PacketReader reader)
    {
        PlayerNetId = reader.ReadULong();
        MonsterEntry = reader.ReadString();
    }

    public GameAction ToGameAction(Player player)
    {
        return new SwitchMonsterAction
        {
            PlayerNetId = this.PlayerNetId,
            MonsterEntry = this.MonsterEntry
        };
    }
}