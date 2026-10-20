using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;
using Serilog;

namespace RoomServer.Packet.Send
{
	public sealed class SendFarmInfo : NetPacket
	{
		// Opcode 91 is the size-171 farm ACK. This packet is opcode 666.
		private const long PremiumUnsetMagic = 1842465389770955L;

		public SendFarmInfo(Account User, NormalRoom room, int FarmUniqueNum, byte last)
		{
			// R186937 enter reader 0x1409b43b0. Same header as park (GameRoom_SendRoomInfo).
			// Kind 75 then reads 0x1408b5250 + 0x140968990 before the final byte.
			// Padding the old tail (Overpop 186/189/190 needing 1) never reached that reader.
			int map = FarmMapNum(room.FarmRoomInfo.FarmTypeNum);
			string roomName = room.Name ?? string.Empty;
			string roomPassword = room.Password ?? string.Empty;
			ns.WriteOP(Opcodes.eServer_ENTER_ROOM_ACK);
			ns.Write(0);
			ns.Write(room.RoomKindID);
			ns.WriteBIG5Fixed_shortSize(roomName);
			ns.Write(room.MaxPlayersCount);
			ns.WriteBIG5Fixed_shortSize(roomPassword);
			ns.Write(2);
			ns.Write(room.ID);
			ns.Write(User.RoomPos);
			ns.Write(map);
			ns.Write(5);
			ns.Write(room.IsTeamPlay);
			ns.Write(room.IsStepOn);
			ns.Write((byte)room.ItemType);
			ns.Write(map);
			ns.Write((byte)0);
			ns.Write(Utility.CurrentTimeMilliseconds());
			ns.Write(room.PosWeight);
			ns.Write(0L);
			ns.Write(0L);
			// Raw unique. Packing unique<<16 here is what opened map_s2\farm_65536.trv.
			// Type slot stays 0. Map is MapNum above (69+type → 70 = farm_01).
			ns.Write(FarmUniqueNum);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(room.FarmRoomInfo.FarmName ?? string.Empty);
			ns.WriteAnsiFixed_intSize(room.FarmRoomInfo.MasterName ?? string.Empty);
			ns.Write(FixFarmTime(room.FarmRoomInfo.ExpireTime));
			ns.Write(FixFarmTime(room.FarmRoomInfo.CreateTime));
			bool ownFarm = (User.MyFarmUniqueNum > 0 && User.MyFarmUniqueNum == FarmUniqueNum)
				|| room.FarmRoomInfo.IsMaster;
			if (ownFarm && User.MyFarmUniqueNum <= 0 && FarmUniqueNum > 0)
			{
				User.MyFarmUniqueNum = FarmUniqueNum;
			}
			ns.Write(string.IsNullOrEmpty(room.FarmRoomInfo.Password));
			ns.Write(ownFarm || room.FarmRoomInfo.isPublic);
			ns.Write((byte)(ownFarm ? 1 : 0));
			ns.Write(room.FarmRoomInfo.TotalCount);
			ns.Write(room.FarmRoomInfo.TodaysVisitorCount);
			// Second owner flag. GetMyFarmInfo writes 1 here; 0 kept the farm in visitor mode.
			ns.Write(ownFarm ? 1 : 0);
			long premExp = room.FarmRoomInfo.PremiumFarmExpireDateTime;
			ns.Write(premExp > 0L ? FixFarmTime(premExp) : PremiumUnsetMagic);
			int farmExp = room.FarmRoomInfo.farmExp;
			if (farmExp < 1 && User.MyFarmInfo != null)
			{
				farmExp = User.MyFarmInfo.farmExp;
			}
			ns.Write(Utility.FarmHudExp((long)farmExp));
			ns.Write((byte)0);
			ns.Write((byte)0);
			ns.Write((byte)0);
			ns.WriteAnsiFixed_intSize(string.Empty);
			ns.Write((byte)0);
			ns.Write((byte)0);
			_ = last;
			Log.Information("SendFarmInfo bytes={0} farm={1} own={2} my={3} kind={4} map={5}",
				ns.Length, FarmUniqueNum, ownFarm, User.MyFarmUniqueNum, room.RoomKindID, map);
		}

		internal static int FarmTypeWire(int farmTypeNum = 1)
		{
			if (farmTypeNum <= 0)
			{
				farmTypeNum = 1;
			}
			return farmTypeNum;
		}

		internal static int PackedFarmTypeNum(int farmTypeNum = 1)
		{
			return FarmTypeWire(farmTypeNum);
		}

		internal static int FarmMapNum(int farmTypeNum)
		{
			if (farmTypeNum <= 0)
			{
				farmTypeNum = 1;
			}
			return 69 + farmTypeNum;
		}

		private static long FixFarmTime(long timeMs)
		{
			const long minOk = 946684800000L; // 2000-01-01
			const long maxOk = 2145916800000L; // ~2038
			if (timeMs <= 0L || timeMs < minOk)
			{
				return maxOk;
			}
			if (timeMs > maxOk)
			{
				return maxOk;
			}
			return timeMs;
		}
	}
}
