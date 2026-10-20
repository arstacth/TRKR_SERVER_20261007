using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.GameReward;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RewardResult : NetPacket
	{
		public GameRoom_RewardResult(List<GameRewardResult> rewardresult, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GAME_REWARD_INFO);
			ns.Write(1);
			ns.Write(rewardresult.Count);
			foreach (GameRewardResult item in rewardresult)
			{
				// Client reward row is 12 bytes. 8-byte rows eat the trailer and Overpop opcode 1772 at EOF needing 4.
				ns.Write(item.RewardID);
				ns.Write(item.RewardAmount);
				ns.Write((int)item.RewardType);
			}
			ns.Write(1);
			ns.Write(1002);
			ns.Write(0);
			// R186937 reads two ints after the reward trailer.
			// One int still Overpops (empty 26 26 4, two items 50 50 4) opcode 1772.
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
