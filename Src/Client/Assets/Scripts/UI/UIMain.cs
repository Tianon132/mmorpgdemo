using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Models;
using Services;
using Managers;

public class UIMain : MonoSingleton<UIMain> {

	public Text avaterName;
	public Text avaterLevel;

	public UITeam TeamWindow;//只会存在Main界面上，故属性直接赋予进行控制

	// Use this for initialization
	protected override void OnStart () {
		this.UpdateAvater();
	}
	
	// Update is called once per frame
	void UpdateAvater () {
		this.avaterName.text = string.Format("{0}[{1}]", User.Instance.CurrentCharacter.Name, User.Instance.CurrentCharacter.Id);
		this.avaterLevel.text = User.Instance.CurrentCharacter.Level.ToString();
	}

	/*  删除这块，移除到菜单UISetting.cs中
	public void BackToCharSelect() //返回到选择角色界面
    {
		SceneManager.Instance.LoadScene("CharSelect");
		UserService.Instance.SendGameLeave();
	}*/

	public void OnClickTest()
    {
		UITest test = UIManager.Instance.Show<UITest>();
		test.setTitle("这是一个测试UI");
    }

	private void  Test_OnClose(UIWindow sender, UIWindow.WindowResult result)
    {
		MessageBox.Show("点击了对话框的：" + result, "对话框响应结果", MessageBoxType.Information);
    }
	
	public void OnClickBag()//测试打开背包
    {
		UIManager.Instance.Show<UIBag>();
    }

	public void OnClickCharEquip()//打开装备
    {
		UIManager.Instance.Show<UICharEquip>();
    }

	public void OnClickQuest()
    {
		UIManager.Instance.Show<UIQuestSystem>();
    }

	public void OnClickFriends()
    {
		UIManager.Instance.Show<UIFriends>();
    }

	public void ShowTeamUI(bool show)
    {
		TeamWindow.ShowTeam(show);
    }

	public void OnClickGuild()//点击公会
    {
		GuildManager.Instance.ShowGuild();//不用UIManager是因为需要一些逻辑要实现
    }

	public void OnClickRide()//点击坐骑
    {
		UIManager.Instance.Show<UIRide>();
    }

	public void OnClickSetting()//点击设置
	{
		UIManager.Instance.Show<UISetting>();
	}

	public void OnClickSkill()//点击技能
    {

    }
}
