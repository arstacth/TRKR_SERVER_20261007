using LocalCommons.Network;
using RoomServer.Holders;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GameUpdateEXP_New : NetPacket
	{
		public GameRoom_GameUpdateEXP_New(int roomKind, Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GAME_RESULT_ACK);
			if (ServerSettingHolder.ServerSettings.useThankOfferingSystem)
			{
				if (roomKind == 183 || roomKind == 184)
				{
					ns.Write(roomKind);
				}
				else
				{
					ns.Write(0);
				}
			}
			ns.Write(User.LadderPoint);
			ns.Write(User.Exp);
			ns.Write(601);
			// After the 601 int the client reads one int, then 12 bytes.
			// A trailing long stops at offset 26 (size 30, need 12) opcode 1655.
			ns.Write(0);
			ns.Fill(12);
			_ = last;
		}
	}
}
