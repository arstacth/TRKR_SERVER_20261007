using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class NewAttendanceGetUserInfoAck : NetPacket
	{
		public NewAttendanceGetUserInfoAck(byte last)
		{
			// 1410 eServer_NEW_ATTENDANCE_GET_USER_INFO_ACK (197): b4.
			ns.Write((ushort)Opcodes.eServer_NEW_ATTENDANCE_GET_USER_INFO_ACK);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
