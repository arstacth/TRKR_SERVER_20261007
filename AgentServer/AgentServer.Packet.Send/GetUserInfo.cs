using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using AgentServer.Structuring;
using AgentServer.Structuring.Fishing;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class GetUserInfo : NetPacket
	{
		public GetUserInfo(Account User, string nickname, List<UserItemDyeing> AvatarItemDyeing, List<UserFishedItem> fishrecord, byte last)
		{
			_ = AvatarItemDyeing;
			ns.WriteOP(Opcodes.eServer_GET_USER_INFO_ACK);
			int value = 0;
			int num = (int)ns.Position;
			ns.Write(value);
			ns.WriteAnsiFixed_intSize(nickname);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_getUserInfo";
				mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					AvatarLock.UserInfo_AvatarLock_Load(Convert.ToInt32(mySqlDataReader["fdUserNum"]), out var AvatarLock_RealItem, out var AvatarLock_CostumeItem);
					bool flag = AvatarLock_RealItem.Count > 0;
					short num2 = Convert.ToInt16(mySqlDataReader["fdCostumeMode"]);
					// R186937: bool + exp + 383-byte slot (real half + costume half + mode).
					ns.Write(Convert.ToByte(mySqlDataReader["attribute"]));
					ns.Write(Convert.ToInt64(mySqlDataReader["fdExp"]));
					WriteAvatarHalf(
						Convert.ToUInt16(mySqlDataReader["character"]),
						Convert.ToUInt16(mySqlDataReader["head"]),
						Convert.ToUInt16(mySqlDataReader["topBody"]),
						Convert.ToUInt16(mySqlDataReader["downBody"]),
						Convert.ToUInt16(mySqlDataReader["foot"]),
						Convert.ToUInt16(mySqlDataReader["acHead"]),
						Convert.ToUInt16(mySqlDataReader["acFace"]),
						Convert.ToUInt16(mySqlDataReader["acHand"]),
						Convert.ToUInt16(mySqlDataReader["acBack"]),
						Convert.ToUInt16(mySqlDataReader["acNeck"]),
						Convert.ToUInt16(mySqlDataReader["pet"]),
						Convert.ToUInt16(mySqlDataReader["expansion"]),
						Convert.ToUInt16(mySqlDataReader["acWrist"]),
						Convert.ToUInt16(mySqlDataReader["acBooster"]),
						Convert.ToUInt16(mySqlDataReader["acTail"]));
					WriteAvatarHalf(
						Convert.ToUInt16(mySqlDataReader["cos_character"]),
						Convert.ToUInt16(mySqlDataReader["cos_head"]),
						Convert.ToUInt16(mySqlDataReader["cos_topBody"]),
						Convert.ToUInt16(mySqlDataReader["cos_downBody"]),
						Convert.ToUInt16(mySqlDataReader["cos_foot"]),
						Convert.ToUInt16(mySqlDataReader["cos_acHead"]),
						Convert.ToUInt16(mySqlDataReader["cos_acFace"]),
						Convert.ToUInt16(mySqlDataReader["cos_acHand"]),
						Convert.ToUInt16(mySqlDataReader["cos_acBack"]),
						Convert.ToUInt16(mySqlDataReader["cos_acNeck"]),
						Convert.ToUInt16(mySqlDataReader["cos_pet"]),
						Convert.ToUInt16(mySqlDataReader["cos_expansion"]),
						Convert.ToUInt16(mySqlDataReader["cos_acWrist"]),
						Convert.ToUInt16(mySqlDataReader["cos_acBooster"]),
						Convert.ToUInt16(mySqlDataReader["cos_acTail"]));
					ns.Write((byte)num2);
					long value2 = 0L;
					if (Convert.IsDBNull(mySqlDataReader["fdGuildName"]))
					{
						ns.WriteAnsiFixed_intSize(string.Empty);
						ns.WriteAnsiFixed_intSize("http://0");
					}
					else
					{
						value2 = Convert.ToInt64(mySqlDataReader["fdGuildNum"]);
						ns.WriteAnsiFixed_intSize(mySqlDataReader["fdGuildName"].ToString());
						ns.WriteAnsiFixed_intSize("http://0");
					}
					ns.Write(Convert.ToInt32(mySqlDataReader["couplenum"]));
					ns.Write(0);
					ns.WriteAnsiFixed_intSize(mySqlDataReader["matename"].ToString());
					ns.Write((mySqlDataReader["createtime"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["createtime"])));
					ns.Write(0L);
					ns.Write((mySqlDataReader["ringChangedTime"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["ringChangedTime"])));
					ns.Fill(12);
					ns.Write(Convert.ToInt32(mySqlDataReader["coupleLevel"]));
					ns.Write(0L);
					ns.Write(Convert.ToInt32(mySqlDataReader["accumulateExp"]));
					ns.Write(0);
					ns.Write((byte)0);
					ns.Write(Convert.ToInt32(mySqlDataReader["emblemCount"]));
					ns.Write(0);
					ns.Write(Convert.ToInt32(mySqlDataReader["playingTime"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["fdLikeable"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["fdGameOption"]));
					ns.Write(value2);
					ns.Write(Utility.NormalizeFarmUnique(Convert.ToInt32(mySqlDataReader["FarmUniqueNum"])));
					ns.Write(0);
					ns.Write(0);
					ns.Write(0);
					ns.Write(0);
					ns.Write(0);
					ns.Write(0);
					ns.WriteAnsiFixed_intSize(mySqlDataReader["Farmname"].ToString());
					ns.WriteAnsiFixed_intSize(string.Empty);
					ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["FarmExpireDateTime"])));
					ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["FarmCreateDateTime"])));
					ns.Write(string.IsNullOrEmpty(mySqlDataReader.GetString("FarmPassword")));
					ns.Write((byte)0);
					ns.Write((byte)0);
					ns.Write(0);
					ns.Write(0);
					ns.Write(0);
					ns.Write(0L);
					ns.Write(Convert.ToInt64(mySqlDataReader["FarmExp"]));
					ns.Write(0L);
					ns.Fill(6);
					int farmType = Convert.ToInt32(mySqlDataReader["type"]);
					long expireTs = Convert.IsDBNull(mySqlDataReader["expireTime"])
						? 0L
						: Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["expireTime"]));
					ns.Write(farmType);
					ns.Write(expireTs);
					int maxSave = Convert.ToInt32(mySqlDataReader["maxSavableCount"]);
					int maxRecv = Convert.ToInt32(mySqlDataReader["maxReceivableCount"]);
					if (maxSave < 0)
					{
						maxSave = 127;
					}
					if (maxSave > 255)
					{
						maxSave = 255;
					}
					if (maxRecv < 0)
					{
						maxRecv = 20;
					}
					if (maxRecv > 255)
					{
						maxRecv = 255;
					}
					ns.Write((byte)maxSave);
					ns.Write((byte)maxRecv);
					ns.Write((byte)0);
					ns.Write((byte)0);
					ns.Write(farmType);
					ns.Write(expireTs);
					ns.Write(Convert.ToInt32(mySqlDataReader["TopRank"]));
					ns.Write(6);
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal0"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal1"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal2"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal3"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal4"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal5"]));
					ns.Write(0);
					ns.Write(Convert.ToInt32(mySqlDataReader["TalesBookBG"]));
					ns.Write(0);
					ns.Write(0);
					ns.Write(fishrecord.Count);
					foreach (UserFishedItem item in fishrecord)
					{
						ns.Write(item.ItemNum);
						ns.Write(item.Size);
						ns.Write(item.Count);
					}
					ns.Write(0);
					ns.Write(0);
					ns.Write(0);
					ns.Write(0);
					ns.Write((byte)0);
					ns.Write(AvatarLock_RealItem.Count);
					foreach (int avatarItem in AvatarLock_RealItem)
					{
						ns.Write(avatarItem);
					}
					ns.Write(AvatarLock_CostumeItem.Count);
					foreach (int costumeItem in AvatarLock_CostumeItem)
					{
						ns.Write(costumeItem);
					}
					ns.Write((byte)(flag ? num2 : 0));
					ns.Write(flag);
				}
				else
				{
					value = 64;
				}
			}
			ns.Write(last);
			ns.Seek(num, SeekOrigin.Begin);
			ns.Write(value);
		}

		private void WriteAvatarHalf(params ushort[] wear15)
		{
			for (int i = 0; i < 15; i++)
			{
				ushort v = i < wear15.Length ? wear15[i] : (ushort)0;
				ns.Write(v == ushort.MaxValue ? (ushort)0 : v);
			}
			for (int i = 0; i < 7; i++)
			{
				ns.Write((ushort)0);
			}
			ns.Write((ushort)0);
			ns.Fill(Conf.AvatarDyePadBytes);
			ns.Fill(15);
		}
	}
}
