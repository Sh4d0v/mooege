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
    class _198521 : QuestEvent  // RumfordProtectorEnd_New and be careful as this shit is also supposed to trigger the next event with leah..TEH HELL
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;


        public _198521()
            : base(198521)
        {
        }

        public override void Execute(Map.World world)
        {
            //StartConversation(world, 198521); 
            Logger.Debug(" Conversation done ");
            if(HadConversation)
            {
                world.Game.Quests.Notify(QuestStepObjectiveType.CompleteQuest, 87700);
                //world.Game.Quests.Advance(87700);
                //world.Game.Quests.NotifyQuest(87700, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.HadConversation, 198521);
          
                HadConversation = false;
            }



            //okay now we send a notify with QuestEvent for every one
            //force leah to have a specific conversation list :p
            //world.GetActorBySNO(4580).Tags.Replace(MarkerKeys.ConversationList, new TagMapEntry(MarkerKeys.ConversationList.ID, 166675, 2));
            //world.Game.Quests.NotifyQuest(87700, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.CompleteQuest, 1);
            foreach (var player in world.Players)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                
                player.Value.InGameClient.SendMessage(new Mooege.Net.GS.Message.Definitions.Quest.QuestMeterMessage()
                {
                    snoQuest = 87700,
                    Field1 = 2,
                    Field2 = 10.0f
                });
                D3.Quests.QuestReward.Builder Reward = new D3.Quests.QuestReward.Builder();
                Reward.SnoQuest = 87700;
                Reward.GoldGranted = 500;
                Reward.XpGranted = 1000;

                Reward.Build();
                player.Value.InGameClient.SendMessage(new Mooege.Net.GS.Message.Definitions.Quest.QuestRewardMessage()
                {
                    QuestReward = Reward.Build()
                });
                //player.Value.World.Game.Quests.CurrentQuest(72095);
                       dbQuestProgress.LastQuest = 87700;
            //           dbQuestProgress.ActiveQuest = 72095;
            //           dbQuestProgress.StepOfQuest = -1;
                       DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                       DBSessions.AccountSession.Flush();
            };

            // starting second quest
            StartConversation(world, 198541);
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