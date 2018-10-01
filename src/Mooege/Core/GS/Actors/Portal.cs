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

using Mooege.Common.Helpers.Hash;
using Mooege.Common.Logging;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Players;
using Mooege.Net.GS.Message.Definitions.Misc;
using Mooege.Net.GS.Message.Definitions.World;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message.Definitions.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using System.Collections.Generic;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.GS.Common.Types.Math;
using System.Threading.Tasks;
using Mooege.Core.GS.Generators;

namespace Mooege.Core.GS.Actors
{
    public class Portal : Actor
    {
        static readonly Logger Logger = LogManager.CreateLogger();

        public override ActorType ActorType { get { return ActorType.Gizmo; } }
        private ResolvedPortalDestination Destination { get; set; }
        private int MinimapIcon;

        public Portal(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {


            try
            {
                this.Destination = new ResolvedPortalDestination
                {
                    WorldSNO = tags[MarkerKeys.DestinationWorld].Id,
                    DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                    StartingPointActorTag = tags[MarkerKeys.DestinationActorTag]
                };

                // Override minimap icon in merkerset tags
                if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                {
                    MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                }
                else
                {
                    MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                }

            }
            catch (KeyNotFoundException)
            {
                if (this.ActorSNO.Id == 168932)
                {
                    var dest = world.Game.GetWorld(60713);

                    this.Destination = new ResolvedPortalDestination
                    {
                        
                        WorldSNO = 60713,//tags[MarkerKeys.DestinationWorld].Id,
                        DestLevelAreaSNO = 60885,//tags[MarkerKeys.DestinationLevelArea].Id,
                        StartingPointActorTag = -102
                    };
                    // Override minimap icon in merkerset tags
                    if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                    {
                        MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                    }
                    else
                    {
                        MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                    }
                    
                    
                    Logger.Warn("Portal {0} forced", this.ActorSNO.Id);
                }
                else if (this.ActorSNO.Id == 221031)
                {
                    try
                    {
                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = tags[MarkerKeys.DestinationWorld].Id,
                            DestLevelAreaSNO = tags[MarkerKeys.DestinationLevelArea].Id,
                            StartingPointActorTag = tags[MarkerKeys.DestinationActorTag]

                        };
                    }
                    catch
                    {
                        this.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = 71150,
                            DestLevelAreaSNO = 60885,
                            StartingPointActorTag = -103

                        };
                    }
                   
                    // Override minimap icon in merkerset tags
                    if (tags.ContainsKey(MarkerKeys.MinimapTexture))
                    {
                        MinimapIcon = tags[MarkerKeys.MinimapTexture].Id;
                    }
                    else
                    {
                        MinimapIcon = ActorData.TagMap[ActorKeys.MinimapMarker].Id;
                    }

                    Logger.Warn("Portal {0} forced", this.ActorSNO.Id);
                }
                else if (this.ActorSNO.Id == 5648)
                {
                    //Generate Portal
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 60713,
                        DestLevelAreaSNO = 33357,
                        StartingPointActorTag = -100
                    };

                    Logger.Warn("Portal to Home {0} created", this.ActorSNO.Id);
                }else if(this.ActorSNO.Id == 176001)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 30021,
                        DestLevelAreaSNO = 33357,
                        StartingPointActorTag = -105
                    };
                    Logger.Warn("Portal {0} forced", this.ActorSNO.Id);
                }
                else
                {
                    Logger.Warn("Portal {0} has incomplete definition", this.ActorSNO.Id);
                }
                
                
            }
            this.Field2 = 16;

            // FIXME: Hardcoded crap; probably don't need to set most of these. /komiga
            this.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
            //this.Attributes[GameAttribute.Hitpoints_Max_Total] = 1f;
            //this.Attributes[GameAttribute.Hitpoints_Max] = 0.0009994507f;
            //this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = 3.051758E-05f;
            //this.Attributes[GameAttribute.Hitpoints_Cur] = 0.0009994507f;
            //this.Attributes[GameAttribute.Level] = 1;

            // EREKOSE STUFF
            //Logger.Debug(" (Portal ctor) position is {0}", this._position);
            //Logger.Debug(" (Portal ctor) quest range is {0}", this._questRange);
            // Logger.Debug(" (Portal ctor) is in scene SNO {0}", this.CurrentScene.SceneSNO);            
            //Logger.Debug(" (Portal Ctor) portal used has actor SNO {3}, SNO Name {0}, exists in world sno {1}, has dest world sno {2}", this.ActorSNO.Name, tags[MarkerKeys.DestinationWorld].Id, tags[MarkerKeys.DestinationWorld].Id, snoId);

        }
        private bool OnKillListenerCain(List<uint> monstersAlive, Core.GS.Map.World world)
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
        private bool OnKillListenerDeadly(List<uint> monstersAlive, Core.GS.Map.World world)
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
        private bool OnKillListenerBossSmithWife(List<uint> monstersAlive, Core.GS.Map.World world)
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
        private bool OnListenerToEnterGraveyard(Players.Player player, Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                if (player.World.WorldSNO.Id == 71150)
                {
                    sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 74614) //trOut_wilderness_MainGraveyard_E02_S03
                    {
                        bool ActivePortal = true;

                        foreach (var playerN in world.Players)
                        {
                            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                            if (dbQuestProgress.StepOfQuest == 6)
                            {
                                ActivePortal = true;
                                dbQuestProgress.ActiveQuest = 72221;
                                dbQuestProgress.StepOfQuest = 7;
                            }
                            else
                            { ActivePortal = false; }
                            DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                            DBSessions.AccountSession.Flush();
                        }

                        if (ActivePortal == true)
                            world.Game.Quests.Advance(72221);
                        break;
                    }
                }
            }

            return true;
        }
        private bool OnListenerToEnterBossScene(Players.Player player, Map.World world)
        {
            while (true)
            {
                //72637 World
                try
                {
                    int sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 61126) //trOut_wilderness_MainGraveyard_E02_S03
                    {
                        bool ActivePortal = true;

                        foreach (var playerN in world.Players)
                        {
                            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                            if (dbQuestProgress.StepOfQuest == 7)
                            {
                                ActivePortal = true;
                                dbQuestProgress.ActiveQuest = 72221;
                                dbQuestProgress.StepOfQuest = 8;
                            }
                            else
                            { ActivePortal = false; }
                            DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                            DBSessions.AccountSession.Flush();
                        }

                        if (ActivePortal == true)
                            world.Game.Quests.Advance(72221);
                        break;
                    }
                }
                catch { }
            }

            return true;
        }
        private bool OnKillKingSkeletonsListener(List<uint> monstersAlive, Map.World world)
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
        private bool OnKillListenerBossEamon(List<uint> monstersAlive, Core.GS.Map.World world)
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
        public static bool setActorOperable(Map.World world, System.Int32 snoId, bool status)
        {
            var actor = world.GetActorBySNO(snoId);
            foreach (var player in world.Players)
            {
                actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated] = status;
            }
            return true;
        }
        
        public override bool Reveal(Player player)
        {
            //Logger.Debug(" (Reveal) portal {0} has location {1}", this.ActorSNO, this._position);


            if (!base.Reveal(player) || this.Destination == null)
                return false;

            player.InGameClient.SendMessage(new PortalSpecifierMessage()
            {
                ActorID = this.DynamicID,
                Destination = this.Destination
            });

            // Show a minimap icon
            Mooege.Common.MPQ.Asset asset;
            string markerName = "";

            if (Mooege.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.LevelArea].TryGetValue(this.Destination.DestLevelAreaSNO, out asset))
                markerName = System.IO.Path.GetFileNameWithoutExtension(asset.FileName);

            player.InGameClient.SendMessage(new MapMarkerInfoMessage()
            {
                Field0 = (int)World.NewSceneID,    // TODO What is the correct id space for mapmarkers?
                Field1 = new WorldPlace()
                {
                    Position = this.Position,
                    WorldID = this.World.DynamicID
                },
                Field2 =  0x00018FB0,  /* Marker_DungeonEntrance.tex */          // TODO Dont mark all portals as dungeon entrances... some may be exits too (although d3 does not necesarrily use the correct markers). Also i have found no hacky way to determine whether a portal is entrance or exit - farmy
                m_snoStringList = 0x0000CB2E, /* LevelAreaNames.stl */          // TODO Dont use hardcoded numbers

                Field3 = StringHashHelper.HashNormal(markerName),
                Field9 = 0,
                Field10 = 0,
                Field11 = 0,
                Field5 = 0,
                Field6 = true,
                Field7 = false,
                Field8 = true,
                Field12 = 0
            });
            return true;
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
            Logger.Debug("(OnTargeted) Portal has been activated ");
            
            var world = this.World.Game.GetWorld(this.Destination.WorldSNO);
            var now_world = player.World;
            
            if(this.Destination.WorldSNO == 71150)
            {
                if (this.ActorSNO.Id == 241660)
                {
                    Vector3D Point = new Vector3D(2867.382f, 2398.66f, 1.813717f);
                    player.Teleport(Point);
                    //OnListenerToEnterGraveyard
                    var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                    if (dbQuestProgress.StepOfQuest == 6)
                    {
                        var ListenerEnterToGraveyard = Task<bool>.Factory.StartNew(() => OnListenerToEnterGraveyard(player, world));
                        ListenerEnterToGraveyard.ContinueWith(delegate
                        {
                            Logger.Debug("Enter to Road Objective done ");

                        });
                    }
                    DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                    DBSessions.AccountSession.Flush();

                }
                if (this.ActorSNO.Id == 241661)
                {
                    Vector3D Point = new Vector3D(2870.336f,2498.836f,3.700585f);
                    player.Teleport(Point);

                }
            }

            if (this.Destination.WorldSNO == 72637)
            {
                // Вход в Оскверненный склеп
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72221)
                {
                    if (dbQuestProgress.StepOfQuest == 7)
                    {
                        var Crypto = world.Game.GetWorld(72637);
                        var ListenerEnterToBossZone = Task<bool>.Factory.StartNew(() => OnListenerToEnterBossScene(player, Crypto));
                        ListenerEnterToBossZone.ContinueWith(delegate
                        {
                            Logger.Debug("Founding Zone Objective done ");
                            //156381 - Chancellor Eamon 
                            var Summoner = world.GetActorBySNO(5387);
                            Crypto.SpawnMonster(156353, Summoner.Position);
                            var ChancellorEamon = world.GetActorBySNO(156353);
                            ChancellorEamon.Attributes[Net.GS.Message.GameAttribute.Using_Bossbar] = true;
                            ChancellorEamon.Attributes[Net.GS.Message.GameAttribute.InBossEncounter] = true;
                            // DOES NOT WORK it should be champion affixes or shit of this kind ...
                            // Увеличиваем здоровье босса!
                            ChancellorEamon.Attributes[Net.GS.Message.GameAttribute.Hitpoints_Max] = 200f;
                            ChancellorEamon.Attributes[Net.GS.Message.GameAttribute.Hitpoints_Cur] = 200f;
                            ChancellorEamon.Attributes[Net.GS.Message.GameAttribute.Movement_Scalar_Reduction_Percent] -= 10f;
                            ChancellorEamon.Attributes[Net.GS.Message.GameAttribute.Quest_Monster] = true;
                            List<uint> Boss = new List<uint> { };
                            Boss.Add(ChancellorEamon.DynamicID);
                            var BossKillEvent = Task<bool>.Factory.StartNew(() => OnKillListenerBossEamon(Boss, Crypto));
                            BossKillEvent.ContinueWith(delegate
                            {
                                dbQuestProgress.StepOfQuest = 9;
                                world.Game.Quests.Advance(72221);
                            });
                        });
                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }

            if (this.Destination.WorldSNO == 62751)
            {
                //Enter в Adrian's Hut
                bool QuestEnter = false;

                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72095)
                {
                    dbQuestProgress.ActiveQuest = 72095;
                    if (dbQuestProgress.StepOfQuest == 5)
                    {
                        dbQuestProgress.StepOfQuest = 6;
                        QuestEnter = true;
                    }

                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();

                if (QuestEnter == true)
                {
                    world.Game.Quests.Advance(72095);
                    QuestEnter = false;
                }
            }

            if (this.Destination.WorldSNO == 50579)
            {
                //Enter в Cathedral level 1
                bool QuestEnter = false;

                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72095)
                {
                    dbQuestProgress.ActiveQuest = 72095;
                    if (dbQuestProgress.StepOfQuest == 10)
                    {
                        dbQuestProgress.StepOfQuest = 11;
                        QuestEnter = true;
                    }

                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();

                if (QuestEnter == true)
                {
                    world.Game.Quests.Advance(72095);
                    QuestEnter = false;

                }
                
                var FakePortal = world.GetActorBySNO(168932);
                
                Portal RealPortal = new Portal(world.Game.GetWorld(50579), 5648, world.Game.GetWorld(60713).StartingPoints[0].Tags);
                Vector3D RealPosition = new Vector3D(643.384f, 339.6074f, -6.970252f);
                RealPortal.Destination = new ResolvedPortalDestination
                {
                    WorldSNO = 60713,
                    DestLevelAreaSNO = 60885,
                    StartingPointActorTag = -102
                };
                RealPortal.EnterWorld(FakePortal.Position);
                
            }

            if (this.Destination.WorldSNO == 136441)
            {
                //Входим в погреб проклятых
                var dest = world.Game.GetWorld(136441);
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72221)
                {
                    if (dbQuestProgress.StepOfQuest == 2)
                    {
                        //98888 - BlacksmithWife
                        //85900 - Mira Eamon Monster
                        //174023,204605,204615,204607,204608,204606,204616,174023,204605
                        //6646 - Ravenous Dead 
                        List<uint> Deadly = new List<uint> { };
                        List<uint> Boss = new List<uint> { };
                        #region Собираем всех!
                        var Preminions1 = dest.GetActorsBySNO(174023);
                        var Preminions2 = dest.GetActorsBySNO(204605);
                        var Preminions3 = dest.GetActorsBySNO(204615);
                        var Preminions4 = dest.GetActorsBySNO(204607);
                        var Preminions5 = dest.GetActorsBySNO(204608);
                        var Preminions6 = dest.GetActorsBySNO(204606);
                        var Preminions7 = dest.GetActorsBySNO(204616);
                        var Preminions8 = dest.GetActorsBySNO(174023);
                        var Preminions9 = dest.GetActorsBySNO(204605);
                        var PreBoss = dest.GetActorBySNO(98888);
                        #endregion
                        #region Убираем мирных и заменяем их злыми)
                        foreach (var minion in Preminions1)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        foreach (var minion in Preminions2)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        foreach (var minion in Preminions3)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        foreach (var minion in Preminions4)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        foreach (var minion in Preminions5)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        foreach (var minion in Preminions6)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        foreach (var minion in Preminions7)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        foreach (var minion in Preminions8)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        foreach (var minion in Preminions9)
                        {
                            dest.SpawnMonster(6646, minion.Position);
                            dest.Leave(minion);
                        }
                        #endregion
                        #region Заполняем массив монстрами
                        var Minions = dest.GetActorsBySNO(6646);
                        foreach (var Minion in Minions)
                        {
                            Deadly.Add(Minion.DynamicID);
                        }
                        #endregion

                        var DeadlyKillEvent = Task<bool>.Factory.StartNew(() => OnKillListenerDeadly(Deadly, dest));
                        DeadlyKillEvent.ContinueWith(delegate
                        {
                            if (dbQuestProgress.StepOfQuest == 2)
                            {
                                world.Game.Quests.Advance(72221);
                                dest.SpawnMonster(85900, PreBoss.Position);
                                dest.Leave(PreBoss);
                                var BOSSMira = dest.GetActorBySNO(85900);
                                // Пытаемся привязать статус босса!
                                BOSSMira.Attributes[Net.GS.Message.GameAttribute.Using_Bossbar] = true;
                                BOSSMira.Attributes[Net.GS.Message.GameAttribute.InBossEncounter] = true;
                                // DOES NOT WORK it should be champion affixes or shit of this kind ...
                                // Увеличиваем здоровье босса!
                                BOSSMira.Attributes[Net.GS.Message.GameAttribute.Hitpoints_Max] = 200f;
                                BOSSMira.Attributes[Net.GS.Message.GameAttribute.Hitpoints_Cur] = 200f;
                                BOSSMira.Attributes[Net.GS.Message.GameAttribute.Movement_Scalar_Reduction_Percent] -= 10f;
                                BOSSMira.Attributes[Net.GS.Message.GameAttribute.Quest_Monster] = true;
                                Boss.Add(BOSSMira.DynamicID);
                                var BossSmithWifeKillEvent = Task<bool>.Factory.StartNew(() => OnKillListenerBossSmithWife(Boss, dest));

                                BossSmithWifeKillEvent.ContinueWith(delegate
                                {
                                    world.Game.Quests.Advance(72221);
                                    dbQuestProgress.StepOfQuest = 4;
                                });
                            }
                        });
                    }
                    
                    

                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }
            if (this.Destination.WorldSNO == 60395)
            {
                //Enter to Drowned Temple
                Vector3D Point = new Vector3D(0f, 0f, 0.2f);
                if (world.Game.GetWorld(60395).StartingPoints.Count == 0)
                    player.ChangeWorld(player.World.Game.GetWorld(60395), Point);
            }

            if (this.Destination.WorldSNO == 60713)
            {
                //Enter to Leoric Hall
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if(dbQuestProgress.StepOfQuest > 12 || dbQuestProgress.ActiveQuest != 72095)
                {
                    var BookShelf = world.GetActorBySNO(5723);
                    world.BroadcastIfRevealed(new Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                    {
                        ActorID = BookShelf.DynamicID,
                        AnimationSNO = AnimationSetKeys.Open.ID
                    }, BookShelf);
                    var minions = world.GetActorsBySNO(80652);
                    foreach (var minion in minions)
                    {
                        minion.Destroy();
                    }
                }
                try
                {
                    if (dbQuestProgress.ActiveQuest == 72095)
                    {
                        dbQuestProgress.ActiveQuest = 72095;
                        if (dbQuestProgress.StepOfQuest == 11)
                        {
                            dbQuestProgress.StepOfQuest = 12;
                            var minions = world.GetActorsBySNO(80652);
                            var CainBrains = world.GetActorBySNO(102386);
                            Vector3D FirstPoint = new Vector3D(112.1694f, 166.0996f, 0.09996167f);
                            Vector3D SecondPoint = new Vector3D(120.07f, 174.9657f, 0.1114834f);
                            Vector3D ThirdPoint = new Vector3D(111.3691f, 182.6697f, 5.229973f);
                            minions[0].Teleport(FirstPoint);
                            minions[1].Teleport(FirstPoint);
                            minions[2].Teleport(FirstPoint);

                        }                        
                    }
                    if(dbQuestProgress.ActiveQuest == 72061)
                    {
                        if (dbQuestProgress.StepOfQuest == 2)
                        {
                            dbQuestProgress.StepOfQuest = 3;
                            world.Game.Quests.Advance(72061);
                           
                        }
                        if (dbQuestProgress.StepOfQuest <= 3)
                        {
                            Vector3D SpawnPortalPosition = new Vector3D(28.91075f, 205.9426f, -24.90778f);
                            // 73261 TagMap[ActorKeys.GizmoGroup]
                            Portal RealPortal = new Portal(world.Game.GetWorld(60713), 5648, world.Game.GetWorld(73261).StartingPoints[0].Tags);
                            RealPortal.Destination = new ResolvedPortalDestination
                            {
                                WorldSNO = 60713,
                                DestLevelAreaSNO = 60885,
                                StartingPointActorTag = -105
                            };
                            RealPortal.EnterWorld(SpawnPortalPosition);
                        }
                    }
                }
                catch { }

                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }
            if (this.Destination.WorldSNO == 50582)
            {
                //Enter to Cathedral Level 2
                
                Vector3D Point = new Vector3D(1146.33f, 1539.594f, 0.2f);
                if (world.Game.GetWorld(50582).StartingPoints.Count == 0)
                    player.ChangeWorld(player.World.Game.GetWorld(50582), Point);
            }

            if (world == null)
            {
                Logger.Warn("Portal's destination world does not exist (WorldSNO = {0})", this.Destination.WorldSNO);
                return;
            }



            //Portal to New Tristram
            if (this.Destination.StartingPointActorTag == -100)
            {

                Vector3D ToPortal = new Vector3D(2988.73f, 2798.009f, 24.66344f);
                //Сохраняем в базу координаты для обратного портала.
                var dbPortalOfToon = DBSessions.AccountSession.Get<DBPortalOfToon>(player.Toon.PersistentID);
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                dbPortalOfToon.WorldDest = now_world.WorldSNO.Id;
                dbPortalOfToon.X = this.Position.X;
                dbPortalOfToon.Y = this.Position.Y;
                dbPortalOfToon.Z = this.Position.Z;
                DBSessions.AccountSession.SaveOrUpdate(dbPortalOfToon);

                Logger.Warn("Data for back portal Saved.");

                if (player.World.Game.GetWorld(71150) != player.World)
                {
                    try
                    {
                        if (player.ActiveHireling != null)
                        {
                            var HirelingToLeave = player.ActiveHireling;
                            now_world.Leave(HirelingToLeave);
                        }

                    }
                    catch { }
                    if (dbQuestProgress.ActiveQuest == 72221 && dbQuestProgress.StepOfQuest == 10)
                    { player.World.Game.Quests.NotifyQuest(72221, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1); }
                    player.ChangeWorld(player.World.Game.GetWorld(71150), ToPortal);
                }
                else
                    player.Teleport(ToPortal);

                //Create Back Portal
                var TristramHome = player.World.Game.GetWorld(71150);
                var OldPortal = TristramHome.GetActorsBySNO(5648);
                foreach (var OldP in OldPortal)
                {
                    OldP.Destroy();
                }
                var ToHome = new Portal(player.World.Game.GetWorld(71150), 5648, player.World.Game.GetWorld(71150).StartingPoints[0].Tags);
                ToHome.Destination = new ResolvedPortalDestination
                {
                    WorldSNO = dbPortalOfToon.WorldDest,
                    DestLevelAreaSNO = 172,
                    StartingPointActorTag = -101
                };
                // Название локации, не работает(
                ToHome.NameSNOId = now_world.WorldSNO.Id;
                ToHome.EnterWorld(ToPortal);

                DBSessions.AccountSession.Flush();
            }
            else if (this.Destination.StartingPointActorTag == -101)
            {
                var dbPortalOfToon = DBSessions.AccountSession.Get<DBPortalOfToon>(player.Toon.PersistentID);
                Vector3D ToPortal = new Vector3D(dbPortalOfToon.X, dbPortalOfToon.Y, dbPortalOfToon.Z);
                var DestWorld = player.World.Game.GetWorld(dbPortalOfToon.WorldDest);
                var oldPortals = DestWorld.GetActorsBySNO(5648);

                foreach (var OldP in oldPortals)
                {
                    //        OldP.Destroy();
                }

                if (player.World.Game.GetWorld(dbPortalOfToon.WorldDest) != player.World)

                    player.ChangeWorld(player.World.Game.GetWorld(dbPortalOfToon.WorldDest), ToPortal);
                else
                    player.Teleport(ToPortal);
            }
            else if (this.Destination.StartingPointActorTag == -102)
            {
                var dest = world.Game.GetWorld(60713);
                Vector3D Point = new Vector3D(237.3005f, 200.94f, 31.17498f);

                bool QuestEnter = false;

                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72095)
                {
                    dbQuestProgress.ActiveQuest = 72095;
                    if (dbQuestProgress.StepOfQuest == 12)
                    {
                        dbQuestProgress.StepOfQuest = 12;
                        QuestEnter = true;
                        var minions = dest.GetActorsBySNO(80652);
                        List<uint> SkilletKiller = new List<uint> { };
                        var CainBrains = world.GetActorBySNO(102386);
                        foreach (var minion in minions)
                        {
                            SkilletKiller.Add(minion.DynamicID);
                        }

                        var CainKillerEvent = Task<bool>.Factory.StartNew(() => OnKillListenerCain(SkilletKiller, dest));

                        CainKillerEvent.ContinueWith(delegate
                        {
                            if (dbQuestProgress.StepOfQuest == 12)
                            {
                                world.Game.Quests.Advance(72095);
                                dbQuestProgress.StepOfQuest = 13;
                            }
                        });
                    }
                    else { QuestEnter = false; }

                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();

                if (QuestEnter == true)
                {
                    world.Game.Quests.Advance(72095);
                    QuestEnter = false;

                }



                player.ChangeWorld(player.World.Game.GetWorld(60713), Point);


            }
            else if (this.Destination.StartingPointActorTag == -103)
            {
                Vector3D Point = new Vector3D(1769.139f, 2914.95f, 20.16885f);
                player.ChangeWorld(player.World.Game.GetWorld(71150), Point);

            }
            // Патч выхода из хижины Адрии
            else if (this.Destination.StartingPointActorTag == -104)
            {
                Vector3D Point = new Vector3D(1769.139f, 2914.95f, 20.16885f);
                player.ChangeWorld(player.World.Game.GetWorld(71150), Point);
            }
            else if (this.Destination.StartingPointActorTag == -105)
            {
                var BossWorld = player.World.Game.GetWorld(73261);
                Vector3D Point = new Vector3D(338.9958f, 468.3622f, -3.859601f);
                player.ChangeWorld(BossWorld, Point);
                for (int i = 0; i < 9; i++) { world.Game.Quests.Advance(72061); }
                Vector3D FistPoint = new Vector3D(291.9193f, 428.6796f, 0.1f);
                Vector3D SecondPoint = new Vector3D(270.9105f, 426.223f, 0.1000026f);
                Vector3D ThirdPoint = new Vector3D(241.2828f, 425.616f, 0.1f);
                Vector3D FourPoint = new Vector3D(241.2051f, 435.0545f, 0.1f);
                BossWorld.SpawnMonster(87012, FistPoint);
                BossWorld.SpawnMonster(87012, SecondPoint);
                BossWorld.SpawnMonster(87012, ThirdPoint);
                BossWorld.SpawnMonster(87012, FourPoint);
                var AllSkeletons = BossWorld.GetActorsBySNO(87012);
                List<uint> SkeletonsList = new List<uint> { };
                foreach (var Skelet in AllSkeletons)
                {
                    SkeletonsList.Add(Skelet.DynamicID);
                }
                var ListenerKingSkeletons = Task<bool>.Factory.StartNew(() => OnKillKingSkeletonsListener(SkeletonsList, BossWorld));
                //Ждём пока убьют
                ListenerKingSkeletons.ContinueWith(delegate
                {
                    world.Game.Quests.Advance(72061);
                    //5765 - Gate
                    var SkeletonGate = BossWorld.GetActorBySNO(5765);
                    BossWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                    {
                        ActorID = SkeletonGate.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 100,
                                AnimationSNO = SkeletonGate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 0.5f
                            }
                            }
                    }, SkeletonGate);
                    BossWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                    {
                        ActorID = SkeletonGate.DynamicID,
                        AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                    }, SkeletonGate);
                });

            }
            else
            {
                var startingPoint = world.GetStartingPointById(this.Destination.StartingPointActorTag);

                if (startingPoint != null)
                {
                    try
                    {
                        if (player.ActiveHireling != null)
                        {
                            var HirelingToLeave = player.ActiveHireling;
                            now_world.Leave(HirelingToLeave);
                            var NewTristram = player.InGameClient.Game.GetWorld(71150);
                            var Leah_Back = NewTristram.GetActorByDynamicId(83);
                            Leah_Back.EnterWorld(Leah_Back.Position);
                        }

                    }
                    catch { }

                    player.ChangeWorld(world, startingPoint);
                    //
                }
                else
                    Logger.Warn("Portal's tagged starting point does not exist (Tag = {0})", this.Destination.StartingPointActorTag);
            }

        }
    }
}