using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Services;
using Managers;
using SkillBridge.Message;

public class UIGuild : UIWindow {

	public GameObject itemPrefab;
	public ListView listMain;
	public Transform itemRoot;
	public UIGuildInfo uiInfo;
	public UIGuildMemberItem selectedItem;

	public GameObject panelAdmin;
	public GameObject panelLeader;//普通成员的不加，因为它永远显示在那里

	void Start()
    {
		GuildService.Instance.OnGuildUpdate += UpdateUI;//公会信息更新  //由于多个界面接收【下面还有个事件还要用到这个UI，不是独占就只能+=】，所以+=
		this.listMain.onItemSelected += this.OnGuildMemberSelected;
		this.UpdateUI();
    }

	void OnDestroy()
    {
		GuildService.Instance.OnGuildUpdate -= UpdateUI;
    }

	void UpdateUI()
    {
		this.uiInfo.Info = GuildManager.Instance.guildInfo;

		ClearList();
		InitItems();

		this.panelAdmin.SetActive(GuildManager.Instance.myMemberInfo.Title > GuildTitle.None);//会长和副会长都显示
		this.panelLeader.SetActive(GuildManager.Instance.myMemberInfo.Title == GuildTitle.President);
    }

	public void OnGuildMemberSelected(ListView.ListViewItem item)
    {
		this.selectedItem = item as UIGuildMemberItem;
    }

	/// <summary>
	/// 初始化所有装备列表
	/// </summary>
	void InitItems()
    {
		foreach (var item in GuildManager.Instance.guildInfo.Members)
        {
			GameObject go = Instantiate(itemPrefab, this.listMain.transform);
			UIGuildMemberItem ui = go.GetComponent<UIGuildMemberItem>();
			ui.SetGuildMemberInfo(item);
			this.listMain.AddItem(ui);
        }
    }

	void ClearList()
    {
		this.listMain.RemoveAll();
    }

	public void OnClickAppliesList()
    {
		UIManager.Instance.Show<UIGuildApplyList>();
    }

	/// <summary>
	/// 离开公会
	/// </summary>
	public void OnClickLeave()
    {
		if(GuildManager.Instance.myMemberInfo.Title == GuildTitle.President)
        {
			MessageBox.Show("你要退出公会怎么办", "退出公会");
		}
		else
        {
			MessageBox.Show("确定要退出公会吗？", "退出公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
			{
				GuildService.Instance.SendGuildLeaveRequest();
			};
		}
    }

	/// <summary>
	/// 聊天
	/// </summary>
	public void OnClickChat()
    {
		MessageBox.Show("下一节再学");
    }

	/// <summary>
	/// 公会踢人
	/// </summary>
	public void OnClickKickout()
    {
		if(selectedItem == null)
        {
			MessageBox.Show("请选择要踢出的成员");
			return;
        }
		MessageBox.Show(string.Format("要踢【{0}】出公会吗？", this.selectedItem.Info.Info.Name), "踢出公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
		{
			GuildService.Instance.SendAdminCommand(GuildAdminCommand.Kickout, this.selectedItem.Info.Info.Id);
		};
    }

	/// <summary>
	/// 公会晋升
	/// </summary>
	public void OnClickPromote()
    {
		if(selectedItem == null)
        {
			MessageBox.Show("请选择要晋升的成员");
			return;
        }
		if (selectedItem.Info.Title != GuildTitle.None)
        {
			MessageBox.Show("对方已经身份尊贵");
			return;
        }
		MessageBox.Show(string.Format("要晋升【{0}】为公会副会长吗？", this.selectedItem.Info.Info.Name), "晋升公会成员", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
		{
			GuildService.Instance.SendAdminCommand(GuildAdminCommand.Promote, this.selectedItem.Info.Info.Id);
		};
    }

	/// <summary>
	/// 公会罢免
	/// </summary>
	public void OnClickDepose()
    {
		if (selectedItem == null)
		{
			MessageBox.Show("请选择要罢免的成员");
			return;
		}
		if (selectedItem.Info.Title == GuildTitle.None)
		{
			MessageBox.Show("对方貌似无职可免");
			return;
		}
		if(selectedItem.Info.Title == GuildTitle.President)
        {
			MessageBox.Show("会长地位不可撼动");
			return;
        }
		MessageBox.Show(string.Format("要罢免【{0}】为公会职务吗？", this.selectedItem.Info.Info.Name), "罢免公会成员", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
		{
			GuildService.Instance.SendAdminCommand(GuildAdminCommand.Depost, this.selectedItem.Info.Info.Id);
		};
	}

	/// <summary>
	/// 公会转让
	/// </summary>
	public void OnClickTransfer()
    {
		if(selectedItem == null)
        {
			MessageBox.Show("请选择要把会长转让给的成员");
			return;
        }
		MessageBox.Show(string.Format("要把会长转给【{0}】吗？", this.selectedItem.Info.Info.Name), "公会转让", MessageBoxType.Confirm, "确定", "取消").OnYes =() => 
		{
			MessageBox.Show(string.Format("再次确认：你真的要把会长转给【{0}】吗？", this.selectedItem.Info.Info.Name), "公会转让", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
			{
				GuildService.Instance.SendAdminCommand(GuildAdminCommand.Transfer, this.selectedItem.Info.Info.Id);
			};
		};
    }

	/// <summary>
	/// 修改公告
	/// </summary>
	public void OnClickSetNotice()
    {
        InputBox.Show("修改公告", "修改公会公告", "提交", "取消", "公告可不能为空哦！").OnSubmit += SetNotice;
	}

    private bool SetNotice(string inputText, out string tips)
    {
		tips = "";
		if (inputText == string.Empty)
        {
			tips = "不能为空";
			return false;
		}
		GuildService.Instance.SendGuild(inputText);
        return true;
	}
}
