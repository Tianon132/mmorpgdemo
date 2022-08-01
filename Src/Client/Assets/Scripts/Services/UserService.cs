using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;
using Network;
using UnityEngine;
using Models;
using SkillBridge.Message;
using Managers;

namespace Services
{
    class UserService : Singleton<UserService>, IDisposable
    {
        public UnityEngine.Events.UnityAction<Result, string> OnRegister;
        public UnityEngine.Events.UnityAction<Result, string> OnLogin;
        public UnityEngine.Events.UnityAction<Result, string> OnCharacterCreate;
        NetMessage pendingMessage = null;
        bool connected = false;

        bool isQuitGame = false;//代表是否退出游戏

        //构造，构造对象（实例）的时候自动生成
        public UserService()
        {
            NetClient.Instance.OnConnect += OnGameServerConnect;
            NetClient.Instance.OnDisconnect += OnGameServerDisconnect;
            MessageDistributer.Instance.Subscribe<UserLoginResponse>(this.OnUserLogin);//登录
            MessageDistributer.Instance.Subscribe<UserRegisterResponse>(this.OnUserRegister);//注册
            MessageDistributer.Instance.Subscribe<UserCreateCharacterResponse>(this.OnUserCreateCharacter);//创建角色

            MessageDistributer.Instance.Subscribe<UserGameEnterResponse>(this.OnGameEnter);//进入游戏
            MessageDistributer.Instance.Subscribe<UserGameLeaveResponse>(this.OnGameLeave);//退出游戏
            //MessageDistributer.Instance.Subscribe<MapCharacterEnterResponse>(this.OnCharacterEnter);//角色进入地图
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<UserLoginResponse>(this.OnUserLogin);//登录
            MessageDistributer.Instance.Unsubscribe<UserRegisterResponse>(this.OnUserRegister);//注册
            MessageDistributer.Instance.Unsubscribe<UserCreateCharacterResponse>(this.OnUserCreateCharacter);//创建角色

            MessageDistributer.Instance.Unsubscribe<UserGameEnterResponse>(this.OnGameEnter);//进入游戏
            MessageDistributer.Instance.Unsubscribe<UserGameLeaveResponse>(this.OnGameLeave);
            //MessageDistributer.Instance.Unsubscribe<MapCharacterEnterResponse>(this.OnCharacterEnter);
            NetClient.Instance.OnConnect -= OnGameServerConnect;
            NetClient.Instance.OnDisconnect -= OnGameServerDisconnect;
        }

        public void Init()
        {

        }

        //连接服务器，第一步
        public void ConnectToServer()
        {
            Debug.Log("ConnectToServer() Start ");
            //NetClient.Instance.CryptKey = this.SessionId;

            NetClient.Instance.Init("127.0.0.1", 8000);//地址先直接写死
            NetClient.Instance.Connect();
        }

        void OnGameServerConnect(int result, string reason)
        {
            Log.InfoFormat("LoadingMesager::OnGameServerConnect :{0} reason:{1}", result, reason);
            if (NetClient.Instance.Connected)
            {
                this.connected = true;
                if(this.pendingMessage!=null)//判断连接前有没有补发的消息
                {
                    NetClient.Instance.SendMessage(this.pendingMessage);
                    this.pendingMessage = null;
                }
            }
            else
            {
                if (!this.DisconnectNotify(result, reason))
                {
                    MessageBox.Show(string.Format("网络错误，无法连接到服务器！\n RESULT:{0} ERROR:{1}", result, reason), "错误", MessageBoxType.Error);
                }
            }
        }

        public void OnGameServerDisconnect(int result, string reason)
        {
            this.DisconnectNotify(result, reason);
            return;
        }

        bool DisconnectNotify(int result,string reason)
        {
            if (this.pendingMessage != null)
            {
                if (this.pendingMessage.Request.userLogin!=null)
                {
                    if (this.OnLogin != null)
                    {
                        this.OnLogin(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
				 else if(this.pendingMessage.Request.userRegister!=null)
                {
                    if (this.OnRegister != null)
                    {
                        this.OnRegister(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
				}
				else
                {
                    if (this.OnCharacterCreate != null)
                    {
                        this.OnCharacterCreate(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
                return true;
            }
            return false;
        }

        //注册
        public void SendRegister(string user, string psw)
        {
            Debug.LogFormat("UserRegisterRequest::user :{0} psw:{1}", user, psw);
            NetMessage message = new NetMessage();//消息
            message.Request = new NetMessageRequest();//消息请求
            message.Request.userRegister = new UserRegisterRequest();//消息请求注册
            message.Request.userRegister.User = user;
            message.Request.userRegister.Passward = psw;

            //判断连接有没有连上，连上就直接发送，没连上就连接
            if (this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }

        void OnUserRegister(object sender, UserRegisterResponse response)
        {
            Debug.LogFormat("OnUserRegister:{0} [{1}]", response.Result, response.Errormsg);

            if (this.OnRegister != null)
            {
                this.OnRegister(response.Result, response.Errormsg);
            }
        }

        //登录
        public void SendLogin(string user, string psw)
        {
            Debug.LogFormat("UserLoginrRequest::user :{0} psw:{1}", user, psw);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.userLogin = new UserLoginRequest();
            message.Request.userLogin.User = user;
            message.Request.userLogin.Passward = psw;

            if (this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }

        void OnUserLogin(object sender, UserLoginResponse response)
        {
            Debug.LogFormat("OnUserLogin:{0} [{1}]", response.Result, response.Errormsg);

            if (response.Result == Result.Success)
            {
                //Model层
                Models.User.Instance.SetupUserInfo(response.Userinfo);//登陆成功逻辑，将服务器的消息保存到本地
            }
            if (this.OnLogin != null)
            {
                this.OnLogin(response.Result, response.Errormsg);
            }
        }

        //角色创建
        public void SendCharacterCreate(string charName, CharacterClass cls)//角色名和职业
        {
            Debug.LogFormat("UserCreateCharacterRequest:Name:{0} Class:{1}", charName, cls);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.createChar = new UserCreateCharacterRequest();
            message.Request.createChar.Name = charName;
            message.Request.createChar.Class = cls;

            if(this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }

        void OnUserCreateCharacter(object sender, UserCreateCharacterResponse response)
        {
            Debug.LogFormat("OnUserCreateCharacter:{0}[{1}]", response.Result, response.Errormsg);

            if(response.Result == Result.Success)
            {
                User.Instance.Info.Player.Characters.Clear();
                User.Instance.Info.Player.Characters.AddRange(response.Characters);
            }

            if(this.OnCharacterCreate != null)
            {
                this.OnCharacterCreate(response.Result, response.Errormsg);
            }
        }

        //进入游戏
        public void SendGameEnter(int characterIdx)
        {
            Debug.LogFormat("UserGameEnterRequest::characterId:{0}", characterIdx);

            ChatManager.Instance.Init();//进入游戏前初始化 29-聊天系统

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.gameEnter = new UserGameEnterRequest();//发送进入游戏的请求

            message.Request.gameEnter.characterIdx = characterIdx;
            NetClient.Instance.SendMessage(message);
        }                                                                               

        //响应：真正的进入游戏
        void OnGameEnter(object sender, UserGameEnterResponse response)
        {
            Debug.LogFormat("OnGameEnter:{0}[{1}]", response.Result, response.Errormsg);
            if(response.Result == Result.Success)
            {
                if(response.Character != null)
                {
                    //任务系统添加1：提前到这里，一旦进入游戏，角色赋值
                    User.Instance.CurrentCharacter = response.Character;

                    ItemManager.Instance.Init(response.Character.Items);//道具系统，初始化，从网络info中获得
                    BagManager.Instance.Init(response.Character.Bag);//初始化背包
                    EquipManager.Instance.Init(response.Character.Equips);//装备

                    //任务系统添加2
                    QuestManager.Instance.Init(response.Character.Quests);

                    //好友系统添加3
                    FriendManager.Instance.Init(response.Character.Friends);

                    //公会系统
                    GuildManager.Instance.Init(response.Character.Guild);
                }
            }
        }

        /*
        //角色进入游戏——是从服务端的MapManager发送过来的信息（上面的游戏进入函数就不进行操作了）
        void OnCharacterEnter(object sender, MapCharacterEnterResponse message)
        {
            Log.InfoFormat("OnMapCharacterEnter:{0}", message.mapId);
            NCharacterInfo info = message.Characters[0];
            User.Instance.CurrentCharacter = info;
            SceneManager.Instance.LoadScene(DataManager.Instance.Maps[message.mapId].Resource);
        }*/

        //离开游戏
        public void SendGameLeave(bool isQuitGame = false)
        {
            this.isQuitGame = isQuitGame;
            Debug.Log("UserGameLeaveReqeust");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.gameLeave = new UserGameLeaveRequest();
            NetClient.Instance.SendMessage(message);
        }
        
        //离开游戏相应：进入的时候是把角色加到角色管理器中，那么退出就是拿出来
        void OnGameLeave(object sender, UserGameLeaveResponse response)
        {
            MapService.Instance.CurrentMapId = 0;//退出地图时，重置Id，防止MapService切换地图判断地图Id相同，导致切换失败
            User.Instance.CurrentCharacter = null;
            Debug.LogFormat("OnGameLeave:{0} [{1}]", response.Result, response.Errormsg);

            if(this.isQuitGame)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else           
                Application.Quit();
#endif
            }
        }

    }
}
