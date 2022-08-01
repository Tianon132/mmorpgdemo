using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkillBridge.Message;
using Services;

public class UIGuildApplyItem : ListView.ListViewItem {

	public Text nickName;
	public Text Level;
	public Text @class;

	public NGuildApplyInfo Info;

	public void SetItemInfo(NGuildApplyInfo info)
    {
		this.Info = info;
		if (this.nickName != null) this.nickName.text = this.Info.Name;
		if (this.Level != null) this.Level.text = this.Info.Level.ToString();
		if (this.@class != null) this.@class.text = this.Info.Class.ToString();
    }

	public void OnAccept()
    {
		MessageBox.Show(string.Format("要通过【{0}】的公会申请吗？", this.Info.Name), "审批申请", MessageBoxType.Confirm, "同意加入", "取消").OnYes = () =>
		 {
			 GuildService.Instance.SendGuildJoinApply(true, this.Info);
		 };
    }

	public void OnDecline()
	{
		MessageBox.Show(string.Format("要拒绝【{0}】的公会申请吗？", this.Info.Name), "审批申请", MessageBoxType.Confirm, "拒绝加入", "取消").OnYes = () =>
		{
			GuildService.Instance.SendGuildJoinApply(false, this.Info);
		};
	}
}
