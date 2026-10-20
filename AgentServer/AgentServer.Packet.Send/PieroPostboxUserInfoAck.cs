using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PieroPostboxUserInfoAck : NetPacket
	{
		public PieroPostboxUserInfoAck(byte last)
		{
			ns.Write((ushort)Opcodes.eServer_PIERO_POSTBOX_USER_INFO_ACK);
			ns.Write(0);
			_ = last;
		}
	}
}
