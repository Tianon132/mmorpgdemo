using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Services;
using Managers;

public class UIGuildApplyList : UIWindow {

	public GameObject itemPrefab;
	public ListView listMain;
	public Transform itemRoot;
	
	// Use this for initialization
	void Start () {
		GuildService.Instance.OnGuildUpdate += UpdateList;
		GuildService.Instance.SendGuildListRequest();
		this.UpdateList();
	}
	
	private void OnDestroy()
    {
		GuildService.Instance.OnGuildUpdate -= UpdateList;
    }

	void UpdateList()
    {
		ClearList();
		InitItems();
    }

	/// <summary>
	/// 初始化所有公会申请列表
	/// </summary>
	void InitItems()
    {
		foreach(var item in GuildManager.Instance.guildInfo.Applies)
        {
			GameObject go = Instantiate(itemPrefab, listMain.transform);
			UIGuildApplyItem ui = go.GetComponent<UIGuildApplyItem>();
			ui.SetItemInfo(item);
			this.listMain.AddItem(ui);
        }
    }


	void ClearList()
    {
		this.listMain.RemoveAll();
    }
}
