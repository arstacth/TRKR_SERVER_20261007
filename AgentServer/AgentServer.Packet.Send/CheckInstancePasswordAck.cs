using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Wire 2462 CHECK_INSTANCE_PASSWORD_ACK. Body is str16 protobuf:
	/// required bool success = 1; optional int32 remaincount = 2 [default=-1];
	/// optional int32 remainseconds = 3 [default=-1].
	/// Client CTRNameChangeManager flag 1 + success → gui\nickname.gui.
	/// </summary>
	public sealed class CheckInstancePasswordAck : NetPacket
	{
		public CheckInstancePasswordAck(bool success, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CHECK_INSTANCE_PASSWORD_ACK);
			byte[] proto = success ? new byte[] { 0x08, 0x01 } : new byte[] { 0x08, 0x00 };
			ns.Write((ushort)proto.Length);
			ns.Write(proto, 0, proto.Length);
			_ = last;
		}
	}
}
