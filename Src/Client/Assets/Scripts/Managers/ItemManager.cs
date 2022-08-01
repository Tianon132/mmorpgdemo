using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Data;
using Models;
using SkillBridge.Message;
using UnityEngine;
using Services;

namespace Managers
{
    //客户端是单例，而服务端不是单例，因为客户端只有一位用户的道具，不用担心别人的，所以可以放心使用实例
    public class ItemManager : Singleton<ItemManager>
    {
        public Dictionary<int, Item> Items = new Dictionary<int, Item>();
        internal void Init(List<NItemInfo> items)
        {
            this.Items.Clear();
            foreach (var info in items)
            {
                Item item = new Item(info);
                this.Items.Add(item.Id, item);

                Debug.LogFormat("ItemManager:Init[{0}]", item);
            }
            StatusService.Instance.RegisterStatusNotify(StatusType.Item, OnItemNotify);
            
        }

        public ItemDefine GetItem(int itemId)
        {
            return null;
        }

        bool OnItemNotify(NStatus status)//注册通知
        {
            if(status.Action == StatusAction.Add)
            {
                this.AddItem(status.Id, status.Value);
            }
            if(status.Action == StatusAction.Delete)
            {
                this.RemoveItem(status.Id, status.Value);
            }

            return true;
        }

        void AddItem(int itemId, int count)//增加道具
        {
            Item item = null;
            if(this.Items.TryGetValue(itemId, out item))//先查找本地有没有，有就只改数量，没有就new
            {
                item.Count += count;
            }
            else
            {
                item = new Item(itemId, count);
                this.Items.Add(itemId, item);//添加到道具列表
            }
            BagManager.Instance.AddItem(itemId, count);
        }

        void RemoveItem(int itemId, int count)
        {
            if (!this.Items.ContainsKey(itemId))
            {
                return;
            }
            Item item = this.Items[itemId];
            if (item.Count < count)
                return;
            item.Count -= count;

            BagManager.Instance.RemoveItem(itemId, count);
        }

        public bool UseItem(int itemId)
        {
            return false;
        }

        public bool UseItem(ItemDefine item)
        {
            return false;
        }
    }
}
