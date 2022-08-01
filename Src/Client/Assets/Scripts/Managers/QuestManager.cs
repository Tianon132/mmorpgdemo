using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SkillBridge.Message;
using Models;
using UnityEngine.Events;
using Services;

namespace Managers
{
    public enum NpcQuestStatus
    {
        None = 0,   //无任务
        Complete,   //已完成，未提交
        Avaliable,  //可接受任务
        Incomplete  //未完成
    }

    class QuestManager : Singleton<QuestManager>
    {
        //所有有效任务
        public List<NQuestInfo> questInfos;
        public Dictionary<int, Quest> allQuests = new Dictionary<int, Quest>();//用字典方便实用Id查询

        //第一个int是npc的id，第二个NpcQuestStatus是npc状态
        public Dictionary<int, Dictionary<NpcQuestStatus, List<Quest>>> npcQuests = new Dictionary<int, Dictionary<NpcQuestStatus, List<Quest>>>();

        public UnityAction<Quest> OnQuestStatusChanged;
        
        public void Init(List<NQuestInfo> quests)//进入游戏的时候调用
        {
            this.questInfos = quests;
            allQuests.Clear();
            this.npcQuests.Clear();
            InitQuests();
        }

        //初始化：根据服务器返回的信息分别把不同的任务添加到不同的npc身上
        void InitQuests()
        {
            //初始化已有任务
            foreach(var info in this.questInfos)
            {
                Quest quest = new Quest(info);
                this.allQuests[quest.Info.QuestId] = quest;//已接任务放到总字典中
            }

            this.CheckAvailableQuests();//可接任务放进去

            foreach(var kv in this.allQuests)
            {
                this.AddNpcQuest(kv.Value.Define.AcceptNPC, kv.Value);
                this.AddNpcQuest(kv.Value.Define.SubmitNPC, kv.Value);
            }
        }

        void CheckAvailableQuests()//检索本地的可接任务到allQuests
        {
            //初始化可用任务
            foreach (var kv in DataManager.Instance.Quests)
            {
                if (kv.Value.LimitClass != CharacterClass.None && kv.Value.LimitClass != User.Instance.CurrentCharacter.Class)
                    continue;//不符合职业

                if (kv.Value.LimitLevel > User.Instance.CurrentCharacter.Level)
                    continue;//不符合等级

                if (this.allQuests.ContainsKey(kv.Key))
                    continue;//任务已经存在

                if (kv.Value.PreQuest > 0)
                {
                    Quest preQuest;
                    if (this.allQuests.TryGetValue(kv.Value.PreQuest, out preQuest))//获取前置任务
                    {
                        if (preQuest.Info == null)
                            continue;//前置任务未接取  则不允许接，不显示该任务
                        if (preQuest.Info.Status != QuestStatus.Finished)
                            continue;//前置任务未完成  同理
                    }
                    else
                        continue;//前置任务还没接
                }

                //添加可添加的任务
                Quest quest = new Quest(kv.Value);
                this.allQuests[quest.Define.ID] = quest;
            }
        }

        /// <summary>
        /// 给Npc添加任务
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="quest"></param>
        void AddNpcQuest(int npcId, Quest quest)
        {
            if (!this.npcQuests.ContainsKey(npcId))
                this.npcQuests[npcId] = new Dictionary<NpcQuestStatus, List<Quest>>();

            List<Quest> availables;
            List<Quest> completes;
            List<Quest> incompletes;

            //初始化三种列表
            if (!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Avaliable, out availables))
            {
                availables = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Avaliable] = availables;
            }
            if (!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Complete, out completes))
            {
                completes = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Complete] = completes;
            }
            if(!this.npcQuests[npcId].TryGetValue(NpcQuestStatus.Incomplete, out incompletes))
            {
                incompletes = new List<Quest>();
                this.npcQuests[npcId][NpcQuestStatus.Incomplete] = incompletes;
            }

            if(quest.Info == null)
            {
                if (npcId == quest.Define.AcceptNPC && !this.npcQuests[npcId][NpcQuestStatus.Avaliable].Contains(quest))
                {
                    this.npcQuests[npcId][NpcQuestStatus.Avaliable].Add(quest);//接任务npc加进去
                }
            }
            else
            {
                //分已完成的npc 和 进行中的未完成提交npc
                if(quest.Define.SubmitNPC == npcId && quest.Info.Status == QuestStatus.Complated)
                {
                    if (!this.npcQuests[npcId][NpcQuestStatus.Complete].Contains(quest))
                    {
                        this.npcQuests[npcId][NpcQuestStatus.Complete].Add(quest);
                    }
                }
                if(quest.Define.SubmitNPC == npcId && quest.Info.Status == QuestStatus.InProgress)
                {
                    if(!this.npcQuests[npcId][NpcQuestStatus.Incomplete].Contains(quest))
                    {
                        this.npcQuests[npcId][NpcQuestStatus.Incomplete].Add(quest);
                    }
                }
            }
        }

        /// <summary>
        /// 获取Npc任务状态
        /// </summary>
        /// <param name="npcId"></param>
        /// <returns></returns>
        public NpcQuestStatus GetQuestStatusByNpc(int npcId)
        {
            Dictionary<NpcQuestStatus, List<Quest>> status = new Dictionary<NpcQuestStatus, List<Quest>>();

            if(this.npcQuests.TryGetValue(npcId, out status))
            {
                if (status[NpcQuestStatus.Complete].Count > 0)//检查状态
                    return NpcQuestStatus.Complete;
                if (status[NpcQuestStatus.Avaliable].Count > 0)
                    return NpcQuestStatus.Avaliable;
                if (status[NpcQuestStatus.Incomplete].Count > 0)
                    return NpcQuestStatus.Incomplete;
            }

            return NpcQuestStatus.None;
        }

        /// <summary>
        /// 打开任务对话框
        /// </summary>
        /// <param name="npcId"></param>
        /// <returns></returns>
        public bool OpenNpcQuest(int npcId)
        {
            Dictionary<NpcQuestStatus, List<Quest>> status = new Dictionary<NpcQuestStatus, List<Quest>>();

            if(this.npcQuests.TryGetValue(npcId, out status))//获取npc任务
            {
                if (status[NpcQuestStatus.Complete].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Complete].First());
                if(status[NpcQuestStatus.Avaliable].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Avaliable].First());
                if (status[NpcQuestStatus.Incomplete].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Incomplete].First());
            }

            return false;
        }

        /// <summary>
        /// 显示任务对话框
        /// </summary>
        /// <param name="quest"></param>
        /// <returns></returns>
        bool ShowQuestDialog(Quest quest)
        {
            if(quest.Info == null || quest.Info.Status == QuestStatus.Complated)
            {
                UIQuestDialog dlg = UIManager.Instance.Show<UIQuestDialog>();
                dlg.SetQuest(quest);
                dlg.OnClose += OnQuestDialogClose;
                return true;
            }

            if(quest.Info != null || quest.Info.Status == QuestStatus.Complated)
            {
                if (!string.IsNullOrEmpty(quest.Define.DialogIncomplete))
                    MessageBox.Show(quest.Define.DialogIncomplete);
            }
            return true;
        }

        /// <summary>
        /// 关闭对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="result"></param>
        void OnQuestDialogClose(UIWindow sender, UIWindow.WindowResult result)
        {
            UIQuestDialog dlg = (UIQuestDialog)sender;
            if (result == UIWindow.WindowResult.Yes)
            {
                //MessageBox.Show(dlg.quest.Define.DialogAccept);
                if (dlg.quest.Info == null)
                    QuestService.Instance.SendQuestAccept(dlg.quest);
                else
                    QuestService.Instance.SendQuestSubmit(dlg.quest);
                    
            }
            else if(result == UIWindow.WindowResult.No)
            {
                MessageBox.Show(dlg.quest.Define.DialogDeny);
            }
        }

        Quest RefreshQuestStatus(NQuestInfo quest)
        {
            this.npcQuests.Clear();//先清除所有的npc任务
            Quest result;
            if(this.allQuests.ContainsKey(quest.QuestId))//检查所有任务的列表
            {
                //存在--更新新的任务状态
                this.allQuests[quest.QuestId].Info = quest;
                result = this.allQuests[quest.QuestId];
            }
            else
            {
                //不存在--创建个新的
                result = new Quest(quest);
                this.allQuests[quest.QuestId] = result;
            }

            this.CheckAvailableQuests();

            foreach(var kv in this.allQuests)
            {
                this.AddNpcQuest(kv.Value.Define.AcceptNPC, kv.Value);
                this.AddNpcQuest(kv.Value.Define.SubmitNPC, kv.Value);
            }

            if (OnQuestStatusChanged != null)
                OnQuestStatusChanged(result);//任务系统没有调用其他，但是NPCcontroller委托调用这里
            return result;
        }

        public void OnQuestAccepted(NQuestInfo info)
        {
            var quest = this.RefreshQuestStatus(info);
            MessageBox.Show(quest.Define.DialogAccept);//弹出任务完成的对话
        }

        public void OnQuestSubmited(NQuestInfo info)
        {
            var quest = this.RefreshQuestStatus(info);
            MessageBox.Show(quest.Define.DialogFinish);
        }
    }
}
