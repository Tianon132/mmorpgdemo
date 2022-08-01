using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Network;
using SkillBridge.Message;
using Models;
using Managers;

namespace Services
{
    class ItemService : Singleton<ItemService>, IDisposable
    {
        public ItemService()//响应的注册
        {
            MessageDistributer.Instance.Subscribe<ItemBuyResponse>(this.OnItemBuy);
            MessageDistributer.Instance.Subscribe<ItemEquipResponse>(this.OnItemEquip);
        }

        public void Dispose()//响应的消除
        {
            MessageDistributer.Instance.Unsubscribe<ItemBuyResponse>(this.OnItemBuy);
            MessageDistributer.Instance.Unsubscribe<ItemEquipResponse>(this.OnItemEquip);
        }

        public void SendBuyItem(int shopId, int shopItemId)
        {
            Debug.Log("SendBuyItem");

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.itemBuy = new ItemBuyRequest();
            message.Request.itemBuy.shopId = shopId;
            message.Request.itemBuy.shopItemId = shopItemId;
            NetClient.Instance.SendMessage(message);//发送购买请求
        }

        private void OnItemBuy(object sender, ItemBuyResponse message)//购买的响应
        {
            MessageBox.Show("购买结果" + message.Result + "\n" + message.Errormsg, "购买完成");
        }

        /**********装备系统添加***********/
        Item pendingEquip = null;//用来保存当前穿的哪件装备
        bool isEquip;//装备或鞋子啊
        public bool SendEquipItem(Item equip, bool isEquip)//发送操作的装备信息
        {
            if (pendingEquip != null)//如果有装备的信息，就返回
                //return false;
                pendingEquip = null;
            Debug.Log("SendEquipItem");

            //记录当前穿的装备
            pendingEquip = equip;
            //得到穿还是脱的结果
            this.isEquip = isEquip;

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.itemEquip = new ItemEquipRequest();
            message.Request.itemEquip.Slot = (int)equip.EquipInfo.slot;
            message.Request.itemEquip.itemId = equip.Id;
            message.Request.itemEquip.isEquip = isEquip;
            NetClient.Instance.SendMessage(message);
            return true;
        }

        private void OnItemEquip(object sender, ItemEquipResponse message)//穿脱装备的响应
        {
            if(message.Result == Result.Success)
            {
                if(pendingEquip != null)
                {
                    if (this.isEquip)
                        EquipManager.Instance.OnEquipItem(pendingEquip);//穿
                    else
                        EquipManager.Instance.OnUnEquipItem(pendingEquip.EquipInfo.slot);//脱
                }
            }
        }
    }
}
