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
using Mooege.Common.MPQ.FileFormats;
using Mooege.Net.GS.Message.Definitions.ACD;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Generators;
using Mooege.Common.Logging;
using System.Threading.Tasks;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _198617 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public _198617()
            : base(198617)
        {
        }

        private Boolean HadConversation = true;

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                world.Game.Quests.Advance(72095);
                world.Game.Quests.Notify(QuestStepObjectiveType.CompleteQuest, 72095);
            }
            foreach (var player in world.Players)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                dbQuestProgress.LastQuest = 72095;
                dbQuestProgress.ActiveQuest = 72221;
                dbQuestProgress.StepOfQuest = 1;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
                player.Value.InGameClient.SendMessage(new Mooege.Net.GS.Message.Definitions.Quest.QuestMeterMessage()
                {
                    snoQuest = 72095,
                    Field1 = 2,
                    Field2 = 10.0f

                });
            };
            Logger.Debug(" Второй квест окончен. ");

            // starting third quest
            world.Game.Quests.Advance(72221);
            var BlacksmithVendor = world.GetActorBySNO(56947);
            Vector3D position = new Vector3D(BlacksmithVendor.Position);
            //world.SpawnMonster(65036, position);// NonVendor - 65036
            var BlacksmithQuest = world.GetActorBySNO(65036);
            BlacksmithQuest.RotationAxis = BlacksmithVendor.RotationAxis;
            BlacksmithQuest.RotationW = BlacksmithVendor.RotationW;
            world.Leave(BlacksmithVendor);

            var TELEGAS = world.GetActorsBySNO(112131);
            Vector3D LastTelega = new Vector3D();
            foreach (var TELEGA in TELEGAS)
            {
                TELEGA.Destroy();
                LastTelega = TELEGA.Position;
            }


        }


        private bool StartConversation(Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
            }
            return true;
        }
    }
}