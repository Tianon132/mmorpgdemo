using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameServer.Entities;
using GameServer.Models;
using Common;
using GameServer.Services;
using SkillBridge.Message;

namespace GameServer.Managers
{
    class ItemManager
    {
        Character Owner;

        public Dictionary<int, Item> Items = new Dictionary<int, Item>();

        public ItemManager(Character owner)//构造，把角色传进来
        {
            this.Owner = owner;

            foreach(var item in owner.Data.Items)
            {
                this.Items.Add(item.ItemID, new Item(item));
            }
        }

        public bool UseItem(int itemId, int count = 1)
        {
            Log.InfoFormat("[{0}]UseItem[{1}:{2}]", this.Owner.Data.ID, itemId, count);
            Item item = null;
            if(this.Items.TryGetValue(itemId,out item))//判断有没有，再取出来，而这个方法一次查询就可完事
            {
                if (item.Count < count)
                    return false;

                //添加使用逻辑
                Items.Remove(count);

                return true;
            }
            return false;
        }

        public bool HasItem(int itemId)//判断存不存在
        {
            Item item = null;
            if (this.Items.TryGetValue(itemId, out item))
                return item.Count > 0;
            return false;
        }

        public Item GetItem(int itemId)//得到Item //上面的函数没调用下面的函数是为了性能考虑，宁可重写，也没必要函数套用函数
        {
            Item item = null;
            this.Items.TryGetValue(itemId, out item);
            Log.InfoFormat("[{0}]UseItem[{1}:{2}]", this.Owner.Data.ID, itemId, item);
            return item;
        }

        public bool AddItem(int itemId, int count)
        {
            Item item = null;
            
            if(this.Items.TryGetValue(itemId, out item))
            {
                item.Add(count);
            }
            else
            {
                //往数据表中插入新数据
                TCharacterItem dbItem = new TCharacterItem();
                dbItem.CharacterID = Owner.Data.ID;
                dbItem.Owner = Owner.Data;
                dbItem.ItemID = itemId;
                dbItem.ItemCount = count;
                Owner.Data.Items.Add(dbItem);
                //DBService.Instance.Entities.CharacterItems.Add(dbItem);
                item = new Item(dbItem);
                this.Items.Add(itemId, item);//插入字典
            }

            //添加物品，使用状态管理器
            this.Owner.StatusManager.AddItemChange(itemId, count, StatusAction.Add);

            Log.InfoFormat("[{0}]AddItem[{1}] addCount:{2}", this.Owner.Data.ID, item, count);
            //DBService.Instance.Save();
            return true;
        }
        
        public bool RemoveItem(int ItemId, int count)
        {
            if(!this.Items.ContainsKey(ItemId))
            {
                return false;
            }
            Item item = this.Items[ItemId];
            if(item.Count < count)
                return false;
            item.Remove(count);

            //删除物品，使用管理器
            this.Owner.StatusManager.AddItemChange(ItemId, count, StatusAction.Delete);

            Log.InfoFormat("[{0}]RemoveItem[{1}] removeCount:{2}", this.Owner.Data.ID, item, count);
            //DBService.Instance.Save();
            return false;
        }

        public void GetItemInFos(List<NItemInfo> list)
        {
            foreach(var item in this.Items)
            {
                list.Add(new NItemInfo() { Id = item.Value.ItemID, Count = item.Value.Count });
            }
        }
    }
}
