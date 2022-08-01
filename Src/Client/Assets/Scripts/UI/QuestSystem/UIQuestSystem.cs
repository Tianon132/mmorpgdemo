using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using Models;
using Common.Data;

//任务系统
public class UIQuestSystem : UIWindow {

	public Text title;

	public GameObject itemPrefab;

	public TabView Tabs;
	public ListView listMain;
	public ListView listBranch;//两个列表框，主线与支线

	public UIQuestInfo questInfo;

	private bool showAvailableList = false;//是否显示可接任务的列表

	private ListView currentList;

	// Use this for initialization
	void Start () {
		this.listMain.onItemSelected += this.OnQuestSelected;//任务Item被选中，添加委托（在右边任务信息中具体显示）
		this.listBranch.onItemSelected += this.OnQuestSelected;
		this.Tabs.OnTabSelect += OnSelectTab;
		RefreshUI();
		//QuestManager.Instance.OnQuestChanged += RefreshUI;
	}

	//实现标签的切换
	void OnSelectTab(int idx)//初次进入start()，显示的是进行中的任务，选中可接的标签之后才会显示可接的任务
    {
		showAvailableList = idx == 1;
		RefreshUI();
    }

	private void OnDestory()
    {
		//QuestManager.Instance.OnQuestChanged -= RefreshUI;
    }

	void RefreshUI()
    {
		ClearAllQuestList();
		InitAllQuestList();
    }

	/// <summary>
	/// 初始化所有的任务列表
	/// </summary>
	void InitAllQuestList()
    {
		foreach(var kv in QuestManager.Instance.allQuests)//拉去所有可用的任务（除了无效的任务都有）
        {
			if(showAvailableList)
            {
				if (kv.Value.Info != null)//不为空说明任务已接，就不需要在已接任务显示
					continue;
            }
			else
            {
				if (kv.Value.Info == null)//不显示
					continue;
            }

			GameObject go = Instantiate(itemPrefab, kv.Value.Define.Type == QuestType.Main ? this.listMain.transform : this.listBranch.transform);//决定在实例化主线还是支线
			UIQuestItem ui = go.GetComponent<UIQuestItem>();
			ui.SetQuestInfo(kv.Value);
			if (kv.Value.Define.Type == QuestType.Main)
				this.listMain.AddItem(ui);
			else
				this.listBranch.AddItem(ui as ListView.ListViewItem);//as可以不用写，父子关系
        }
    }

	void ClearAllQuestList()
    {
		this.listMain.RemoveAll();
		this.listBranch.RemoveAll();
    }

	//显示被选择的任务信息
	public void OnQuestSelected(ListView.ListViewItem item)
    {
		if(this.currentList == null)
        {
			this.currentList = item.owner;
        }
		else if(this.currentList != item.owner)
        {
			this.currentList.SelectedItem = null;
			this.currentList = item.owner;
        }

		UIQuestItem questItem = item as UIQuestItem;//父类转化为子类
		this.questInfo.SetQuestInfo(questItem.quest);//设置任务信息
    }
}
