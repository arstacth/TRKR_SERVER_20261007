using AgentServer.Holders;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetShopCategoryList : NetPacket
	{
		public GetShopCategoryList(byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_CATEGORY_ACK);
			ns.Write(0);
			// Official RenewalShop CategoryACK (158)(44). Filtering to packed displays
			// dropped tabs (140/42). Send full category + limit tables.
			ns.Write(ShopHolder.ShopCategoryList.Count);
			foreach (ShopCategory shopCategory in ShopHolder.ShopCategoryList)
			{
				ns.Write(shopCategory.Category1);
				ns.Write(shopCategory.Category2);
				ns.Write(shopCategory.Category3);
				ns.WriteAnsiFixed_intSize(shopCategory.CategoryName);
				// R186937 tab slot is fdCategory2Btn / fdCategory3Btn / fdCategoryType.
				// Sub/origin are 0 on every row, so every tab landed on button 0.
				ns.Write(shopCategory.Category2Btn);
				ns.Write(shopCategory.Category3Btn);
				ns.Write(shopCategory.CategoryType);
			}
			ns.Write(ShopHolder.ShopCategoryLimits.Count);
			foreach (ShopCategoryLimit shopCategoryLimit in ShopHolder.ShopCategoryLimits)
			{
				ns.Write(shopCategoryLimit.CategoryNum);
				ns.Write(shopCategoryLimit.Type);
				ns.Write(shopCategoryLimit.OperatorType);
				ns.Write(shopCategoryLimit.Value);
			}
			_ = last;
		}
	}
}
