using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ReportSystemUserReportInfoAck : NetPacket
	{
		public ReportSystemUserReportInfoAck(byte last)
		{
			_ = last;
			// 1410 eServer_REPORT_SYSTEM_USER_REPORT_INFO_ACK (2036): b4.
			ns.Write((ushort)Opcodes.eServer_REPORT_SYSTEM_USER_REPORT_INFO_ACK);
			ns.Write(0);
			ns.Write((ushort)0);
		}
	}
}
