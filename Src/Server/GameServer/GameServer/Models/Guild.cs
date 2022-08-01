using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Entities;
using SkillBridge.Message;
using GameServer.Services;
using Common;
using GameServer.Managers;
using Common.Utils;

namespace GameServer.Models
{
    class Guild
    {
        public int Id { get { return this.Data.Id; } }              //公会Id

        public string Name { get { return this.Data.Name; } }       //公会名 称

        public double timestamp;                                     //时间戳

        public TGuild Data;                                         //公会信息  数据库中

        public Guild(TGuild guild)
        {
            this.Data = guild;//得到数据库中的数据
        }

        /// <summary>
        /// 创建 加入公会 的申请（普通用户）
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        public bool JoinApply(NGuildApplyInfo apply)
        {
            var oldApply = this.Data.Applies.FirstOrDefault(v => v.CharacterId == apply.characterId);//返回第一个答案要么就是返回null，而first没有结果直接抛出异常
            if(oldApply != null)//校验：通过检查当前公会的applies的记录，先判断该character申请有没有重复，一个人只能申请一次
            {
                return false;//【没有才能继续添加新的申请】
            }

            var dbApply = DBService.Instance.Entities.GuildApplies.Create();//创建一个新的T数据
            dbApply.GuildId = apply.GuildId;
            dbApply.CharacterId = apply.characterId;
            dbApply.Name = apply.Name;
            dbApply.Class = apply.Class;
            dbApply.Level = apply.Level;
            dbApply.ApplyTime = DateTime.Now;

            DBService.Instance.Entities.GuildApplies.Add(dbApply);
            this.Data.Applies.Add(dbApply);//一个公会有多个申请【公会 一对多 申请】  还有公会成员

            DBService.Instance.Save();

            this.timestamp = TimeUtil.timestamp;
            return true;
        }

        /// <summary>
        /// 审批 的 结果 - 会长
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        public bool JoinAppove(NGuildApplyInfo apply)
        {
            var oldApply = this.Data.Applies.FirstOrDefault(v => v.CharacterId == apply.characterId && v.Result == 0);
            if(oldApply == null)//先判断没有结果 的 这条申请 还在不在，【有才能继续添加结果】
            {
                return false;
            }

            oldApply.Result = (int)apply.Result;//结果批准

            if(apply.Result == ApplyResult.Accept)//如果同意添加该成员
            {
                this.AddMember(apply.characterId, apply.Name, apply.Class, apply.Level, GuildTitle.None);//最后一个参数是职位
            }

            DBService.Instance.Save();

            this.timestamp = TimeUtil.timestamp;
            return true;
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="class"></param>
        /// <param name="level"></param>
        /// <param name="title"></param>
        public void AddMember(int characterId, string name, int @class, int level, GuildTitle title)
        {
            DateTime now = DateTime.Now;
            TGuildMember dbMember = new TGuildMember()//创建一个新的T数据库数据
            {
                CharacterId = characterId,
                Name = name,
                Class = @class,
                Level = level,
                Title = (int)title,
                JoinTime = now,
                LastTime = now
            };
            this.Data.Members.Add(dbMember);//添加到该公会的TGuild类的Data，可以通过DB保存到数据库中

            var character = CharacterManager.Instance.GetCharacter(characterId);
            if (character != null)
                character.Data.GuildId = this.Id;//角色在线：直接赋值
            else
            {
                //DBService.Instance.Entities.Database.ExecuteSqlCommand("UPDATE Characters SET GuildId = @p0, WHERE CharacterId = @p1", this.Id, characterId);
                TCharacter dbchar = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == characterId);
                dbchar.GuildId = this.Id;  //不在线：给数据库中GuildId赋值
            }
            
            timestamp = TimeUtil.timestamp;
        }

        public void Leave(int characterId)
        {
            Log.InfoFormat("Leave Guild: :{0}:{1}", characterId, this.Name);

            TGuildMember dbMember = DBService.Instance.Entities.GuildMembers.SingleOrDefault(c => c.Id == characterId);
            DBService.Instance.Entities.GuildMembers.Remove(dbMember);
            this.Data.Members.Remove(dbMember);//首先：删除数据库中的信息

            var character = CharacterManager.Instance.GetCharacter(characterId);
            if (character != null)
            {
                character.Data.GuildId = 0;//角色在线：直接赋值
                character.Guild = null;
            }
            else
            {
                TCharacter dbchar = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == characterId);
                dbchar.GuildId = 0;  //不在线：给数据库中GuildId赋值
            }

            timestamp = TimeUtil.timestamp;
        }

        public void PostProcess(Character from, NetMessageResponse message)
        {
            if(message.Guild == null)//判断公会是不是空的
            {
                message.Guild = new GuildResponse();
                message.Guild.Result = Result.Success;
                message.Guild.guildInfo = this.GuildInfo(from);
            }
        }

        //得到网络格式的 包含所有Member的 NGuildInfo【公会】信息
        public NGuildInfo GuildInfo(Character from)//公会信息
        {
            //得到公会Info
            NGuildInfo info = new NGuildInfo()
            {
                Id = this.Id,
                GuildName = this.Name,
                Notice = this.Data.Notice,
                leaderId = this.Data.LeaderID,
                leaderName = this.Data.LeaderName,
                createTime = (long)TimeUtil.GetTimestamp(this.Data.CreateTime),
                memberCount = this.Data.Members.Count
            };

            //得到Members和Applies
            if(from != null)//有值说明是当前公会的成员：才可以看具体信息
            {
                info.Members.AddRange(GetMemberInfos());//添加成员
                if (from.Id == this.Data.LeaderID)
                    info.Applies.AddRange(GetApplyInfos());//若是会长，还要添加申请信息
            }
            return info;
        }

        /// <summary>
        /// 得到 多个 公会【成员】信息
        /// </summary>
        /// <returns></returns>
        List<NGuildMenmberInfo> GetMemberInfos()
        {
            List<NGuildMenmberInfo> members = new List<NGuildMenmberInfo>();

            foreach (var member in this.Data.Members)//遍历数据库中的列表
            {
                var memberInfo = new NGuildMenmberInfo()//为单个成员 创建网络NGuildMenmberInfo 信息
                {
                    Id = member.Id,
                    characterId = member.CharacterId,
                    Title = (GuildTitle)member.Title,
                    joinTime = (long)TimeUtil.GetTimestamp(member.JoinTime),
                    lastTime = (long)TimeUtil.GetTimestamp(member.LastTime)
                };
                //应该增加更多的检查
                var character = CharacterManager.Instance.GetCharacter(member.CharacterId);
                if(character != null)//若在线
                {
                    memberInfo.Info = character.GetBasicInfo();//得到该成员的网络格式信息
                    memberInfo.Status = 1;
                    member.Level = character.Data.Level;//更新数据库T的信息
                    member.Name = character.Data.Name;
                    member.LastTime = DateTime.Now;//更新一下数据库当前的记录
                }
                else//不在线
                {
                    memberInfo.Info = this.GetMemberInfo(member);//那么服务端没有该角色数据，必须调用数据库生成N格式信息
                    memberInfo.Status = 0;
                }

                members.Add(memberInfo);
            }
            return members;
        }

        //不在线成员：需从数据库中调出生成网络格式信息
        NCharacterInfo GetMemberInfo(TGuildMember member)
        {
            return new NCharacterInfo()
            {
                Id = member.CharacterId,
                Name = member.Name,
                Class = (CharacterClass)member.Class,
                Level = member.Level
            };
        }

        //得到所有的申请信息：给会长看的，初始化公会信息调用【GuildInfo(character)】
        List<NGuildApplyInfo> GetApplyInfos()
        {
            List<NGuildApplyInfo> applies = new List<NGuildApplyInfo>();
            foreach (var apply in this.Data.Applies)
            {
                if (apply.Result != (int)ApplyResult.None) continue;//如果申请加入的申请已经接受或拒绝，就不需要再次加载了
                applies.Add(new NGuildApplyInfo()
                {
                    characterId = apply.CharacterId,
                    GuildId = apply.GuildId,
                    Class = apply.Class,
                    Level = apply.Level,
                    Name = apply.Name,
                    Result = (ApplyResult)apply.Result
                });
            }
            return applies;
        }

        //得到某个成员的数据库信息
        TGuildMember GetDBMember(int characterId)
        {
            foreach(var member in this.Data.Members)
            {
                if (member.CharacterId == characterId)
                    return member;
            }
            return null;
        }

        public void ExecuteAdmin(GuildAdminCommand command, int targetId, int sourceId)
        {
            var target = GetDBMember(targetId);
            var source = GetDBMember(sourceId);
            switch(command)
            {
                case GuildAdminCommand.Promote:     //晋升-升为副会长
                    target.Title = (int)GuildTitle.VicePresident;
                    break;
                case GuildAdminCommand.Depost:      //罢免-降为普通群众
                    target.Title = (int)GuildTitle.None;
                    break;
                case GuildAdminCommand.Transfer:    //转让会长
                    target.Title = (int)GuildTitle.President;
                    source.Title = (int)GuildTitle.None;
                    this.Data.LeaderID = targetId;
                    this.Data.LeaderName = target.Name;
                    break;
                case GuildAdminCommand.Kickout:     //踢人
                    Leave(targetId);
                    break;
            }
            DBService.Instance.Save();
            timestamp = TimeUtil.timestamp;
        }

        //修改公告【自己编写】
        public void ModifyNotice(string notice)
        {
            this.Data.Notice = notice;

            DBService.Instance.Save();
            timestamp = TimeUtil.timestamp;
        }
    }
}
