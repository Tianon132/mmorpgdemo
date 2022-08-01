using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using SkillBridge.Message;
using Network;
using UnityEngine;
using Managers;

namespace Services
{
    class GuildService : Singleton<GuildService>, IDisposable
    {
        public UnityAction OnGuildUpdate;//刷新自己公会的信息
        public UnityAction<bool> OnGuildCreateResult;//创建公会 收到结果时的响应

        public UnityAction<List<NGuildInfo>> OnGuildListResult;//刷新公会列表信息 收到公会列表的响应

        public void Init()
        {

        }

        public GuildService()
        {
            MessageDistributer.Instance.Subscribe<GuildCreateResponse>(this.OnGuildCreate);
            MessageDistributer.Instance.Subscribe<GuildListResponse>(this.OnGuildList);//list是多个公会
            MessageDistributer.Instance.Subscribe<GuildJoinRequest>(this.OnGuildJoinRequest);
            MessageDistributer.Instance.Subscribe<GuildJoinResponse>(this.OnGuildJoinResponse);
            MessageDistributer.Instance.Subscribe<GuildResponse>(this.OnGuild);//单个公会
            MessageDistributer.Instance.Subscribe<GuildLeaveResponse>(this.OnGuildLeave);
            MessageDistributer.Instance.Subscribe<GuildAdminResponse>(this.OnGuildAdmin);
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<GuildCreateResponse>(this.OnGuildCreate);
            MessageDistributer.Instance.Unsubscribe<GuildListResponse>(this.OnGuildList);
            MessageDistributer.Instance.Unsubscribe<GuildJoinRequest>(this.OnGuildJoinRequest);
            MessageDistributer.Instance.Unsubscribe<GuildJoinResponse>(this.OnGuildJoinResponse);
            MessageDistributer.Instance.Unsubscribe<GuildResponse>(this.OnGuild);
            MessageDistributer.Instance.Unsubscribe<GuildLeaveResponse>(this.OnGuildLeave);
            MessageDistributer.Instance.Unsubscribe<GuildAdminResponse>(this.OnGuildAdmin);
        }

        /// <summary>
        /// 发送创建公会
        /// </summary>
        /// <param name="guildName"></param>
        /// <param name="notice"></param>
        public void SendGuildCreate(string guildName, string notice)
        {
            Debug.Log("SendGuildCreate");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildCreate = new GuildCreateRequest();
            message.Request.guildCreate.GuildName = guildName;
            message.Request.guildCreate.GuildNotice = notice;
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 收到创建响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private void OnGuildCreate(object sender, GuildCreateResponse response)
        {
            Debug.LogFormat("OnGuildCreate : {0}", response.Result);
            if (OnGuildCreateResult != null)
            {
                this.OnGuildCreateResult(response.Result == Result.Success);//关闭界面的调用
            }
            if (response.Result == Result.Success)
            {
                GuildManager.Instance.Init(response.guildInfo);//初始化公会信息
                MessageBox.Show(string.Format("{0} 公会创建成功", response.guildInfo.GuildName), "公会");
            }
            else
                MessageBox.Show(string.Format("{0} 公会创建失败", response.guildInfo.GuildName), "公会");
        }

        /// <summary>
        /// 发送加入公会请求：普通成员加入申请
        /// </summary>
        /// <param name="guildId"></param>
        public void SendGuildJoinRequest(int guildId)
        {
            Debug.Log("SendGuildJoinRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildjoinReq = new GuildJoinRequest();
            message.Request.guildjoinReq.Apply = new NGuildApplyInfo();
            message.Request.guildjoinReq.Apply.GuildId = guildId;
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 发送加入公会请求：会长处理的结果发送
        /// </summary>
        /// <param name="accept"></param>
        /// <param name="request"></param>
        public void SendGuildJoinResponse(bool accept, GuildJoinRequest request)
        {
            Debug.Log("SendGuildJoinResponse");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildjoinRes = new GuildJoinResponse();
            message.Request.guildjoinRes.Result = Result.Success;
            message.Request.guildjoinRes.Apply = request.Apply;
            message.Request.guildjoinRes.Apply.Result = accept ? ApplyResult.Accept : ApplyResult.Beject;
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 收到加入公会的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnGuildJoinRequest(object sender, GuildJoinRequest request)
        {
            var confirm = MessageBox.Show(string.Format("{0} 申请加入公会", request.Apply.Name), "公会申请", MessageBoxType.Confirm, "确定", "拒绝");
            confirm.OnYes = () =>
            {
                SendGuildJoinResponse(true, request);
            };
            confirm.OnNo = () =>
            {
                SendGuildJoinResponse(false, request);
            };
        }

        /// <summary>
        /// 收到加入公会的响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private void OnGuildJoinResponse(object sender, GuildJoinResponse response)
        {
            Debug.LogFormat("OnGuildJoinResponse : {0}", response.Result);
            if (response.Result == Result.Success)
                MessageBox.Show("加入公会成功", "公会");
            else
                MessageBox.Show(response.Errormsg, "公会");
        }

        //公会信息刷新【自己添加】
        public void SendGuild(string notice)
        {
            Debug.LogFormat("SendGuild,内容：{0}", notice);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.Guild = new GuildRequest();
            message.Request.Guild.guildInfo = new NGuildInfo();
            message.Request.Guild.guildInfo = GuildManager.Instance.guildInfo;
            message.Request.Guild.guildInfo.Notice = notice;
            NetClient.Instance.SendMessage(message);
        }

        private void OnGuild(object sender, GuildResponse response)
        {
            Debug.LogFormat("OnGuild: {0} {1}:{2}", response.Result, response.guildInfo.Id, response.guildInfo.GuildName);
            
            //修改公告【自己添加】
            if(!string.IsNullOrEmpty(response.Errormsg))
            {
                MessageBox.Show(response.Errormsg, "修改公告");
            }

            GuildManager.Instance.Init(response.guildInfo);
            if (this.OnGuildUpdate != null)
                this.OnGuildUpdate();
        }

        /// <summary>
        /// 发送离开公会的请求
        /// </summary>
        public void SendGuildLeaveRequest()
        {
            Debug.LogFormat("SendGuildLeaveRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildLeave = new GuildLeaveRequest();
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 离开公会的响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private void OnGuildLeave(object sender, GuildLeaveResponse response)
        {
            if (response.Result == Result.Success)
            {
                GuildManager.Instance.Init(null);
                MessageBox.Show("离开公会成功", "公会");
            }
            else
                MessageBox.Show("离开公会失败", "公会",MessageBoxType.Error);
        }

        /// <summary>
        /// 发送 【加入公会】多个公会列表 刷新的请求
        /// </summary>
        public void SendGuildListRequest()
        {
            Debug.Log("SendGuildListRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildList = new GuildListRequest();
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 收到【加入公会】 多个公会列表 刷新 的响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnGuildList(object sender, GuildListResponse response)
        {
            if (OnGuildListResult != null)
            {
                this.OnGuildListResult(response.Guilds);
            }
        }

        /// <summary>
        /// 发送加入公会审批 【管理员审批加入请求】 的请求
        /// </summary>
        /// <param name="accept"></param>
        /// <param name="apply"></param>
        public void SendGuildJoinApply(bool accept, NGuildApplyInfo apply)
        {
            Debug.Log("SendGuildJoinApply");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildjoinRes = new GuildJoinResponse();
            message.Request.guildjoinRes.Result = Result.Success;
            message.Request.guildjoinRes.Apply = apply;
            message.Request.guildjoinRes.Apply.Result = accept ? ApplyResult.Accept : ApplyResult.Beject;
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 发送管理公会的指令 请求
        /// </summary>
        /// <param name="command"></param>
        /// <param name="characterId"></param>
        public void SendAdminCommand(GuildAdminCommand command, int characterId)
        {
            Debug.Log("SemdAdminCommand");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildAdmin = new GuildAdminRequest();
            message.Request.guildAdmin.Command = command;
            message.Request.guildAdmin.Target = characterId;
            NetClient.Instance.SendMessage(message);
        }

        /// <summary>
        /// 收到加入公会的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnGuildAdmin(object sender, GuildAdminResponse message)
        {
            Debug.LogFormat("OnGuildAdmin :{0}:{1}", message.Command, message.Result);
            MessageBox.Show(string.Format("执行操作：{0} 结果：{1} {2}", message.Command, message.Result, message.Errormsg));
        }
    }
}
