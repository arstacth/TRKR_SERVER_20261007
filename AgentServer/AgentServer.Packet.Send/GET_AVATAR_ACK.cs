using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GET_AVATAR_ACK : NetPacket
	{
		public GET_AVATAR_ACK(Account User, bool bRequestNickName, byte last)
		{
			User.EnsureAvatarDyeingSlots();
			ns.WriteOP(Opcodes.eServer_GET_AVATAR_ACK);
			ns.Write(0);
			long bodyStart = ns.Position;
			WriteAvatarHalf(User, costume: false, 0);
			WriteAvatarHalf(User, costume: true, 12);
			ns.Write(User.advancedAvatarInfo.isUseCostume);
			// R186937: memcpy 0x17F (383) then one flag byte. Flag=1 → GET_NICKNAME.
			const int bodyNeed = 383;
			int written = (int)(ns.Position - bodyStart);
			if (written < bodyNeed)
			{
				ns.Fill(bodyNeed - written);
			}
			else if (written > bodyNeed)
			{
				ns.Position = bodyStart + bodyNeed;
			}
			ns.Write(bRequestNickName ? (byte)1 : (byte)0);
			_ = last;
		}

		private void WriteAvatarHalf(Account user, bool costume, int dyeStart)
		{
			AdvancedAvatarInfo av = user.advancedAvatarInfo;
			for (byte i = 0; i < 15; i = (byte)(i + 1))
			{
				ns.Write(costume
					? (ushort)(int)av.m_costumeAvatarInfo.m_nItemPartArry[i]
					: (ushort)(int)av.m_realAvatarInfo.m_nItemPartArry[i]);
			}
			for (byte i = 0; i < 7; i = (byte)(i + 1))
			{
				ns.Write(costume
					? (ushort)(int)av.m_costumeAvatarInfo.m_nGameAccArry[i]
					: (ushort)(int)av.m_realAvatarInfo.m_nGameAccArry[i]);
			}
			ns.Write(costume
				? (ushort)(int)av.m_costumeAvatarInfo.m_nEFItemArry[0]
				: (ushort)(int)av.m_realAvatarInfo.m_nEFItemArry[0]);
			for (int i = 0; i < 12; i++)
			{
				UserItemDyeing dye = user.AvatarItemDyeing[dyeStart + i];
				// Same 191-byte half as the room avatar blob: dye part is a short.
				// A byte part left each half 15 bytes short and the salon preview
				// read colors out of the end pad.
				ns.Write((ushort)dye.DyeingPart);
				ns.Write(dye.Color1, 0, 3);
				ns.Write(dye.Color2, 0, 3);
				ns.Write(dye.Color3, 0, 3);
			}
			ns.Fill(13);
		}
	}
}
