using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

using System.Threading;

using Network;
using GameServer.Services;
using GameServer.Managers;

namespace GameServer
{
    class GameServer
    {
        Thread thread;
        bool running = false;
        NetService network;

        public bool Init()
        {
            network = new NetService();
            network.Init(8000);

            //Service
            UserService.Instance.Init();//服务器启动，User是被动服务，故不用在Start函数中调用user的Start方法
            DBService.Instance.Init();

            //Managers
            DataManager.Instance.Load();
            //MapManager.Instance.Init();
            MapService.Instance.Init();

            ItemService.Instance.Init();

            //任务系统
            QuestService.Instance.Init();
            //好友系统
            FriendService.Instance.Init();
            //队伍系统
            TeamService.Instance.Init();
            //公会系统
            GuildService.Instance.Init();
            //聊天系统
            ChatService.Instance.Init();

            thread = new Thread(new ThreadStart(this.Update));
            return true;
        }

        public void Start()
        {
            network.Start();

            running = true;
            thread.Start();
        }


        public void Stop()
        {
            running = false;
            thread.Join();
        }

        public void Update()//多线程调用
        {
            var mapManager = MapManager.Instance;
            while (running)
            {
                Time.Tick();
                Thread.Sleep(100);//100毫秒跑1帧，1秒跑10帧（比较低的帧率来进行的）
                //Console.WriteLine("{0} {1} {2} {3} {4}", Time.deltaTime, Time.frameCount, Time.ticks, Time.time, Time.realtimeSinceStartup);
                
                mapManager.Update();//地图管理器会调用所有的地图进行Update
            }
        }
    }
}
