using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SkillBridge.Message;
using Common.Data;

namespace Models
{
    public class Item
    {
        public int Id;
        public int Count;
        public ItemDefine Define;
        //装备信息
        public EquipDefine EquipInfo;

        public Item(NItemInfo item) : this(item.Id, item.Count) //在21节商店系统这里改为了重载
        {
            /*
            this.Id = item.Id;
            this.Count = item.Count;
            this.Define = DataManager.Instance.Items[item.Id];
            */
        }

        public Item(int id, int count)
        {
            this.Id = id;
            this.Count = count;
            //this.Define = DataManager.Instance.Items[this.Id];
            //这次的修改 方便之后 查看装备信息，道具和装备同时加载
            DataManager.Instance.Items.TryGetValue(this.Id, out this.Define);
            DataManager.Instance.Equips.TryGetValue(this.Id, out this.EquipInfo);
        }

        public override string ToString()
        {
            return string.Format("Id:{0},Count:{1}", this.Id, this.Count);
        }
    }
}
