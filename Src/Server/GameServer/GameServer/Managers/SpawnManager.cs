using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;
using GameServer.Models;

namespace GameServer.Managers
{
    /// <summary>
    /// 刷怪管理器：管理的是刷怪规则
    /// </summary>
    class SpawnManager
    {
        public List<Spawner> Rules = new List<Spawner>();

        private Map Map;

        /// <summary>
        /// Init初始化得到的是 本地data文件中的刷怪规则
        /// </summary>
        /// <param name="map"></param>
        public void Init(Map map)
        {
            this.Map = map;
            if (DataManager.Instance.SpawnRules.ContainsKey(map.Define.ID))//读取刷怪规则表
            {
                foreach(var define in DataManager.Instance.SpawnRules[map.Define.ID].Values)
                {
                    this.Rules.Add(new Spawner(define, this.Map));
                }
            }
        }

        public void Update()
        {
            if (Rules.Count == 0)
                return;

            for(int i=0; i<this.Rules.Count; i++)
            {
                this.Rules[i].Update();//调用每条规则的Update
            }
        }


    }
}
