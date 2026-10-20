using System;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Holders
{
	public sealed class ESTIMATED_REMAIN_TIME_FOR_LOGIN_ACK : NetPacket
	{
		public ESTIMATED_REMAIN_TIME_FOR_LOGIN_ACK(int waitUserNum, int totalWaitLoginNum, byte last)
		{
			_ = totalWaitLoginNum;
			// 1410 eServer_ESTIMATED_REMAIN_TIME_FOR_LOGIN_ACK: b4 b4 + sequence.
			ns.Write((ushort)Opcodes.eServer_ESTIMATED_REMAIN_TIME_FOR_LOGIN_ACK);
			ns.Write(waitUserNum);
			ns.Write(Math.Max(1, waitUserNum * 3));
			ns.Write(last);
		}
	}
}
