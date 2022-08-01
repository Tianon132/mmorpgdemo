using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Managers;
using GameServer.Entities;

namespace GameServer.Services
{
    class GuildService : Singleton<GuildService>
    {
        public GuildService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildCreateRequest>(this.OnGuildCreate);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildListRequest>(this.OnGuildList);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinRequest>(this.OnGuildJoinRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinResponse>(this.OnGuildJoinResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildLeaveRequest>(this.OnGuildLeave);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildAdminRequest>(this.OnGuildAdmin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildRequest>(this.OnGuild);
        }

        public void Init()
        {
            GuildManager.Instance.Init();
        }

        /// <summary>
        /// 创建公会
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnGuildCreate(NetConnection<NetSession> sender, GuildCreateRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildCreate: : GuildName:{0} character:[{1}]{2}", request.GuildName, character.Id, character.Name);
            sender.Session.Response.guildCreate = new GuildCreateResponse();
            if(character.Guild != null)//做两个检查
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "已经有公会";
                sender.SendResponse();
                return;
            }
            if(GuildManager.Instance.CheckNameExisted(request.GuildName))
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "公会名称已存在";
                sender.SendResponse();
                return;
            }

            GuildManager.Instance.CreateGuild(request.GuildName, request.GuildNotice, character);
            sender.Session.Response.guildCreate.guildInfo = character.Guild.GuildInfo(character);
            sender.Session.Response.guildCreate.Result = Result.Success;
            sender.SendResponse();
        }

        private void OnGuildList(NetConnection<NetSession> sender, GuildListRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildList: character:[{0}]{1}", character.Id, character.Name);

            sender.Session.Response.guildList = new GuildListResponse();
            sender.Session.Response.guildList.Guilds.AddRange(GuildManager.Instance.GetGuildsInfo());//直接发回多个
            sender.Session.Response.guildList.Result = Result.Success;
            sender.SendResponse();
        }

        /// <summary>
        /// 收到加公会的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnGuildJoinRequest(NetConnection<NetSession> sender, GuildJoinRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinRequest: : GuildId:{0} characterId:[{1}]{2}", request.Apply.GuildId, request.Apply.characterId, request.Apply.Name);
            var guild = GuildManager.Instance.GetGuild(request.Apply.GuildId);
            if(guild == null)
            {
                sender.Session.Response.guildjoinRes = new GuildJoinResponse();
                sender.Session.Response.guildjoinRes.Result = Result.Failed;
                sender.Session.Response.guildjoinRes.Errormsg = "公会不存在";
                sender.SendResponse();
            }

            request.Apply.characterId = character.Data.ID;//补充发送者的信息
            request.Apply.Name = character.Data.Name;
            request.Apply.Class = character.Data.Class;
            request.Apply.Level = character.Data.Level;

            if(guild.JoinApply(request.Apply))//公会加入一条加入请求
            {
                var leader = SessionManager.Instance.GetSession(guild.Data.LeaderID);
                if(leader != null)//判断会长在不在线，在线才发送
                {//给会长发送请求
                    leader.Session.Response.guildjoinReq = request;
                    leader.SendResponse();
                }
            }
            else//不用重复请求
            {
                sender.Session.Response.guildjoinRes = new GuildJoinResponse();
                sender.Session.Response.guildjoinRes.Result = Result.Failed;
                sender.Session.Response.guildjoinRes.Errormsg = "请勿重复申请";
                sender.SendResponse();
            }
        }

        /// <summary>
        /// 收到加公会的响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private void OnGuildJoinResponse(NetConnection<NetSession> sender, GuildJoinResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinResponse: : GuildId:{0} characterId:[{1}]{2}", response.Apply.GuildId, response.Apply.characterId, response.Apply.Name);

            var guild = GuildManager.Instance.GetGuild(response.Apply.GuildId);
            if(response.Result == Result.Success)
            {//会长接受了加公会的请求
                guild.JoinAppove(response.Apply);//修改数据库---修改这条【申请Apply】的信息以及添加一条新的【公会Member】的信息

                var requester = SessionManager.Instance.GetSession(response.Apply.characterId);//发送给申请者信息
                if (requester != null)
                {
                    requester.Session.Character.Guild = guild;//赋予公会信息

                    //向 请求者  发送加入公会成功的消息
                    requester.Session.Response.guildjoinRes = response;
                    requester.Session.Response.guildjoinRes.Result = Result.Success;
                    requester.Session.Response.guildjoinRes.Errormsg = "加入公会成功";
                    requester.SendResponse();
                }
            }
            
        }

        /// <summary>
        /// 离开公会
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnGuildLeave(NetConnection<NetSession> sender, GuildLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildLeave: :chracter:{0}", character.Id);
            sender.Session.Response.guildLeave = new GuildLeaveResponse();

            character.Guild.Leave(character.Id);
            sender.Session.Response.guildLeave.Result = Result.Success;

            DBService.Instance.Save();
            sender.SendResponse();
        }

        /// <summary>
        /// 收到 管理员操作 请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnGuildAdmin(NetConnection<NetSession> sender, GuildAdminRequest message)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildAdmin: :character:{0}", character.Id);
            sender.Session.Response.guildAdmin = new GuildAdminResponse();

            if(character.Guild == null)//如果没公会，说明这个人是非常规手段发送信息
            {
                sender.Session.Response.guildAdmin.Result = Result.Failed;
                sender.Session.Response.guildAdmin.Errormsg = "你没公会就不要乱来";
                sender.SendResponse();
                return;
            }

            character.Guild.ExecuteAdmin(message.Command, message.Target, character.Id);//默认一定成功

            var target = SessionManager.Instance.GetSession(message.Target);
            if(target != null)//在线发送结果
            {
                target.Session.Response.guildAdmin = new GuildAdminResponse();
                target.Session.Response.guildAdmin.Result = Result.Success;
                target.Session.Response.guildAdmin.Command = message;
                target.SendResponse();
            }

            sender.Session.Response.guildAdmin.Result = Result.Success;
            sender.Session.Response.guildAdmin.Command = message;
            sender.SendResponse();
        }

        /// <summary>
        /// 修改公会Notice的响应【自己编写】
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnGuild(NetConnection<NetSession> sender, GuildRequest message)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuild: :character:{0}:{1} 修改公会Notice的请求", character.Id, character.Name);

            character.Guild.ModifyNotice(message.guildInfo.Notice);
            sender.Session.Response.Guild = new GuildResponse();
            sender.Session.Response.Guild.guildInfo = message.guildInfo;
            sender.Session.Response.Guild.Result = Result.Success;
            sender.Session.Response.Guild.Errormsg = "公告修改成功";
            sender.SendResponse();
        }
    }
}
