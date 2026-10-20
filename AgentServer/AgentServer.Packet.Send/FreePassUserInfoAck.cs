using System;
using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class FreePassUserInfoAck : NetPacket
	{
		public FreePassUserInfoAck(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FREE_PASS_USER_INFO_ACK);
			// 1410 wire: b4 b4 b8 (result, type, expire).
			ns.Write(0);
			int type = User != null ? User.FreePassType : 0;
			long expire = 0;
			if (User != null)
			{
				try
				{
					using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
					{
						mySqlConnection.Open();
						using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
						mySqlCommand.CommandType = CommandType.StoredProcedure;
						mySqlCommand.CommandText = "usp_freepass_getUserDesc";
						mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
						using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
						if (mySqlDataReader.Read())
						{
							type = Convert.ToInt32(mySqlDataReader["type"]);
							User.FreePassType = type;
							if (!mySqlDataReader.IsDBNull(mySqlDataReader.GetOrdinal("expireTime")))
							{
								expire = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["expireTime"]));
							}
						}
					}
				}
				catch
				{
					// SP missing or empty: still ACK so FreePass UI does not hang.
				}
			}
			ns.Write(type);
			ns.Write(expire);
			_ = last;
		}
	}
}
