using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Managers
{
    class NpcManager : Singleton<NpcManager>
    {
        public delegate bool NpcActionHandler(NpcDefine npc);//用委托管理事件

        Dictionary<NpcFunction, NpcActionHandler> eventMap = new Dictionary<NpcFunction, NpcActionHandler>();
        Dictionary<int, Vector3> npcPosition = new Dictionary<int, Vector3>();

        public void RegisterNpcEvent(NpcFunction function, NpcActionHandler action)//注册事件，有Function的注册
        {
            if (!eventMap.ContainsKey(function))
            {
                eventMap[function] = action;
            }
            else
                eventMap[function] += action;
        }//事件是告诉Npc系统我要干什么

        public NpcDefine GetNpcDefine(int npcID)
        {
            NpcDefine npc = null;
            DataManager.Instance.Npcs.TryGetValue(npcID, out npc);//职位获取定义文件中的NPC信息
            return npc;
        }

        public bool Interactive(int npcId)//交互（用来判断）
        {
            if(DataManager.Instance.Npcs.ContainsKey(npcId))
            {
                var npc = DataManager.Instance.Npcs[npcId];
                return Interactive(npc);
            }
            return false;
        }

        public bool Interactive(NpcDefine npc)//真正的交互
        {
            /* 不再进行判断。因为无论有没有任务都需要检查一遍
            if(npc.Type == NpcType.Task)
            {
                return DotaskInteractive(npc);
            }*/
            if(DotaskInteractive(npc))
            {
                return true;
            }
            else if(npc.Type == NpcType.Functional)
            {
                return DoFunctionInteractive(npc);
            }
            return false;
        }

        private bool DotaskInteractive(NpcDefine npc)//任务型NPC
        {
            var status = QuestManager.Instance.GetQuestStatusByNpc(npc.ID);

            if (status == NpcQuestStatus.None)
                return false;

            return QuestManager.Instance.OpenNpcQuest(npc.ID);
        }

        private bool DoFunctionInteractive(NpcDefine npc)//功能型NPC
        {
            if (npc.Type != NpcType.Functional)
                return false;

            if (!eventMap.ContainsKey(npc.Function))
                return false;

            return eventMap[npc.Function](npc);
        }

        internal void UpdateNpcPosition(int npc, Vector3 pos)//设置Npc位置
        {
            this.npcPosition[npc] = pos;
        }

        internal Vector3 GetNpcPosition(int npc)//获取NPC位置
        {
            return this.npcPosition[npc];
        }
    }
}
