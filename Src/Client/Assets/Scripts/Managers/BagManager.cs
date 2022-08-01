using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Models;

namespace Managers
{
    class BagManager : Singleton<BagManager>
    {
        public int Unlocked;//解锁了第几个格子

        public BagItem[] Items;

        NBagInfo Info;

        unsafe public void Init(NBagInfo info)
        {
            this.Info = info;
            this.Unlocked = info.Unlocked;//赋值

            Items = new BagItem[this.Unlocked];
            if(info.Items != null && info.Items.Length >= this.Unlocked)//长度够，走分析
            {
                Analyze(info.Items);
            }
            else//重新建立数组，重新配置一下
            {
                Info.Items = new byte[sizeof(BagItem) * this.Unlocked];
                Reset();
            }
        }

        //重置
        public void Reset()
        {
            int i = 0;
            foreach(var kv in ItemManager.Instance.Items)
            {
                if(kv.Value.Count <= kv.Value.Define.StackLimit)//数量小于叠层限制数量（假如上限容量99）
                {
                    this.Items[i].ItemId = (ushort)kv.Key;
                    this.Items[i].Count = (ushort)kv.Value.Count;
                }
                else//否则就拆分（假如大小超过99就拆分显示）
                {
                    int count = kv.Value.Count;
                    while(count > kv.Value.Define.StackLimit)
                    {
                        this.Items[i].ItemId = (ushort)kv.Key;
                        this.Items[i].Count = (ushort)kv.Value.Define.StackLimit;
                        i++;
                        count -= kv.Value.Define.StackLimit;
                    }
                    this.Items[i].ItemId = (ushort)kv.Key;
                    this.Items[i].Count = (ushort)count;
                }
                i++;
            }
        }

        unsafe void Analyze(byte[] data)
        {
            fixed(byte* pt = data)//指针，想用指针只能用fixed，这样能保证背包数据的地址不会发生改变
            {
                for(int i = 0; i < this.Unlocked; i++)//可能是20个格子
                {
                    BagItem* item = (BagItem*)(pt + i * sizeof(BagItem));//开始的指针pt + i第几个格子
                    Items[i] = *item;//依次放入数据，地址不会发生改变，故用结构体，因为能实现不改变地址
                }
            }
        }

        unsafe public NBagInfo GetBagInfo()//得到格子信息
        {
            fixed (byte* pt = Info.Items)
            {
                for(int i=0;i<this.Unlocked;i++)
                {
                    BagItem* item = (BagItem*)(pt + i * sizeof(BagItem));
                    *item = Items[i];
                }
            }
            return this.Info;
        }

        //添加物品
        public void AddItem(int itemId, int count)
        {
            ushort addCount = (ushort)count;//要添加的数目
            for(int i=0; i<Items.Length;i++)
            {
                if(this.Items[i].ItemId == itemId)//找到同ID的Item，判断是否可以在原格子上添加
                {
                    ushort canAdd = (ushort)(DataManager.Instance.Items[itemId].StackLimit - this.Items[i].Count);
                    if(canAdd >= addCount)
                    {
                        this.Items[i].Count += addCount;
                        addCount = 0;
                        break;
                    }
                    else
                    {
                        this.Items[i].Count += canAdd;
                        addCount -= canAdd;
                    }
                }
            }
            if(addCount > 0)//如果还有剩余的就添加到新的空白格子
            {
                //ushort LimitAdd = (ushort)DataManager.Instance.Items[itemId].StackLimit;
                for (int i=0; i<Items.Length;i++)
                {
                    if (this.Items[i].ItemId == 0)
                    {
                        this.Items[i].ItemId = (ushort)itemId;
                        this.Items[i].Count = addCount;
                        break;
                    }
                    //if(this.Items[i].ItemId == 0 && addCount > 0)//寻找空槽且还有剩余的没加入
                    //{
                    //    if(LimitAdd < addCount)//添加的大于限制的数目
                    //    {
                    //        this.Items[i].ItemId = (ushort)itemId;
                    //        this.Items[i].Count = LimitAdd;
                    //        addCount -= LimitAdd;
                    //    }
                    //    else
                    //    {
                    //        this.Items[i].ItemId = (ushort)itemId;
                    //        this.Items[i].Count = addCount;
                    //        addCount = 0;
                    //        break;
                    //    }
                    //}
                }
            }
        }

        public void RemoveItem(int ItemId, int count)
        {

        }
    }
}
