using GameServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Message;
using GameServer.Entities;

namespace GameServer.Managers
{
    class MonsterManager
    {
        private Map Map;

        public Dictionary<int, Monster> Monsters = new Dictionary<int, Monster>();

        internal void Init(Map map)
        {
            this.Map = map;
        }

        internal Monster Create(int spawnMonID, int spawnLevel, NVector3 position, NVector3 direction)
        {
            //跟AddCharacter一样在Character类中
            Monster monster = new Monster(spawnMonID, spawnLevel, position, direction);
            EntityManager.Instance.AddEntity(this.Map.ID, monster);

            monster.Id = monster.entityId;//怪物没有db的Id，所以直接用entity的Id
            monster.Info.Id = monster.entityId;
            monster.Info.mapId = this.Map.ID;
            Monsters[monster.Id] = monster;

            this.Map.MonsterEnter(monster);//需要让地图通知玩家 怪物进来了
            return monster;
        }
    }
}
