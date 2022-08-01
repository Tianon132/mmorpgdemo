using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace GameServer.Services
{
    class DBService : Singleton<DBService>
    {
        ExtremeWorldEntities entities;

        public ExtremeWorldEntities Entities
        {
            get { return this.entities; }
        }

        public void Init()
        {
            entities = new ExtremeWorldEntities();
        }

        public void Save(bool async = false)//异步保存数据，ItemManager调用
        {
            if(async)
                entities.SaveChangesAsync();//异步save
            else
                entities.SaveChanges();//同步save
        }
        
    }
}
