using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Services;
using SkillBridge.Message;

public class UIGuildList : UIWindow {

	public GameObject itemPrefab;
	public ListView listMain;
	public Transform itemRoot;
	public UIGuildInfo uiInfo;
	public UIGuildItem selectedItem;

	// Use this for initialization
	public void Start () {
		this.listMain.onItemSelected += this.OnGuildMemberSelected;//监听一个选中事件
		this.uiInfo.Info = null;
		GuildService.Instance.OnGuildListResult += UpdateGuildList;//得到一个刷新response就触发 刷新
		GuildService.Instance.SendGuildListRequest();
	}
	
	public void OnDestroy()
    {
		GuildService.Instance.OnGuildListResult -= UpdateGuildList;
    }

	void UpdateGuildList(List<NGuildInfo> guilds)//刷新
    {
		ClearList();
		InitItems(guilds);
    }

	public void OnGuildMemberSelected(ListView.ListViewItem item)//当选中Item的时候赋值
    {
		this.selectedItem = item as UIGuildItem;
		this.uiInfo.Info = this.selectedItem.Info;
    }

	/// <summary>
	/// 初始化所有公会列表
	/// </summary>
	/// <param name="guilds"></param>
	void InitItems(List<NGuildInfo> guilds)
    {
		foreach(var item in guilds)
        {
			GameObject go = Instantiate(itemPrefab, this.listMain.transform);
			UIGuildItem ui = go.GetComponent<UIGuildItem>();
			ui.SetGuildInfo(item);
			this.listMain.AddItem(ui);
        }
    }

	void ClearList()
    {
		this.listMain.RemoveAll();
    }

	public void OnClickJoin()
    {
		if(selectedItem == null)
        {
			MessageBox.Show("请选择要加入的公会");
			return;
        }
		MessageBox.Show(string.Format("确定要加入公会【{0}】吗？", selectedItem.Info.GuildName), "申请加入公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
		{
			 GuildService.Instance.SendGuildJoinRequest(this.selectedItem.Info.Id);
		};
    }
}
