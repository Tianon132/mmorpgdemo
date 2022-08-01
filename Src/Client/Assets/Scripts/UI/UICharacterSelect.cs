using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkillBridge.Message;
using Services;
using Models;

public class UICharacterSelect : MonoBehaviour {

	public GameObject panelCreate;
	public GameObject panelSelect;
	public GameObject btnCreateCancel;
	public InputField charName;
	CharacterClass charClass;

	public Transform uiCharList;
	public GameObject uiCharInfo;
	public List<GameObject> uiChars = new List<GameObject>();

	private int selectCharacterIdx = -1;

	public UICharacterView characterView;

	public Image[] titles;
	public Text[] names;
	public Text doscs;

	// Use this for initialization
	void Start () {
		//DataManager.Instance.Load();  //在没网络的时候调用

		InitCharacterSelect(true);//初始化--进入选择界面
		UserService.Instance.OnCharacterCreate = OnCharacterCreate;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void InitCharacterSelect(bool init)
    {
		panelCreate.SetActive(false);
		panelSelect.SetActive(true);

		if(init)
        {
			//清除掉原来列表上的角色信息
			foreach(var old in uiChars)
            {
				Destroy(old);
            }
			uiChars.Clear();

			//添加右边列表上的角色信息
			for(int i=0; i<User.Instance.Info.Player.Characters.Count; i++)
            {
				GameObject go = Instantiate(uiCharInfo, this.uiCharList);
				UICharInfo chrinfo = go.GetComponent<UICharInfo>();
				chrinfo.info = User.Instance.Info.Player.Characters[i];//获得信息

				//对控件进行初始化、没有绑定，动态创建
				Button button = go.GetComponent<Button>();
				int idx = i;
				button.onClick.AddListener(() =>    //对onClick添加监听器。监听当被按下的时候监听什么事，当被按下的时候执行选择角色
				{
					OnSelectCharacter(idx);
				});

				uiChars.Add(go);
				go.SetActive(true);
            }
        }
    }

	//点击按钮：切换创建角色界面
	public void InitCharacterCreate()
    {
		panelCreate.SetActive(true);
		panelSelect.SetActive(false);
    }

	//点击按钮：点击开始冒险按钮
	public void OnClickCreate()
    {
		if(string.IsNullOrEmpty(this.charName.text))
        {
			MessageBox.Show("请输入角色名称");
			return;
        }

		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
		UserService.Instance.SendCharacterCreate(this.charName.text, this.charClass);
    }

	//注册角色界面中：选择3个不同职业功能
	public void OnSelectClass(int charClass)
    {
		this.charClass = (CharacterClass)charClass;

		characterView.CurrentCharacter = charClass - 1;//当前显示的角色，减1是因为外面Class是用1,2,3保存的，而列表是从0起步

		for(int i=0; i<3; i++)
        {
			titles[i].gameObject.SetActive(i == charClass - 1);//右上角标题
			names[i].text = DataManager.Instance.Characters[i + 1].Name;//查询职业类型的名字
        }
		doscs.text = DataManager.Instance.Characters[charClass].Description;//查询内容描述
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

	void OnCharacterCreate(Result result, string message)
    {
		if (result == Result.Success)
			InitCharacterSelect(true);
		else
			MessageBox.Show(message, "错误", MessageBoxType.Error);
    }

	//点击：选择列表上的角色，显示（选择页面）
	public void OnSelectCharacter(int idx)
    {
		this.selectCharacterIdx = idx;
		var cha = User.Instance.Info.Player.Characters[idx];
		Debug.LogFormat("Select Char:[{0}{1}{2}]", cha.Id, cha.Name, cha.Class);
		//User.Instance.CurrentCharacter = cha;  //这个是最早的时候方便客户端使用添加，但到后面在这里赋值并没有entityId赋值，故删除；在MapService中进入主城的响应中赋予
		characterView.CurrentCharacter = ((int)cha.Class - 1);//角色选择


		//作用：选择界面中点击右边列表，显示相应的角色以及按钮亮起
		/*
		UICharInfo ci = this.uiChars[idx].GetComponent<UICharInfo>();
		ci.Selected = true;
		*/
		
		for (int i=0;i<User.Instance.Info.Player.Characters.Count;i++)//循环一遍
        {
			UICharInfo ci = this.uiChars[i].GetComponent<UICharInfo>();
			//********************这个地方纠结了四天，仅仅因为一个中括号内的i
			ci.Selected = idx == i;
        }
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

	//按钮：进入游戏
	public void OnClickPlay()
	{
		SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
		if (selectCharacterIdx >= 0)
        {
			//MessageBox.Show("进入游戏", "进入游戏", MessageBoxType.Confirm);
			UserService.Instance.SendGameEnter(selectCharacterIdx);
        }
    }
}
