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
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _80681 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        private Players.Player MasterPlayer;

        public _80681()
            : base(80681)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" SKELETON KING QUEST STARTED ");
            foreach (var player in world.Players)
            {
                
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                dbQuestProgress.LastQuest = 72221;
                dbQuestProgress.ActiveQuest = 72061;
                if(player.Value.PlayerIndex == 0)
                {
                    MasterPlayer = player.Value;
                    //if (dbQuestProgress.ActiveQuest != 72061)
                    //    world.Game.Quests.Advance(72061);
                    if (dbQuestProgress.ActiveQuest == 72061 && dbQuestProgress.StepOfQuest == 0)
                    {
                        world.Game.Quests.Advance(72061);
                        dbQuestProgress.StepOfQuest = 1;
                    }
                }
                
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
                Logger.Debug(" Progress Saved ");
            };

            var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnEnterToParkListener(MasterPlayer, world));
            //Wait for portal to be used or player going to scene.
            ListenerUsePortalTask.ContinueWith(delegate
            {
                Logger.Debug(" Waypoint_Park Objective done "); 

            });
            StartConversation(world, 154570);
        }
        private bool OnEnterToParkListener(Players.Player player,Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                try
                {
                    sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 93992)
                    {
                        world.Game.Quests.NotifyQuest(72061, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterLevelArea, 19938);
                        world.Game.Quests.Advance(72061);
                        foreach (var playerN in world.Players)
                        {
                            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                            dbQuestProgress.ActiveQuest = 72061;
                            dbQuestProgress.StepOfQuest = 2;
                            DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                            DBSessions.AccountSession.Flush();
                        }
                        break;
                    }
                }
                catch { }
            }
            return true;
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