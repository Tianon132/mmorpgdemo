using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Entities;
using SkillBridge.Message;
using Network;
using Common.Data;
using GameServer.Services;

namespace GameServer.Managers
{
    class QuestManager
    {
        Character Owner;

        public QuestManager(Character owner)//构造
        {
            this.Owner = owner;
        }

        public void GetQuestInfos(List<NQuestInfo> list)//得到任务信息
        {
            foreach(var quest in this.Owner.Data.Quests)
            {
                list.Add(GetQuestInfo(quest));
            }
        }

        public NQuestInfo GetQuestInfo(TCharacterQuest quest)//用数据库的数据构造一份网络数据
        {
            return new NQuestInfo()
            {
                QuestId = quest.QuestID,
                QuestGold = quest.Id,
                Status = (QuestStatus)quest.Status,
                Targets = new int[3]
                {
                    quest.Target1,
                    quest.Target2,
                    quest.Target3,
                }
            };
        }

        public Result AcceptQuest(NetConnection<NetSession> sender, int questId)
        {
            Character character = sender.Session.Character;//获得当前角色才能做具体的任务

            QuestDefine quest;
            if(DataManager.Instance.Quests.TryGetValue(questId, out quest))
            {
                var dbquest = DBService.Instance.Entities.TCharacterQuests.Create();
                dbquest.QuestID = quest.ID;
                if (quest.Target1 == QuestTarget.None)
                {   //没有目标直接完成
                    dbquest.Status = (int)QuestStatus.Complated;
                }
                else
                {   //有目标的
                    dbquest.Status = (int)QuestStatus.InProgress;
                }
                sender.Session.Response.questAccept.Quest = this.GetQuestInfo(dbquest);
                character.Data.Quests.Add(dbquest);
                DBService.Instance.Save();
                return Result.Success;
            }
            else
            {
                sender.Session.Response.questAccept.Errormsg = "任务不存在";
                return Result.Failed;
            }
        }

        public Result SubmitQuest(NetConnection<NetSession> sender, int questId)
        {
            Character character = sender.Session.Character;

            QuestDefine quest;
            if (DataManager.Instance.Quests.TryGetValue(questId, out quest))
            {
                var dbquest = character.Data.Quests.Where(q => q.QuestID == questId).FirstOrDefault();//返回序列中的第一个元素，有没有一个Id==questId
                if(dbquest != null)
                {
                    if (dbquest.Status != (int)QuestStatus.Complated)//任务条件的完成
                    {//还不是完成状态
                        sender.Session.Response.questSubmit.Errormsg = "任务未完成";
                        return Result.Failed;
                    }
                    dbquest.Status = (int)QuestStatus.Finished;//任务完成
                    sender.Session.Response.questSubmit.Quest = this.GetQuestInfo(dbquest);
                    DBService.Instance.Save();

                    //处理任务奖励
                    if (quest.RewardGold > 0)
                    {
                        character.Gold += quest.RewardGold;
                    }
                    if(quest.RewardExp>0)
                    {
                        //character.Exp += quest.RewardExp;
                    }

                    if(quest.RewardItem1 > 0)
                    {
                        character.ItemManager.AddItem(quest.RewardItem1, quest.RewardItem1Count);
                    }
                    if (quest.RewardItem2 > 0)
                    {
                        character.ItemManager.AddItem(quest.RewardItem2, quest.RewardItem2Count);
                    }
                    if (quest.RewardItem3 > 0)
                    {
                        character.ItemManager.AddItem(quest.RewardItem3, quest.RewardItem3Count);
                    }
                    DBService.Instance.Save();
                    return Result.Success;
                }
                sender.Session.Response.questSubmit.Errormsg = "任务不存在[2]";//数据库里不存在
                return Result.Failed;
            }
            else
            {
                sender.Session.Response.questSubmit.Errormsg = "任务不存在[1]";//数据表里不存在
                return Result.Failed;
            }
        }
    }
}
