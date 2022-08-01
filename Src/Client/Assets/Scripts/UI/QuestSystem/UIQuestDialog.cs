using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Models;

public class UIQuestDialog : UIWindow {

	public UIQuestInfo questInfo;//任务的信息内容

	public Quest quest;

	public GameObject openButtons;
	public GameObject submitButtons;

	// Use this for initialization
	void Start () {
		
	}

	//设置任务信息
	public void SetQuest(Quest quest)
    {
		this.quest = quest;
		this.UpdateQuest();
		if(this.quest.Info == null)//还没接过任务时
        {
			openButtons.SetActive(true);
			submitButtons.SetActive(false);
        }
		else
        {
			if(this.quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
            {
				openButtons.SetActive(false);
				submitButtons.SetActive(true);
            }
			else
            {
				openButtons.SetActive(false);
				submitButtons.SetActive(false);
            }
        }
    }

	// 更新任务信息
	void UpdateQuest () {
		if(this.quest != null)//任务不为空
        {
			if (this.questInfo != null)//任务信息不为空
            {
				this.questInfo.SetQuestInfo(quest);
            }
        }
	}
}
