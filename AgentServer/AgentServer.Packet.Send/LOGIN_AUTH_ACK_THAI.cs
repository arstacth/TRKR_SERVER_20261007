using AgentServer;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using Serilog;

namespace AgentServer.Packet.Send
{
	public sealed class LOGIN_AUTH_ACK : NetPacket
	{
		public LOGIN_AUTH_ACK(string UserID, bool LoginCheckOK)
		{
			ns.WriteOP(Opcodes.eServer_LOGIN_AUTH_ACK);
			// Korea LOGIN_AUTH_ACK: int result, str16, int64.
			ns.Write(LoginCheckOK ? 0 : 13);
			ns.WriteAnsiFixed_intSize(LoginCheckOK ? (UserID ?? string.Empty) : string.Empty);
			ns.Write(0L);
			if (Conf.ProtocolDebug)
			{
				Log.Information("LOGIN_AUTH_ACK ok:{0} user:{1} ns:{2}", LoginCheckOK, UserID, ns.Length);
			}
		}
	}
}
