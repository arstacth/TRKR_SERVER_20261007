using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class FirstLoginHandle
	{
		public static void Handle_SelectStartCharacter(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int charid = reader.ReadLEInt16();
			Log.Information("SELECT_START_CHARACTER user={0} charKind={1}", currentAccount.UserID, charid);
			if (currentAccount.AvatarItemDyeing.Count < 24)
			{
				currentAccount.AvatarItemDyeing.AddRange(Enumerable.Repeat(new UserItemDyeing(), 24 - currentAccount.AvatarItemDyeing.Count));
			}
			bool selected = selectStartCharacter(currentAccount, charid, last, out bool alreadyHad);
			if (selected || alreadyHad)
			{
				if (currentAccount.noNickName)
				{
					currentAccount.AwaitingNicknameAfterSelect = true;
					Log.Warning("SELECT_START_CHARACTER → SELECT_OK+avatar(nick) user={0} char={1} alreadyHad={2}",
						currentAccount.UserID, charid, alreadyHad);
					ItemHandle.SendFirstLoginNickSequence(currentAccount, last);
				}
				else
				{
					Client.SendAsync(new FirstLoginMakeStartCharacterOK(last));
					ItemHandle.getCurrentAvatarInfo(currentAccount, bRequestNickName: true, last);
				}
			}
			else
			{
				Client.SendAsync(new FirstLoginMakeStartCharacterFail(last));
			}
		}

		/// <summary>
		/// Tutorial nick step sends empty 2463 (requestCaptchaByActionObj). Official
		/// replies CHECK_INSTANCE_PASSWORD_ACK success so CTRNameChangeManager opens
		/// gui\nickname.gui. Skip the instance-password UI on a private server.
		/// </summary>
		public static void Handle_InstancePassword(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = reader;
			Account user = Client.CurrentAccount;
			Log.Warning("INSTANCE_PASSWORD → CHECK_ACK success (nickname.gui) user={0}",
				user != null ? user.UserID : "?");
			Client.SendAsync(new CheckInstancePasswordAck(success: true, last));
		}

		public static void Handle_SetNewNickName(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			try
			{
				bool flag = false;
				int result = 0;
				int fixedLength = reader.ReadLEInt16();
				string text = reader.ReadBig5StringSafe(fixedLength);
				// Korea R186937: Hangul + ASCII. Thai/CJK kept for mixed clients.
				Regex regex = new Regex("^[0-9A-Za-z\\u0E00-\\u0E7F\\u4E00-\\u9FFF\\uAC00-\\uD7A3]+$");
				int byteCount = Encoding.GetEncoding(949).GetByteCount(text);
				if (byteCount < 4 || byteCount > 12 || !regex.IsMatch(text))
				{
					flag = false;
					result = 2;
				}
				else
				{
					reader.Clear();
					flag = SetNewNickNameCheck(currentAccount, text, out result);
				}
				if (flag && result == 0)
				{
					currentAccount.AwaitingNicknameAfterSelect = false;
					Client.SendAsync(new FirstLoginSetNewNickNameOK(currentAccount.NickName, last));
					Client.SendAsync(new LoginGetNickName_0X1C(currentAccount, last));
					Client.SendAsync(new FirstLogin_CreateFarm_GetMyFarmInfo(currentAccount, last));
					// Now nickname exists — allow GET_AVATAR OK + deferred lobby/Shu burst.
					ItemHandle.getCurrentAvatarInfo(currentAccount, bRequestNickName: false, last);
					if (!currentAccount.DyeListSent)
					{
						Client.SendAsync(new DyedItemList(currentAccount, last));
						currentAccount.DyeListSent = true;
					}
					LoginTrafficManager.SendPostNicknameLoginBurst(Client, currentAccount, last);
				}
				else if (!flag && result == 1)
				{
					Client.SendAsync(new FirstLoginSetNewNickNameFail_0X4C(last));
				}
				else
				{
					Client.SendAsync(new FirstLoginSetNewNickNameFail_0x63(text, last));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private static bool SetNewNickNameCheck(Account User, string NickName, out int result)
		{
			result = 0;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_setNickname";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = NickName;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				result = Convert.ToInt32(mySqlDataReader["result"]);
				if (result == 0)
				{
					User.noNickName = false;
					User.NickName = NickName;
					return true;
				}
			}
			return false;
		}

		private static bool ClearLeftoverStartCharacter(Account User)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using (MySqlCommand cmd = new MySqlCommand(
					"DELETE FROM UserCostumeCharacterSetting WHERE fdUserNum=@u;" +
					"DELETE FROM tblAvatarCharacterSetting WHERE fdUserNum=@u;" +
					"DELETE FROM tblAvatarUserCharacter WHERE fdUserNum=@u;" +
					"UPDATE UserInfoGame SET fdAvatarCharacterSettingNum=0 WHERE fdUserNum=@u;", conn))
				{
					cmd.Parameters.AddWithValue("@u", User.UserNum);
					cmd.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Warning("ClearLeftoverStartCharacter user={0}: {1}", User.UserID, ex.Message);
				return false;
			}
		}

		/// <summary>
		/// After leftover char-item grant fails, still attach a setting row so SELECT_OK can fire.
		/// 12:23 cleared tblAvatarCharacterSetting but left tblAvatarUserCharacter, so the SP
		/// SIGNALed "give item error" and we sent SELECT FAIL 64 (client stayed on StartCharacter).
		/// </summary>
		private static bool EnsureStartCharacterRow(Account User, int charid)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using (MySqlCommand cmd = new MySqlCommand(@"
UPDATE tblAvatarCharacterSetting t
INNER JOIN EssenRandomStartCharacter e ON e.fdCharacter = t.fdCharacter
SET t.fdHead = IFNULL(e.fdHeadKind,0),
    t.fdTopBody = IFNULL(e.fdTopBodyKind,0),
    t.fdDownBody = IFNULL(e.fdDownBodyKind,0),
    t.fdFoot = IFNULL(e.fdFootKind,0),
    t.fdACHead = IFNULL(e.fdACHeadKind,0),
    t.fdACFace = IFNULL(e.fdACFaceKind,0),
    t.fdACHand = IFNULL(e.fdACHandKind,0),
    t.fdACBack = IFNULL(e.fdACBackKind,0),
    t.fdACNeck = IFNULL(e.fdACNeckKind,0),
    t.fdPet = IFNULL(e.fdPetKind,0),
    t.fdExpansion = IFNULL(e.fdExpansionKind,0),
    t.fdACWrist = IFNULL(e.fdACWristKind,0),
    t.fdACBooster = IFNULL(e.fdACBoosterKind,0),
    t.fdACTail = IFNULL(e.fdACTailKind,0)
WHERE t.fdUserNum=@u AND t.fdCharacter=@c;
INSERT INTO tblAvatarCharacterSetting
 (fdUserNum, fdCharacter, fdHead, fdTopBody, fdDownBody, fdFoot,
  fdACHead, fdACFace, fdACHand, fdACBack, fdACNeck, fdPet, fdExpansion,
  fdACWrist, fdACBooster, fdPart, fdACTail)
SELECT @u, e.fdCharacter,
  IFNULL(e.fdHeadKind,0), IFNULL(e.fdTopBodyKind,0), IFNULL(e.fdDownBodyKind,0), IFNULL(e.fdFootKind,0),
  IFNULL(e.fdACHeadKind,0), IFNULL(e.fdACFaceKind,0), IFNULL(e.fdACHandKind,0), IFNULL(e.fdACBackKind,0),
  IFNULL(e.fdACNeckKind,0), IFNULL(e.fdPetKind,0), IFNULL(e.fdExpansionKind,0),
  IFNULL(e.fdACWristKind,0), IFNULL(e.fdACBoosterKind,0), 0, IFNULL(e.fdACTailKind,0)
FROM EssenRandomStartCharacter e
WHERE e.fdCharacter=@c
  AND NOT EXISTS (
    SELECT 1 FROM tblAvatarCharacterSetting s
    WHERE s.fdUserNum=@u AND s.fdCharacter=@c)
LIMIT 1;
UPDATE UserInfoGame g
INNER JOIN tblAvatarCharacterSetting t ON t.fdUserNum=g.fdUserNum AND t.fdCharacter=@c
SET g.fdAvatarCharacterSettingNum = t.fdItemCharacterSettingNum
WHERE g.fdUserNum=@u;", conn))
				{
					cmd.Parameters.AddWithValue("@u", User.UserNum);
					cmd.Parameters.AddWithValue("@c", charid);
					cmd.ExecuteNonQuery();
				}
				using (MySqlCommand q = new MySqlCommand(
					"SELECT COUNT(*) FROM tblAvatarCharacterSetting WHERE fdUserNum=@u AND fdCharacter=@c", conn))
				{
					q.Parameters.AddWithValue("@u", User.UserNum);
					q.Parameters.AddWithValue("@c", charid);
					return Convert.ToInt32(q.ExecuteScalar()) > 0;
				}
			}
			catch (Exception ex)
			{
				Log.Warning("EnsureStartCharacterRow user={0} char={1}: {2}", User.UserID, charid, ex.Message);
				return false;
			}
		}

		private static bool selectStartCharacter(Account User, int charid, byte last, out bool alreadyHad, bool retryLeftover = true)
		{
			List<NetItemInfo> list = new List<NetItemInfo>();
			bool flag = false;
			alreadyHad = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_selectStartCharacter");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.AddParamInt("charKind", charid);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					NetItemInfo netItemInfo = new NetItemInfo();
					netItemInfo.clear();
					netItemInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemdescnum");
					netItemInfo.m_character = mySqlCommandHelper.GetInt("character");
					netItemInfo.m_position = mySqlCommandHelper.GetInt("position");
					netItemInfo.m_kind = mySqlCommandHelper.GetInt("kind");
					netItemInfo.m_tGot = mySqlCommandHelper.GetDateTime("gotDateTime", 0L);
					if (mySqlCommandHelper.IsDBNull("expireTime"))
					{
						netItemInfo.m_bHasExpireTime = false;
						netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
					}
					else
					{
						netItemInfo.m_bHasExpireTime = true;
						netItemInfo.m_expireTime = mySqlCommandHelper.GetDateTime("expiretime", 0L);
						netItemInfo.m_count = mySqlCommandHelper.GetInt("count");
					}
					netItemInfo.m_bUsing = mySqlCommandHelper.GetBoolean("using");
					list.Add(netItemInfo);
				}
				flag = true;
			}
			catch (Exception ex)
			{
				string msg = ex.Message ?? string.Empty;
				bool leftover = msg.IndexOf("have character", StringComparison.OrdinalIgnoreCase) >= 0
					|| msg.IndexOf("give item error", StringComparison.OrdinalIgnoreCase) >= 0
					|| msg.IndexOf("give random character", StringComparison.OrdinalIgnoreCase) >= 0
					|| msg.IndexOf("insert character error", StringComparison.OrdinalIgnoreCase) >= 0;
				if (leftover)
				{
					alreadyHad = true;
					Log.Warning("usp_selectStartCharacter leftover '{0}' user={1} char={2}",
						msg, User.UserID, charid);
					// "give item error" means tblAvatarUserCharacter already has the char.
					// Do not delete those rows (12:23 retry then failed again). Attach a setting.
					bool haveChar = msg.IndexOf("have character", StringComparison.OrdinalIgnoreCase) >= 0;
					if (haveChar && retryLeftover && User.noNickName && ClearLeftoverStartCharacter(User))
					{
						Log.Warning("cleared leftover start char, retry select user={0} char={1}",
							User.UserID, charid);
						alreadyHad = false;
						bool retried = selectStartCharacter(User, charid, last, out alreadyHad, retryLeftover: false);
						if (retried || alreadyHad)
							return true;
					}
					if (User.noNickName && EnsureStartCharacterRow(User, charid))
					{
						alreadyHad = true;
						Log.Warning("EnsureStartCharacterRow user={0} char={1}", User.UserID, charid);
						return true;
					}
				}
				else
				{
					Log.Error("usp_selectStartCharacter Error: {0}", ex.ToString());
				}
			}
			if (flag && !User.noNickName)
			{
				User.onRecvActiveFuncItem(flag, list, bEquipmentItem: false);
			}
			return flag;
		}
	}
}
