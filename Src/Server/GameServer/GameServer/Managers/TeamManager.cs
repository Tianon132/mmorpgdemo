using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameServer.Entities;
using GameServer.Models;

namespace GameServer.Managers
{
    class TeamManager : Singleton<TeamManager>
    {
        public List<Team> Teams = new List<Team>();//方便遍历使用
        //前面的int是character的Id，这里具体指的是leader的Id
        public Dictionary<int, Team> CharacterTeams = new Dictionary<int, Team>();//方便精准查询使用
        //上面：list和Dictionary里面的Team是同一个Team，它们各自所保存的只是一个引用，
        //所以被占用的只有几个指针的空间，并没有浪费多少************重要

        public void Init()
        {

        }

        public Team GetTeamByCharacter(int characterId)//通过id得到队伍信息
        {
            Team team = null;
            this.CharacterTeams.TryGetValue(characterId, out team);
            return team;
        }

        public void AddTeamMember(Character leader, Character member)
        {
            if(leader.Team == null)
            {
                leader.Team = CreateTeam(leader);
            }
            leader.Team.AddMember(member);
        }

        //由于创建和解散队伍 会给内存造成极大的压力，所以这里默认只会创造队伍，不会释放，队伍只会越来越多
        Team CreateTeam(Character leader)
        {
            Team team = null;
            for(int i=0; i<this.Teams.Count; i++)
            {
                team = this.Teams[i];
                if(team.Members.Count == 0)
                {
                    team.AddMember(leader);
                    return team;
                }
            }
            team = new Team(leader);//创建
            this.Teams.Add(team);
            team.Id = this.Teams.Count;
            return team;
        }
    }
}
