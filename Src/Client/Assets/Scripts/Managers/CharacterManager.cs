using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Entities;
using SkillBridge.Message;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    class CharacterManager : Singleton<CharacterManager>, IDisposable
    {
        public Dictionary<int, Character> Characters = new Dictionary<int, Character>();

        public UnityAction<Character> OnCharacterEnter;
        public UnityAction<Character> OnCharacterLeave;

        public CharacterManager()
        {
        }

        public void Dispose()
        {

        }

        public void Init()
        {

        }

        //
        public void Clear()
        {
            int[] keys = this.Characters.Keys.ToArray();
            foreach(var key in keys)
            {
                this.RemoveCharacter(key);
            }
            //先从实体中清除再清除角色列表

            this.Characters.Clear();
        }

        //加入角色到角色管理器中
        public void AddCharacter(NCharacterInfo cha)
        {
            Debug.LogFormat("AddCharacter:{0} : {1} Map:{2} Entity:{3}", cha.Id, cha.Name, cha.mapId, cha.Entity.String());
            Character character = new Character(cha);
            this.Characters[character.EntityId] = character;
            EntityManager.Instance.AddEntity(character);//entity添加

            if(OnCharacterEnter!=null)//GameObjectManager中的事件   只有在不为空的时候采用此函数，否则直接初始OnStart化生成角色，不用专门生成
            {
                OnCharacterEnter(character);
            }
        }

        //从角色管理器中移除角色
        public void RemoveCharacter(int entityId)
        {
            Debug.LogFormat("RemoveCharacter:{0}", entityId);
            //this.Characters.Remove(characterId);

            //专门删除Entity部分
            if(this.Characters.ContainsKey(entityId))
            {
                EntityManager.Instance.RemoveEntity(this.Characters[entityId].Info.Entity);//entity删除
                //Debug.LogFormat("EntityId:{0}", this.Characters[characterId].Info.Entity.Id);

                //通知事件的接收者要离开了，也就是GameObjectManager删除
                if (OnCharacterLeave != null)
                    OnCharacterLeave(this.Characters[entityId]);

                this.Characters.Remove(entityId);
            }
        }

        public Character GetCharacter(int Id)
        {
            Character character;
            this.Characters.TryGetValue(Id, out character);
            return character;
        }
    }
}
