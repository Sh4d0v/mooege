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
//using Mooege.Common.MPQ.FileFormats;
using Mooege.Net.GS.Message.Definitions.ACD;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Generators;
using Mooege.Common.Logging;
using System.Threading.Tasks;
using System.Threading;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Net.GS.Message;
using MonsterFF = Mooege.Common.MPQ.FileFormats.Monster;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _198503 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();

        public _198503()
            : base(198503)
        {
        }

        static int wretchedMotherAID = 219725;
        private static ThreadLocal<Random> _threadRand = new ThreadLocal<Random>(() => new Random());
        public static Random Rand { get { return _threadRand.Value; } }
        static int wretchedMotherQueenAID = 176889;
        static int portalAID = 192164; // 176007
        static int bonusTaskID = 1; // this is the specific ID to be send to client for updates regarding this very FUCKIN bonus objective (like how hard it was to find..doh')


        List<uint> monstersAlive1 = new List<uint> { }; //We use this for the killeventlistener.
        List<uint> monstersAlive2 = new List<uint> { }; //We use this for the killeventlistener.
        List<uint> monstersAliveBonus = new List<uint> { }; //We use this for the killeventlistener.

        public override void Execute(Map.World world)
        {
            //The spawning positions for each monster in its wave. Basically, you add here the "number" of mobs, accoring to each vector LaunchWave() will spawn every mob in its position.
            //Vector3D[] WretchedMotherSpawn = { new Vector3D(2766.513f, 2913.982f, 24.04533f) };

            //Somehow shes already spawned when the Inn event finishes.. so we search for the ID and add it to the kill event listener.
            var actor = world.GetActorBySNO(wretchedMotherAID);
            if (actor == null)
            {
                Logger.Debug("Could not find the Wretched Mother ACTOR ID {0}", wretchedMotherAID);
                Vector3D FirstMother = new Vector3D(2766.513f, 2913.982f, 24.04533f);
                world.SpawnMonster(wretchedMotherAID, FirstMother);
                actor = world.GetActorBySNO(wretchedMotherAID);
                monstersAlive1.Add(actor.DynamicID);
            }
            else
            {
                monstersAlive1.Add(actor.DynamicID);
            }

            //Убираем телегу
            var FactorToShoot = world.GetActorBySNO(81699);
            try
            {
                world.Leave(FactorToShoot);
            }
            catch { }

            //FactorToShoot.Destroy();

            //Запуск отслеживания убийства
            var ListenerWretchedMother = Task<bool>.Factory.StartNew(() => OnKillListener(monstersAlive1, world));
            //Ждём пока убьют
                ListenerWretchedMother.ContinueWith(delegate
                {
                    // WMQ + 3 WM (group)
                    // portal + rumford (group)
                    // WMQ + 3 WM (group)

                    // force next quest step 
                    world.Game.Quests.Advance(87700);
                    Logger.Debug("Event finished");
                    // launch dialog
                    StartConversation(world, 156223);

                    // position of the wretched mother
                    Vector3D[] WretchedMotherPosSpawn = new Vector3D[3]; // too hard 3 elems..
                    WretchedMotherPosSpawn[0] = new Vector3D(2427.788f, 2852.193f, 27.1f);
                    WretchedMotherPosSpawn[1] = new Vector3D(2356.931f, 2528.715f, 27.1f);
                    WretchedMotherPosSpawn[2] = new Vector3D(2119.563f, 2489.693f, 27.1f);

                    // spawn 3 wretched mother
                    Logger.Debug(" spawn 1  Wretched Mother ");
                    world.SpawnMonster(wretchedMotherAID, WretchedMotherPosSpawn[0]);
                    Logger.Debug(" spawn 1  Wretched Mother ");
                    world.SpawnMonster(wretchedMotherAID, WretchedMotherPosSpawn[1]);
                    Logger.Debug(" spawn 1  Wretched Mother ");
                    world.SpawnMonster(wretchedMotherAID, WretchedMotherPosSpawn[2]);

                    // ugly hack to get all actors with the same snoID..no idea if it is lmegit or if game will crash and summon diablo on my pc...
                    var actorsWM = world.GetActorsBySNO(wretchedMotherAID); // this is the List of wretched mother ACTOR ID
                    var actorWQM = world.GetActorBySNO(wretchedMotherQueenAID); // this is the wretched queen mother ACTOR ID
                    
                    Logger.Debug(" world contains {0} WM ", actorsWM.Count);

                    if (actorsWM.Count > 0)
                    {
                        monstersAliveBonus.Add(actorsWM.ElementAt(0).DynamicID); monstersAliveBonus.Add(actorsWM.ElementAt(0).DynamicID); monstersAliveBonus.Add(actorsWM.ElementAt(0).DynamicID);
                        // run killbonus event listener 
                        var ListenerWQTask = Task<bool>.Factory.StartNew(() => OnKillBonusListener(monstersAliveBonus, world, bonusTaskID));
                        //Wait for wretched queen mother to be killed.
                        ListenerWQTask.ContinueWith(delegate //Once killed:
                        {
                            Logger.Debug("Bonus Event Completed ");
                        });
                    }
                    else
                    {
                        Logger.Debug("Could not get/spawn the Wretched Mother ACTOR ID {0}", wretchedMotherAID);
                    }
                    if (actorWQM != null)
                    {
                        // Пытаемся привязать статус босса!
                        actorWQM.Attributes[Net.GS.Message.GameAttribute.Using_Bossbar] = true;
                        actorWQM.Attributes[Net.GS.Message.GameAttribute.InBossEncounter] = true;
                        //actorWQM.Attributes[Net.GS.Message.GameAttribute.Si]
                        // DOES NOT WORK it should be champion affixes or shit of this kind ...
                        // Увеличиваем здоровье босса!
                        //actorWQM.Q
                        actorWQM.Attributes[GameAttribute.Hitpoints_Max] = 150f;
                        actorWQM.Attributes[GameAttribute.Hitpoints_Cur] = 150f;

                        //Запуск отслеживания убийства королевы
                        var ListenerWQMTask = Task<bool>.Factory.StartNew(() => OnWMQKillListener(actorWQM.DynamicID, world));
                        //Wait for wretched queen mother to be killed.
                        ListenerWQMTask.ContinueWith(delegate //Once killed:
                        {
                            Logger.Debug(" Wretch Queen Event done !!"); // WretchedQueenIsDead                             

                            // portal shit 
                            var portalActorId = world.GetActorBySNO(portalAID);
                            var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnUseTeleporterListener(portalActorId.DynamicID, world));
                            //Wait for portal to be used .
                            ListenerUsePortalTask.ContinueWith(delegate //Once killed:
                            {
                                Logger.Debug(" Waypoint_OldTristram Objective done "); // Waypoint_OldTristram
                            });
                            //conversation with rumford... delegated to another class... since we have a nice conversation system :p                            
                        });
                    }
                    else
                    {
                        Logger.Debug("Could not find the Wretched Mother QUEEN ACTOR ID {0}", wretchedMotherQueenAID);
                    }
                });
            
        }

        //This is the way we Listen for mob killing events.
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

        //This is so ugly but I don't knwo how to put few mob with same actor ID in a "group"...
        private bool OnKillBonusListener(List<uint> monstersAlive, Map.World world, int taskID)
        {
            var monsterCount = monstersAlive.Count; //Since we are removing values while iterating, this is set at the first real read of the mob counting.
            Logger.Debug(" dead to be counted {0} world contains {1} WM ", monsterCount, world.GetActorsBySNO(wretchedMotherAID).Count);
            while (true)
            {
                if (world.GetActorsBySNO(wretchedMotherAID).Count < monsterCount)
                {
                    //If dead we count one less and send the update for the bonus stuff :p
                    Logger.Debug( "A wretched mother has been killed");
                    monsterCount--;
                    world.Game.Quests.NotifyBonus(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.BonusStep, bonusTaskID);
                    if (monsterCount == 0)
                        break;
                }
            }
            return true;
        }

        //just for the WMQ 
        private bool OnWMQKillListener(uint monsterDynID, Map.World world)
        {
            while (true)
            {
                if (world.HasMonster(monsterDynID))
                {
                    //RAS 
                }
                else {
                    Logger.Debug(" The wretched mother queen has been killed");

                    world.Game.Quests.NotifyQuest(87700, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1);
                    foreach (var player in world.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                        dbQuestProgress.ActiveQuest = 87700;
                        dbQuestProgress.StepOfQuest = 8;
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    };
                    //Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.KillMonster, 1);
                    break;
                }
            }
            return true;
        }

        //just for the use of the portal
        private bool OnUseTeleporterListener(uint actorDynID, Map.World world)
        {
            if (world.HasActor(actorDynID))
            {
                var actor = world.GetActorByDynamicId(actorDynID); // it is not null :p



                Logger.Debug(" supposed portal has type {3} has name {0} and state {1} , has gizmo  been operated ? {2} ", actor.NameSNOId, actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_State], actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated], actor.GetType());

                while (true)
                {
                    if (actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated])
                    {
                        world.Game.Quests.NotifyQuest(87700, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.InteractWithActor, portalAID);
                        break;
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