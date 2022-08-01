using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Network;
using SkillBridge.Message;
using Common;
using GameServer.Entities;
using GameServer.Managers;

using Common.Data;

namespace GameServer.Services
{
    class MapService : Singleton<MapService>
    {
        public MapService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapEntitySyncRequest>(this.OnMapEntitySync);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapTeleportRequest>(this.OnMapTeleport);
        }

        public void Init()
        {
            MapManager.Instance.Init();
        }

        //给角色发布同步消息
        public void SendEntityUpdate(NetConnection<NetSession> conn, NEntitySync entity)
        {
            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            conn.Session.Response.mapEntitySync = new MapEntitySyncResponse();
            conn.Session.Response.mapEntitySync.entitySyncs.Add(entity);

            //byte[] data = PackageHandler.PackMessage(message);
            //conn.SendData(data, 0, data.Length);
            conn.SendResponse();
        }

        private void OnMapEntitySync(NetConnection<NetSession> sender, MapEntitySyncRequest request)
        {
            //地图知道地图上有多少人
            Character character = sender.Session.Character;
            Log.InfoFormat("OnMapEntitySync: characterID:{0}:{1} Entity.Id:{2} Evt:{3} Entity:{4}", character.Id, character.Info.Name, request.entitySync.Id, request.entitySync.Event, request.entitySync.Entity.String());

            MapManager.Instance[character.Info.mapId].UpdateEntity(request.entitySync);

        }

        //传送点响应
        void OnMapTeleport(NetConnection<NetSession> sender, MapTeleportRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnMapTeleport: characterID:{0}:{1} TeleporterId:{2}", character.Id, character.Data, request.teleporterId);

            if(!DataManager.Instance.Teleporters.ContainsKey(request.teleporterId))//检测定义文件中是否含有该源传送点
            {
                Log.WarningFormat("Source TeleporterID[{0}] not exist", request.teleporterId);
                return;
            }
            TeleporterDefine source = DataManager.Instance.Teleporters[request.teleporterId];

            if(source.LinkTo == 0 || DataManager.Instance.Teleporters.ContainsKey(source.LinkTo))//检测是否存在目标传送点
            {
                Log.WarningFormat("Sourece TeleporterID[{0}] LinkTo ID[{2}] not exist", request.teleporterId, source.LinkTo);
            }
            TeleporterDefine target = DataManager.Instance.Teleporters[source.LinkTo];

            MapManager.Instance[source.MapID].CharacterLeave(character);
            character.Position = target.Position;
            character.Direction = target.Direction;
            MapManager.Instance[target.MapID].CharacterEnter(sender, character);
        }
    }
}
