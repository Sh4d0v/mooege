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
using Mooege.Net.GS.Message;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _72817 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;
        private Players.Player MasterPlayer;
        private int EnterStep;
        public _72817()
            : base(72817)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Conversation done ");
            if (HadConversation)
            {
               
                foreach (var player in world.Players)
                {
                    var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                    if (player.Value.PlayerIndex == 0)
                    {
                        if (dbQuestProgress.StepOfQuest < 2)
                        {
                            world.Game.Quests.Advance(73236);
                        }
                        MasterPlayer = player.Value;
                    }
                    DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                    DBSessions.AccountSession.Flush();
                }
                
                HadConversation = false;
            }
            var AttackedTown = world.Game.GetWorld(72882);
            var startingPoint = AttackedTown.StartingPoints[1].Position;

            foreach (var player in world.Players)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                if (player.Value.PlayerIndex == 0)
                {
                    if (dbQuestProgress.ActiveQuest == 73236)
                    {
                        EnterStep = dbQuestProgress.StepOfQuest;
                    } else
                        EnterStep = -1;
                    //player.Value.ChangeWorld(AttackedTown, startingPoint);
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }

            var Maghda = AttackedTown.GetActorBySNO(129345);
            AttackedTown.Leave(Maghda);
            if (EnterStep < 3)
            {
                var ListenerEnterToCenterTownEnter = Task<bool>.Factory.StartNew(() => OnListenerToCenterTownEnter(MasterPlayer, world));
                ListenerEnterToCenterTownEnter.ContinueWith(delegate
                {
                    Logger.Debug("Enter to Center Town done ");

                   /* foreach (var playerN in world.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                        dbQuestProgress.ActiveQuest = 73236;
                        dbQuestProgress.StepOfQuest = 3;
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }*/

                    var Cultists1 = AttackedTown.GetActorsBySNO(90367);
                    var Cultists2 = AttackedTown.GetActorsBySNO(90008);
                    List<uint> CultistList = new List<uint> { };
                    foreach (var Cultist in Cultists1)
                    {
                        if (Cultist.CurrentScene.SceneSNO.Id == 76000)
                        {
                            CultistList.Add(Cultist.DynamicID);
                            Cultist.Attributes[Net.GS.Message.GameAttribute.Quest_Monster] = true;

                        }
                    }
                    foreach (var Cultist in Cultists2)
                    {
                        if (Cultist.CurrentScene.SceneSNO.Id == 76000)
                        {
                            CultistList.Add(Cultist.DynamicID);
                            Cultist.Attributes[Net.GS.Message.GameAttribute.Quest_Monster] = true;
                        }
                    }
                    var ListenerSkeletons = Task<bool>.Factory.StartNew(() => OnKillCultistInTownListener(CultistList, AttackedTown));
                    ListenerSkeletons.ContinueWith(delegate
                    {
                        
                        Maghda.EnterWorld(Maghda.Position);
                        Maghda.Attributes[GameAttribute.Hitpoints_Max] = 20000000f;
                        Maghda.Attributes[GameAttribute.Hitpoints_Cur] = 20000000f;
                        AttackedTown.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                        {
                            ActorID = Maghda.DynamicID,
                            Field1 = 5,
                            Field2 = 0,
                            tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                        {
                            Duration = 100,
                            AnimationSNO = 193535,
                            PermutationIndex = 0,
                            Speed = 0.9f
                        }
                            }
                        }, Maghda);

                        StartConversation(AttackedTown, 194933);
                        StartConversation(AttackedTown, 194942);

                        //StartConversation(world,1);

                    });

                });
            }
           
        }

        private bool OnKillCultistInTownListener(List<uint> monstersAlive, Map.World world)
        {
            System.Int32 monstersKilled = 0;
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

        private bool OnListenerToCenterTownEnter(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                try
                {
                    if (player.World.WorldSNO.Id == 72882)
                    {
                        sceneID = player.CurrentScene.SceneSNO.Id;
                        if (sceneID == 76000)
                        {
                            
                            //world.Game.Quests.Advance(73236);
                            world.Game.Quests.NotifyQuest(73236, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1);
                            break;
                        }
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