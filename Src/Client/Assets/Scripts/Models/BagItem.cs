using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Models
{
    [StructLayout(LayoutKind.Sequential, Pack =1)]//结构布局，代表这个结构体在内存中的存储格式
    
    
    //用结构体还是用类模式 是要看是值类型还是引用类型

    //如果是引用类型，A和B不能交换，而值可以，所以为了方便之后两个格子直接可以交换，故使用结构体来重载方法
    struct BagItem
    {
        public ushort ItemId;//ushort无符号16位，1个类型占2个字节，两个字节能表示的范围是65535
        //用3个字节表示id，用一个字节表示数量
        //如果叠加数量只有99，没有255，那么其实用一个字节来存储上限数量就够了，这样可以节约存储
        public ushort Count;

        public static BagItem zero = new BagItem { ItemId = 0, Count = 0 };

        public BagItem(int itemId, int count)
        { 
            this.ItemId = (ushort)itemId;
            this.Count = (ushort)count;
        }

        //重载操作符:为了和其他格子比较方便一点
        public static bool operator ==(BagItem lhs, BagItem rhs)
        {
            return lhs.ItemId == rhs.ItemId && lhs.Count == rhs.Count;
        }

        public static bool operator !=(BagItem lhs, BagItem rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// return true if the objects are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if(other is BagItem)
            {
                return Equals((BagItem)other);
            }
            return false;
        }

        public bool Equals(BagItem other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return ItemId.GetHashCode() ^ (Count.GetHashCode() << 2);
        }
    }
}
