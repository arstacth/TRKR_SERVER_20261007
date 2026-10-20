using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GameOptionGetAck : NetPacket
	{
		public GameOptionGetAck(int option, byte last)
		{
			// 1410 eServer_GAME_OPTION_GET_ACK (815): 3 str16 + 8 flag bytes.
			// Live ACK size is 17; omitting last left Remain=1. Flags are the little-endian
			// GameOption int (and a trailing 0 int), matching Korea R186937 packing.
			ns.Write((ushort)Opcodes.eServer_GAME_OPTION_GET_ACK);
			ns.Write((ushort)0);
			ns.Write((ushort)0);
			ns.Write((ushort)0);
			ns.Write(option);
			ns.Write(0);
			// Size 17 Remain 1 — packed GET_ACK is 16 bytes (no TCP last).
			_ = last;
		}
	}
}
