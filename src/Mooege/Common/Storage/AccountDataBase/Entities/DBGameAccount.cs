/*
 * Copyright (C) 2011 - 2018 mooege project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Data;

namespace Mooege.Common.Storage.AccountDataBase.Entities
{
    public class DBGameAccount : Entity
    {
        public DBGameAccount()
        {
            this.DBToons = new List<DBToon>();
            this.DBInventories = new List<DBInventory>();
        }
        public new virtual ulong Id { get; protected set; }
        public virtual DBAccount DBAccount { get; set; }
        public virtual byte[] Banner { get; set; }
        public virtual long LastOnline { get; set; }
        public virtual IList<DBToon> DBToons { get; protected set; }
        public virtual IList<DBInventory> DBInventories { get; protected set; }
        public virtual DBToon LastPlayedHero { get; set; }
        public virtual int Gold { get; set; }
        public virtual int GoldHC { get; set; }
        public virtual int StashSize { get; set; }
        public virtual int StashSizeHC { get; set; }
        //Статистика аккаунта
        public virtual uint BarbarianPlayedTime { get; set; }
        public virtual uint BarbarianHighestLevel { get; set; }
        public virtual uint BarbarianHighestDifficulty { get; set; }
        public virtual uint DemonHunterPlayedTime { get; set; }
        public virtual uint DemonHunterHighestLevel { get; set; }
        public virtual uint DemonHunterHighestDifficulty { get; set; }
        public virtual uint MonkPlayedTime { get; set; }
        public virtual uint MonkHighestLevel { get; set; }
        public virtual uint MonkHighestDifficulty { get; set; }
        public virtual uint WitchDoctorPlayedTime { get; set; }
        public virtual uint WitchDoctorHighestLevel { get; set; }
        public virtual uint WitchDoctorHighestDifficulty { get; set; }
        public virtual uint WizardPlayedTime { get; set; }
        public virtual uint WizardHighestLevel { get; set; }
        public virtual uint WizardHighestDifficulty { get; set; }
        public virtual uint HighestDifficulty { get; set; }
        public virtual uint GoldCollected { get; set; }
        public virtual uint ElitesKilled { get; set; }
        public virtual uint MonstersKilled { get; set; }
        public virtual uint HardcoreMonstersKilled { get; set; }
        public virtual uint HighestHardcoreLevel { get; set; }
    }
}
