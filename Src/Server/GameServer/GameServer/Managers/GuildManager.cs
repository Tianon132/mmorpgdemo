using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameServer.Models;
using GameServer.Services;
using GameServer.Entities;
using SkillBridge.Message;
using Common.Utils;

namespace GameServer.Managers
{
    class GuildManager : Singleton<GuildManager>
    {
        public Dictionary<int, Guild> Guilds = new Dictionary<int, Guild>();
        private HashSet<string> GuildNames = new HashSet<string>();

        public void Init()
        {
            this.Guilds.Clear();
            foreach(var guild in DBService.Instance.Entities.Guilds)
            {
                this.AddGuild(new Guild(guild));//从TGuild转化为Guild
            }
        }

        void AddGuild(Guild guild)
        {
            this.Guilds.Add(guild.Id, guild);//添加公会信息
            this.GuildNames.Add(guild.Name);//保存公会名称
            guild.timestamp = TimeUtil.timestamp;
        }

        public bool CheckNameExisted(string name)
        {
            return GuildNames.Contains(name);//哈希查询，这样可以非常高效的查询一个东西是否存在
        }

        /// <summary>
        /// 创建公会
        /// </summary>
        /// <param name="name"></param>
        /// <param name="notice"></param>
        /// <param name="leader"></param>
        /// <returns></returns>
        public bool CreateGuild(string name, string notice, Character leader)
        {
            DateTime now = DateTime.Now;
            TGuild dbGuild = DBService.Instance.Entities.Guilds.Create();
            dbGuild.Name = name;
            dbGuild.Notice = notice;
            dbGuild.LeaderID = leader.Id;
            dbGuild.LeaderName = leader.Name;
            dbGuild.CreateTime = now;
            DBService.Instance.Entities.Guilds.Add(dbGuild);//添加到数据库

            Guild guild = new Guild(dbGuild);
            guild.AddMember(leader.Id, leader.Name, leader.Data.Class, leader.Data.Level, GuildTitle.President);////更新数据库-更新成员-添加会长
            leader.Guild = guild;
            DBService.Instance.Save();
            leader.Data.GuildId = dbGuild.Id;//更新数据库-Character中的GuildId信息
            DBService.Instance.Save();
            this.AddGuild(guild);

            return true;
        }

        /// <summary>
        /// 得到共公会具体信息
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public Guild GetGuild(int guildId)
        {
            if (guildId == 0)
                return null;
            Guild guild = null;
            this.Guilds.TryGetValue(guildId, out guild);
            return guild;
        }

        /// <summary>
        /// 得到 多个 公会信息【这时候还没加入公会，故传入null】
        /// </summary>
        /// <returns></returns>
        public List<NGuildInfo> GetGuildsInfo()
        {
            List<NGuildInfo> result = new List<NGuildInfo>();
            foreach(var kv in this.Guilds)
            {
                result.Add(kv.Value.GuildInfo(null));//这样不用获取成员具体信息，只看公会信息
            }
            return result;
        }
    }
}
