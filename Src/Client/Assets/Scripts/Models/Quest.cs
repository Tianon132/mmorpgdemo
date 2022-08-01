using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Data;
using SkillBridge.Message;

namespace Models
{
    public class Quest
    {
        public QuestDefine Define;//本地配置信息
        public NQuestInfo Info;//网络信息（任务如果还没接，是不会存在网络信息的）

        public Quest()
        {

        }

        public Quest(NQuestInfo info)
        {
            this.Info = info;
            this.Define = DataManager.Instance.Quests[info.QuestId];
        }

        public Quest(QuestDefine define)
        {
            this.Define = define;
            this.Info = null;
        }

        public string GetTypeName()
        {
            return EnumUtil.GetEnumDescription(this.Define.Type);
        }
    }
}
