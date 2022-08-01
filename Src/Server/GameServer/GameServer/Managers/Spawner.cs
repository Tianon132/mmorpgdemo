using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Data;
using GameServer.Models;

namespace GameServer.Managers
{
    class Spawner
    {
        private Map Map;

        /// <summary>
        /// 刷新时间
        /// </summary>
        private float spawnTime = 0;

        /// <summary>
        /// 消失时间
        /// </summary>
        private float unspawnTime = 0;

        private bool spawned = false;//有没有刷过怪，没刷过就快刷

        private SpawnPointDefine spawnPoint = null;//刷怪点 构造中通过data来读取
        public SpawnRuleDefine Define { get; set; }//刷怪规则 spawnManager通过构造函数传进来

        public Spawner(SpawnRuleDefine define, Map map)//构造：加载刷怪点
        {
            this.Define = define;//保存从spawnManager 传进来的规则
            this.Map = map;

            if(DataManager.Instance.SpawnPoints.ContainsKey(this.Map.ID))
            {
                if(DataManager.Instance.SpawnPoints[this.Map.ID].ContainsKey(this.Define.SpawnPoint))
                {
                    spawnPoint = DataManager.Instance.SpawnPoints[this.Map.ID][this.Define.SpawnPoint];
                }
                else
                {
                    Log.InfoFormat("SpawnRule:[{0}] SpawnPoint[{1}] not existed", this.Define.ID, this.Define.SpawnPoint);
                }
            }
        }

        public void Update()//每帧刷新一次
        {
            if(this.CanSpawn())
            {
                this.Spawn();
            }
        }

        bool CanSpawn()
        {
            if (this.spawned)//刷过了吗
                return false;
            if (this.unspawnTime + this.Define.SpawnPeriod > Time.time)//+刷怪周期
                return false;

            return true;
        }

        public void Spawn()
        {
            this.spawned = true;
            Log.InfoFormat("Map:[{0}] Spawn[{1}: Mon:{2},Lv:{3}] At Point:{4}", this.Define.MapID, this.Define.ID, this.Define.SpawnMonID, this.Define.SpawnLevel, this.Define.SpawnPoint);
            this.Map.MonsterManager.Create(this.Define.SpawnMonID, this.Define.SpawnLevel, this.spawnPoint.Position, this.spawnPoint.Direction);
        }
    }
}
