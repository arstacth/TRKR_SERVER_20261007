using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GET_AVATAR_ITEMS_ACK : NetPacket
	{
		public GET_AVATAR_ITEMS_ACK(int charid, int position, byte remainpage, short startindex, List<NetItemInfo> AvatarItems, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_AVATAR_ITEMS_ACK);
			// Korea: ushort char, int position, byte last-page, short start, short count, 36-byte rows.
			// The item rows are always read. A non-zero last-page byte skips the UI update,
			// so a hardcoded 1 left the wardrobe empty.
			ns.Write((ushort)charid);
			ns.Write(position);
			ns.Write(remainpage);
			ns.Write(startindex);
			ns.Write((short)AvatarItems.Count);
			foreach (NetItemInfo AvatarItem in AvatarItems)
			{
				ns.Write((ushort)(int)AvatarItem.m_character);
				ns.Write((ushort)(int)AvatarItem.m_position);
				ns.Write((ushort)(int)AvatarItem.m_kind);
				ns.Write(AvatarItem.m_iItemDescNum);
				ns.Write(AvatarItem.m_expireTime);
				ns.Write(AvatarItem.m_tGot);
				ns.Write(AvatarItem.m_count);
				ns.Write(AvatarItem.m_exp);
				ns.Write(AvatarItem.m_bHasExpireTime);
				ns.Write(AvatarItem.m_bUsing);
			}
			// Korea R186937 RemainSize=1 when last was written — client does not pop sequence on this ACK.
			_ = last;
		}
	}
}
