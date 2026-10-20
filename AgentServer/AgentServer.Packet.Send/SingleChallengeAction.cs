using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SingleChallengeAction : NetPacket
	{
		/// <summary>
		/// Match Korea R186937 give-up: ActionType, 0, Max, 0, Max, 0 — then pad to 10 ints.
		/// Field7=1 left the client looping OnRecvChallengeMapNPCMatchEndAck.
		/// </summary>
		public SingleChallengeAction(int ActionType, int MapNum, int GoalSecMs, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CHALLENGE_MAP_END_ACK);
			ns.Write(ActionType);
			ns.Write(0);
			ns.Write(int.MaxValue);
			ns.Write(0);
			ns.Write(int.MaxValue);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			_ = MapNum;
			_ = GoalSecMs;
			_ = last;
		}
	}
}
