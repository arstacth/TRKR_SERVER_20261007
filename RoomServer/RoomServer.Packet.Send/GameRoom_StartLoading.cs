using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_StartLoading : NetPacket
	{
		public GameRoom_StartLoading(int mapid, int randseed, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_START_LOADING_ACK);
			ns.Write(mapid);
			ns.Write(randseed);
			ns.Write((byte)0);
			// dbgtrace 18:01:45 Overpop 11 11 4 opcode 766. Two ready players
			// already passed the start check; the client left because this ACK
			// ended after the flag byte. 0 = no extra bonus map (BonusStageSelect
			// already swapped PlayingMapNum when a bonus map was picked).
			ns.Write(0);
			_ = last;
		}
	}
}
