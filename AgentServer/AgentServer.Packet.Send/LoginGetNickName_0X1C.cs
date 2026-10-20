using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginGetNickName_0X1C : NetPacket
	{
		public LoginGetNickName_0X1C(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_NICKNAME_ACK);
			if (User.noNickName)
			{
				// R186937 dbgtrace: result 0x37 logs ENTER_NEW_NICKNAME_ACK (opens first-login /
				// char-select + nickname path). Result 57 logs GET_NICKNAME_ACK FAILED (00:38:13).
				ns.Write(0x37);
			}
			else
			{
				ns.Write(0);
				ns.WriteAnsiFixed_intSize(User.NickName ?? string.Empty);
			}
		}
	}
}
