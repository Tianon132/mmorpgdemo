using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkillBridge.Message;

public class UIFriendItem : ListView.ListViewItem {

	public Text nickname;
	public Text @class;
	public Text level;
	public Text status;

	public Image background;
	public Sprite normalBg;
	public Sprite selectedBg;

	public override void onSelected(bool selected)
    {
		this.background.overrideSprite = selected ? selectedBg : normalBg;//selected为真在前，为错在后
	}

	public NFriendInfo Info;

	// Use this for initialization
	void Start () {
		
	}

	public void SetFriendInfo(NFriendInfo item)
    {
		this.Info = item;
		if (this.nickname != null) this.nickname.text = this.Info.friendInfo.Name;
		if (this.@class != null) this.@class.text = this.Info.friendInfo.Class.ToString();
		if (this.level != null) this.level.text = this.Info.friendInfo.Level.ToString();
		if (this.status != null) this.status.text = this.Info.Status == 1 ? "在线" : "离线";
    }
}
