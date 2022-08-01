using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkillBridge.Message;
using Common;

public class UIGuildInfo : MonoBehaviour {

	public Text guildName;
	public Text guildID;
	public Text leader;

	public Text notice;
	public Text memberNember;

	private NGuildInfo info;
	public NGuildInfo Info
    {
        get { return this.info; }
        set { this.info = value; this.UpdateUI(); }//自动更新UI
    }

	void UpdateUI()
    {
		if (this.info == null)
        {
			this.guildName.text = "无";
			this.guildID.text = "ID：0";
			this.leader.text = "会长：无";
			this.notice.text = "";
			this.memberNember.text = string.Format("成员数量：0/{0}", 40);// GameDefine.GuildMaxMemberCount);
        }
		else
        {
			this.guildName.text = this.Info.GuildName;
			this.guildID.text = "ID：" + this.Info.Id;
			this.leader.text = "会长：" + this.Info.leaderName;
			this.notice.text = this.Info.Notice;
			this.memberNember.text = string.Format("成员数量：{0}/{1}", this.Info.memberCount, 40);// GameDefine.GuildMaxMemberCount);
		}
    }
}
