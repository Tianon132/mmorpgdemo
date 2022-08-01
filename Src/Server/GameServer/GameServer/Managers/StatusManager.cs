using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameServer.Entities;
using SkillBridge.Message;
using Common.Data;

namespace GameServer.Managers
{
    class StatusManager
    {
        Character Owner;

        private List<NStatus> Status { get; set; }

        public bool HasStatus
        {
            get { return this.Status.Count > 0; }
        }

        public StatusManager(Character owner)//构造函数，创建新的列表进行操作
        {
            this.Owner = owner;
            this.Status = new List<NStatus>();
        }

        public void AddStatus(StatusType type, int id, int value, StatusAction action)
        {
            this.Status.Add(new NStatus()
            {
                Type = type,
                Id = id,
                Value = value,
                Action = action
            });
        }

        public void AddGoldChange(int goldDelta)//加金币
        {
            if(goldDelta > 0)
            {
                this.AddStatus(StatusType.Money, 0, goldDelta, StatusAction.Add);
            }
            if(goldDelta < 0)
            {
                this.AddStatus(StatusType.Money, 0, -goldDelta, StatusAction.Delete);
            }
        }

        public void AddItemChange(int id, int count, StatusAction action)//添加道具
        {
            this.AddStatus(StatusType.Item, id, count, action);
        }

        public void PostProcess(NetMessageResponse message)//应用响应   //后处理接口修改
        {
            if (message.statusNotify == null)
                message.statusNotify = new StatusNotify();
            foreach (var status in this.Status)
            {
                message.statusNotify.Status.Add(status);
            }
            this.Status.Clear();
        }
    }
}
