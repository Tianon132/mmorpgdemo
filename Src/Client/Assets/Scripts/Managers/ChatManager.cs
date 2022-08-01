using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillBridge.Message;
using System;
using Models;
using System.Text;
using Services;

namespace Managers
{
	public class ChatManager : Singleton<ChatManager>
	{
		public enum LocalChannel
        {
			All = 0,//所有
			Local = 1,//本地
			World = 2,//世界
			Team = 3,//队伍
			Guild = 4,//公会
			Private = 5,//私聊
        }

		//过滤器，要显示的频道
		private ChatChannel[] ChannelFilter = new ChatChannel[6]
		{
			ChatChannel.Local | ChatChannel.World | ChatChannel.Guild | ChatChannel.Team | ChatChannel.Private | ChatChannel.System,
			ChatChannel.Local,
			ChatChannel.World,
			ChatChannel.Team,
			ChatChannel.Guild,
			ChatChannel.Private
		};

		/// <summary>
		/// 开始私聊
		/// </summary>
		/// <param name="targetId"></param>
		/// <param name="targetName"></param>
		public void StartPrivateChat(int targetId, string targetName)
        {
			this.PrivateID = targetId;
			this.PrivateName = targetName;

			this.sendChannel = LocalChannel.Private;
			if (this.OnChat != null)
				this.OnChat();
        }

		public List<ChatMessage>[] Messages = new List<ChatMessage>[6]//聊天信息-6个频道
		{
			new List<ChatMessage>(),
			new List<ChatMessage>(),
			new List<ChatMessage>(),
			new List<ChatMessage>(),
			new List<ChatMessage>(),
			new List<ChatMessage>(),
		};

		public LocalChannel displayChannel;//显示频道

		public LocalChannel sendChannel;//发送频道
		public ChatChannel SendChannel
        {
            get
            {
				switch(sendChannel)
                {
					case LocalChannel.Local: return ChatChannel.Local;
					case LocalChannel.World: return ChatChannel.World;
					case LocalChannel.Team: return ChatChannel.Team;
					case LocalChannel.Guild: return ChatChannel.Guild;
					case LocalChannel.Private: return ChatChannel.Private;
                }
				return ChatChannel.Local;
            }
        }

		public int PrivateID = 0;
		public string PrivateName = "";//私聊对象信息

		public Action OnChat { get; internal set; }

		public void Init()
        {
			foreach(var messages in this.Messages)
            {
				messages.Clear();//清空聊天记录
            }
        }

		public void SendChat(string content, int toId = 0, string toName = "")//发送消息
        {
			ChatService.Instance.SendChat(this.SendChannel, content, toId, toName);
        }

		public bool SetSendChannel(LocalChannel channel)//修改发送的频道
        {
			if(channel == LocalChannel.Team)
            {
				if(User.Instance.TeamInfo == null)//判断队伍
                {
					this.AddSystemMessage("你没有加入任何队伍");
					return false;
                }
            }
			if (channel == LocalChannel.Guild)//判断公会
			{
				if (User.Instance.CurrentCharacter.Guild == null)
				{
					this.AddSystemMessage("你木有加入任何公会");
					return false;

				}
			}
			this.sendChannel = channel;
			Debug.LogFormat("Set Channel :{0}", this.sendChannel);
			return true;
		}

		public void AddMessages(ChatChannel channel, List<ChatMessage> messages)
        {
			for(int ch = 0; ch < 6; ch++)
            {
				if((this.ChannelFilter[ch] & channel) == channel)//2的n次 位运算
                {
					this.Messages[ch].AddRange(messages);
                }
            }
			if (this.OnChat != null)
				this.OnChat();
        }

		/// <summary>
		/// 系统信息【聊天框的提示】--临时用列表保存放在这里
		/// </summary>
		/// <param name="message"></param>
		/// <param name="form"></param>
		public void AddSystemMessage(string message, string form = "")
        {
			this.Messages[(int)LocalChannel.All].Add(new ChatMessage()//默认系统加到综合All上，没有做专门的系统显示
			{
				Channel = ChatChannel.System,
				Message = message,
				FromName = form
			});
			if (this.OnChat != null)
				this.OnChat();
        }

		/// <summary>
		/// 得到当前消息
		/// </summary>
		/// <returns></returns>
		public string GetCurrentMessages()
        {
			StringBuilder sb = new StringBuilder();
			foreach(var message in Messages[(int)displayChannel])
            {
				sb.AppendLine(FormatMessage(message));
            }
			return sb.ToString();
        }

		/// <summary>
		/// 格式化消息
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private string FormatMessage(ChatMessage message)
        {
			switch(message.Channel)
			{
				case ChatChannel.Local:
					return string.Format("[本地]{0}{1}", FormatFromPlayer(message), message.Message);
				case ChatChannel.World:
					return string.Format("<color=cyan>[世界]{0}{1}</color>", FormatFromPlayer(message), message.Message);
				case ChatChannel.System:
					return string.Format("<color=yellow>[系统]{0}</color>", message.Message);
				case ChatChannel.Private:
					return string.Format("<color=magenta>[私聊]{0}{1}</color>", FormatFromPlayer(message), message.Message);
				case ChatChannel.Team:
					return string.Format("<color=green>[队伍]{0}{1}</color>", FormatFromPlayer(message), message.Message);
				case ChatChannel.Guild:
					return string.Format("<color=blue>[公会]{0}{1}</color>", FormatFromPlayer(message), message.Message);
			}
			return "";
        }

		/// <summary>
		/// 格式化玩家信息
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private string FormatFromPlayer(ChatMessage message)
        {
			if (message.FromId == User.Instance.CurrentCharacter.Id)
			{
				return "<a name=\"\" class=\"player\">[我]</a>";
			}
			else
				return string.Format("<a name=\"c:{0}:{1}\" class=\"player\">[{1}]</a>", message.FromId, message.FromName);
        }
	}
}

