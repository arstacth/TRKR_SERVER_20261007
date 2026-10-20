using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GET_ACTIVE_FUNC_ITEM_IN_POSITION_ACK : NetPacket
	{
		public GET_ACTIVE_FUNC_ITEM_IN_POSITION_ACK(int position, byte remainpage, short startindex, List<NetItemInfo> AvatarItems, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_ACK);
			ns.Write(0);
			ns.Write((int)Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_IN_POSITION_REQ);
			ns.Write(position);
			// Korea: layout byte 0 (36-byte rows), flag byte, start, count, rows, extra-count short.
			_ = remainpage;
			ns.Write((byte)0);
			ns.Write((byte)0);
			ns.Write(startindex);
			ns.Write((short)AvatarItems.Count);
			GET_ACTIVE_FUNC_ITEM_ACK.WriteItems(ns, AvatarItems);
			ns.Write((short)0);
			_ = last;
		}
	}
}
