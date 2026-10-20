using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AgentServer;
using AgentServer.Cryptography;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using NetMsg.LBS;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class LoginHandle
	{
		public static void Handle_LoginCheck(ClientConnection Client, PacketReader reader)
		{
			try
			{
				byte[] raw = reader.Buffer;
				if (Conf.ProtocolDebug)
				{
					Log.Information("Login raw ({0}): {1}", raw.Length, Utility.ByteArrayToString(raw));
				}
				int fixedLength = reader.ReadLEInt16();
				string text = reader.ReadBig5StringSafe(fixedLength);
				bool flag = false;
				string loginUser = text ?? string.Empty;
				// Korea R186937: -authkey is base64("user:password"), e.g. dGVzdDp0ZXN0 = test:test.
				if (TryDecodeAuthKeyUserPass(text, out string credUser, out string credPass))
				{
					loginUser = credUser;
					if (!new Regex("='\"").IsMatch(loginUser) && loginUser.Length != 0
						&& CheckUserAccountResult(loginUser, credPass) == 1)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					Log.Warning("Login auth failed user={0} ip={1}", loginUser, Client.IP);
				}
				if (Conf.ProtocolDebug)
				{
					Log.Information("LoginCheck user:{0} remain:{1} ok:{2}", loginUser, reader.Remaining, flag);
				}
				if (flag)
				{
					byte[] array = new byte[16];
					new RNGCryptoServiceProvider().GetBytes(array);
					byte[] xorKey = Encrypt.EncryptKey(array);
					long num = Utility.CurrentTimeMilliseconds();
					Account account2 = (Client.CurrentAccount = new Account
					{
						UserID = loginUser,
						Connection = Client,
						LastIp = Client.IP,
						Port = (short)((IPEndPoint)Client.EP).Port,
						EncryptKey = array,
						XorKey = xorKey,
						Session = Client.session,
						LoginDateTime = DateTime.Now,
						bLogin = false,
						LoginAuthPassed = true,
						LastCheckTime = num,
						LastPingTime = num
					});
					ClientConnection.CurrentAccounts.TryAdd(Client.session, Client.CurrentAccount);
				}
				else
				{
					if (Client.CurrentAccount != null)
					{
						Client.CurrentAccount.LoginAuthPassed = false;
						Client.CurrentAccount.UserID = loginUser ?? string.Empty;
					}
					Log.Warning("{0} Login Fail! ip:{1}", loginUser, Client.IP);
				}
				Client.SendAsync(new LOGIN_AUTH_ACK(loginUser, flag));
				ClientConnection.DDOS_IP.TryRemove(Client.IP, out var _);
			}
			catch (Exception ex)
			{
				Log.Error("LoginCheck Error:{0}", ex.ToString());
			}
		}

		public static void Handle_GetClientKey(ClientConnection Client, PacketReader reader)
		{
			try
			{
				if (Client.CurrentAccount == null || !Client.CurrentAccount.LoginAuthPassed)
				{
					Log.Warning("GetClientKey rejected: login auth not passed ip:{0}", Client.IP);
					return;
				}
				Client.CurrentAccount.GotClientKey = true;
				byte[] collection = reader.ReadByteArray(255);
				List<byte> list = new List<byte>();
				list.AddRange(collection);
				list.Add(0);
				byte[] key = Encrypt.TFUNC_1_W(list.ToArray(), Client.CurrentAccount.EncryptKey, Conf.ServerIP);
				Client.SendAsync(new LoginGenKey(key));
			}
			catch (Exception ex)
			{
				Log.Error("GetClientKey Error:{0}", ex.Message);
			}
		}

		public static void Handle_LoginSuccess(ClientConnection Client, PacketReader reader, byte last)
		{
			reader.ReadByte();
			short num = reader.ReadLEInt16();
			int fixedLength = reader.ReadLEInt16();
			reader.ReadBig5StringSafe(fixedLength);
			int fixedLength2 = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength2);
			int fixedLength3 = reader.ReadLEInt16();
			reader.ReadBig5StringSafe(fixedLength3);
			Account currentAccount = Client.CurrentAccount;
			if (currentAccount == null || !currentAccount.LoginAuthPassed)
			{
				Log.Warning("LOGIN_REQ rejected: login auth not passed ip:{0}", Client.IP);
				Client.SendAsync(new LoginError(13, last, 0));
				return;
			}
			if (Conf.ProtocolDebug)
			{
				Log.Information("Client protocol version userid:{0} ver:{1}", currentAccount.UserID, num);
			}
			if (num < 1)
			{
				currentAccount.isBlocked = true;
				Log.Error("Incorrect Protocol version userid : {0}, server(4,{1}max), client(4,{2}max)", currentAccount.UserID, (short)Opcodes.eUDPProtocol_MAX, num);
				Client.SendAsync(new LoginError(5, last, 0));
				return;
			}
			if (Conf.HashCheck && !ServerSettingHolder.HashList.Contains(text))
			{
				currentAccount.isBlocked = true;
				Log.Error("InCorrect hash. {0} : {1}", currentAccount.UserID, text);
				Client.SendAsync(new LoginError(8, last, 0));
				return;
			}
			ServerStatus.LBServerActor.Tell(new AccountCheckRequest
			{
				Session = currentAccount.Session,
				UID = currentAccount.UserID,
				ServerID = ServerStatus.MyAgentID
			});
			if (ClientConnection.CurrentAccounts.Count((KeyValuePair<int, Account> c) => c.Value.isLogin) > Conf.MaxUserCount)
			{
				currentAccount.isBlocked = true;
				Log.Error("User [{0}] can't login because server full!", currentAccount.UserID);
				Client.SendAsync(new LoginError(7, last, 0));
			}
			else
			{
				LoginTrafficManager.LoginProcess(Client, last);
			}
		}

		public static void Handle_NOTIFY_MY_UDP(ClientConnection Client, PacketReader reader, byte last)
		{
			int n = reader.Remaining;
			byte[] info = null;
			if (n > 0)
			{
				info = reader.ReadByteArray(n > 48 ? 48 : n);
				byte[] padded = new byte[48];
				Buffer.BlockCopy(info, 0, padded, 0, info.Length);
				Client.CurrentAccount.UDPInfo = padded;
			}
			Client.SendAsync(new Login_NOTIFY_MY_UDP(last));
			if (Conf.ProtocolDebug)
			{
				Log.Information("NOTIFY_MY_UDP from {0} bytes {1} {2}", Client.CurrentAccount.UserID, n,
					info != null ? BitConverter.ToString(info, 0, Math.Min(info.Length, 32)) : "");
			}
		}

		public static void Handle_GetNickName(ClientConnection Client, PacketReader reader, byte last)
		{
			Account user = Client.CurrentAccount;
			// 11:31: GET_AVATAR flag=1 → GET_NICKNAME → ENTER_NEW 0x37 after SELECT.
			// Client stayed idle; 0x37 is openIntro (vtable+0x388) and drops StartCharacter.
			if (user.noNickName && user.AwaitingNicknameAfterSelect)
			{
				Log.Warning("GET_NICKNAME skip ENTER_NEW after SELECT user={0}", user.UserID);
				return;
			}
			Client.SendAsync(new LoginGetNickName_0X1C(user, last));
		}

		public static void Handle_GetUserCash(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (currentAccount.CashNeedUpdateFromDB)
			{
				getUserCash(currentAccount);
			}
			Client.SendAsync(new LoginGetUserCash(currentAccount, last));
		}

		public static void Handle_FF7F01(ClientConnection Client, byte last)
		{
			Client.SendAsync(new Login_FF7F01_0x180(last));
		}

		public static void Handle_ReportSystemUserInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new ReportSystemUserInfoAck(last));
		}

		public static void Handle_ReportSystemUserReportInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new ReportSystemUserReportInfoAck(last));
		}

		public static void Handle_PieroPostboxUserInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new PieroPostboxUserInfoAck(last));
		}

		public static void Handle_82(ClientConnection Client, byte last)
		{
			Client.SendAsync(new Login_82_0x83(Client.CurrentAccount, last));
		}

		public static void Handle_GetCommunityAgentServer(ClientConnection Client, byte last)
		{
			Client.SendAsync(new GetCommunityAgentServer(last));
		}

		public static void Handle_GetExtraAbilities(ClientConnection Client, byte last)
		{
			getExtraAbilities(Client.CurrentAccount, last);
		}

		/// <summary>
		/// -authkey value is base64 of "user:password". Returns false when the string is not that form.
		/// </summary>
		private static bool TryDecodeAuthKeyUserPass(string authKey, out string userId, out string password)
		{
			userId = null;
			password = null;
			if (string.IsNullOrWhiteSpace(authKey))
			{
				return false;
			}
			string trimmed = authKey.Trim();
			if (trimmed.Length < 4 || (trimmed.Length % 4) != 0)
			{
				return false;
			}
			byte[] raw;
			try
			{
				raw = Convert.FromBase64String(trimmed);
			}
			catch (FormatException)
			{
				return false;
			}
			string decoded = Encoding.UTF8.GetString(raw);
			int split = decoded.IndexOf(':');
			if (split <= 0 || split >= decoded.Length - 1)
			{
				return false;
			}
			userId = decoded.Substring(0, split);
			password = decoded.Substring(split + 1);
			return userId.Length != 0 && password.Length != 0;
		}

		private static int CheckUserAccountResult(string userid, string password)
		{
			if (string.IsNullOrEmpty(userid) || password == null)
			{
				return 0;
			}
			int num = TryCheckUserAccountCode(userid, password);
			if (num == 1)
			{
				return 1;
			}
			string md5Ascii = CreateMD5(password);
			num = TryCheckUserAccountCode(userid, md5Ascii);
			if (num == 1)
			{
				return 1;
			}
			string md5Utf = CreateMD5Utf8(password);
			if (md5Utf != md5Ascii)
			{
				num = TryCheckUserAccountCode(userid, md5Utf);
				if (num == 1)
				{
					return 1;
				}
			}
			return num;
		}

		private static int TryCheckUserAccountCode(string userid, string password)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_checkUserAccount");
				mySqlCommandHelper.AddParamVarString("userid", userid);
				mySqlCommandHelper.AddParamVarString("password", password);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					string text = mySqlCommandHelper.GetString("result");
					if (int.TryParse(text, out var result))
					{
						return result;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_checkUserAccount Error: {0}", ex.Message);
			}
			return 0;
		}

		private static void getUserCash(Account User)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getUserCash");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					User.Cash = mySqlCommandHelper.GetInt("cash");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getUserCash Error: {0}", ex.Message);
			}
			User.CashNeedUpdateFromDB = false;
		}

		public static void getExtraAbilities(Account User, byte last)
		{
			Dictionary<int, ExtraAbilityInfo> dictionary = new Dictionary<int, ExtraAbilityInfo>();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getExtraAbilities");
				mySqlCommandHelper.AddParamInt("pUserNum", User.UserNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					while (mySqlCommandHelper.HasResult())
					{
						short key = (short)mySqlCommandHelper.GetInt("attrType");
						float @float = mySqlCommandHelper.GetFloat("attrValue");
						int @int = mySqlCommandHelper.GetInt("itemnum");
						int iLimitTime = mySqlCommandHelper.GetInt("limit") * 1000;
						long dateTime = mySqlCommandHelper.GetDateTime("gottime", 0L);
						if (!dictionary.TryGetValue(@int, out var value))
						{
							value = new ExtraAbilityInfo();
						}
						value.iItemDescNum = @int;
						value.iLimitTime = iLimitTime;
						value.tGotTime = dateTime;
						value.mapAttributes[key] = @float;
						if (!dictionary.ContainsKey(@int))
						{
							dictionary.Add(@int, value);
						}
					}
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_getExtraAbilities Error: {0}", ex.Message);
			}
			if (flag)
			{
				User.Connection.SendAsync(new GetExtraAbilities_ACK(dictionary, last));
			}
		}

		private static string CreateMD5(string input)
		{
			return CreateMD5(input, Encoding.ASCII);
		}

		private static string CreateMD5Utf8(string input)
		{
			return CreateMD5(input, Encoding.UTF8);
		}

		private static string CreateMD5(string input, Encoding encoding)
		{
			using MD5 mD = MD5.Create();
			byte[] bytes = encoding.GetBytes(input);
			byte[] array = mD.ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
	}
}
