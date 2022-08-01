using Candlelight.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UIChat : MonoBehaviour {

	public HyperText textArea;//聊天内容显示区域

	public TabView channelTab;

	public InputField chatText;//聊天输入控件
	public Text chatTarget;

	public Dropdown channelSelect;

	// Use this for initialization
	void Start () {
		this.channelTab.OnTabSelect += OnDisplayChannelSelected;//当选择切换框的时候 触发
		ChatManager.Instance.OnChat += RefreshUI;//有消息发回来的时候刷新
	}

	private void OnDestroy()
    {
		ChatManager.Instance.OnChat -= RefreshUI;//界面关闭注销
	}

	// Update is called once per frame
	void Update () {
		InputManager.Instance.IsInputMode = chatText.isFocused;//是否拥有焦点来决定是否在输入
	}

	/// <summary>
	/// 切换显示频道
	/// </summary>
	/// <param name="idx"></param>
	private void OnDisplayChannelSelected(int idx)
	{
		ChatManager.Instance.displayChannel = (ChatManager.LocalChannel)idx;//更换频道
		RefreshUI();
	}

    public void RefreshUI()
    {
		this.textArea.text = ChatManager.Instance.GetCurrentMessages();//聊天框
		this.channelSelect.value = (int)ChatManager.Instance.sendChannel - 1;//纠正最左下角发送的频道显示是否正确
		
		if(ChatManager.Instance.SendChannel == SkillBridge.Message.ChatChannel.Private)
        {
			this.chatTarget.gameObject.SetActive(true);//私聊模式 才显示目标对象
			if (ChatManager.Instance.PrivateID != 0)//先判断有没有 私聊对象的具体ID
			{
				this.chatTarget.text = ChatManager.Instance.PrivateName + ":";
			}
			else
				this.chatTarget.text = "<无>:";
        }
		else
        {
			this.chatTarget.gameObject.SetActive(false);
        }
    }

	public void OnClickChatLink(HyperText text, HyperText.LinkInfo link)//绑定在聊天框上
    {
		if (string.IsNullOrEmpty(link.Name))
			return;

		//<a name="c:1001:name" class="player">Name</a>  c开头为角色
		//<a name="i:1001:name" class="item">Name</a>  i开头为道具  自己设定
		if (link.Name.StartsWith("c:"))//如果点中一个玩家的名字就拆分
        {
			string[] strs = link.Name.Split(":".ToCharArray());
			UIPopCharMenu menu = UIManager.Instance.Show<UIPopCharMenu>();
			menu.targetId = int.Parse(strs[1]);
			menu.targetName = strs[2];
        }
    }

	public void OnClickSend()
	{
		OnEndInput(this.chatText.text);
	}

	public void OnEndInput(string text)//失去焦点时发送--按回车发送并清空
    {
		if (!string.IsNullOrEmpty(text.Trim()))
			this.SendChat(text);

		this.chatText.text = "";
    }

	public void SendChat(string content)//发送私聊：聊天信息
	{
		ChatManager.Instance.SendChat(content, ChatManager.Instance.PrivateID, ChatManager.Instance.PrivateName);
    }

	public void OnSendChannelChanged(int idx)//发送频道 的改变
    {
		if (ChatManager.Instance.sendChannel == (ChatManager.LocalChannel)(idx + 1))//发送频道是少一个频道
			return;

		if (!ChatManager.Instance.SetSendChannel((ChatManager.LocalChannel)idx + 1))//没有队伍或公会会设置失败
        {
			this.channelSelect.value = (int)ChatManager.Instance.sendChannel - 1;
        }
		else
        {
			this.RefreshUI();
        }
    }
}
