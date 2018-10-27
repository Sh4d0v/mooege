using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Data;
using Mooege.Core.MooNet.Toons;

namespace Mooege.Common.Storage.AccountDataBase.Entities
{
    public class DBArtisansOfToon : Entity
    {
        public new virtual ulong Id { get; protected set; }
        public virtual DBGameAccount DBGameAccount { get; set; }
        public virtual DBToon DBToon { get; set; }
        public virtual int Blacksmith { get; set; }
        public virtual int Jeweler { get; set; }
        public virtual int Mystic { get; set; }
        


    }
}
