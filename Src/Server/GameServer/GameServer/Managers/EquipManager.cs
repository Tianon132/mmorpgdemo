using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using SkillBridge.Message;
using Network;
using GameServer.Entities;
using GameServer.Services;

namespace GameServer.Managers
{
    class EquipManager : Singleton<EquipManager>
    {
        //穿装备
        public Result EquipItem(NetConnection<NetSession> sender, int slot, int itemId, bool isEquip)//首先考虑什么槽位，道具Id，装还是脱
        {
            Character character = sender.Session.Character;
            if (!character.ItemManager.Items.ContainsKey(itemId))//校验角色有没有这个装备：防外挂（需要从服务端检查），检查有没有这个道具，并不是你说穿就穿
                return Result.Failed;

            UpdateEquip(character.Data.Equips, slot, itemId, isEquip);//更新装备

            DBService.Instance.Save();
            return Result.Success;
        }

        unsafe void UpdateEquip(byte[] equipData, int slot, int itemId, bool isEquip)//更新装备，
        {
            fixed (byte* pt = equipData)    /*************这部分和背包一样****************/
            {
                int* slotid = (int*)(pt + slot * sizeof(int));//用当前的指针（指向数组的指针）+槽子Id*槽子的大小（一个数字就是一个槽子），指向目标格子
                if (isEquip)
                    *slotid = itemId;//得到槽位的指针
                else
                    *slotid = 0;//位置清0，就是移除
            }
        }
    }
}
