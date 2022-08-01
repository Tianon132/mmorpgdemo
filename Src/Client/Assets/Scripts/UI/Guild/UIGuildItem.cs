using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkillBridge.Message;

public class UIGuildItem : ListView.ListViewItem {

	public Text ID;
	public Text GuildName;
	public Text memberCount;
	public Text leaderName;

	public Image background;
	public Sprite normalBg;
	public Sprite selectedBg;

    public override void onSelected(bool selected)
    {
		this.background.overrideSprite = selected ? selectedBg : normalBg;
    }

    public NGuildInfo Info;

	public void SetGuildInfo(NGuildInfo item)
    {
		this.Info = item;
		if (this.ID != null) ID.text = this.Info.Id.ToString();
		if (this.GuildName != null) ID.text = this.Info.GuildName.ToString();
		if (this.memberCount != null) ID.text = this.Info.memberCount.ToString();
		if (this.leaderName != null) ID.text = this.Info.leaderName.ToString();

	}
}
