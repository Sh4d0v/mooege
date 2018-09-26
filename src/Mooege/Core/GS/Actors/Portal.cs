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


namespace Mooege.Core.GS.Actors
{
    public class Portal : Actor
    {
        static readonly Logger Logger = LogManager.CreateLogger();

        public override ActorType ActorType { get { return ActorType.Gizmo; } }
        private bool FirstEnter = true;
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
                    
                    this.Destination = new ResolvedPortalDestination
                    {
                        
                        WorldSNO = 60713,
                        DestLevelAreaSNO = 60885,
                        StartingPointActorTag = 1
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
                    
                    var FakePortal = this;


                    
                    Logger.Warn("Portal {0} forced", this.ActorSNO.Id);
                }else if (this.ActorSNO.Id == 5648)
                {
                    //Generate Portal
                    this.Destination = new ResolvedPortalDestination
                    {
                        WorldSNO = 60713,
                        DestLevelAreaSNO = 33357,
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
                Field2 = MinimapIcon, //   0x00018FB0,  /* Marker_DungeonEntrance.tex */          // TODO Dont mark all portals as dungeon entrances... some may be exits too (although d3 does not necesarrily use the correct markers). Also i have found no hacky way to determine whether a portal is entrance or exit - farmy
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
                //waypoint_is_activation.Attributes[G]
                //waypoint_is_activation.Attributes[Net.GS.Message.GameAttribute.Operatable] = true;
                //BossPortal.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated] = true;
                //BossPortal.Attributes[Net.GS.Message.GameAttribute.Operatable] = true;
                //BossPortal.Attributes[Net.GS.Message.GameAttribute.Untargetable] = true;
                //waypoint_is_activation.Attributes[Net.GS.Message.GameAttribute.Untargetable] = false;

                
                var FakePortal = world.GetActorBySNO(168932);
                
                Portal RealPortal = new Portal(world.Game.GetWorld(50579), 5648, world.Game.GetWorld(60713).StartingPoints[0].Tags);
                RealPortal.Destination = new ResolvedPortalDestination
                {
                    WorldSNO = 60713,
                    DestLevelAreaSNO = 60885,
                    StartingPointActorTag = -102
                };
                RealPortal.EnterWorld(FakePortal.Position);
                FirstEnter = false;
            }

            if (this.Destination.WorldSNO == 60395)
            {
                //Enter to Drowned Temple
                
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
                    
                    //CainWorld.AddScene(CainScene);

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
            else if(this.Destination.StartingPointActorTag == -101)
            {
                var dbPortalOfToon = DBSessions.AccountSession.Get<DBPortalOfToon>(player.Toon.PersistentID);
                Vector3D ToPortal = new Vector3D(dbPortalOfToon.X, dbPortalOfToon.Y, dbPortalOfToon.Z);
                var DestWorld = player.World.Game.GetWorld(dbPortalOfToon.WorldDest);
                var oldPortals = DestWorld.GetActorsBySNO(5648);
                foreach (var OldP in oldPortals)
                {
                    OldP.Destroy();
                }

                if (player.World.Game.GetWorld(dbPortalOfToon.WorldDest) != player.World)
                    player.ChangeWorld(player.World.Game.GetWorld(dbPortalOfToon.WorldDest), ToPortal);
                else
                    player.Teleport(ToPortal);
            }
            else if (this.Destination.StartingPointActorTag == -102)
            {
                Vector3D Point = new Vector3D(237.3005f,200.94f,31.17498f);
                player.ChangeWorld(player.World.Game.GetWorld(60713), Point);
                
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
                }
                else
                    Logger.Warn("Portal's tagged starting point does not exist (Tag = {0})", this.Destination.StartingPointActorTag);
            }

        }
    }
}