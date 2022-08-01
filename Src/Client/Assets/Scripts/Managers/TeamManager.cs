using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SkillBridge.Message;
using Models;

namespace Managers
{
    class TeamManager : Singleton<TeamManager>
    {
        public void Init()
        {

        }

        public void UpdateTeamInfo(NTeamInfo team)//网络信息扔给user
        {
            User.Instance.TeamInfo = team;
            ShowTeamUI(team != null);
        }

        public void ShowTeamUI(bool show)
        {
            if(UIMain.Instance != null)//只有存在的时候再刷新
            {
                UIMain.Instance.ShowTeamUI(show);
            }
        }
    }
}
