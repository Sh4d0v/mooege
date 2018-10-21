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
using Mooege.Core.GS.Ticker;
using Mooege.Core.GS.Common.Types.SNO;
using System.Windows;
using Mooege.Core.GS.Actors.Implementations.Hirelings;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Actors.Implementations;

namespace Mooege.Core.GS.Actors
{
    public class Portal : Actor
    {
        static readonly Logger Logger = LogManager.CreateLogger();

        public override ActorType ActorType { get { return ActorType.Gizmo; } }
        private ResolvedPortalDestination Destination { get; set; }
        public Mooege.Common.MPQ.FileFormats.Scene.NavZoneDef NavZone { get; private set; }
        private int MinimapIcon;


        public Portal(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {


            try
            {
                //Выход к Мяснику
                if (world.WorldSNO.Id == 58983 && this.ActorSNO.Id == 158944)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 78839,
                        DestLevelAreaSNO = 90881,
                        StartingPointActorTag = 172
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
                else
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
            }
            catch (KeyNotFoundException)
            {
                #region Первый этаж Агонии
                //Вход
                if (world.WorldSNO.Id == 2826 && this.ActorSNO.Id == 175999)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 75049,
                        DestLevelAreaSNO = 100854,
                        StartingPointActorTag = 171
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
                //Выход на 2 этаж
                if (world.WorldSNO.Id == 2826 && this.ActorSNO.Id == 175482)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 58982,
                        DestLevelAreaSNO = 19775,
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
                #endregion

                #region Второй этаж Агонии
                //Вход
                if (world.WorldSNO.Id == 58982 && this.ActorSNO.Id == 175999)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 2826,
                        DestLevelAreaSNO = 19774,
                        StartingPointActorTag = 171
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
                #endregion

                #region Переправа в высокогорье
                //Вход
                if (world.WorldSNO.Id == 87707 && this.ActorSNO.Id == 176001)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 58982,
                        DestLevelAreaSNO = 19775,
                        StartingPointActorTag = 171
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
                #endregion

                #region Проклятая крепость
                //Выход на 3 этаж Агонии
                if (world.WorldSNO.Id == 94676 && this.ActorSNO.Id == 175999)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 58983,
                        DestLevelAreaSNO = 19776,
                        StartingPointActorTag = 172
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
                #endregion
                #region Третий этаж Агонии
                //Вход
                if (world.WorldSNO.Id == 58983 && this.ActorSNO.Id == 175999)
                {
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 94676,
                        DestLevelAreaSNO = 94672,
                        StartingPointActorTag = 171
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
                
                #endregion
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
                        WorldSNO = 71150,
                        DestLevelAreaSNO = 19947,
                        StartingPointActorTag = -100
                    };

                    Logger.Warn("Portal to Home {0} created", this.ActorSNO.Id);
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
        private bool OnKillButcherListener(List<uint> monstersAlive, Map.World world)
        {
            System.Int32 monstersKilled = 0;
            bool MidMidle_Active = false;
            var monsterCount = monstersAlive.Count; //Since we are removing values while iterating, this is set at the first real read of the mob counting.
            while (monstersKilled != monsterCount)
            {
                
                /*
                if (MidMidle_Active == false)
                {
                    MidMidle_Active = true;

                    var Panel_MidMiddle_Base = world.GetActorBySNO(201426);

                    if (Panel_MidMiddle_Base == null)
                    {
                        world.SpawnMonster(201426, new Vector3D(120.9595f, 121.6244f, -0.1068707f));
                        Panel_MidMiddle_Base = world.GetActorBySNO(201426);
                    }

                    TickTimer Timeout1 = new SecondsTickTimer(world.Game, 2f);
                    var TimeoutToReady = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout1));
                    TimeoutToReady.ContinueWith(delegate
                    {
                        world.SpawnMonster(201428, Panel_MidMiddle_Base.Position);
                        var Panel_MidMiddle_Ready = world.GetActorBySNO(201428);

                        TickTimer Timeout2 = new SecondsTickTimer(world.Game, 4f);
                        var TimeoutToActive = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout2));
                        TimeoutToActive.ContinueWith(delegate
                        {
                            Panel_MidMiddle_Ready.Destroy();
                            world.SpawnMonster(201430, Panel_MidMiddle_Base.Position);
                            var Panel_MidMiddle_Active = world.GetActorBySNO(201430);

                            TickTimer Timeout3 = new SecondsTickTimer(world.Game, 5f);
                            var TimeoutToOff = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout3));
                            TimeoutToOff.ContinueWith(delegate
                            {
                                Panel_MidMiddle_Active.Destroy();
                                MidMidle_Active = false;
                            });
                        });
                    });
                }
                */
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
        private bool OnKillCultistsInJailListener(List<uint> monstersAlive, Map.World world)
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
        private bool OnKillSkeletonsInTempleListener(List<uint> monstersAlive, Map.World world)
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
        private bool OnKillSkeletonsInTemple2Listener(List<uint> monstersAlive, Map.World world)
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
                        if (monstersKilled == 1)
                            StartConversation(world, 109245);
                        else if (monstersKilled == 2)
                            StartConversation(world, 109247);
                        else if (monstersKilled == 3)
                            StartConversation(world, 109249);
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
        private bool OnEnterZoneListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                try
                {
                    sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 33065)
                    {
                        Vector3D PointToLeoricGhost = new Vector3D(650.6354f, 574.0665f, 0.10000005f);
                        var TouchWorld = player.CurrentScene.World;
                        TouchWorld.SpawnMonster(5360, PointToLeoricGhost);
                        StartConversation(TouchWorld, 17921);
                        var LeoricGhost = TouchWorld.GetActorBySNO(5360);

                        TickTimer Timeout = new SecondsTickTimer(TouchWorld.Game, 5f);
                        var ListenerKing = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
                        ListenerKing.ContinueWith(delegate
                        {
                              TouchWorld.Leave(LeoricGhost);
                        });
                        break;
                    }
                }
                catch { }
            }
            return true;
        }
        private bool WaitToSpawn(TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

            }
            return true;
        }
        private bool OnKhazraCaveListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            
            while (true)
            {
                try
                {
                    int sceneID = player.CurrentScene.SceneSNO.Id;
                    //116976
                    if (sceneID == 116976)
                    {
                        break;
                    }
                }
                catch { }
            }
            return true;
        }
        private bool OnJailListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {

            while (true)
            {
                try
                {
                    int sceneID = player.CurrentScene.SceneSNO.Id;
                    //116976
                    if (sceneID == 135396)
                    {
                        break;
                    }
                }
                catch { }
            }
            return true;
        }
        private bool OnKillListenerKhazra(List<uint> monstersAlive, Core.GS.Map.World world)
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
        private bool StartConversation(Core.GS.Map.World world, System.Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
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
            if (world == null)
            {
                Logger.Warn("Portal's destination world does not exist (WorldSNO = {0})", this.Destination.WorldSNO);
                return;
            }

            if (this.Destination.WorldSNO == 71150)
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
                if (this.ActorSNO.Id == 175482)
                {
                    var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                    if (dbQuestProgress.ActiveQuest == 72546)
                    {
                        if (dbQuestProgress.StepOfQuest == 8)
                        {
                            this.World.Game.Quests.Advance(72546);
                            dbQuestProgress.StepOfQuest = 9;

                        }
                    }
                    DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                    DBSessions.AccountSession.Flush();
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
            if (this.Destination.WorldSNO == 60395)// || this.Destination.StartingPointActorTag == -101)
            {
                //Enter to Drowned Temple
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72738)
                {
                    if (dbQuestProgress.StepOfQuest == 12)
                    {
                        world.Game.Quests.Advance(72738);
                        world.Game.Quests.Advance(72738);
                        dbQuestProgress.ActiveQuest = 72738;
                        dbQuestProgress.StepOfQuest = 14;
                        Logger.Debug(" Progress Saved ");
                    }
                    if(dbQuestProgress.StepOfQuest == 14)
                    {
                        var DrownedTempleWorld = player.World.Game.GetWorld(60395);
                        //Сюда соберём всех жертв)
                        List<uint> SkeletonsList = new List<uint> { };
                        List<uint> Skeletons2List = new List<uint> { };
                        // Магхда
                        //[Actor] [Type: Monster] SNOId:129345 DynamicId: 1943 Position: x:170,6767 y:256,0987 z:-68,2796 Name: Maghda_A_TempProjection}	Mooege.Core.GS.Actors.Actor {Mooege.Core.GS.Actors.Monster}
                        //Скелетоны
                        //[Actor] [Type: Monster] SNOId:5347 DynamicId: 1904 Position: x:289,1984 y:359,3138 z:-75,79334 Name: SkeletonArcher_B}	Mooege.Core.GS.Actors.Actor {Mooege.Core.GS.Actors.Implementations.Monsters.ReturnedArcher}
                        //[Actor] [Type: Monster] SNOId:139757 DynamicId: 1949 Position: x:298,6743 y:280,9939 z:-76,54692 Name: Nephalem_Ghost_A_DrownedTemple_Martyr_Skeleton}	Mooege.Core.GS.Actors.Actor {Mooege.Core.GS.Actors.Monster}
                        //[Actor] [Type: Monster] SNOId:5276 DynamicId: 1902 Position: x:295,7219 y:326,8748 z:-75,79003 Name: Shield_Skeleton_B}	Mooege.Core.GS.Actors.Actor {Mooege.Core.GS.Actors.Implementations.Monsters.ReturnedShieldMan}
                        //[Actor] [Type: Monster] SNOId:5395 DynamicId: 2033 Position: x:299,8383 y:372,2672 z:-76,8657 Name: Skeleton_B}	Mooege.Core.GS.Actors.Actor {Mooege.Core.GS.Actors.Implementations.Monsters.Returned}
                        //[Actor] [Type: Monster] SNOId:5388 DynamicId: 1914 Position: x:300,6144 y:371,7983 z:-76,8657 Name: SkeletonSummoner_B}	Mooege.Core.GS.Actors.Actor {Mooege.Core.GS.Actors.Monster}
                        // Тот скелет
                        //[Actor] [Type: Monster] SNOId:139757 DynamicId: 1920 Position: x:289,4304 y:277,368 z:-76,47368 Name: Nephalem_Ghost_A_DrownedTemple_Martyr_Skeleton}	Mooege.Core.GS.Actors.Actor {Mooege.Core.GS.Actors.InteractiveNPC}
                        // if(Brain != null)
                        // Аларик
                        //[Actor] [Type: Monster] SNOId:108882 DynamicId: 1933 Position: x:259,6319 y:270,5458 z:-73,87531 Name: GhostKnight1_Festering}	Mooege.Core.GS.Actors.Actor {Mooege.Core.GS.Actors.Monster} 

                        //Берём всех в охапку
                        var AllSkeletons1 = DrownedTempleWorld.GetActorsBySNO(5347);
                        var AllSkeletons2 = DrownedTempleWorld.GetActorsBySNO(5276);
                        var AllSkeletons3 = DrownedTempleWorld.GetActorsBySNO(5395);
                        var AllSkeletons4 = DrownedTempleWorld.GetActorsBySNO(5388);
                        var AllSkeletons5 = DrownedTempleWorld.GetActorsBySNO(139757);
                        //Последующие

                        #region Варим солянку
                        //Кидаем в солянку
                        foreach (var Skelet in AllSkeletons1)
                        {
                            SkeletonsList.Add(Skelet.DynamicID);
                        }
                        foreach (var Skelet in AllSkeletons2)
                        {
                            SkeletonsList.Add(Skelet.DynamicID);
                        }
                        foreach (var Skelet in AllSkeletons3)
                        {
                            SkeletonsList.Add(Skelet.DynamicID);
                        }
                        foreach (var Skelet in AllSkeletons4)
                        {
                            SkeletonsList.Add(Skelet.DynamicID);
                        }
                        world.SpawnMonster(139757, AllSkeletons5[0].Position);
                        foreach (var Skelet in AllSkeletons5)
                        {
                            Skelet.Destroy();
                        }
                        var AllSkeletons5Again = DrownedTempleWorld.GetActorsBySNO(139757);
                        foreach (var Skelet in AllSkeletons5Again)
                        {
                            SkeletonsList.Add(Skelet.DynamicID);
                            Skelet.Attributes[GameAttribute.Hitpoints_Max] = 1200f;
                            Skelet.Attributes[GameAttribute.Hitpoints_Cur] = 1200f;
                            Skelet.Attributes[GameAttribute.Damage_Weapon_Min, 0] = 70f;
                            Skelet.Attributes[GameAttribute.Damage_Weapon_Delta, 0] = 100f;
                            Skelet.Attributes[GameAttribute.Quest_Monster] = true;
                        }
                        #endregion

                        var ListenerSkeletons = Task<bool>.Factory.StartNew(() => OnKillSkeletonsInTempleListener(SkeletonsList, DrownedTempleWorld));
                        //Ждём пока убьют
                        ListenerSkeletons.ContinueWith(delegate
                        {
                            /*
                            [139713] Nephalem_Ghost_A_DrownedTemple_Martyr1_Skeleton
                            [139715] Nephalem_Ghost_A_DrownedTemple_Martyr2_Skeleton
                            [139756] Nephalem_Ghost_A_DrownedTemple_Martyr3_Skeleton
                            */
                            //Spawn Actor 92387
                            var AllTablets = DrownedTempleWorld.GetActorsBySNO(92387);
                            //Анимация открытия склепов
                            foreach (var Tablet in AllTablets)
                            {
                                DrownedTempleWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                                {
                                    ActorID = Tablet.DynamicID,
                                    Field1 = 5,
                                    Field2 = 0,
                                    tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                                    {
                                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                                        {
                                            Duration = 300,
                                            AnimationSNO = Tablet.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                            PermutationIndex = 0,
                                            Speed = 0.9f
                                        }
                                    }
                                }, Tablet);
                                DrownedTempleWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                                {
                                    ActorID = Tablet.DynamicID,
                                    AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                                }, Tablet);
                            }
                            //Спауним гадов
                            DrownedTempleWorld.SpawnMonster(139713, AllTablets[0].Position);
                            Skeletons2List.Add(DrownedTempleWorld.GetActorBySNO(139713).DynamicID);
                            world.GetActorBySNO(139713).Attributes[GameAttribute.Hitpoints_Max] = 800f;
                            world.GetActorBySNO(139713).Attributes[GameAttribute.Hitpoints_Cur] = 800f;
                            world.GetActorBySNO(139713).Attributes[GameAttribute.Damage_Weapon_Min, 0] = 70f;
                            world.GetActorBySNO(139713).Attributes[GameAttribute.Damage_Weapon_Delta, 0] = 100f;
                            world.GetActorBySNO(139713).Attributes[GameAttribute.Quest_Monster] = true;

                            DrownedTempleWorld.SpawnMonster(139715, AllTablets[1].Position);
                            Skeletons2List.Add(DrownedTempleWorld.GetActorBySNO(139715).DynamicID);
                            world.GetActorBySNO(139715).Attributes[GameAttribute.Hitpoints_Max] = 800f;
                            world.GetActorBySNO(139715).Attributes[GameAttribute.Hitpoints_Cur] = 800f;
                            world.GetActorBySNO(139715).Attributes[GameAttribute.Damage_Weapon_Min, 0] = 70f;
                            world.GetActorBySNO(139715).Attributes[GameAttribute.Damage_Weapon_Delta, 0] = 100f;
                            world.GetActorBySNO(139715).Attributes[GameAttribute.Quest_Monster] = true;

                            DrownedTempleWorld.SpawnMonster(139756, AllTablets[2].Position);
                            Skeletons2List.Add(DrownedTempleWorld.GetActorBySNO(139756).DynamicID);
                            world.GetActorBySNO(139756).Attributes[GameAttribute.Hitpoints_Max] = 800f;
                            world.GetActorBySNO(139756).Attributes[GameAttribute.Hitpoints_Cur] = 800f;
                            world.GetActorBySNO(139756).Attributes[GameAttribute.Damage_Weapon_Min, 0] = 70f;
                            world.GetActorBySNO(139756).Attributes[GameAttribute.Damage_Weapon_Delta, 0] = 100f;
                            world.GetActorBySNO(139756).Attributes[GameAttribute.Quest_Monster] = true;

                            var ListenerSecondSkeletons = Task<bool>.Factory.StartNew(() => OnKillSkeletonsInTemple2Listener(Skeletons2List, DrownedTempleWorld));
                            ListenerSecondSkeletons.ContinueWith(delegate
                            {
                                world.Game.Quests.Advance(72738);
                                StartConversation(DrownedTempleWorld, 108256);
                                StartConversation(DrownedTempleWorld, 133372);
                            });

                            StartConversation(DrownedTempleWorld, 133356);

                        });

                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
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
                        }                        
                    }
                    if(dbQuestProgress.ActiveQuest == 72061)
                    {
                        if (dbQuestProgress.StepOfQuest == 2)
                        {
                            dbQuestProgress.StepOfQuest = 3;
                            world.Game.Quests.Advance(72061);
                           
                        }

                        //Задать верное расположение!
                        Vector3D SpawnPortalPosition = new Vector3D(28.91075f, 205.9426f, -24.90778f);
                        // 73261 TagMap[ActorKeys.GizmoGroup]
                        Portal RealPortal = new Portal(world.Game.GetWorld(60713), 5648, world.Game.GetWorld(73261).StartingPoints[0].Tags);
                        RealPortal.Destination = new ResolvedPortalDestination
                        {
                            WorldSNO = 60713,
                            DestLevelAreaSNO = 60885,
                            StartingPointActorTag = -505
                        };
                        RealPortal.EnterWorld(SpawnPortalPosition);
                    }
                }
                catch { }

                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }
            if (this.Destination.WorldSNO == 50584)
            {
                //Enter to Cathedral Level 2
                
                Vector3D Point = new Vector3D(1146.33f, 1539.594f, 0.2f);
           /*     var CathedralLevel2 = world.Game.GetWorld(50584);
                var StartPoint = CathedralLevel2.GetActorBySNO(5502).Position;
                var ExitToTomb = CathedralLevel2.GetActorBySNO(4067);

                try
                {
                    Portal RealPortal = new Portal(world.Game.GetWorld(50584), 5648, world.Game.GetWorld(73261).StartingPoints[0].Tags);
                    RealPortal.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 73261,
                        DestLevelAreaSNO = 60885,
                        StartingPointActorTag = -106
                    };
                    RealPortal.EnterWorld(ExitToTomb.Position);
                    Logger.Warn("Congratulations) Level builded ok)");
                }
                catch { Logger.Warn("Sorry, Error of Build("); }*/
          /*      if (world.Game.GetWorld(50582).StartingPoints.Count == 0)
                    player.ChangeWorld(player.World.Game.GetWorld(50582), Point);
         /*       else
                {
                    var startingPoint = world.GetStartingPointById(this.Destination.StartingPointActorTag);
                    player.ChangeWorld(world, startingPoint);
                }*/
            }
            if (this.Destination.WorldSNO == 117405)
            {
                //Покои: 117405
                // To Tyrael Zone
                /*
                [117035] trDun_Crypt_W_Exit_Stranger_01
                */
                //Vector3D PointToSpawn = new Vector3D(196.2606f, 300.2404f, 25.237f);
                //player.ChangeWorld(player.World.Game.GetWorld(117405), PointToSpawn);
                
                //World_SceneChunk_SceneParametr - Связь мира и сцены
                //World - Сам Мир
                //Scene_Chunck - Сцена
                // Тираэль в кратере 180900
                // First COnv - 181910
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72061)
                {
                    if (dbQuestProgress.StepOfQuest == 15)
                    {
                        dbQuestProgress.StepOfQuest = 16;
                        world.Game.Quests.Advance(72061);
                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
                try
                {
                    var Crater_World = player.World.Game.GetWorld(117405);
                    var Stranger = Crater_World.GetActorBySNO(180900);

                    Crater_World.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                    {
                        ActorID = Stranger.DynamicID,
                        AnimationSNO = 141437,
                    }, Stranger);
                }
                catch { }
                // Second Conv - 181912 // actor 117365


            }
            if (this.Destination.WorldSNO == 119888)
            {
                var ToWorld = player.World.Game.GetWorld(119888);
                //Установить ожидание зоны.
                var ListenerKhazraEnterTask = Task<bool>.Factory.StartNew(() => OnKhazraCaveListener(player, ToWorld));
                ListenerKhazraEnterTask.ContinueWith(delegate
                {
                    world.Game.Quests.Advance(117779);
                    
                    //OnKillListenerKhazra
                    var minions = ToWorld.GetActorsBySNO(178213);
                    var Urik = ToWorld.GetActorBySNO(131131);
                    var MaghdaSpirit = ToWorld.GetActorBySNO(129345);
                    MaghdaSpirit.Attributes[Net.GS.Message.GameAttribute.Hitpoints_Max] = 200000f;
                    MaghdaSpirit.Attributes[Net.GS.Message.GameAttribute.Hitpoints_Cur] = 200000f;
                    List<uint> KhazrasKiller = new List<uint> { };
                    foreach (var minion in minions)
                    {
                        KhazrasKiller.Add(minion.DynamicID);
                        minion.Attributes[Net.GS.Message.GameAttribute.Quest_Monster] = true;
                    }
                    KhazrasKiller.Add(Urik.DynamicID);
                    Urik.Attributes[Net.GS.Message.GameAttribute.Using_Bossbar] = true;
                    Urik.Attributes[Net.GS.Message.GameAttribute.InBossEncounter] = true;
                    Urik.Attributes[Net.GS.Message.GameAttribute.Quest_Monster] = true;
                    //Установить счётчик убийств
                    var CainKillerEvent = Task<bool>.Factory.StartNew(() => OnKillListenerKhazra(KhazrasKiller, ToWorld));
                    var MaindbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                    if (MaindbQuestProgress.StepOfQuest < 5)
                    {
                        StartConversation(ToWorld, 131144);
                    }
                    DBSessions.AccountSession.SaveOrUpdate(MaindbQuestProgress);
                    DBSessions.AccountSession.Flush();
                    CainKillerEvent.ContinueWith(delegate
                    {
                        world.Game.Quests.Advance(117779);
                        foreach (var playerN in player.World.Players)
                        {
                            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                            if (dbQuestProgress.ActiveQuest == 117779)
                            {
                                dbQuestProgress.StepOfQuest = 5;
                            }
                            DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                            DBSessions.AccountSession.Flush();
                        }
                        //
                    });
                });


                //PowerManager - Запустить диалог после использования части клинка.

                //PowerManager - Ожидание телепорта домой.
            }
            if (this.Destination.WorldSNO == 167721)
            {
                //Вход в подвал часовни
                //LevelArea\A1_trOut_TownAttack_ChapelCellar.lvl
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 73236)
                {
                    if (dbQuestProgress.StepOfQuest == 7)
                    {
                        dbQuestProgress.StepOfQuest = 8;
                        world.Game.Quests.Advance(73236);
                    }
                    if (dbQuestProgress.StepOfQuest == 8)
                    {
                        var ToWorld = player.World.Game.GetWorld(167721);
                        var MaghdaSpirit = ToWorld.GetActorBySNO(129345);
                        ToWorld.Leave(MaghdaSpirit);

                        TickTimer Timeout = new SecondsTickTimer(world.Game, 4f);
                        var ListenerWaiting = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
                        ListenerWaiting.ContinueWith(delegate
                        {
                            MaghdaSpirit.EnterWorld(MaghdaSpirit.Position);
                            MaghdaSpirit.Attributes[Net.GS.Message.GameAttribute.Hitpoints_Max] = 200000f;
                            MaghdaSpirit.Attributes[Net.GS.Message.GameAttribute.Hitpoints_Cur] = 200000f;
                            ToWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                            {
                                ActorID = MaghdaSpirit.DynamicID,
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
                            }, MaghdaSpirit);
                            StartConversation(ToWorld, 165080);
                            //StartConversation(ToWorld, 143182);

                           
                        });
                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }
            if (this.Destination.WorldSNO == 180550)
            {
                //Вход в паучьи пещеры
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72546)
                {
                    if (dbQuestProgress.StepOfQuest < 2)
                    {
                        dbQuestProgress.StepOfQuest = 2;
                        world.Game.Quests.Advance(72546);
                    }
                    if (dbQuestProgress.StepOfQuest == 2)
                    {
                        dbQuestProgress.StepOfQuest = 3;
                       // world.Game.Quests.Advance(72546);
                       world.Game.Quests.NotifyQuest(72546, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 180550);
                    }

                    //Vector3D SpawnPortalPosition = new Vector3D(28.91075f, 205.9426f, -24.90778f);
                    Vector3D SpawnPortalPosition = world.GetActorBySNO(183032).Position;
                    // 73261 TagMap[ActorKeys.GizmoGroup]
                    
                    Portal RealPortal = new Portal(world.Game.GetWorld(180550), 5648, world.Game.GetWorld(182976).StartingPoints[0].Tags);
                    RealPortal.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 182976,
                        DestLevelAreaSNO = 62726,
                        StartingPointActorTag = -104 //30
                    };
                    RealPortal.EnterWorld(SpawnPortalPosition);
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }

            if (this.Destination.WorldSNO == 182976)
            {
                //Покои королевы
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.StepOfQuest == 3)
                {
                    dbQuestProgress.StepOfQuest = 4;
                    world.Game.Quests.Advance(72546);
                    // world.Game.Quests.NotifyQuest(72546, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 180550);
                }
                if (dbQuestProgress.StepOfQuest > 7)
                {
                    try {
                        var UsedWeb = world.GetActorBySNO(104545);
                        UsedWeb.Destroy();
                    }
                    catch { }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            }

            if (this.Destination.WorldSNO == 75049)
            {
                //LevelArea = 100854
                world.Game.Quests.NotifyQuest(72546, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 75049);
            }
            if (this.Destination.WorldSNO == 2826)
            {
                // Первый этаж агоний
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72801)
                {
                    if (dbQuestProgress.StepOfQuest < 2)
                    {
                        dbQuestProgress.StepOfQuest = 2;
                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();

                world.Game.Quests.NotifyQuest(72801, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 2826);
            }
            if (this.Destination.WorldSNO == 58982)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72801)
                {
                    if (dbQuestProgress.StepOfQuest < 3)
                    {
                        dbQuestProgress.StepOfQuest = 3;
                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
                // Второй этаж агоний
                world.Game.Quests.NotifyQuest(72801, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 58982);
            }
            if (this.Destination.WorldSNO == 87707)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72801)
                {
                    if (dbQuestProgress.StepOfQuest < 4)
                    {
                        dbQuestProgress.StepOfQuest = 4;
                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
                // Перевал в Высокогорье
                world.Game.Quests.NotifyQuest(72801, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 87707);
            }
            if (this.Destination.WorldSNO == 94676)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                if (dbQuestProgress.ActiveQuest == 72801)
                {
                    if (dbQuestProgress.StepOfQuest < 5)
                    {
                        dbQuestProgress.StepOfQuest = 5;
                    }
                }
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
                // Проклятая крепость
                world.Game.Quests.NotifyQuest(72801, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 94676);

            }
            if (this.Destination.WorldSNO == 58983)
            {
                // Третий этаж Агонии МОЕЙ ЖОПЫ БЛЯТЬ!
                var ToHome = new Portal(player.World.Game.GetWorld(58983), 5648, player.World.Game.GetWorld(78839).StartingPoints[0].Tags);
                ToHome.Destination = new ResolvedPortalDestination
                {
                    WorldSNO = 78839,
                    DestLevelAreaSNO = 90881,
                    StartingPointActorTag = -108
                };
                // Название локации, не работает(
                ToHome.EnterWorld(world.GetActorBySNO(158944).Position);
            }
            if (this.Destination.WorldSNO == 135713)
            {
                //Камеры обречённых
                try
                {
                    var TyraelJail = world.Game.GetWorld(135713);
                    var TyraelNo = TyraelJail.GetActorBySNO(183125);

                    TyraelJail.Leave(TyraelNo);

                    var ListenerJailEnterTask = Task<bool>.Factory.StartNew(() => OnJailListener(player, TyraelJail));
                    ListenerJailEnterTask.ContinueWith(delegate
                    {
                        TyraelJail.Game.Quests.Advance(72801);
                        var Cultists = TyraelJail.GetActorsBySNO(102452);
                    //Tyrael - 183125 / Stranger - 205446 / Dialog_Stranger - 117365 / 183117 - Stranger_Ritual
                    List<uint> CultistsList = new List<uint> { };
                        foreach (var Cultist in Cultists)
                            CultistsList.Add(Cultist.DynamicID);
                        var ListenerCultistsInJail = Task<bool>.Factory.StartNew(() => OnKillCultistsInJailListener(CultistsList, TyraelJail));
                    //Ждём пока убьют
                    ListenerCultistsInJail.ContinueWith(delegate
                        {
                            TyraelJail.Game.Quests.Advance(72801);
                            TyraelJail.Leave(TyraelJail.GetActorBySNO(205446));

                        });
                    });
                }
                catch { }
            }

            #region Не санкционированные порталы)
            if (this.Destination.StartingPointActorTag == -100)
            {

                Vector3D ToPortal = new Vector3D(2985.6241f, 2795.627f, 24.04532f);
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
                     //       now_world.Leave(HirelingToLeave);
                        }
            
                    }
                    catch { }
                    if (dbQuestProgress.ActiveQuest == 72221 && dbQuestProgress.StepOfQuest == 10)
                    { player.World.Game.Quests.NotifyQuest(72221, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1); }
                    player.ChangeWorld(player.World.Game.GetWorld(71150), ToPortal);
                    if (dbQuestProgress.ActiveQuest == 72738 && dbQuestProgress.StepOfQuest == 18)
                    {
                        //player.World.Game.Quests.NotifyQuest(72738, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1);
                        dbQuestProgress.ActiveQuest = 73236;
                        dbQuestProgress.StepOfQuest = -1;
                    }
                    player.ChangeWorld(player.World.Game.GetWorld(71150), ToPortal);
                }
                else
                {
                    player.Teleport(ToPortal);
                    if (player.ActiveHireling != null)
                    {
                        player.ActiveHireling.Teleport(ToPortal);
                    }
                }
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
            //Портал из Тристрама
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
                {
                    player.Teleport(ToPortal);
                    if (player.ActiveHireling != null)
                    {
                        player.ActiveHireling.Teleport(ToPortal);
                    }
                }
            }
            //Портал в Залл Леорика
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
                        var Wrongminions = dest.GetActorsBySNO(80652);
                        Vector3D FirstPoint = new Vector3D(112.1694f, 166.0996f, 0.09996167f);
                        Vector3D SecondPoint = new Vector3D(120.07f, 174.9657f, 0.1114834f);
                        Vector3D ThirdPoint = new Vector3D(111.3691f, 182.6697f, 5.229973f);
                        Vector3D BossPoint = new Vector3D(111.3691f, 187.6697f, 5.229973f);
                        dest.SpawnMonster(87012, FirstPoint);
                        dest.SpawnMonster(87012, SecondPoint);
                        dest.SpawnMonster(87012, ThirdPoint);
                        dest.SpawnMonster(115403, BossPoint);
                        var minions = dest.GetActorsBySNO(87012);

                        foreach (var minion in Wrongminions)
                        {
                            minion.Destroy();
                        }

                        List<uint> SkilletKiller = new List<uint> { };
                        var CainBrains = world.GetActorBySNO(102386);
                        foreach (var minion in minions)
                        {
                            SkilletKiller.Add(minion.DynamicID);
                        }
                        SkilletKiller.Add(dest.GetActorBySNO(115403).DynamicID);
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
            // Патч выхода из хижины Адрии
            else if (this.Destination.StartingPointActorTag == -103)
            {
                Vector3D Point = new Vector3D(1769.139f, 2914.95f, 20.16885f);
                player.ChangeWorld(player.World.Game.GetWorld(71150), Point);

            }
            // Паучиха --
            else if (this.Destination.StartingPointActorTag == -104)
            {
                //var startingPoint = new Vector3D(0f,0f,0f);//world.GetStartingPointById(this.Destination.StartingPointActorTag);
                var startingPoint = new Vector3D(183.8976f, 133.8681f, 19.6342f);
                player.ChangeWorld(world, startingPoint);

            }
            // Портал к четвертому уровню собора
            else if (this.Destination.StartingPointActorTag == -106)
            {
                //var BossWorld = player.World.Game.GetWorld(73261);
                //Второй уровень собора: 50582
                //Третий уровень собора: 105406
                //Четвертый уровень собора: 50584
                //Гробница: 50585
                //Босс-Зона: 73261
                //Покои: 117405
                // To Tyrael Zone
                /*
                [148748] a1dun_Leor_Tyrael_Back_Skybox_01
                [135396] a1dun_Leor_Tyrael_jail_01
                [135521] a1dun_Leor_Tyrael_Stairs_A_01
                [135710] a1dun_Leor_Tyrael_Filler_02
                */

                var CathedralLevel4 = player.World.Game.GetWorld(50584);
                var StartPoint = CathedralLevel4.GetActorBySNO(5502).Position;
                var ExitToTomb = CathedralLevel4.GetActorBySNO(4067);
                
                try
                {
                    Portal RealPortal = new Portal(world.Game.GetWorld(50584), 5648, world.Game.GetWorld(73261).StartingPoints[0].Tags);
                    RealPortal.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 73261,
                        DestLevelAreaSNO = 60885,
                        StartingPointActorTag = -106
                    };
                    RealPortal.EnterWorld(ExitToTomb.Position);
                    Vector3D Point = new Vector3D(338.9958f, 468.3622f, -3.859601f);
                    Logger.Warn("Congratulations) Level builded ok)");
                    player.ChangeWorld(CathedralLevel4, StartPoint);
                }
                catch
                {
                    Logger.Warn("Sorry, Error of Build(");
                    player.ChangeWorld(CathedralLevel4, StartPoint);
                }
                


                /*
                
                */
            }
            // Портал в Гробницу королей
            else if (this.Destination.StartingPointActorTag == -505)
            {
                //Leoric Ghost - 5365
                var KingTombWorld = player.World.Game.GetWorld(50585);
                Vector3D EnterToKingTomb = new Vector3D(1060.87f, 493.0155f, 0.100031f);
                player.ChangeWorld(KingTombWorld, EnterToKingTomb);

                //Boss_Portal_SkeletonKing 159573
                #region Временный портал в босс зону
                var NotWorkBossPortal = KingTombWorld.GetActorBySNO(159573);
                Portal RealPortal = new Portal(world.Game.GetWorld(50585), 5648, world.Game.GetWorld(73261).StartingPoints[0].Tags);
                RealPortal.Destination = new ResolvedPortalDestination
                {
                    WorldSNO = 73261,
                    DestLevelAreaSNO = 60885,
                    StartingPointActorTag = -107
                };
                RealPortal.EnterWorld(NotWorkBossPortal.Position);
                #endregion


                var WallCrypt = KingTombWorld.GetActorBySNO(5788);
                TickTimer Timeout = new SecondsTickTimer(world.Game, 3f);
                var ListenerWaiting = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
                ListenerWaiting.ContinueWith(delegate
                {
                    KingTombWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                    {
                        ActorID = WallCrypt.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 5000,
                                AnimationSNO = WallCrypt.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 0.5f
                            }
                            }
                    }, WallCrypt);
                    KingTombWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                    {
                        ActorID = WallCrypt.DynamicID,
                        AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                    }, WallCrypt);
                    //9926 - Spawn2/ 9900 - Spawn
                    //Skeleton_A - 5393
                });
                
                
                var ListenerEnterToLocalTask = Task<bool>.Factory.StartNew(() => OnEnterZoneListener(player, world));
                //Wait for portal to be used or player going to scene.
                ListenerEnterToLocalTask.ContinueWith(delegate
                {
                    Logger.Debug(" Waypoint_Park Objective done ");

                });

                
                for (int i = 0; i < 8; i++) { world.Game.Quests.Advance(72061); }

            }
            // Портал в покои Леорика
            else if (this.Destination.StartingPointActorTag == -107)
            {
                var BossWorld = player.World.Game.GetWorld(73261);
                Vector3D Point = new Vector3D(338.9958f, 468.3622f, -3.859601f);
                world.Game.Quests.Advance(72061);
                player.ChangeWorld(BossWorld, Point);
                var AllSpawnPoint = world.GetActorsBySNO(5913);
                Vector3D FistPoint = new Vector3D(291.9193f, 428.6796f, 0.1f);
                Vector3D SecondPoint = new Vector3D(270.9105f, 426.223f, 0.1000026f);
                Vector3D ThirdPoint = new Vector3D(241.2828f, 425.616f, 0.1f);
                Vector3D FourPoint = new Vector3D(241.2051f, 435.0545f, 0.1f);

                var SkeletonKing_Bridge = BossWorld.GetActorBySNO(461);
                
                BossWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                {
                    ActorID = SkeletonKing_Bridge.DynamicID,
                    AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                }, SkeletonKing_Bridge);
                // 461 -trDun_SkeletonKing_Bridge_Active

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
                    var WalkableSkeletonGate = new Door(BossWorld, 5765, world.GetActorBySNO(5765).Tags);
                    WalkableSkeletonGate.Field2 = 16;
                    WalkableSkeletonGate.RotationAxis = world.GetActorBySNO(5765).RotationAxis;
                    WalkableSkeletonGate.RotationW = world.GetActorBySNO(5765).RotationW;
                    WalkableSkeletonGate.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                    //NoDownGate.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                    WalkableSkeletonGate.Attributes[GameAttribute.Gizmo_State] = 1;
                    WalkableSkeletonGate.Attributes[GameAttribute.Untargetable] = true;
                    WalkableSkeletonGate.Attributes.BroadcastChangedIfRevealed();
                    WalkableSkeletonGate.EnterWorld(world.GetActorBySNO(5765).Position);
                    SkeletonGate.Destroy();

                    BossWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                    {
                        ActorID = WalkableSkeletonGate.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 100,
                                AnimationSNO = WalkableSkeletonGate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 0.5f
                            }
                            }
                    }, WalkableSkeletonGate);
                    BossWorld.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                    {
                        ActorID = WalkableSkeletonGate.DynamicID,
                        AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                    }, WalkableSkeletonGate);

                });
            }
            // Портал к Мяснику.
            else if (this.Destination.StartingPointActorTag == -108)
            {
                world.Game.Quests.NotifyQuest(72801, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld, 78839);
                //var startingPoint = new Vector3D(0f, 0f, 0f);
                var startingPoint = new Vector3D(143.3902f, 143.1758f, 0.09997044f);
                var ButcherLair = world.Game.GetWorld(78839);
                ButcherLair.SpawnMonster(3526,new Vector3D(92.82627f,90.92698f,0.09997056f));
                
                player.ChangeWorld(world, startingPoint);

                StartConversation(ButcherLair, 211980);
                List<uint> ButcherList = new List<uint> { };
                //Отключить дверь
                //ID двери - 105361
                var BlockDoor = ButcherLair.GetActorBySNO(105361);
                BlockDoor.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                BlockDoor.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                BlockDoor.Attributes[GameAttribute.Gizmo_State] = 1;
                BlockDoor.Attributes[GameAttribute.Untargetable] = true;
                BlockDoor.Attributes.BroadcastChangedIfRevealed();

                //[Actor] [Type: Monster] SNOId:201426 DynamicId: 2591 Position: x:120,9595 y:121,6244 z:-0,1068707 Name: ButcherLair_FloorPanel_MidMiddle_Base
                /*
                [201426] ButcherLair_FloorPanel_MidMiddle_Base
                [201428] ButcherLair_FloorPanel_MidMiddle_Telegraph
                [201430] ButcherLair_FloorPanel_MidMiddle_Active
                */
                
                

                //ButcherLair.SpawnMonster()

                //[Actor] [Type: Monster] SNOId:200969 DynamicId: 2593 Position: x:149,4937 y:150,5052 z:-0,3369803 Name: ButcherLair_FloorPanel_LowerMid_Base

                ButcherList.Add(ButcherLair.GetActorBySNO(3526).DynamicID);
                var ListenerButcher = Task<bool>.Factory.StartNew(() => OnKillButcherListener(ButcherList, ButcherLair));
                //Ждём пока убьют
                ListenerButcher.ContinueWith(delegate
                {
                    ButcherLair.Game.Quests.Advance(72801);
                    //Включить дверь
                    
                    var OpenDoor = new Door(BlockDoor.World, BlockDoor.ActorSNO.Id, BlockDoor.Tags);
                    OpenDoor.Field2 = 0;
                    OpenDoor.RotationAxis = BlockDoor.RotationAxis;
                    OpenDoor.RotationW = BlockDoor.RotationW;
                    OpenDoor.Attributes.BroadcastChangedIfRevealed();
                    OpenDoor.EnterWorld(BlockDoor.Position);
                    BlockDoor.Destroy();
                });
            }

            #endregion

            else
            {
                var startingPoint = world.GetStartingPointById(this.Destination.StartingPointActorTag);

                if (startingPoint != null)
                {
                    try
                    {
                        if (player.ActiveHireling != null)
                        {
                            // now_world.Leave(HirelingToLeave);
                            //var NewTristram = player.InGameClient.Game.GetWorld(71150);
                            //var Leah_Back = NewTristram.GetActorByDynamicId(83);
                            //Leah_Back.EnterWorld(Leah_Back.Position);
                            if (player.ActiveHireling.ActorSNO.Id == 4580)
                            {
                                var HirelingToLeave = player.ActiveHireling;
                                now_world.Leave(HirelingToLeave);
                                player.ChangeWorld(world, startingPoint);
                                //var NewTristram = player.InGameClient.Game.GetWorld(71150);
                                //var Leah_Back = NewTristram.GetActorByDynamicId(83);
                                //Leah_Back.EnterWorld(Leah_Back.Position);
                            }
                            else
                            {
                                //Берём текущего спутника
                                var HirelingToLeave = player.ActiveHireling;
                                //Убираем его из мира

                                //Телепортируемся сами.
                                player.ChangeWorld(world, startingPoint);
                                HirelingToLeave.ChangeWorld(world, startingPoint);
                                world.Leave(HirelingToLeave);
                                HirelingToLeave.Master = player;
                                HirelingToLeave.EnterWorld(player.Position);
                                //now_world.Leave(HirelingToLeave);
                                //Телепортируем нашего товарища.
                                //Клон

                               
                            }

                        }
                        else
                        {
                            if (this.Destination.WorldSNO == 130161)
                            {
                                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
                                if (dbQuestProgress.ActiveQuest == 73236)
                                {
                                    if (dbQuestProgress.StepOfQuest == 9)
                                    {
                                        //Включение кат-сцены

                                        //Пока её нет)

                                        this.Destroy();
                                        var Leah = now_world.GetActorByDynamicId(25);

                                        now_world.SpawnMonster(121208, Leah.Position);

                                        StartConversation(now_world, 138268);
                                    }else
                                    { player.ChangeWorld(world, startingPoint); }
                                }
                                else { player.ChangeWorld(world, startingPoint); }

                                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                                DBSessions.AccountSession.Flush();
                            }
                            else
                            {
                                player.ChangeWorld(world, startingPoint);
                            }
                        }
                    }
                    catch {
                        Logger.Warn("Ошибка телепортации.");
                        player.ChangeWorld(world, startingPoint);
                    }

                    //player.ChangeWorld(world, startingPoint);
                    //
                }
                else
                   
                    Logger.Warn("Portal's tagged starting point does not exist (Tag = {0})", this.Destination.StartingPointActorTag);
            }

        }
    }
}