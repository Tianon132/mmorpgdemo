using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using SkillBridge.Message;
using Network;
using Common.Data;
using GameServer.Services;

namespace GameServer.Managers
{
    class ShopManager : Singleton<ShopManager>//ItemManager不是单例类，ShopManager是
    {
        //谁买东西服务谁，故需要Session.Sender
        public Result BuyItem(NetConnection<NetSession> sender, int shopId, int shopItemId)//只传Id，不传价格，黑客黑不动，因为只能改Id
        {
            if (!DataManager.Instance.Shops.ContainsKey(shopId))//先验证商店Id在不在
                return Result.Failed;

            ShopItemDefine shopItem;
            if(DataManager.Instance.ShopItems[shopId].TryGetValue(shopItemId, out shopItem))
            {
                Log.InfoFormat("BuyItem: :Character:{0}:Item:{1} Count:{2} Price:{3}", sender.Session.Character.Id, shopItem.ItemID, shopItem.Count, shopItem.Price);
                if(sender.Session.Character.Gold > shopItem.Price)
                {
                    sender.Session.Character.ItemManager.AddItem(shopItem.ItemID, shopItem.Count);//AddItem
                    sender.Session.Character.Gold -= shopItem.Price;
                    DBService.Instance.Save();
                    return Result.Success;
                }
            }
            return Result.Failed;
        }
    }
}
