using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetLobbyQuestEventNotify : NetPacket
	{
		public GetLobbyQuestEventNotify(byte last)
		{
			ns.WriteOP(Opcodes.eServer_LOBBY_QUEST_EVENT_INFO_NOTIFY);
			ns.Write((byte)1);
			ns.Write(MissionHolder.LobbyQuestEventTypes.Count);
			foreach (int type in MissionHolder.LobbyQuestEventTypes)
			{
				ns.Write(type);
			}
			_ = last;
		}
	}
}
