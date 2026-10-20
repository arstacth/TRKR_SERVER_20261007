using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RoomChat : NetPacket
	{
		public GameRoom_RoomChat(byte pos, byte[] text, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ROOM_CHATTING);
			// R186937 ACK is 1-byte room slot, then the ushort length Write(text) adds.
			// A short slot steals the length's low byte (len 6 becomes 0x0600) and the
			// client reports RemainSize negative on 1979 in room, park, and farm.
			ns.Write(pos);
			ns.Write(text, 0);
			_ = last;
		}
	}
}
