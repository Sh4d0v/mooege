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
using Mooege.Common.MPQ.FileFormats;
using Mooege.Net.GS.Message.Definitions.ACD;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Generators;
using Mooege.Common.Logging;
using System.Threading.Tasks;
using System.Threading;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.GS.Actors.Interactions;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _181912 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;


        public _181912()
            : base(181912)
        {

        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Conversation done ");

            if (HadConversation)
            {
                world.Game.Quests.Advance(72061);
                world.Game.Quests.Advance(72061);

                HadConversation = false;
            }


            foreach (var player in world.Players)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);

                dbQuestProgress.ActiveQuest = 72061;
                dbQuestProgress.StepOfQuest = 18;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            };

        }

        //Launch Conversations.
        private bool StartConversation(Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId); // this does the job of sending the proper stuff :p
            }
            return true;
        }
    }
}