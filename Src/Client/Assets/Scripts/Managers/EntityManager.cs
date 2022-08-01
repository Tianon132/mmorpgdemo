using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Entities;
using SkillBridge.Message;

namespace Managers
{
    interface IEntityNotify//删除的时候自动通知
    {
        void OnEntityRemoved();
        void OnEntityChanged(Entity entity);
        void OnEntityEvent(EntityEvent @event, int param);
    }

    class EntityManager : Singleton<EntityManager>
    {
        Dictionary<int, Entity> entities = new Dictionary<int, Entity>();

        Dictionary<int, IEntityNotify> notifiers = new Dictionary<int, IEntityNotify>();
                    //使用了接口，因为可以将来一个接受者可以接受多个事件
                    //而委托得写好多个，所以接口是最佳的，（设计模式）

        public void RegisterEntityChangeNotify(int entityId, IEntityNotify notify)
        {
            this.notifiers[entityId] = notify;
        }

        public void AddEntity(Entity entity)
        {
            entities[entity.EntityId] = entity;
        }

        public void RemoveEntity(NEntity entity)
        {
            this.entities.Remove(entity.Id);
            if(notifiers.ContainsKey(entity.Id))
            {
                notifiers[entity.Id].OnEntityRemoved();
                notifiers.Remove(entity.Id);
            }
        }

        public void OnEntitySync(NEntitySync data)
        {
            //第一，知道是谁；第二，数据更新掉
            Entity entity = null;
            entities.TryGetValue(data.Id, out entity);//查找key值存不存在，查完了还要改value，性能消耗多一半，所以这个函数很方便，建议使用！

            if(entity != null)
            {
                if (data.Entity != null)
                    entity.EntityData = data.Entity;//消息里的Entity给本地数据更新
                if(notifiers.ContainsKey(data.Id))
                {
                    notifiers[entity.EntityId].OnEntityChanged(entity);
                    notifiers[entity.EntityId].OnEntityEvent(data.Event, data.Param);
                }
            }
        }
    }
}
