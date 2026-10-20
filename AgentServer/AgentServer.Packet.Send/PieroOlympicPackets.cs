using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PieroOlympic_GetMyInfo_ACK : NetPacket
	{
		public PieroOlympic_GetMyInfo_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_GET_MY_PIERO_OLYMPIC_INFO_ACK);
			// 1410 wire b16.
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}

	public sealed class PieroOlympic_GetShop_ACK : NetPacket
	{
		public PieroOlympic_GetShop_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_GET_SHOP_ACK);
			// 1410 wire b4.
			ns.Write(0);
			_ = last;
		}
	}

	public sealed class PieroOlympic_ShopBuy_ACK : NetPacket
	{
		public PieroOlympic_ShopBuy_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_SHOP_BUY_ITEM_ACK);
			// 1410 wire b2 b4.
			ns.Write((short)0);
			ns.Write(0);
			_ = last;
		}
	}

	public sealed class PieroOlympic_Point_ACK : NetPacket
	{
		public PieroOlympic_Point_ACK(ushort ackOp, int intCount, byte last)
		{
			ns.WriteOP(ackOp);
			for (int i = 0; i < intCount; i++)
			{
				ns.Write(0);
			}
			_ = last;
		}
	}

	public sealed class PieroOlympic_JoinParty_ACK : NetPacket
	{
		public PieroOlympic_JoinParty_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_JOIN_PARTY_ACK);
			// 1410 wire b1 b4.
			ns.Write((byte)0);
			ns.Write(0);
			_ = last;
		}
	}

	public sealed class PieroOlympic_LevelUpReward_ACK : NetPacket
	{
		public PieroOlympic_LevelUpReward_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_GIVE_LEVEL_UP_REWARD_ACK);
			// 1410 wire b4 b4.
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
