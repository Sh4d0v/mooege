﻿/*
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
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Objects;
using Mooege.Core.GS.Actors.Implementations;
using Mooege.Core.GS.AI;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Net.GS.Message;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class SurviveTheWaves : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();

        public SurviveTheWaves()
            : base(151087)    // 198199 // 80088  // 151102
        {
        }

        List<uint> monstersAlive = new List<uint> { }; //We use this for the killeventlistener.
        List<Int32> monstersId = new List<Int32> { };
        List<Vector3D> ActorsVector3D = new List<Vector3D> { }; //We fill this with the vectors of the actors
        //bool started = false;

        public override void Execute(Map.World world)
        {
            ////Disable RumFord so he doesn't offer the quest. Somehow, hes supposed to mark it as readed and not offer it while theres no other quest available but he does,
            ////so you can trigger the event multiple times while the event is already running, therefor, we disable his interaction till the event is done.-Wesko

            setActorOperable(world, 3739, false); // no need for it now the update conversation list is laucnhed once the conversation is marked as read :p
            StartConversation(world, 198199);
          /*  foreach (var player in world.Players)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                dbQuestProgress.ActiveQuest = 87700;
                dbQuestProgress.StepOfQuest = 1;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            };*/
            var wave1Actors = world.GetActorsInGroup("GizmoGroup1");
            monstersId.Clear();
            ActorsVector3D.Clear();
            foreach (var actor in wave1Actors)
            {
                if (actor.ActorSNO.Id == 76857)
                    monstersId.Add(6632);
                else
                    monstersId.Add(6644);
                ActorsVector3D.Add(new Vector3D(actor.Position.X, actor.Position.Y, actor.Position.Z));
            }
            var zombieWave1 = Task<bool>.Factory.StartNew(() => LaunchWave(ActorsVector3D, world, monstersId));
            zombieWave1.Wait();
            var ListenerZombie1 = Task<bool>.Factory.StartNew(() => OnKillListener(monstersAlive, world));
            ListenerZombie1.ContinueWith(delegate //Once killed:
            {
                //Wave three: Skinnies + RumFord conversation #2 "They Keep Comming!".
                StartConversation(world, 80088);
                var wave2Actors = world.GetActorsInGroup("GizmoGroup2");
                monstersId.Clear();
                ActorsVector3D.Clear();
                foreach (var actor in wave2Actors)
                {
                    if (actor.ActorSNO.Id == 76857)
                        monstersId.Add(6632);
                    else
                        monstersId.Add(6644);
                    ActorsVector3D.Add(new Vector3D(actor.Position.X, actor.Position.Y, actor.Position.Z));

                }
                var zombieWave2 = Task<bool>.Factory.StartNew(() => LaunchWave(ActorsVector3D, world, monstersId));
                zombieWave2.Wait();
                var ListenerZombie2 = Task<bool>.Factory.StartNew(() => OnKillListener(monstersAlive, world));
                ListenerZombie2.ContinueWith(delegate //Once killed:
                {
                    StartConversation(world, 151102);
                    world.Game.Quests.Advance(87700);
                    foreach (var player in world.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                        dbQuestProgress.ActiveQuest = 87700;
                        dbQuestProgress.StepOfQuest = 2;
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    };
                    var OldGate = world.GetActorBySNO(90419);
                    
                    var NoDownGate = new Door(world,90419,world.GetActorBySNO(90419).Tags);
                    NoDownGate.Field2 = 16;
                    NoDownGate.RotationAxis = world.GetActorBySNO(90419).RotationAxis;
                    NoDownGate.RotationW = world.GetActorBySNO(90419).RotationW;
                    NoDownGate.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                    //NoDownGate.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                    NoDownGate.Attributes[GameAttribute.Gizmo_State] = 1;
                    NoDownGate.Attributes[GameAttribute.Untargetable] = true;
                    NoDownGate.Attributes.BroadcastChangedIfRevealed();
                    NoDownGate.EnterWorld(world.GetActorBySNO(90419).Position);
                    OldGate.Destroy();

                    world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                    {
                        ActorID = NoDownGate.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[] { new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                                    {
                                        Duration = 100,
                                        AnimationSNO = NoDownGate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                        PermutationIndex = 0,
                                        Speed = 0.5f
                                    }
                                }
                    }, NoDownGate);

                    world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                    {
                        ActorID = NoDownGate.DynamicID,
                        AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                    }, NoDownGate);
                    NoDownGate.Field2 = 16;
                    NoDownGate.Attributes[Net.GS.Message.GameAttribute.Operatable] = false;

                    Logger.Debug("Event finished");
                    // wyjebanie leah                      
                    var actorToShoot = world.GetActorByDynamicId(72);
                    if (actorToShoot != null)
                    {
                        Logger.Debug("trying to shoot actor SNO {0}, world contains {1} such actors ", actorToShoot.ActorSNO, world.GetActorsBySNO(3739).Count);
                        world.Leave(actorToShoot);
                    }
                    else
                    {
                        Logger.Debug("No actor to shoot yet");
                    }

                    setActorOperable(world, 3739, true);
                });
            });

            // check rumford state :p
            var rumfordActor = world.GetActorBySNO(3739);

            // display real type for rumford actor
            Logger.Debug(" Rumford has type {0}", rumfordActor.GetType());

            //var rumfordBrain = (rumfordActor as Living).Brain;
            //try
            //{
            //    Logger.Debug(" Rumford as a brain {0}, activating now ! ", (rumfordActor as CaptainRumford).Brain);
            //    (rumfordActor as CaptainRumford).Brain.Activate();

            //}
            //catch (System.NullReferenceException e)
            //{
            //    Logger.Debug(" brain in rumford has a lots of problems !");
            //}


        }

        private bool OnKillListener(Map.World world, string group)
        {
            while (world.HasActorsInGroup(group))
            {
            }
            return true;
        }

        private bool _status = false;
        private bool WaitConversation(Map.World world)
        {
            var players = world.Players;
            while (!_status)
            {
                foreach (var player in players)
                {
                    if (player.Value.Conversations.ConversationRunning() == true)
                    {
                        _status = false;
                        return true;
                    }
                    else
                    {
                        //Logger.Debug("Waiting");
                    }
                }
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

        public static bool setActorOperable(Map.World world, Int32 snoId, bool status)
        {
            var actor = world.GetActorBySNO(snoId);
            foreach (var player in world.Players)
            {
                actor.Attributes[Net.GS.Message.GameAttribute.NPC_Is_Operatable] = status;
            }
            return true;
        }

        private bool LaunchWave(List<Vector3D> Coordinates, Map.World world, List<Int32> SnoId)
        {
            for (Int32 i = 0; i < SnoId.Count; i++)
            {
                var monsterSNOHandle = new Common.Types.SNO.SNOHandle(SnoId[i]);
                var monsterActor = monsterSNOHandle.Target as Mooege.Common.MPQ.FileFormats.Actor;

                Parallel.ForEach(world.Players, player => //Threading because many spawns at once with out Parallel freezes D3.
                {
                    var PRTransform = new PRTransform()
                    {
                        Quaternion = new Quaternion()
                        {
                            W = 0.590017f,
                            Vector3D = new Vector3D(0, 0, 0)
                        },
                        Vector3D = Coordinates[i]
                    };

                    //Load the actor here.
                    var actor = WorldGenerator.loadActor(monsterSNOHandle, PRTransform, world, monsterActor.TagMap);
                    monstersAlive.Add(actor);

                    //If Revealed play animation.
                    world.BroadcastIfRevealed(new PlayAnimationMessage
                    {
                        ActorID = actor,
                        Field1 = 9,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                        {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 0x00000048,
                                AnimationSNO = 0x00029A08,
                                PermutationIndex = 0x00000000,
                                Speed = 1f
                            }
                        }
                    }, player.Value);
                });
            }
            return true;
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

    }
}