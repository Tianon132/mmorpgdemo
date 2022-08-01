using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Network;
using SkillBridge.Message;
using UnityEngine;
using Models;

namespace Services
{
    class StatusService : Singleton<StatusService>, IDisposable //状态服务
    {
        public delegate bool StatusNotifyHandler(NStatus status);

        Dictionary<StatusType, StatusNotifyHandler> eventMap = new Dictionary<StatusType, StatusNotifyHandler>();

        HashSet<StatusNotifyHandler> handles = new HashSet<StatusNotifyHandler>();//22装备新加

        public void Init()
        {

        }

        public void RegisterStatusNotify(StatusType function, StatusNotifyHandler action)
        {
            if (handles.Contains(action))   //22装备新加
                return;

            if (!eventMap.ContainsKey(function))
            {
                eventMap[function] = action;
            }
            else
                eventMap[function] += action;

            handles.Add(action);    //22装备新加 1:33，因为manager.instance连续三种，重复instance本脚本会产生问题，故采用哈希表解决
        }

        public StatusService()
        {
            MessageDistributer.Instance.Subscribe<StatusNotify>(this.OnStatusNotify);
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<StatusNotify>(this.OnStatusNotify);
        }
        private void OnStatusNotify(object sender, StatusNotify notify)
        {
            foreach(NStatus status in notify.Status)//遍历状态的协议
            {
                Notify(status);
            }
        }

        private void Notify(NStatus status)
        {
            Debug.LogFormat("StatusNotify:[{0}]{1}：[{2}]{3}", status.Type,  status.Id, status.Action, status.Value);

            if(status.Type == StatusType.Money)//直接通知的方法
            {
                if (status.Action == StatusAction.Add)
                    User.Instance.AddGold(status.Value);
                else
                    User.Instance.AddGold(-status.Value);
            }

            StatusNotifyHandler handler;
            if(eventMap.TryGetValue(status.Type, out handler))
            {
                handler(status);
            };
        }

    }
}
