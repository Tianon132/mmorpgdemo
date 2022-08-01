using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;

namespace GameServer.Services
{
    class UserService : Singleton<UserService>
    {

        public UserService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserCreateCharacterRequest>(this.OnCreateCharacter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameEnterRequest>(this.OnGameEnter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameLeaveRequest>(this.OnGameLeave);
        }

        public void Init()
        {

        }

        void OnRegister(NetConnection<NetSession> sender, UserRegisterRequest request)
        {
            Log.InfoFormat("UserRegisterRequest: User:{0}  Pass:{1}", request.User, request.Passward);

            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            //message.Response.userRegister = new UserRegisterResponse();
            sender.Session.Response.userRegister = new UserRegisterResponse();

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user != null)
            {
                sender.Session.Response.userRegister.Result = Result.Failed;
                sender.Session.Response.userRegister.Errormsg = "用户已存在.";
            }
            else
            {
                TPlayer player = DBService.Instance.Entities.Players.Add(new TPlayer());
                DBService.Instance.Entities.Users.Add(new TUser() { Username = request.User, Password = request.Passward, Player = player });
                DBService.Instance.Entities.SaveChanges();
                sender.Session.Response.userRegister.Result = Result.Success;
                sender.Session.Response.userRegister.Errormsg = "注册成功";
            }

            //byte[] data = PackageHandler.PackMessage(message);
            //sender.SendData(data, 0, data.Length);
            sender.SendResponse();
        }

        void OnLogin(NetConnection<NetSession> sender, UserLoginRequest request)
        {   
            Log.InfoFormat("UserLoginRequest: User:{0}  Pass:{1}", request.User, request.Passward);

            // NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            //message.Response.userLogin = new UserLoginResponse();
            sender.Session.Response.userLogin = new UserLoginResponse();

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user == null)
            {
                sender.Session.Response.userLogin.Result = Result.Failed;
                sender.Session.Response.userLogin.Errormsg = "用户不存在";
            }
            else if (user.Password != request.Passward)//判断密码对不对
            {
                sender.Session.Response.userLogin.Result = Result.Failed;
                sender.Session.Response.userLogin.Errormsg = "密码错误";
            }
            else
            {
                //正式登录
                sender.Session.User = user;

                sender.Session.Response.userLogin.Result = Result.Success;
                sender.Session.Response.userLogin.Errormsg = "登陆成功";
                sender.Session.Response.userLogin.Userinfo = new NUserInfo();
                sender.Session.Response.userLogin.Userinfo.Id = (int)user.ID ;
                sender.Session.Response.userLogin.Userinfo.Player = new NPlayerInfo();
                sender.Session.Response.userLogin.Userinfo.Player.Id = user.Player.ID;

                foreach (var c in user.Player.Characters)//已经有的角色信息填充到协议内
                {
                    NCharacterInfo info = new NCharacterInfo();
                    info.Id = c.ID;
                    info.Name = c.Name;
                    info.Type = CharacterType.Player;//角色有玩家，怪物，NPC
                    info.Class = (CharacterClass)c.Class;
                    info.ConfigId = c.TID;
                    sender.Session.Response.userLogin.Userinfo.Player.Characters.Add(info);
                }
            }
            //byte[] data = PackageHandler.PackMessage(message);
            //sender.SendData(data, 0, data.Length);
            sender.SendResponse();
        }

        void OnCreateCharacter(NetConnection<NetSession> sender, UserCreateCharacterRequest request)
        {
            Log.InfoFormat("UserRegisterRequest: Name:{0}  Class:{1}", request.Name, request.Class);

            TCharacter character = new TCharacter()
            {
                Name = request.Name,
                Class = (int)request.Class,
                TID = (int)request.Class,
                Level = 1,
                MapID = 1,
                MapPosX = 5000,
                MapPosY = 4000,
                MapPosZ = 820,
                Gold = 100000,//初始给了10万金币
                Equips = new byte[28],
            };

            
            //背包系统测试//
            //背包系统测试//
            var bag = new TCharacterBag();
            bag.Owner = character;
            bag.Items = new byte[0];//背包没数据，但又不能为0
            bag.Unlocked = 20;//预先解锁20个格子
            character.Bag = DBService.Instance.Entities.CharacterBags.Add(bag);//添加背包
            //背包系统测试//
            //背包系统测试//

            character = DBService.Instance.Entities.Characters.Add(character);

            //22装备系统设计//
            //新手大礼包//
            //新手大礼包//
            character.Items.Add(new TCharacterItem()
            {
                Owner = character,
                ItemID = 1,
                ItemCount = 20,
            });
            character.Items.Add(new TCharacterItem()
            {
                Owner = character,
                ItemID = 2,
                ItemCount = 20,
            });
            //新手大礼包//
            //新手大礼包//

            sender.Session.User.Player.Characters.Add(character);//session会话，每一个用户登录服务器就有一个会话
            DBService.Instance.Entities.SaveChanges();//DB保存

            /*
            //回发消息，结果给用户
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.createChar = new UserCreateCharacterResponse();*/
            sender.Session.Response.createChar = new UserCreateCharacterResponse();

            //结果
            sender.Session.Response.createChar.Result = Result.Success;
            sender.Session.Response.createChar.Errormsg = "None";

            foreach(var c in sender.Session.User.Player.Characters)//后加的，为选择界面的UI列表刷新，返回信息
            {
                NCharacterInfo info = new NCharacterInfo();
                info.Id = c.ID;
                info.Name = c.Name;
                info.Type = CharacterType.Player;//角色有玩家，怪物，NPC
                info.Class = (CharacterClass)c.Class;
                info.ConfigId = c.TID;
                sender.Session.Response.createChar.Characters.Add(info);
            }

            //byte[] data = PackageHandler.PackMessage(message);
            //sender.SendData(data, 0, data.Length);
            sender.SendResponse();
        }
        //能看到消息吗
        //进入游戏
        void OnGameEnter(NetConnection<NetSession> sender, UserGameEnterRequest request)
        {
            TCharacter dbchar = sender.Session.User.Player.Characters.ElementAt(request.characterIdx);//数据库中获取数据
            Log.InfoFormat("UserGameEnterRequest: CharacterID:{0}:{1} Map:{2}", dbchar.ID, dbchar.Name, dbchar.MapID);

            Character character = CharacterManager.Instance.AddCharacter(dbchar);//角色管理器管理进入的角色   把角色存入角色管理器
            SessionManager.Instance.AddSession(character.Id, sender);
            
            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            sender.Session.Response.gameEnter = new UserGameEnterResponse();
            sender.Session.Response.gameEnter.Result = Result.Success;
            sender.Session.Response.gameEnter.Errormsg = "None";

            sender.Session.Character = character;//可得知是session的character得知是哪个角色发来的信息
            sender.Session.PostResponser = character;//后处理器是由角色进行的  早这里赋值

            //进入成功，发送初始角色信息
            sender.Session.Response.gameEnter.Character = character.Info;//在道具系统即添加ItemManager之后，
                                                                         //gameEter协议中添加了Character这一信息，为了添加其中的Item信息

            /*
            //道具系统测试//
            //道具系统测试//背包测试
            int itemId = 1;
            bool hasItem = character.ItemManager.HasItem(itemId);
            Log.InfoFormat("HasItem:[{0}]{1}", itemId, hasItem);
            if(hasItem)
            {
                //character.ItemManager.RemoveItem(itemId, 1);//测试：如果有删一个
            }
            else
            {
                character.ItemManager.AddItem(1, 200);
                character.ItemManager.AddItem(2, 100);
                character.ItemManager.AddItem(3, 30);
                character.ItemManager.AddItem(4, 120);//容量上限是99，故用来测试背包
            }
            Models.Item item = character.ItemManager.GetItem(itemId);

            Log.InfoFormat("Item:[{0}]{1}", itemId, item);
            //道具系统测试//
            //道具系统测试//
            */

            //byte[] data = PackageHandler.PackMessage(message);
            //sender.SendData(data, 0, data.Length);//发送客户端
            sender.SendResponse();

            MapManager.Instance[dbchar.MapID].CharacterEnter(sender, character);//进入地图
        }

        //退出游戏
        void OnGameLeave(NetConnection<NetSession> sender, UserGameLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("UserGameLeaveRequest: characterID:{0}:{1} Map:{2}", character.Id, character.Info.Name, character.Info.mapId);

            //SessionManager.Instance.RemoveSession(character.Id);  //因为推出的方式变多，所以移到下面统一退出
            CharacterLeave(character);//退出角色管理器和地图管理器

            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            sender.Session.Response.gameLeave = new UserGameLeaveResponse();
            sender.Session.Response.gameLeave.Result = Result.Success;
            sender.Session.Response.gameLeave.Errormsg = "None";

            //byte[] data = PackageHandler.PackMessage(message);
            //sender.SendData(data, 0, data.Length);
            sender.SendResponse();
        }

        public void CharacterLeave(Character character)
        {
            Log.InfoFormat("CharacterLeave: characterID:{0}:{1}", character.Id, character.Info.Name);
            SessionManager.Instance.RemoveSession(character.Id);
            CharacterManager.Instance.RemoveCharacter(character.Id);//进入的时候是把角色加到角色管理器中，那么退出就是拿出来
            
            character.Clear();//更新 好友 的 在线 状态问题(这个在退出地图之前，以便在退出地图时将在线状态更改的消息给携带出去)
            MapManager.Instance[character.Info.mapId].CharacterLeave(character);//退出地图
        }
    }
}
