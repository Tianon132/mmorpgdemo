using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Entities;
using SkillBridge.Message;
using GameServer.Services;
using Common;
namespace GameServer.Managers
{
    class FriendManager
    {
        Character Owner;

        List<NFriendInfo> friends = new List<NFriendInfo>();

        bool friendChanged = false;

        public FriendManager(Character owner)
        {
            this.Owner = owner;
            this.InitFriends();
        }

        public void GetFriendInfos(List<NFriendInfo> list)//从数据库中获取
        {
            foreach(var f in this.friends)
            {
                list.Add(f);
            }
        }

        public void InitFriends()
        {
            this.friends.Clear();
            foreach(var friend in this.Owner.Data.Friends)
            {
                this.friends.Add(GetFriendInfo(friend));
            }
        }

        public void AddFriend(Character friend)
        {
            TCharacterFriend tf = new TCharacterFriend()
            {
                FriendID = friend.Id,
                FriendName = friend.Data.Name,
                Class = friend.Data.Class,
                Level = friend.Data.Level
            };

            this.Owner.Data.Friends.Add(tf);
            friendChanged = true;//标记：发生改变
        }

        public bool RemoveFriendByFriendId(int friendid)
        {
            var removeItem = this.Owner.Data.Friends.FirstOrDefault(v => v.FriendID == friendid);
            if(removeItem != null)
            {
                DBService.Instance.Entities.TCharacterFriends.Remove(removeItem);
            }
            friendChanged = true;
            return true;
        }

        public bool RemoveFriendByID(int id)
        {
            var removeItem = this.Owner.Data.Friends.FirstOrDefault(v => v.Id == id);
            if(removeItem != null)
            {
                DBService.Instance.Entities.TCharacterFriends.Remove(removeItem);
            }
            friendChanged = true;
            return true;
        }

        /// <summary>
        /// 创建（获取）一个新的朋友信息：用于往friends列表中添加朋友信息时（初始化）使用
        /// </summary>
        /// <param name="friend"></param>
        /// <returns></returns>
        public NFriendInfo GetFriendInfo(TCharacterFriend friend)
        {
            NFriendInfo friendInfo = new NFriendInfo();
            var character = CharacterManager.Instance.GetCharacter(friend.FriendID);
            friendInfo.friendInfo = new NCharacterInfo();
            friendInfo.Id = friend.Id;
            if(character == null)//不在 ，说明不在线，需要读取数据库中的数据T。
            {
                friendInfo.friendInfo.Id = friend.FriendID;
                friendInfo.friendInfo.Name = friend.FriendName;
                friendInfo.friendInfo.Class = (CharacterClass)friend.Class;
                friendInfo.friendInfo.Level = friend.Level;
                friendInfo.Status = 0;
            }
            else//在线，直接从CharacterMananger中拉过来
            {
                friendInfo.friendInfo = character.GetBasicInfo();
                friendInfo.friendInfo.Name = character.Info.Name;
                friendInfo.friendInfo.Class = character.Info.Class;
                friendInfo.friendInfo.Level = character.Info.Level;
                
                if(friend.Level != character.Info.Level)
                {
                    friend.Level = character.Info.Level;
                }
                
                character.FriendManager.UpdateFriendInfo(this.Owner.Info, 1);//character是好友，在好友的列表里把我改成在线状态
                friendInfo.Status = 1;
            }
            Log.InfoFormat("{0}:{1} GetFriendInfo: {2}:{3} Status:{4}", this.Owner.Id, this.Owner.Info.Name, friendInfo.friendInfo.Id, friendInfo.friendInfo.Name, friendInfo.Status);
            return friendInfo;
        }

        /*
        NCharacterInfo GetBasicInfo(NCharacterInfo info)
        {
            return new NCharacterInfo()
            {
                Id = info.Id,
                Name = info.Name,
                Class = info.Class,
                Level = info.Level
            };
        }*/

        /// <summary>
        /// 得到朋友的信息：从现有的列表中获取
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        public NFriendInfo GetFriendInfo(int friendId)
        {
            foreach(var f in friends)
            {
                if(f.friendInfo.Id == friendId)
                {
                    return f;
                }
            }
            return null;
        }

        /// <summary>
        /// 更新朋友信息：改变friends列表中 某个朋友 的 【在线状态】
        /// </summary>
        /// <param name="friendInfo"></param>
        /// <param name="status"></param>
        public void UpdateFriendInfo(NCharacterInfo friendInfo, int status)
        {
            foreach(var f in this.friends)//循环列表中的朋友，改变他的在线状态
            {
                if(f.friendInfo.Id == friendInfo.Id)
                {
                    f.Status = status;
                    break;
                }
            }
            this.friendChanged = true;
        }

        public void OfficeNotify()//下线通知
        {
            foreach(var friendInfo in this.friends)//下线的时候遍历自己的好友，如果在线就去通知自己下线
            {
                var friend = CharacterManager.Instance.GetCharacter(friendInfo.friendInfo.Id);
                if(friend != null)
                {
                    friend.FriendManager.UpdateFriendInfo(this.Owner.Info, 0);//下线通知
                }
            }
        }

        /// <summary>
        /// 更新朋友列表FriendList的后处理器
        /// </summary>
        /// <param name="message"></param>
        public void PostProcess(NetMessageResponse message)
        {
            if(friendChanged)//若为真
            {
                Log.InfoFormat("PostProcess > FriendManager : characterID:{0}:{1}", this.Owner.Id, this.Owner.Info.Name);
                this.InitFriends();//刚开始重新初始化，拉取数据库中的信息
                if(message.friendList == null)
                {
                    message.friendList = new FriendListResponse();
                    message.friendList.Friends.AddRange(this.friends);//把当前好友列表添加一遍
                                                                      //原理是：被动的，在客户端接收消息的时候，会检查friendChanged是否为真，为真就顺便把消息列表刷新的消息给带回去
                                                                      //这里是添加好友时一定会带回去，所以被动得带刷新功能
                }
                friendChanged = false;
            }
        }
    }
}
