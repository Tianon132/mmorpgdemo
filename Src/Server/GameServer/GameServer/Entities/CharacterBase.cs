using Common.Data;
using GameServer.Core;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Managers;

namespace GameServer.Entities
{
    class CharacterBase : Entity
    {

        public int Id { get; set; }
        public string Name { get { return this.Info.Name; } }

        public NCharacterInfo Info;
        public CharacterDefine Define;

        public CharacterBase(Vector3Int pos, Vector3Int dir) : base(pos, dir)
        {

        }

        public CharacterBase(CharacterType type, int configId, int level, Vector3Int pos, Vector3Int dir) :
           base(pos, dir)
        {
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.ConfigId = configId;//配置Id【tid】
            this.Info.Entity = this.EntityData;
            this.Info.EntityId = this.entityId;//实体Id

            this.Define = DataManager.Instance.Characters[this.Info.ConfigId];//职业配置信息
            this.Info.Name = this.Define.Name;
        }

    }
}
