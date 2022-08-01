using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SkillBridge.Message;
using GameServer.Entities;
using Common;

namespace GameServer.Managers
{
    class CharacterManager : Singleton<CharacterManager>
    {
        public Dictionary<int, Character> Characters = new Dictionary<int, Character>();//角色信息
        //使用字典的好处就是省去遍历，直接可以找到
        //缺点是：遍历效率比较低【后期再考虑优化，换成遍历的方式】

        public CharacterManager()//构造函数
        {
        }

        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Clear()
        {
            /*
            int[] keys = this.Characters.Keys.ToArray();
            foreach(var key in keys)
            {
                this.RemoveCharacter(key);
            }*/
            this.Characters.Clear();
        }

        public Character AddCharacter(TCharacter cha)//TChar是DB角色，只在服务端存在
        {
            Character character = new Character(CharacterType.Player, cha);//根据读出来的数据创建实体对象
            EntityManager.Instance.AddEntity(cha.MapID, character);//除了角色管理中删除，还要在实体中删除

            character.Info.EntityId = character.entityId;//让NCharacterInfo中的Id与NEntity的id中统一

            this.Characters[character.Id] = character;
            return character;
        }

        public void RemoveCharacter(int characterId)
        {
            if(this.Characters.ContainsKey(characterId))
            {
                var cha = this.Characters[characterId];
                EntityManager.Instance.RemoveEntity(cha.Data.MapID, cha);
                this.Characters.Remove(characterId);
            }
        }

        //管理器三大部分：清理，添加，移除

        public Character GetCharacter(int characterId)
        {
            Character character = null;
            this.Characters.TryGetValue(characterId, out character);
            return character;
        }
    }
}
