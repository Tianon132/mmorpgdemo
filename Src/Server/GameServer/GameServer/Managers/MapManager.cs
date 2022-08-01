using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using GameServer.Models;
using GameServer.Managers;

namespace GameServer.Managers
{
    class MapManager : Singleton<MapManager>
    {
        Dictionary<int, Map> Maps = new Dictionary<int, Map>();

        public void Init()
        {
            foreach (var mapdefine in DataManager.Instance.Maps.Values)//直接遍历Data中数据
            {
                Map map = new Map(mapdefine);//构建地图，直接传人
                Log.InfoFormat("MapManager.Init > Map:{0}{1}", map.Define.ID, map.Define.Name);
                this.Maps[mapdefine.ID] = map;
            }
        }

        //this索引器，特殊的写法，类似于重载一个操作符一样
        //为了方便调用此类下的Map而编写的
        public Map this[int key]
        {
            get
            {
                return this.Maps[key];
            }
        }

        //将来在地图中有逻辑，地图会更新的
        //一般来说，管理器不存在自主服务，不过在整个项目中几乎只有MapManager存在自主服务（使用Update更新），其他的管理器更多的是请求响应式的被动服务
        public void Update()
        {
            foreach (var map in this.Maps.Values)
            {
                map.Update();
            }
        }
    }
}
