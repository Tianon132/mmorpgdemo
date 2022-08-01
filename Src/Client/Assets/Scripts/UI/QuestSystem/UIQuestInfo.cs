using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;
using Managers;

public class UIQuestInfo : MonoBehaviour {

	public Text title;

	public Text[] targets;

	public Text description;

	public Text overview;//任务预览

	public UIIconItem rewardItems;

	public Text rewardMoney;
	public Text rewardExp;

	public Button navButton;//导航按钮的绑定
	public int npc = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	//任务具体信息
	public void SetQuestInfo(Quest quest)
    {
		this.title.text = string.Format("[{0}]{1}", quest.Define.Type, quest.Define.Name);//任务标题
		if (this.overview != null) this.overview.text = quest.Define.Overview;//任务预览显示

		if(this.description != null)
        {
			if (quest.Info == null)
			{
				this.description.text = quest.Define.Dialog;//任务描述赋值
			}
			else
			{
				if (quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
				{
					this.description.text = quest.Define.DialogFinish;//根据任务状态判断是否任务完成的对话
				}
			}
		}

		this.rewardMoney.text = quest.Define.RewardGold.ToString();
		this.rewardExp.text = quest.Define.RewardExp.ToString();

		//Nav寻路
		if (quest.Info == null)
		{
			this.npc = quest.Define.AcceptNPC;//任务还没接，找接受npc
		}
		else if (quest.Info.Status == SkillBridge.Message.QuestStatus.Complated)
        {
			this.npc = quest.Define.SubmitNPC;//任务完成，找提交npc
        }
		this.navButton.gameObject.SetActive(this.npc > 0);//既不可接，也不可完成就不显示寻路按钮
		

		foreach(var fifter in this.GetComponentsInChildren<ContentSizeFitter>())//自适应布局，刷新一遍强制布局一次
        {
			fifter.SetLayoutVertical();
        }
    }

	public void OnClickAbandon()
    {

    }

	public void OnClickNav()
    {
		Vector3 pos = NpcManager.Instance.GetNpcPosition(this.npc);
		User.Instance.CurrentCharacterObject.StartNav(pos);
		UIManager.Instance.Close<UIQuestSystem>();
    }
}
