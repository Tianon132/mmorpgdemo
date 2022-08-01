using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Data;
using Services;

namespace Managers
{
    class ShopManager : Singleton<ShopManager>
    {
        public void Init()
        {
            NpcManager.Instance.RegisterNpcEvent(NpcFunction.InvokeShop, OnOpenShop);
        }



        private bool OnOpenShop(NpcDefine npc)//根据Npc来判断打开哪种商店，在NPCDefien中有param属性来判断
        {
            this.ShowShop(npc.Param);
            return true;
        }

        public void ShowShop(int shopId)
        {
            ShopDefine shop;
            if(DataManager.Instance.Shops.TryGetValue(shopId, out shop))//先查找这个shopId是否存在
            {
                UIShop uIShop = UIManager.Instance.Show<UIShop>();//打开ui
                if(uIShop != null)
                {
                    uIShop.SetShop(shop);//如果不为空，设置shop的UI
                }
            }
        }//以上都是通过NPC打开商店

        public bool BuyItem(int shopId, int shopItemId)
        {
            ItemService.Instance.SendBuyItem(shopId, shopItemId);
            return true;
        }
    }
}
