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
using System.Threading;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.GS.Actors.Interactions;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _103388 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;
        List<uint> Ghosts = new List<uint> { };
        List<uint> Warden = new List<uint> { };

        public _103388()
            : base(103388)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Conversation done ");
            if (HadConversation)
            {
                HadConversation = false;
            }
            /*
            foreach (var player in world.Players)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                dbQuestProgress.ActiveQuest = 72801;
                dbQuestProgress.StepOfQuest = 5;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            };
            */
            //103381 - Королева
            
            //[102927] Ghost_Jail_Prisoner - Останки узника
            //world.SpawnMonster(102927, new Vector3D(188.899f, 152.7945f, 0.09996948f));
            //world.SpawnMonster(102927, new Vector3D(152.8937f, 158.7531f, 0.09996949f));
            world.SpawnMonster(102927, new Vector3D(188.899f, 632.7945f, 0.09996948f));
            world.SpawnMonster(102927, new Vector3D(152.8937f, 638.7531f, 0.09996949f));

            world.SpawnMonster(102927, new Vector3D(157.9025f, 557.83541f, 0.09996948f));
            world.SpawnMonster(102927, new Vector3D(187.6742f, 551.57829f, 0.09996951f));

            world.SpawnMonster(102927, new Vector3D(73.7037f, 567.84306f, 0.09996996f));
            world.SpawnMonster(102927, new Vector3D(216.9432f, 601.3437f, 0.09996957f));
            world.Leave(world.GetActorBySNO(103381));
            var AllGhosts = world.GetActorsBySNO(102927);
            foreach (var Ghost in AllGhosts)
            {
                Ghosts.Add(Ghost.DynamicID);
            }

            var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnUseListener(Ghosts, world));
            
            ListenerUsePortalTask.ContinueWith(delegate
            {
                Logger.Debug("Все призраки найдены");
                StartConversation(world, 109728);
                world.SpawnMonster(98879,new Vector3D(604.2187f,596.0316f,0.9996948f));
                Warden.Add(world.GetActorBySNO(98879).DynamicID);
                var ListenerWarden = Task<bool>.Factory.StartNew(() => OnKillListener(Warden, world));
                ListenerWarden.ContinueWith(delegate
                {
                    //Удаляем дверь
                    world.Game.Quests.NotifyQuest(72801, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.KillMonster, 98879);
                    world.GetActorBySNO(100862).Destroy();
                });
            });
            //Первая точка (188.899f,152.7945f,0.09996948f)
            //Вторая точка (152.8937f,158.7531f,0.09996949f)

        }


        private bool OnKillListener(List<uint> monstersAlive, Map.World world)
        {
            Int32 monstersKilled = 0;
            var monsterCount = monstersAlive.Count; //Since we are removing values while iterating, this is set at the first real read of the mob counting.
            while (monstersKilled != monsterCount)
            {
                //Iterate through monstersAlive List, if found dead we start to remove em till all of em are dead and removed.
                for (int i = monstersAlive.Count - 1; i >= 0; i--)
                {
                    if (world.HasMonster(monstersAlive[i]))
                    {
                        //Alive: Nothing.
                    }
                    else
                    {
                        //If dead we remove it from the list and keep iterating.
                        Logger.Debug(monstersAlive[i] + " has been killed");
                        monstersAlive.RemoveAt(i);
                        monstersKilled++;
                    }
                }
            }
            return true;
        }

        private bool OnUseListener(List<uint> actorSDynID, Map.World world)
        {
            var ActiveNeedCount = actorSDynID.Count;
            Int32 ActiveNeed = 0;
            while (ActiveNeedCount != ActiveNeed)
            {
                for (int i = actorSDynID.Count - 1; i >= 0; i--)
                {
                    if (world.GetActorByDynamicId(actorSDynID[i]).Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated])
                    {
                        //Alive: Nothing.
                        Logger.Debug(actorSDynID[i] + " has been used");
                        actorSDynID.RemoveAt(i);
                        ActiveNeed++;
                    }
                    else
                    {
                        //Nothing.

                    }
                }
            }
            return true;
        }

        //Launch Conversations.
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