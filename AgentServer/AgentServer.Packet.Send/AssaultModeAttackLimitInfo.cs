using System;
using System.Collections.Generic;
using AgentServer;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet.Send
{
	public sealed class AssaultModeAttackLimitInfo : NetPacket
	{
		public AssaultModeAttackLimitInfo(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ROOMKIND_ENTRY_CONDITION_ACK);
			// Korea: count + rows (i32 roomKind + i64 minExp + three list counts).
			List<EntryRow> rows = LoadRows();
			ns.Write(rows.Count);
			foreach (EntryRow row in rows)
			{
				// Korea row: roomKind, minExp, then three list counts.
				// A non-zero count reads more ints and overpops at the end of the packet.
				ns.Write(row.RoomKindId);
				ns.Write(row.MinExp);
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
				_ = row.MinDamage;
				_ = row.MaxDamage;
				_ = row.AnimalMinDamage;
			}
			_ = last;
		}

		private struct EntryRow
		{
			public int RoomKindId;
			public long MinExp;
			public int MinDamage;
			public int MaxDamage;
			public int AnimalMinDamage;
		}

		private static List<EntryRow> LoadRows()
		{
			List<EntryRow> rows = new List<EntryRow>();
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdRoomKindID, fdMinExp, IFNULL(fdMinDamage,0), IFNULL(fdMaxDamage,0), "
					+ "IFNULL(fdAnimalMinDamage,0) FROM EssenRoomKindEntryCondition ORDER BY fdRoomKindID",
					conn);
				using MySqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					rows.Add(new EntryRow
					{
						RoomKindId = Convert.ToInt32(reader[0]),
						MinExp = Convert.ToInt64(reader[1]),
						MinDamage = Convert.ToInt32(reader[2]),
						MaxDamage = Convert.ToInt32(reader[3]),
						AnimalMinDamage = Convert.ToInt32(Math.Round(Convert.ToDouble(reader[4])))
					});
				}
			}
			catch (Exception ex)
			{
				Log.Warning("ROOMKIND_ENTRY_CONDITION load: {0}", ex.Message);
			}
			return rows;
		}
	}
}
