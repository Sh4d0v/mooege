using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Data;
using Mooege.Core.MooNet.Toons;

namespace Mooege.Common.Storage.AccountDataBase.Entities
{
    public class DBProgressToon : Entity
    {
        public new virtual ulong Id { get; protected set; }
        public virtual DBGameAccount DBGameAccount { get; set; }
        public virtual DBToon DBToon { get; set; }
        public virtual int MaximumQuest { get; set; }
        public virtual int MaximumAct { get; set; }
        public virtual int ActiveQuest { get; set; }  
        public virtual int StepOfQuest { get; set; }
        public virtual int StepIDofQuest { get; set; }
        public virtual int Side_ActiveQuest { get; set; }
        public virtual int Side_StepOfQuest { get; set; }
        public virtual int Side_StepIDofQuest { get; set; }
        public virtual int ActiveAct { get; set; }
        
        
    }
}
