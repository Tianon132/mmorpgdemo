using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Data
{
    public enum ItemFunction//枚举定义功能
    {
        RecoverHP,
        RecoverMP,
        AddBuff,
        AddExp,
        AddMoney,
        AddItem,
        AddSkillPoint,
    }

    public class ItemDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public string Category { get; set; }

        //装备系统新加部分
        public int Level { get; set; }
        public CharacterClass LimitClass { get; set; }//职业限制

        public bool CanUse { get; set; }
        public float UseCD { get; set; }
        public int Price { get; set; }
        public int SellPrice { get; set; }

        public int StackLimit { get; set; }//堆叠限制
        public string Icon { get; set; }

        public ItemFunction Function { get; set; }//道具功能
        public int Param { get; set; }
        public List<int> Params { get; set; }
    }
}
