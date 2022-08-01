using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameServer;
using GameServer.Entities;
using SkillBridge.Message;
using GameServer.Services;

namespace Network
{
    class NetSession : INetSession
    {
        public TUser User { get; set; }
        public Character Character { get; set; }
        public NEntity Entity { get; set; }

        public IPostResponser PostResponser { get; set; }//好友那里：响应后处理器

        internal void Disconnected()
        {
            this.PostResponser = null;
            if (this.Character != null)
                UserService.Instance.CharacterLeave(this.Character);
        }

        NetMessage response;//私有的

        public NetMessageResponse Response//共有的方法
        {
            get
            {
                if(response == null)
                {
                    response = new NetMessage();//没有new一个
                }
                if (response.Response == null)
                    response.Response = new NetMessageResponse();//没有再new一个，这两个是不变的
                return response.Response;
            }
        }

        public byte[] GetResponse()
        {
            if(response != null)
            {
                /*
                if(this.Character != null && this.Character.StatusManager.HasStatus)//第一次登陆角色为空并且判断HasStatus有没有变化，进入游戏才不为空
                {
                    this.Character.StatusManager.ApplyResponse(Response);
                }*/
                if (PostResponser != null)
                    this.PostResponser.PostProcess(Response);//后处理器：主要给角色用

                byte[] data = PackageHandler.PackMessage(response);
                response = null;//设为空，保证会话结束（发送完信息后）就清空
                return data;
            }
            return null;
        }
    }
}
