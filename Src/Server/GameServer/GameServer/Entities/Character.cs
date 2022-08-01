using Common.Data;
using GameServer.Core;
using GameServer.Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network;
using GameServer.Models;
using Common;

namespace GameServer.Entities
{
    class Character : CharacterBase,IPostResponser
    {
       
        public TCharacter Data;//数据库中的数据

        public ItemManager ItemManager;
        public StatusManager StatusManager;//状态管理器
        public QuestManager QuestManager;//任务
        public FriendManager FriendManager;//好友管理器

        public Team Team;
        public double TeamUpdateTS;//时间戳

        public Guild Guild;
        public double GuildUpdateTS;//也是个时间戳

        public Chat Chat;//聊天 不需要bd

        public Character(CharacterType type,TCharacter cha):
            base(new Core.Vector3Int(cha.MapPosX, cha.MapPosY, cha.MapPosZ),new Core.Vector3Int(100,0,0))
        {
            this.Data = cha;
            this.Id = cha.ID;

            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Id = cha.ID;
            this.Info.EntityId = this.entityId;
            this.Info.Name = cha.Name;
            this.Info.Level = 10;//cha.Level;
            this.Info.ConfigId = cha.TID;
            this.Info.Class = (CharacterClass)cha.Class;
            this.Info.mapId = cha.MapID;
            this.Info.Gold = cha.Gold;//21：同步金钱
            this.Info.Ride = 0;//31.坐骑系统
            this.Info.Entity = this.EntityData;
            this.Define = DataManager.Instance.Characters[this.Info.ConfigId];

            this.ItemManager = new ItemManager(this);
            this.ItemManager.GetItemInFos(this.Info.Items);

            //初始化背包：不用管理器是因为角色与背包是一对一的关系
            this.Info.Bag = new NBagInfo();
            this.Info.Bag.Unlocked = this.Data.Bag.Unlocked;
            this.Info.Bag.Items = this.Data.Bag.Items;

            //22：装备
            this.Info.Equips = this.Data.Equips;

            //23-24.任务
            this.QuestManager = new QuestManager(this);
            this.QuestManager.GetQuestInfos(this.Info.Quests);//初始化任务manager，填充

            //状态管理器
            this.StatusManager = new StatusManager(this);

            //25.好友管理器
            this.FriendManager = new FriendManager(this);
            this.FriendManager.GetFriendInfos(this.Info.Friends);

            //26.公会系统
            this.Guild = GuildManager.Instance.GetGuild(this.Data.GuildId);

            //29.聊天
            this.Chat = new Chat(this);
        }

        public long Gold
        {
            get { return this.Data.Gold; }//得到金币数量
            set
            {
                if (this.Data.Gold == value)//先判断新旧金币是否相等
                    return;
                this.StatusManager.AddGoldChange((int)(value - this.Data.Gold));//新金币减去老金币  得到金币的变化值
                this.Data.Gold = value;
            }
        }

        public int Ride//封装Ride
        {
            get { return this.Info.Ride; }
            set
            {
                if (this.Info.Ride == value)
                    return;
                this.Info.Ride = value;
            }
        }

        //执行所有的 后处理的 接口
        public void PostProcess(NetMessageResponse message)
        {
            Log.InfoFormat("PostProcess > Character: characterID:{0}:{1}", this.Id, this.Info.Name);
            this.FriendManager.PostProcess(message);//更新好友列表信息

            if(this.Team != null)//更新组队信息
            {
                Log.InfoFormat("PostProcess > Team: characterID:{0}:{1} {2}<{3}", this.Id, this.Info.Name, TeamUpdateTS, this.Team.timestamp);
                if(TeamUpdateTS < this.Team.timestamp)
                {
                    TeamUpdateTS = Team.timestamp;
                    this.Team.PostProcess(message);
                }
            }

            if(this.Guild != null)//公会信息
            {
                Log.InfoFormat("Proprocess > Guild: characterId:{0}:{1} {2}<{3}", this.Id, this.Info.Name, GuildUpdateTS, this.Guild.timestamp);
                if(this.Info.Guild == null)//补充公会信息
                {
                    this.Info.Guild = this.Guild.GuildInfo(this);
                    if (message.mapCharacterEnter != null)//如果不是第一次登陆才赋值
                        GuildUpdateTS = Guild.timestamp;
                }
                if(GuildUpdateTS < this.Guild.timestamp && message.mapCharacterEnter == null)
                {
                    GuildUpdateTS = Guild.timestamp;
                    this.Guild.PostProcess(this, message);
                }
            }

            if(this.StatusManager.HasStatus)
            {
                this.StatusManager.PostProcess(message);//状态管理扔在这里
            }

            this.Chat.PostProcess(message);//聊天后处理
        }

        /// <summary>
        /// 角色离开时调用
        /// </summary>
        public void Clear()
        {
            //this.FriendManager.UpdateFriendInfo(this.Info, 0);  //有时候无法触发
            this.FriendManager.OfficeNotify();//直接通知在线的还有自己下线
        }

        //得到（生成）该character的基础信息【对于好友、组队系统所需要的】
        public NCharacterInfo GetBasicInfo()
        {
            return new NCharacterInfo()
            {
                Id = this.Id,
                Name = this.Info.Name,
                Class = this.Info.Class,
                Level = this.Info.Level
            };
        }
    }
}
