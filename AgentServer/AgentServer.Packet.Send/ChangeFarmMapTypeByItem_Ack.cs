using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ChangeFarmMapTypeByItem_Ack : NetPacket
	{
		public ChangeFarmMapTypeByItem_Ack(int FarmUniqueNum, int terrainType, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ChangeFarmMapTypeByItem_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(terrainType);
			_ = last;
		}
	}
}
