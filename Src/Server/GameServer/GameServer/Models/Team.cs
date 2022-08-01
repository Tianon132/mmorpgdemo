using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Entities;
using Common;
using SkillBridge.Message;
using Common.Utils;

namespace GameServer.Models
{
    class Team
    {
        public int Id;
        public Character Leader;

        public List<Character> Members = new List<Character>();

        public double timestamp;//时间戳：队伍信息更改的时间

        //初始化队伍：添加队长
        public Team(Character leader)
        {
            this.AddMember(leader);
        }

        //队伍添加成员
        public void AddMember(Character member)
        {
            if(this.Members.Count == 0)
            {
                this.Leader = member;//队长为第一个人
            }
            this.Members.Add(member);
            member.Team = this;
            timestamp = TimeUtil.timestamp;
        }

        //队伍移除成员
        public void RemoveMember(Character member)
        {
            Log.InfoFormat("Leave Team : {0}:{1}", member.Id, member.Info.Name);
            this.Members.Remove(member);
            if(member == this.Leader)
            {
                if (this.Members.Count > 0)
                    this.Leader = this.Members[0];//任命新队长
                else
                    this.Leader = null;
            }
            member.Team = null;
            timestamp = TimeUtil.timestamp;
        }

        //队伍列表更新的后处理：被动更新，被携带的更新
        public void PostProcess(NetMessageResponse message)
        {
            if(message.teamInfo == null)//取消想好友那样的true，false机制是因为发送完一个人，true、flase状态就马上切换，不能够给其他成员再次发送，所以需要用时间戳这个变量
            {
                message.teamInfo = new TeamInfoResponse();
                message.teamInfo.Result = Result.Success;
                message.teamInfo.Team = new NTeamInfo();
                message.teamInfo.Team.Id = this.Id;
                message.teamInfo.Team.Leader = this.Leader.Id;
                foreach(var member in this.Members)//添加成员，给Client发送过去
                {
                    message.teamInfo.Team.Members.Add(member.GetBasicInfo());
                }
            }
        }
    }
}
