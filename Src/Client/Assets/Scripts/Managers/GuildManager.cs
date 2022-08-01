using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkillBridge.Message;
using Models;

namespace Managers
{
    class GuildManager : Singleton<GuildManager>//管理自己所在公会的信息
    {
        public NGuildInfo guildInfo;

        public NGuildMenmberInfo myMemberInfo;//自己在公会中的职务信息

        public bool HasGuild//自己是否有公会
        {
            get { return this.guildInfo != null; }//通过N公会信息判断自己有没有公会
        }

        public void Init(NGuildInfo guild)
        {
            this.guildInfo = guild;

            if(guild == null)
            {
                myMemberInfo = null;//如果还没有公会，自己的公会职务信息清空
                return;
            }

            foreach(var mem in guild.Members)
            {
                if (mem.characterId == User.Instance.CurrentCharacter.Id)//遍历并找到自己在公会中的信息
                {
                    myMemberInfo = mem;
                    break;
                }
            }
        }

        public void ShowGuild()//显示公会
        {
            if(this.HasGuild)
            {
                UIManager.Instance.Show<UIGuild>();
            }
            else
            {
                var win = UIManager.Instance.Show<UIGuildPopNoGuild>();
                win.OnClose += PopNoGuild_OnClose;
            }
        }

        //点击按钮的时候 判断Yes/No 来决定创建还是加入
        private void PopNoGuild_OnClose(UIWindow sender, UIWindow.WindowResult result)
        {
            if(result == UIWindow.WindowResult.Yes)
            {//创建公会
                UIManager.Instance.Show<UIGuildPopCreate>();
            }
            else if(result == UIWindow.WindowResult.No)
            {//加入公会
                UIManager.Instance.Show<UIGuildList>();
            }
        }
    }
}
