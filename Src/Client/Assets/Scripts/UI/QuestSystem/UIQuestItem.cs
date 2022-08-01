using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;

public class UIQuestItem : ListView.ListViewItem {

	public Text title;

	public Image background;
	public Sprite normalBg;
	public Sprite selectedBg;

	public override void onSelected(bool selected)
    {
		this.background.overrideSprite = selected ? selectedBg : normalBg;
    }

	public Quest quest;

	// Use this for initialization
	void Start () {
		
	}

	bool isEquiped = false;

	public void SetQuestInfo(Quest item)//右左边的Item来决定右边要显示的任务
    {
		this.quest = item;
		if (this.title != null) this.title.text = this.quest.Define.Name;
    }
}
