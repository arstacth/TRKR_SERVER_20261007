using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using Serilog;

namespace AgentServer.Packet
{
	/// <summary>
	/// Catch-all lobby stubs ported from KR_TR_SRC, using Korea R186937 wire opcodes
	/// (this Agent stores shuffled wire IDs directly in <see cref="Opcodes"/>).
	/// Called after PackedAckMap so R186937 stubs win first.
	/// </summary>
	public static class LobbyUnhandledHandle
	{
		public static bool TryHandle(ClientConnection client, ushort serverOp, PacketReader reader, byte last)
		{
			if (PieroOlympicHandle.TryHandle(client, serverOp, reader, last))
			{
				return true;
			}
			switch (serverOp)
			{
			case (ushort)Opcodes.eServer_CHECK_JUDGEMENT_REQ:
				LoginHandle.Handle_FF7F01(client, last);
				return true;
			case (ushort)Opcodes.eServer_SUCCESS_JUDGEMENT_REQ:
				return true;
			case (ushort)Opcodes.eServer_ALONERUN_GAME_OVER_REQ:
				LobbyHandle.Handle_AloneRunGameOver(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_Divination_CHECK_FREE_REQ:
				ParkHandle.Handle_CheckDivinationFree(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_Divination_GET_RESULT_FREE_REQ:
				ParkHandle.Handle_ParkUseDivinationFree(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_Divination_UPDATE_COUPLE_EXTRA_ABILITY_REQ:
				ParkHandle.Handle_ParkUpdateCoupleAbility(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_Blocking_To_Me_REQ:
				GMCommandHandle.Handle_ClientCheckAutoBan(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_USE_ITEM_BY_BUTTON_REQ:
				if (reader.Remaining > 0)
				{
					reader.ReadByte();
				}
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_USE_ITEM_BY_BUTTON_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_TALESKNIGHT_PVP_BATTLE_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_TALESKNIGHT_PVP_BATTLE_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_QUEST_INTERROGATION_INFO_REQ:
			case (ushort)Opcodes.eServer_QUEST_INTERROGATION_CONSUME_REQ:
			case (ushort)Opcodes.eServer_QUEST_INTERROGATION_QUESTION_REQ:
			case (ushort)Opcodes.eServer_QUEST_INTERROGATION_FINISH_REQ:
			case (ushort)Opcodes.eServer_QUEST_INTERROGATION_REWARD_REQ:
			case (ushort)Opcodes.eServer_QUEST_INTERROGATION_SCHEDULE_INFO_REQ:
			case (ushort)Opcodes.eServer_QUEST_INTERROGATION_LIKEABILITY_INFO_REQ:
			case (ushort)Opcodes.eServer_QUEST_INTERROGATION_POINT_REWARD_REQ:
				_ = last;
				return true;
			case (ushort)Opcodes.eServer_LOBBY_QUEST_ADD_REQ:
				Mission.Handle_QuestAdd(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_LOBBY_QUEST_REMOVE_REQ:
				Mission.Handle_QuestRemove(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_LOBBY_QUEST_REWARD_REQ:
				Mission.Handle_QuestReward(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_LOBBY_QUEST_COMPLETE_CHECK_REQ:
				_ = last;
				return true;
			case (ushort)Opcodes.eServer_COMPETITION_EVENT_LAST_CHAOS_OCCUPATION_WAR_RESULT_REWARD_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_COMPETITION_EVENT_LAST_CHAOS_OCCUPATION_WAR_RESULT_REWARD_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_ItemCollection_PROTOCOL:
				LobbyHandle.Handle_ItemCollectionProtocol(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_StampBoard_PROTOCOL:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_StampBoard_PROTOCOL, last));
				return true;
			case (ushort)Opcodes.eServer_SELECT_SHOP_TRADE_INFO_REQ:
				client.SendAsync(new SelectShopTradeInfoAck(last));
				return true;
			case (ushort)Opcodes.eServer_SELECT_SHOP_TRADE_INFO_DETAIL_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_SELECT_SHOP_TRADE_INFO_DETAIL_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_SELECT_SHOP_USER_INFO_REQ:
				client.SendAsync(new SelectShopUserInfoAck(client.CurrentAccount, last));
				return true;
			case (ushort)Opcodes.eServer_FREE_PASS_USER_INFO_REQ:
				client.SendAsync(new FreePassUserInfoAck(client.CurrentAccount, last));
				return true;
			case (ushort)Opcodes.eServer_ONEDAY_BUFF_LEVELUP_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_ONEDAY_BUFF_LEVELUP_ACK, last, writeLast: false, 0));
				return true;
			case (ushort)Opcodes.eServer_ONEDAY_BUFF_LOAD_MY_DATA_REQ:
				// 264 = DOLIMPAN coupon toast. 99 is unknown on this client
				// (Unknown 99 + Remain 4). Empty buffs: consume, send nothing.
				_ = last;
				return true;
			case (ushort)Opcodes.eServer_ONEDAY_BUFF_SAVE_MY_DATA_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_ONEDAY_BUFF_LEVELUP_ACK, last, writeLast: false, 0));
				return true;
			case (ushort)Opcodes.eRoom_MAKE_ITEM_FOR_MAP_GENERATE_MULTI_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eRoom_MAKE_ITEM_FOR_MAP_GENERATE_MULTI_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_EXAM_DATA_PRIOR_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_EXAM_DATA_PRIOR_ACK, last, writeLast: false, 0));
				return true;
			case (ushort)Opcodes.eServer_AVATAR_LOCK_LOAD_REQ:
				AvatarLock.Handle_AvatarLock_Load(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_AVATAR_LOCK_SAVE_REQ:
				AvatarLock.Handle_AvatarLock_Save(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_PRIVATE_PUZZLE_GET_TRY_REWARD_LIST_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_PRIVATE_PUZZLE_GET_TRY_REWARD_LIST_ACK, last, 0, 0));
				return true;
			case (ushort)Opcodes.eServer_FISHING_FARM_MASTER_REWARD_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_FISHING_FARM_MASTER_REWARD_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_ReportSystemUserReportInfo:
				client.SendAsync(new ReportSystemUserReportInfoAck(last));
				return true;
			case (ushort)Opcodes.eServer_TRIPLE_BBOB_PROTOCOL:
			case (ushort)Opcodes.eServer_NICK_NAME_AUCTION_PROTOCOL:
				// Case 2174 reads one int. The trailing last byte was RemainSize=1.
				client.SendAsync(new LobbyEmptyAck(serverOp, last, false, 0));
				return true;
			case (ushort)Opcodes.eServer_MAP_BOOKMARK_GET_USER_INFO_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_MAP_BOOKMARK_GET_USER_INFO_ACK, last, 0, 0));
				return true;
			case (ushort)Opcodes.eServer_NewAttendanceGetUserInfo:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_NEW_ATTENDANCE_GET_USER_INFO_ACK, last, 0, 0));
				return true;
			case (ushort)Opcodes.eServer_ATTENDANCE_INFO_REQ:
				AttendanceHandle.Handle_AttendanceInfo(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_SHOP_USER_BUY_COUNT_LIST_REQ:
			case (ushort)Opcodes.eServer_SHOP_USER_BUY_COUNT_LIST_ACK:
				ShopHandle.Handle_GetUserBuyList(client, reader, last);
				return true;
			case (ushort)Opcodes.eServer_LENS_GET_USER_INFO_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_LENS_GET_USER_INFO_ACK, last, 0, 0));
				return true;
			case (ushort)Opcodes.eServer_STAT_SYSTEM_MYINFO_REQ:
				{
					var acc = client.CurrentAccount;
					client.SendAsync(new StatSystemMyInfoAck(last, acc != null ? acc.StatSystemPageCount : 1, acc?.StatSystemTitles));
					return true;
				}
			case (ushort)Opcodes.eServer_STAT_SYSTEM_SAVE_PAGE_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_STAT_SYSTEM_SAVE_PAGE_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_STAT_SYSTEM_SAVE_TITLE_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_STAT_SYSTEM_SAVE_TITLE_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_STAT_SYSTEM_SET_PAGENUM_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_STAT_SYSTEM_SET_PAGENUM_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eServer_CASH_POINT_REQ:
				if (client.CurrentAccount != null)
				{
					client.SendAsync(new CashPointAck(client.CurrentAccount, last));
				}
				return true;
			case (ushort)Opcodes.eServer_GUG_GET_USER_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eServer_GUG_GET_USER_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eRoom_FLASHCARD_ANSWER_REQ:
				client.SendAsync(new LobbyEmptyAck((ushort)Opcodes.eRoom_FLASHCARD_ANSWER_ACK, last, 0));
				return true;
			case (ushort)Opcodes.eRoom_TYPEINGRUN_GOBLINRACING_QUESTION_ACK:
			case (ushort)Opcodes.eRoom_FLASHCARD_QUESTION_ACK:
			case (ushort)Opcodes.eRoom_FLASHCARD_RESULT:
				return true;
			default:
				Log.Information("Lobby opcode {0} ignored remain={1} user:{2}", serverOp, reader.Remaining, client.CurrentAccount != null ? client.CurrentAccount.UserID : "?");
				if (reader.Remaining > 0)
				{
					reader.Offset += reader.Remaining;
				}
				return true;
			}
		}
	}
}
