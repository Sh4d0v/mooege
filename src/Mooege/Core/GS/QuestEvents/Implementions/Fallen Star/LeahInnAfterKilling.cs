/*
 * Copyright (C) 2018 DiIiS project
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
using System.Threading;
using System.Threading.Tasks;
using Mooege.Common.MPQ.FileFormats;
using Mooege.Net.GS.Message.Definitions.ACD;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Generators;
using Mooege.Common.Logging;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;


namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class LeahInnAfterKilling : QuestEvent
    {
        //ActorID: 0x7A3100DD  
        //ZombieSkinny_A_LeahInn.acr (2050031837)
        //ActorSNOId: 0x00031971:ZombieSkinny_A_LeahInn.acr

        private static readonly Logger Logger = LogManager.CreateLogger();

        public LeahInnAfterKilling()
            : base(151167)
        {
        }

        private Boolean HadConversation = true;

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                world.Game.Quests.Advance(87700);
                foreach (var player in world.Players)
                {
                    var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                    dbQuestProgress.ActiveQuest = 87700;
                    dbQuestProgress.StepOfQuest = 5;
                    dbQuestProgress.StepIDofQuest = 50;
                    DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                    DBSessions.AccountSession.Flush();
                };
            }
        }


    }
}