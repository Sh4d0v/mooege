using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using Mooege.Common.Storage.AccountDataBase.Entities;

namespace Mooege.Common.Storage.AccountDataBase.Mapper
{
    public class DBArtisansOfToonMapper : ClassMap<DBArtisansOfToon>
    {
        public DBArtisansOfToonMapper()
        {
            Id(e => e.Id).GeneratedBy.Native();
            References(e => e.DBGameAccount).Nullable();
            References(e => e.DBToon).Nullable();
            Map(e => e.Blacksmith);
            Map(e => e.Jeweler);
            Map(e => e.Mystic);
            
        }
    }
}