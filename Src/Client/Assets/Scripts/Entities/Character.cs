using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using SkillBridge.Message;
using Common.Data;
using Models;

namespace Entities
{
    public class Character : Entity
    {
        public NCharacterInfo Info;

        public CharacterDefine Define;//数据表中的定义

        public int Id
        {
            get { return this.Info.Id; }//方便使用db 的 Id
        }

        public string Name
        {
            get
            {
                if (this.Info.Type == CharacterType.Player)
                    return this.Info.Name;
                else
                    return this.Define.Name;
            }
        }

        public bool IsPlayer
        {
            //get { return this.Info.Id == User.Instance.CurrentCharacter.Id; }
            get
            {
                return this.Info.Type == CharacterType.Player;//因为不管是自己还是其他玩家都是Player
            }
        }

        public bool isCurrentPlayer//是否是当前玩家，自己玩
        {
            get
            {
                if (!IsPlayer) return false;
                return this.Info.Id == Models.User.Instance.CurrentCharacter.Id;
            }
        }

        public Character(NCharacterInfo info) : base(info.Entity)
        {
            this.Info = info;
            this.Define = DataManager.Instance.Characters[info.ConfigId];
        }

        public void MoveForward()
        {
            Debug.LogFormat("MoveForward");
            this.speed = this.Define.Speed;
        }

        public void MoveBack()
        {
            Debug.LogFormat("MoveBack");
            this.speed = -this.Define.Speed;
        }

        public void Stop()
        {
            Debug.LogFormat("Stop");
            this.speed = 0; 
        }

        public void SetDirection(Vector3Int direction)
        {
            Debug.LogFormat("SetDirection:{0}", direction);
            this.direction = direction;
        }

        public void SetPosition(Vector3Int position)
        {
            Debug.LogFormat("SetPosition:{0}", position);
            this.position = position;
        }
    }
}
