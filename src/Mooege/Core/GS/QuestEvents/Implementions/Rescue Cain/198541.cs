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
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.GS.Actors.Implementations.Hirelings;
using Mooege.Core.GS.Actors.Implementations.Minions;
using Mooege.Core.GS.Powers.Implementations;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.AI.Brains;
//
namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _198541 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();

        public _198541()
            : base(198541)
        {
        }
        private Boolean HadConversation = true;
        private int portalAID;
        private Players.Player MasterPlayer;
        private static ThreadLocal<Random> _threadRand = new ThreadLocal<Random>(() => new Random());
        public static Random Rand { get { return _threadRand.Value; } }
        List<Vector3D> monstersAlive = new List<Vector3D> { };

        public override void Execute(Map.World world)
        {
            // test kill leah
            if (world.HasActor(72))
            {
                world.GetActorByDynamicId(72).Destroy();
            }

            var Gate = world.GetActorBySNO(108466);

            var NoGate = new Actors.Implementations.Door(world, 108466, world.GetActorBySNO(108466).Tags);

            NoGate.Field2 = 16;
            NoGate.RotationAxis = Gate.RotationAxis;
            NoGate.RotationW = Gate.RotationW;
            NoGate.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
            //NoGate.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
            NoGate.Attributes[GameAttribute.Gizmo_State] = 1;
            NoGate.Attributes[GameAttribute.Untargetable] = true;
            NoGate.Attributes.BroadcastChangedIfRevealed();
            NoGate.EnterWorld(Gate.Position);
            Gate.Destroy();

            world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
            {
                ActorID = NoGate.DynamicID,
                Field1 = 5,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
            {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 50,
                                AnimationSNO = NoGate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 1
                            }
            }
            }, NoGate);

            world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
            {
                ActorID = NoGate.DynamicID,
                AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID
            }, NoGate);


            //Get Leah
            var LeahBrains = world.GetActorByDynamicId(83);

            //Get Portal
            var NewTristramPortal = world.GetActorByDynamicId(34);
            portalAID = NewTristramPortal.ActorSNO.Id;
            if (HadConversation)
            {
                HadConversation = false;
                Logger.Debug(" RESCUE CAIN QUEST STARTED ");
                Logger.Debug(" Quests.Advance(72095) ");
                world.Game.Quests.NotifyQuest(72095, QuestStepObjectiveType.EventReceived, -1);
                //world.Game.Quests.HasCurrentQuest(72095, -1);
                //    world.Game.Quests.Advance(72095);
            }
            // Away Leah
            try { world.Leave(LeahBrains); }
            catch { }
            
            //LeahBrains.OnLeave(world);
            // Create Friend Leah for Party
            Hireling LeahFriend = new Hireling(world, LeahBrains.ActorSNO.Id, LeahBrains.Tags);
            LeahFriend.Brain = new MinionBrain(LeahFriend);

            
            // Point to spawn Leah
            var NewPoint = new Vector3D(LeahBrains.Position.X, LeahBrains.Position.Y + 5, LeahBrains.Position.Z);
            //LeahBrains.EnterWorld(NewPoint);
            foreach (var player in world.Players)
            {
                if (player.Value.PlayerIndex == 0)
                {
                    LeahFriend.GBHandle.Type = 4;
                    LeahFriend.GBHandle.GBID = 717705071;
                    LeahFriend.Attributes[GameAttribute.Pet_Creator] = player.Value.PlayerIndex;
                    LeahFriend.Attributes[GameAttribute.Pet_Type] = 0x8;
                    LeahFriend.Attributes[GameAttribute.Hitpoints_Max] = 100f;
                    LeahFriend.Attributes[GameAttribute.Hitpoints_Cur] = 80f;
                    LeahFriend.Attributes[GameAttribute.Attacks_Per_Second] = 1.6f;
                    LeahFriend.Attributes[GameAttribute.Pet_Owner] = player.Value.PlayerIndex;
                    LeahFriend.Attributes[GameAttribute.Untargetable] = false;
                    LeahFriend.Position = RandomDirection(player.Value.Position, 3f, 8f);
                    LeahFriend.RotationW = LeahBrains.RotationW;
                    LeahFriend.RotationAxis = LeahBrains.RotationAxis;
                    LeahFriend.EnterWorld(NewPoint);
                    LeahFriend.Attributes[GameAttribute.Level]++;
                    player.Value.ActiveHireling = LeahFriend;
                    player.Value.SelectedNPC = null;
                    LeahFriend.Brain.Activate();
                    MasterPlayer = player.Value;
                }

                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);

                dbQuestProgress.ActiveQuest = 72095;
                dbQuestProgress.StepOfQuest = 1;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }


            LeahBrains.OnLeave(world);
            var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnUseTeleporterListener(NewTristramPortal.DynamicID, world));
            //Wait for portal to be used .
            ListenerUsePortalTask.ContinueWith(delegate //Once killed:
            {
                Logger.Debug(" Waypoint_NewTristram Objective done "); // Waypoint_NewTristram

            });
            
            var ListenerEnterToOldTristram = Task<bool>.Factory.StartNew(() => OnListenerToEnter(MasterPlayer, world));
            ListenerEnterToOldTristram.ContinueWith(delegate //Once killed:
            {
                Logger.Debug("Enter to Road Objective done "); 
                var ListenerEnterToAdriaEnter = Task<bool>.Factory.StartNew(() => OnListenerToAndriaEnter(MasterPlayer, world));
                ListenerEnterToAdriaEnter.ContinueWith(delegate //Once killed:
                {
                    Logger.Debug("Enter to Adria Objective done ");
                    world.Game.Quests.Advance(72095);
                });
            });
        }

        //just for the use of the portal
        private bool OnUseTeleporterListener(uint actorDynID, Map.World world)
        {
            if (world.HasActor(actorDynID))
            {
                var actor = world.GetActorByDynamicId(actorDynID); // it is not null :p

                while (true)
                {
                    if (actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated])
                    {
                        //world.Game.Quests.Advance(72095);
                        world.Game.Quests.NotifyQuest(72095, QuestStepObjectiveType.InteractWithActor, -1);
                        foreach (var player in world.Players)
                        {
                            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                            dbQuestProgress.ActiveQuest = 72095;
                            dbQuestProgress.StepOfQuest = 2;
                            DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                            DBSessions.AccountSession.Flush();
                        }
                        break;
                    }
                }
            }
            return true;
        }

        private bool OnListenerToEnter(Players.Player player, Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                sceneID = player.CurrentScene.SceneSNO.Id;
                if (sceneID == 90196)
                {
                    bool ActivePortal = true;

                    foreach (var playerN in world.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                        if (dbQuestProgress.StepOfQuest == 1)
                            ActivePortal = false;
                        dbQuestProgress.ActiveQuest = 72095;
                        dbQuestProgress.StepOfQuest = 3;
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }
                    if (ActivePortal == true)
                        StartConversation(world, 166678);
                    else { world.Game.Quests.Advance(72095); StartConversation(world, 166678); }
                    try
                    {
                        

                        if (player.ActiveHireling != null)
                        {
                            var HirelingToLeave = player.ActiveHireling;
                            world.Leave(HirelingToLeave);
                            Vector3D OutDoor = new Vector3D(1896.382f, 2782.988f, 32.85f);
                            Vector3D NearDoor = new Vector3D(1935.697f, 2792.971f, 40.23627f);
                            var Leah_Back = world.GetActorByDynamicId(83);
                            Leah_Back.EnterWorld(OutDoor);
                        }

                    }

                    catch { }
                    break;
                }
            }
        
            return true;
        }
        private bool OnListenerToAndriaEnter(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
             while (true)
            {
                try
                {
                    if (player.World.WorldSNO.Id == 71150)
                    {
                        sceneID = player.CurrentScene.SceneSNO.Id;
                        if (sceneID == 90293)
                        {
                            foreach (var playerN in world.Players)
                            {
                                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                                dbQuestProgress.ActiveQuest = 72095;
                                dbQuestProgress.StepOfQuest = 5;
                                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                                DBSessions.AccountSession.Flush();
                            }
                            world.Game.Quests.Advance(72095);
                            break;
                        }
                    }
                }
                catch { }
            }

            return true;
        }
        public Vector3D RandomDirection(Vector3D position, float minRadius, float maxRadius)
        {
            float angle = (float)(Rand.NextDouble() * Math.PI * 2);
            float radius = minRadius + (float)Rand.NextDouble() * (maxRadius - minRadius);
            return new Vector3D(position.X + (float)Math.Cos(angle) * radius,
                                position.Y + (float)Math.Sin(angle) * radius,
                                position.Z);
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