using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Services;

namespace Managers
{
    class EquipManager : Singleton<EquipManager>//管理的就是7个格子，那么Equits[]就是管理器要管理的对象
    {
        public delegate void OnEquipChangeHandler();

        public event OnEquipChangeHandler OnEquipChanged;//委托-UICharEquip.cs调用

        public Item[] Equips = new Item[(int)EquipSlot.SlotMax];//声明一个定长、七个格子的装备数据（GameService是byte格式）

        byte[] Data;//int list  维护两者之间的转换

        unsafe public void Init(byte[] data)//初始化数据，得到槽的信息【指针只能用unsafe】
        {
            this.Data = data;
            this.ParseEquipData(data);//解析数据
        }

        public bool Contains(int equipId)//包含：查找是否包含以equipId为Id的槽
        {
            for(int i=0; i<this.Equips.Length; i++)
            {
                if(Equips[i] != null && Equips[i].Id == equipId)
                    return true;
            }
            return false;
        }

        public Item GetEquip(EquipSlot slot)//得到第slot个Equip槽的道具信息
        {
            return Equips[(int)slot];
        }

        unsafe void ParseEquipData(byte[] data)//解析数据 【跟背包一模一样】
        {
            fixed (byte* pt = this.Data)
            {
                for (int i = 0; i < this.Equips.Length; i++)
                {
                    int itemId = *(int*)(pt + i * sizeof(int));
                    if (itemId > 0)
                        Equips[i] = ItemManager.Instance.Items[itemId];
                    else
                        Equips[i] = null;
                }
            }
        }

        unsafe public byte[] GetEquipData()//发送服务器的时候，需要从装备信息还原到data【这种方法一般用不到，因为一直用的equip的Id传递】
        {
            fixed (byte* pt = Data)
            {
                for (int i = 0; i < (int)EquipSlot.SlotMax; i++)
                {
                    int* itemId = (int*)(pt + i * sizeof(int));
                    if (Equips[i] == null)
                        *itemId = 0;
                    else
                        *itemId = Equips[i].Id;
                }
            }
            return this.Data;
        }

        public void EquipItem(Item equip)//发送到服务器：穿  装备
        {
            ItemService.Instance.SendEquipItem(equip, true);
        }

        public void UnEquipItem(Item equip)//脱
        {
            ItemService.Instance.SendEquipItem(equip, false);
        }

        public void OnEquipItem(Item equip)//客户端响应指令：穿
        {
            if (this.Equips[(int)equip.EquipInfo.slot] != null && this.Equips[(int)equip.EquipInfo.slot].Id == equip.Id)
            {
                return;
            }
            this.Equips[(int)equip.EquipInfo.slot] = ItemManager.Instance.Items[equip.Id];

            if (OnEquipChanged != null)
                OnEquipChanged();
        }

        public void OnUnEquipItem(EquipSlot slot)//脱
        {
            if(this.Equips[(int)slot] != null)
            {
                this.Equips[(int)slot] = null;
                if (OnEquipChanged != null)
                    OnEquipChanged();
            }
        }
    }
}
