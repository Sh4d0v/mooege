﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Mooege.Common.Storage.AccountDataBase.Entities;

namespace Mooege.Common.Storage.AccountDataBase.Mapper
{
    public class DBProgressToonMapper : ClassMap<DBProgressToon>
    {
        public DBProgressToonMapper()
        {
            Id(e => e.Id).GeneratedBy.Native();
            References(e => e.DBGameAccount).Nullable();
            References(e => e.DBToon).Nullable();
            Map(e => e.MaximumQuest);
            Map(e => e.MaximumAct);
            Map(e => e.ActiveQuest);
            Map(e => e.StepOfQuest);
            Map(e => e.StepIDofQuest);
            Map(e => e.Side_ActiveQuest);
            Map(e => e.Side_StepOfQuest);
            Map(e => e.Side_StepIDofQuest);
            Map(e => e.ActiveAct);
        
        }
    }
}