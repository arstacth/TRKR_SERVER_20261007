using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ReportSystemUserInfoAck : NetPacket
	{
		public ReportSystemUserInfoAck(byte last)
		{
			_ = last;
			// 1410 eServer_REPORT_SYSTEM_USER_INFO_ACK (292). Body stays i32 + u8 + i32 + i32.
			ns.Write((ushort)Opcodes.eServer_REPORT_SYSTEM_USER_INFO_ACK);
			ns.Write(0);
			ns.Write((byte)0);
			ns.Write(0);
			ns.Write(0);
		}
	}
}
