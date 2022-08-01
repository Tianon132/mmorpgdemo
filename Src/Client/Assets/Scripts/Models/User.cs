using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Data;
using UnityEngine;
using SkillBridge.Message;
using Services;

namespace Models
{
    class User : Singleton<User>
    {
        SkillBridge.Message.NUserInfo userInfo;


        public SkillBridge.Message.NUserInfo Info
        {
            get { return userInfo; }
        }


        public void SetupUserInfo(SkillBridge.Message.NUserInfo info)
        {
            this.userInfo = info;
        }

        public SkillBridge.Message.NCharacterInfo CurrentCharacter { get; set; }

        public MapDefine CurrentMapData { get; set; }//方便地图资源使用，特别引入地图名字

        public PlayerInputController CurrentCharacterObject { get; set; }//当前角色的当前的游戏对象--坐骑修改

        public NTeamInfo TeamInfo { get; set; }//当前队伍信息

        public void AddGold(int gold)
        {
            this.CurrentCharacter.Gold += gold;
        }

        public int CurrentRide = 0;
        internal void Ride(int id)//坐骑
        {
            if(CurrentRide != id)
            {
                CurrentRide = id;
                CurrentCharacterObject.SendEntityEvent(EntityEvent.Ride, CurrentRide);//骑坐骑
            }
            else
            {
                CurrentRide = 0;
                CurrentCharacterObject.SendEntityEvent(EntityEvent.Ride, 0);//不骑坐骑
            }
        }
    }
}
