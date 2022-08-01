using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Models;
using System;
using Services;
using Managers;

public class UIFriends : UIWindow {

	public GameObject itemPrefab;
	public ListView listMain;
	public Transform itemRoot;
	public UIFriendItem selectedItem;

	// Use this for initialization
	void Start () {
		FriendService.Instance.OnFriendUpdate = RefreshUI;//网络发来更新消息就刷新好友列表
		this.listMain.onItemSelected += this.OnItemSelected;
		RefreshUI();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnItemSelected(ListView.ListViewItem item)//当某一行玩家信息被点击时记录该item
    {
		this.selectedItem = item as UIFriendItem;
    }

	public void OnClickFriendAdd()//点击-弹出添加好友界面
    {
		InputBox.Show("输入要添加的好友名称或ID", "添加好友").OnSubmit += OnFriendAddSubmit;
    }

	private bool OnFriendAddSubmit(string input, out string tips)//添加好友功能，添加到InputBox界面
    {
		tips = "";
		int friendId = 0;
		string friendName = "";

		if (!int.TryParse(input, out friendId))//字符串转换为数字类型
			friendName = input;
		if (friendId == User.Instance.CurrentCharacter.Id || friendName == User.Instance.CurrentCharacter.Name)
        {
			tips = "开玩笑吗？不能添加自己哦";
			return false;
        }

		FriendService.Instance.SendFriendAddRequest(friendId, friendName);
		return true;
    }

	public void OnClickFriendChar()//点击-私聊
    {
		if (selectedItem == null)
		{
			MessageBox.Show("请选择要私聊的好友");
			return;
		}
		ChatManager.Instance.StartPrivateChat(selectedItem.Info.friendInfo.Id, selectedItem.Info.friendInfo.Name);
    }

	public void OnClickFriendTeamInvite()
    {
		if(selectedItem == null)
        {
			MessageBox.Show("请选择要邀请的好友");
			return;
        }
		if(selectedItem.Info.Status == 0)
        {
			MessageBox.Show("请选择在线的好友");
			return;
        }
		MessageBox.Show(string.Format("确定要邀请好友【{0}】加入队伍吗", selectedItem.Info.friendInfo.Name), "邀请好友组队", MessageBoxType.Confirm, "邀请", "取消").OnYes = () =>
		  {
			  TeamService.Instance.SendTeamInviteRequest(this.selectedItem.Info.friendInfo.Id, this.selectedItem.Info.friendInfo.Name);
		  };
    }

	public void OnClickFriendRemove()//点击-删除好友
    {
		if(selectedItem == null)
        {
			MessageBox.Show("请先选择要删除的好友");
			return;
        }
		else
        {
			var confirm = MessageBox.Show(string.Format("确定要删除好友【{0}】吗？", selectedItem.Info.friendInfo.Name), "删除好友", MessageBoxType.Confirm, "删除", "取消");//弹出框确认一下
			confirm.OnYes = () =>
			{
				FriendService.Instance.SendFriendRemoveRequest(this.selectedItem.Info.Id, this.selectedItem.Info.friendInfo.Id);
			};
			
        }
    }

	void RefreshUI()
    {
		ClearFriendList();
		InitFriendItems();
    }

	/// <summary>
	/// 清除好友列表
	/// </summary>
    private void ClearFriendList()
    {
		this.listMain.RemoveAll();
    }

	/// <summary>
	/// 初始化好友列表
	/// </summary>
	private void InitFriendItems()
	{
		foreach(var item in FriendManager.Instance.allFriends)
        {
			GameObject go = Instantiate(itemPrefab, this.listMain.transform);
			UIFriendItem ui = go.GetComponent<UIFriendItem>();
			ui.SetFriendInfo(item);
			this.listMain.AddItem(ui);
        }
	}
}
