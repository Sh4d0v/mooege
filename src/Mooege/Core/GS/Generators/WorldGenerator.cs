﻿/*
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

using System.Collections.Generic;
using System.Linq;
using Mooege.Common.Helpers.Math;
using Mooege.Common.Logging;
using Mooege.Common.MPQ;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Core.GS.Games;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Common.MPQ.FileFormats;
using World = Mooege.Core.GS.Map.World;
using Scene = Mooege.Core.GS.Map.Scene;
using Mooege.Core.GS.Common.Types.Scene;

namespace Mooege.Core.GS.Generators
{
    public static class WorldGenerator
    {
        static readonly Logger Logger = LogManager.CreateLogger();

        public static World Generate(Game game, int worldSNO)
        {
            if (!MPQStorage.Data.Assets[SNOGroup.Worlds].ContainsKey(worldSNO))
            {
                Logger.Error("Can't find a valid world definition for sno: {0}", worldSNO);
                return null;
            }

            var worldAsset = MPQStorage.Data.Assets[SNOGroup.Worlds][worldSNO];
            var worldData = (Mooege.Common.MPQ.FileFormats.World)worldAsset.Data;

            #region Инфа для высокогорья
            /*
                [075347] trOut_Highlands_Entrance_E04_N01
                [075348] trOut_Highlands_Entrance_E04_S01
                [075789] trOut_Highlands_Entrance_E04_S04 - "3000"	"3840"
                [075790] trOut_Highlands_Entrance_E04_S05 - "3120"	"3840"

                [074865] trOut_Highlands_Entrance_E05_S01
                [074861] trOut_Highlands_Entrance_E05_S02
                [074866] trOut_Highlands_Entrance_E05_S03
                [074854] trOut_Highlands_Entrance_E05_S04
                [075346] trOut_Highlands_Entrance_E05_N01

                [074864] trOut_Highlands_Entrance_E06_S01
                [074860] trOut_Highlands_Entrance_E06_S02
                [074857] trOut_Highlands_Entrance_E06_S03
                [074853] trOut_Highlands_Entrance_E06_S04

                [074863] trOut_Highlands_Entrance_E07_S01
                [074859] trOut_Highlands_Entrance_E07_S02
                [074856] trOut_Highlands_Entrance_E07_S03
                [074852] trOut_Highlands_Entrance_E07_S04

                [074862] trOut_Highlands_Entrance_E08_S01
                [074858] trOut_Highlands_Entrance_E08_S02
                [074855] trOut_Highlands_Entrance_E08_S03
                [074851] trOut_Highlands_Entrance_E08_S04

                [123952] trOut_Highlands_Sub80_MysticWagon - Вагон (СубСцена)
                
                [076172] trOut_Highlands_Roads_EW_01
                [076179] trOut_Highlands_Roads_NE_01
                [076186] trOut_Highlands_Roads_NSEW_01
                [076193] trOut_Highlands_Roads_NS_01
                [076200] trOut_Highlands_Roads_NW_01
                [076207] trOut_Highlands_Roads_SE_01
                [076214] trOut_Highlands_Roads_SW_01
                [082761] trOut_Highlands_Roads_SW_02
                [083007] trOut_Highlands_Roads_SE_02
                [199507] trOut_Highlands_Roads_NS_01_AO_1
                [083128] trOut_Highlands_Roads_NW_02
                [083257] trOut_Highlands_Roads_NS_03
                [083286] trOut_Highlands_Roads_NS_02
                [083326] trOut_Highlands_Roads_NE_02
                [083395] trOut_Highlands_Roads_EW_03
                [083453] trOut_Highlands_Roads_EW_02
            */
            #endregion
            #region Лощина стенаний
            /*
             
            

             
            [078420] trOut_Wilderness_Road_NE_02
            [074241] trOut_Wilderness_Road_EW_01
            [074248] trOut_Wilderness_Road_NE_01
            [074276] trOut_Wilderness_Road_SW_01
            [078798] trOut_Wilderness_Road_SW_02
            */
            #endregion
            #region Проклятый замок
            /*
             [093289]
        a1dun_Leor_Jail_S_Entrance_01

            [093450] a1dun_Leor_Jail_NSEW_04
            [091982] a1dun_Leor_Jail_NSEW_02
            [092335] a1dun_Leor_Jail_NSEW_03 // Босс батл
            [097158] a1dun_Leor_Jail_Filler_Exit_01
            [091385] a1dun_Leor_Jail_W_01
            [091389] a1dun_Leor_Jail_S_01
            [091393] a1dun_Leor_Jail_N_01
            [091397] a1dun_Leor_Jail_E_01
            [091435] a1dun_Leor_Jail_NSEW_01
            [099174] a1dun_Leor_Jail_Filler_NE_01
            [099178] a1dun_Leor_Jail_Filler_NW_01
            [099182] a1dun_Leor_Jail_Filler_SE_01
            [099186] a1dun_Leor_Jail_Filler_SW_01
            [091311] a1dun_Leor_Jail_Filler_01

            [095362] a1dun_Leor_Jail_N_Exit_01
            [093835] a1dun_Leor_Jail_NSEW_05*/
            #endregion
            #region Замок Леорика

            /*
             * 
            [World] SNOId: 75049 DynamicId: 117440514 Name: a1dun_Leor_Manor}

            [135710] a1dun_Leor_Tyrael_Filler_02
            [148748] a1dun_Leor_Tyrael_Back_Skybox_01
            [135396] a1dun_Leor_Tyrael_jail_01
            [135521] a1dun_Leor_Tyrael_Stairs_A_01
            
            [030928] a1dun_Leor_E_Entrance_01 // Начало второго уровня агоний
            [030945] a1dun_Leor_N_Entrance_01 // Начало первого уровня агоний
            [030953] a1dun_Leor_S_Entrance_01 
            [030956] a1dun_Leor_W_Entrance_01
            
            [030957] a1dun_Leor_W_Exit_01
            [030954] a1dun_Leor_S_Exit_01 //Выход на второй уровень
            [001778] a1dun_Leor_N_Exit_01 
            [030929] a1dun_leor_E_Exit_01
            [080422] a1dun_Leor_W_Exit_Boss_01 //Выход на переправу высокогорья
            
            [109551] a1dun_Leor_SW_Hall_01_B
            [109578] a1dun_Leor_SW_Hall_01_C
            [052681] a1dun_Leor_EW_01_BellowsRoom
            [052683] a1dun_Leor_NS_01_Executioner
            [030924] a1dun_Leor_01_Filler
            
            [030948] a1dun_Leor_SE_01
            [030949] a1dun_Leor_SE_02
            
            [030940] a1dun_Leor_NS_01
            [030941] a1dun_Leor_NS_02
            
            [030942] a1dun_Leor_NW_01
            [030943] a1dun_Leor_NW_02
            
            [030925] a1dun_Leor_EW_01
            [030926] a1dun_Leor_EW_02
            
            [030932] A1dun_Leor_NE_01
            [030933] a1dun_Leor_NE_02
            [183305] a1dun_Leor_NE_03
            
            [030936] a1dun_Leor_NSE_01
            [030937] a1dun_Leor_NSE_02
            
            [030938] a1dun_Leor_NSW_01
            [030939] a1dun_Leor_NSW_02
            
            [030946] A1dun_Leor_SEW_01
            [030947] a1dun_Leor_SEW_02
            
            [030930] a1dun_Leor_NEW_01
            [030931] a1dun_Leor_NEW_02
            
            [030934] a1dun_Leor_NSEW_01
            [030935] a1dun_Leor_NSEW_02
            [052682] a1dun_Leor_NSEW_03
            
            [030944] a1dun_Leor_N_Dead_End_01
            [030927] a1dun_Leor_E_Dead_End_01
            [030952] a1dun_Leor_S_Dead_End_01
            [030955] a1dun_Leor_W_Dead_End_01
            
            [030950] a1dun_Leor_SW_Hall_01
            [030951] a1dun_Leor_SW_Hall_02
            */
            #endregion

            /*
                
            [076489] [Scene] trOut_Highlands_ManorInterior_E01_S01
            [076485] [Scene] trOut_Highlands_ManorInterior_E01_S02
            [076482] [Scene] trOut_Highlands_ManorInterior_E01_S03
            
            [076488] [Scene] trOut_Highlands_ManorInterior_E02_S01
            [076484] [Scene] trOut_Highlands_ManorInterior_E02_S02
            [076481] [Scene] trOut_Highlands_ManorInterior_E02_S03
            
            [076487] [Scene] trOut_Highlands_ManorInterior_E03_S01
            [076490] [Scene] trOut_Highlands_ManorInterior_E03_S02
            [076480] [Scene] trOut_Highlands_ManorInterior_E03_S03
            
            [076483] [Scene] trOut_Highlands_ManorInterior_E04_S02
             */
            if (worldAsset.SNOId == 2826)
            {
                //1 Уровень залов агонии 
                worldData.IsGenerated = false;
            }
            if (worldAsset.SNOId == 58982)
            {
                //2 Уровень залов агонии
                worldData.IsGenerated = false;
            }
            if (worldAsset.SNOId == 58983)
            {
                //3 Уровень залов агонии
                worldData.IsGenerated = false;
            }
            if (worldAsset.SNOId == 60395)
            {
                worldData.IsGenerated = false;
            }
            if (worldAsset.SNOId == 72637)
            {
                worldData.IsGenerated = false;
            }
            if (worldAsset.SNOId == 180550)
            {
                worldData.IsGenerated = false;
            }
            if (worldAsset.SNOId == 50584)
            {
                worldData.IsGenerated = false;
                /*
                [032936] trDun_Cath_01_Filler
                [032938] trDun_Cath_E
                [032939] trDun_Cath_EW_01
                [032940] trDun_Cath_EW_Entrance_01
                [032941] trDun_Cath_EW_Hall_01
                [032943] trDun_Cath_E_Dead_End_01
                [032944] trDun_Cath_E_Entrance01
                [032945] trDun_Cath_E_Entrance_02
                [032946] trDun_Cath_E_Exit
                [032947] trDun_Cath_E_Exit_02
                [032948] trDun_Cath_E_Exit_Crypt_01
                [032951] trDun_Cath_NEW_01
                [032952] trDun_Cath_NE_01
                [032954] trDun_Cath_NE_Hall_01
                [032955] trDun_Cath_NE_Hall_02
                [032956] trDun_Cath_NE_Hall_03
                [032957] trDun_Cath_NE_Hall_04
                [032958] trDun_Cath_NSEW_01
                [032960] trDun_Cath_NSE_01
                [032961] trDun_Cath_NS_01
                [032963] trDun_Cath_NS_Hall_01
                [032964] trDun_Cath_NS_Hall_02
                [032965] trDun_Cath_NW_01
                [032967] trDun_Cath_NW_Hall_01
                [032968] trDun_Cath_NW_Hall_02
                [032969] trDun_Cath_N_01
                [032970] trDun_Cath_N_02
                [032971] trDun_Cath_N_Dead_End_01
                [032974] trDun_Cath_N_Entrance_02
                [032975] trDun_Cath_N_Entrance_03
                [032976] trDun_Cath_N_Exit_01
                [032979] trDun_Cath_SEW_01
                [032981] trDun_Cath_SE_01
                [032982] trDun_Cath_SE_Hall_01
                [032983] trDun_Cath_SE_Hall_02
                [032984] trDun_Cath_SE_Hall_03
                [032985] trDun_Cath_SW_01
                [032986] trDun_Cath_SW_Hall_01
                [032987] trDun_Cath_SW_Hall_02
                [032989] trDun_Cath_S_01
                [032990] trDun_Cath_S_02_Stranger
                [032991] trDun_Cath_S_Dead_End_01
                [032992] trDun_Cath_S_Entrance_01
                [032993] trDun_Cath_S_Exit_01
                [032995] trDun_Cath_S_Exit_03
                [032997] trDun_Cath_S_Exit_Crypt_02
                [032999] trDun_Cath_W_Dead_End_01
                [033000] trDun_Cath_W_Entrance_01
                [033001] trDun_Cath_W_Exit_01
                [033002] trDun_Cath_W_Exit_Crypt_01
                [136166] trDun_Cath_E_DungeonStone_Exit_01
                [136167] trDun_Cath_N_DungeonStone_Exit_01
                [136168] trDun_Cath_S_DungeonStone_Exit_01
                [136170] trDun_Cath_W_DungeonStone_Exit_01
                [001883] trDun_Cath_EW_Hall_02
                [001884] trDun_Cath_NSW_01
                [001885] trDun_Cath_SEW_02
                [001886] trDun_Cath_W_01
                [091612] trDun_Cath_NSEW_02_Templar
                [060885] trDun_Cath_Cain_Intro_01
                [066589] trDun_Cath_NE_02
                [066919] trDun_Cath_NS_02
                [066925] trDun_Cath_SE_02
                [067021] trDun_Cath_EW_02
                */

            }

            if (worldAsset.SNOId == 117405)
            {

                //worldData.IsGenerated = true;
               
            }
            //trDun_Cave_Middle_RiverA - затопленный храм.
            //trDun_Cave_Middle_ChasmA - Пристанище войнов.
            //trDun_Cave_Middle_LostCampA - 

            if(worldAsset.SNOId == 119888)
            {
                worldData.IsGenerated = false;
                /*
                 
                [116976] trDun_Cave_Goat_NSEW_02
                [075138] trDun_Cave_Goat_S_Entrance_01
                [075141] trDun_Cave_Goat_NE_01
                [075144] trDun_Cave_Goat_NSW_01
                [075186] trDun_Cave_Goat_NS_01
                [075261] trDun_Cave_Goat_EW_01
                [075353] trDun_Cave_Goat_NSEW_01
                [075606] trDun_Cave_Goat_W_Entrance_01


                 */

            }

            if (worldData.IsGenerated)
            {
                Logger.Error("World {0} [{1}] is a dynamic world! Can't generate proper dynamic worlds yet!", worldAsset.Name, worldAsset.SNOId);

                if (!GenerateRandomDungeon(worldSNO, worldData))
                    return null;
            }

            var world = new World(game, worldSNO);
            var levelAreas = new Dictionary<int, List<Scene>>();
                       

            // Create a clusterID => Cluster Dictionary
            var clusters = new Dictionary<int, Mooege.Common.MPQ.FileFormats.SceneCluster>();
            foreach (var cluster in worldData.SceneClusterSet.SceneClusters)
                clusters[cluster.ClusterId] = cluster;

            // Scenes are not aligned to (0, 0) but apparently need to be -farmy
            float minX = worldData.SceneParams.SceneChunks.Min(x => x.PRTransform.Vector3D.X);
            float minY = worldData.SceneParams.SceneChunks.Min(x => x.PRTransform.Vector3D.Y);

            // Count all occurences of each cluster /fasbat
            var clusterCount = new Dictionary<int, int>();

            foreach (var sceneChunk in worldData.SceneParams.SceneChunks)
            {
                var cID = sceneChunk.SceneSpecification.ClusterID;
                if (cID != -1 && clusters.ContainsKey(cID)) // Check for wrong clusters /fasbat
                {
                    if (!clusterCount.ContainsKey(cID))
                        clusterCount[cID] = 0;
                    clusterCount[cID]++;
                }
            }

            // For each cluster generate a list of randomly selected subcenes /fasbat
            var clusterSelected = new Dictionary<int, List<Mooege.Common.MPQ.FileFormats.SubSceneEntry>>();
            foreach (var cID in clusterCount.Keys)
            {
                var selected = new List<Mooege.Common.MPQ.FileFormats.SubSceneEntry>();
                clusterSelected[cID] = selected;
                var count = clusterCount[cID];
                foreach (var group in clusters[cID].SubSceneGroups) // First select from each subscene group /fasbat
                {
                    for (int i = 0; i < group.I0 && count > 0; i++, count--) //TODO Rename I0 to requiredCount? /fasbat
                    {
                        var subSceneEntry = RandomHelper.RandomItem(group.Entries, entry => entry.Probability);
                        selected.Add(subSceneEntry);
                    }

                    if (count == 0)
                        break;
                }

                while (count > 0) // Fill the rest with defaults /fasbat
                {
                    //Default subscenes are not currently stored in db, use first if available
                    //var subSceneEntry = RandomHelper.RandomItem(clusters[cID].Default.Entries, entry => entry.Probability);
                    if (clusters[cID].SubSceneGroups.Count > 0)
                    {
                        var subSceneEntry = RandomHelper.RandomItem(clusters[cID].SubSceneGroups.First().Entries, entry => entry.Probability);
                        selected.Add(subSceneEntry);
                    }
                    count--;
                }
            }

            foreach (var sceneChunk in worldData.SceneParams.SceneChunks)
            {
                var position = sceneChunk.PRTransform.Vector3D - new Vector3D(minX, minY, 0);
                var scene = new Scene(world, position, sceneChunk.SNOHandle.Id, null)
                {
                    MiniMapVisibility = true,
                    RotationW = sceneChunk.PRTransform.Quaternion.W,
                    RotationAxis = sceneChunk.PRTransform.Quaternion.Vector3D,
                    SceneGroupSNO = -1
                    
                };

                // If the scene has a subscene (cluster ID is set), choose a random subscenes from the cluster load it and attach it to parent scene /farmy
                if (sceneChunk.SceneSpecification.ClusterID != -1)
                {
                    if (!clusters.ContainsKey(sceneChunk.SceneSpecification.ClusterID))
                    {
                        Logger.Warn("Referenced clusterID {0} not found for chunk {1} in world {2}", sceneChunk.SceneSpecification.ClusterID, sceneChunk.SNOHandle.Id, worldSNO);
                    }
                    else
                    {
                        var entries = clusterSelected[sceneChunk.SceneSpecification.ClusterID]; // Select from our generated list /fasbat
                        Mooege.Common.MPQ.FileFormats.SubSceneEntry subSceneEntry = null;

                        if (entries.Count > 0)
                        {
                            //subSceneEntry = entries[RandomHelper.Next(entries.Count - 1)];

                            subSceneEntry = RandomHelper.RandomItem<Mooege.Common.MPQ.FileFormats.SubSceneEntry>(entries, entry => 1); // TODO Just shuffle the list, dont random every time. /fasbat
                            entries.Remove(subSceneEntry);
                        }
                        else
                            Logger.Error("No SubScenes defined for cluster {0} in world {1}", sceneChunk.SceneSpecification.ClusterID, world.DynamicID);

                        Vector3D pos = FindSubScenePosition(sceneChunk); // TODO According to BoyC, scenes can have more than one subscene, so better enumerate over all subscenepositions /farmy

                        if (pos == null)
                        {
                            Logger.Error("No scene position marker for SubScenes of Scene {0} found", sceneChunk.SNOHandle.Id);
                        }
                        else
                        {
                            // subSceneEntry is null is there is no subSceneGroups, until we get default subScene saved in DB.
                            if (subSceneEntry != null)
                            {
                                // HACK: avoid trying to create scenes with SNOs that aren't valid
                                if (MPQStorage.Data.Assets[SNOGroup.Scene].ContainsKey(subSceneEntry.SNOScene))
                                {
                                    var subScenePosition = scene.Position + pos;
                                    var subscene = new Scene(world, subScenePosition, subSceneEntry.SNOScene, scene)
                                    {
                                        MiniMapVisibility = true,
                                        RotationW = sceneChunk.PRTransform.Quaternion.W,
                                        RotationAxis = sceneChunk.PRTransform.Quaternion.Vector3D,
                                        Specification = sceneChunk.SceneSpecification
                                    };
                                    scene.Subscenes.Add(subscene);
                                    subscene.LoadMarkers();
                                }
                                else
                                {
                                    Logger.Error("Scene not found in mpq storage: {0}", subSceneEntry.SNOScene);
                                }
                            }
                        }
                    }

                }
                scene.Specification = sceneChunk.SceneSpecification;
                try
                {
                    scene.LoadMarkers();
                }
                catch { }

                // add scene to level area dictionary
                foreach (var levelArea in scene.Specification.SNOLevelAreas)
                {
                    if (levelArea != -1)
                    {
                        if (!levelAreas.ContainsKey(levelArea))
                            levelAreas.Add(levelArea, new List<Scene>());

                        levelAreas[levelArea].Add(scene);
                    }
                }
            }

            loadLevelAreas(levelAreas, world);
            return world;
        }

        /// <summary>
        /// Status of an added exit to world
        /// Used when a new tile is needed in a specific place
        /// </summary>
        public enum ExitStatus
        {
            Free, //no tile in that direction
            Blocked, //"wall" in that direction
            Open //"path" in that direction
        }

        private static bool GenerateRandomDungeon(int worldSNO, Mooege.Common.MPQ.FileFormats.World worldData)
        {
            if (worldData.DRLGParams.Count == 0)
                return false;

            Dictionary<int, TileInfo> tiles = new Dictionary<int, TileInfo>();

            //Each DRLGParam is a level
            for (int paramIndex = 0; paramIndex < worldData.DRLGParams.Count; paramIndex++)
            {
                var drlgparam = worldData.DRLGParams[paramIndex];
                foreach (var tile in drlgparam.Tiles)
                {
                    Logger.Debug("RandomGeneration: TileType: {0}", (TileTypes)tile.TileType);
                    tiles.Add(tile.SNOScene, tile);
                }

                TileInfo entrance = new TileInfo();
                //HACK for Defiled Crypt as there is no tile yet with type 200. Maybe changing in DB would make more sense than putting this hack in
                //    [11]: {[161961, Mooege.Common.MPQ.MPQAsset]}Worlds\\a1trDun_Cave_Old_Ruins_Random01.wrl
                if (worldSNO == 161961)
                {
                    entrance = tiles[131902];
                    //tiles.Remove(131902);
                }
                else
                    entrance = GetTileInfo(tiles, TileTypes.Entrance);

                Vector3D initialStartTilePosition = new Vector3D(480, 480, 0);
                Dictionary<Vector3D, TileInfo> worldTiles = new Dictionary<Vector3D, TileInfo>();
                worldTiles.Add(initialStartTilePosition, entrance);
                AddAdjacentTiles(worldTiles, entrance, tiles, 0, initialStartTilePosition);
                AddFillers(worldTiles, tiles);

                foreach (var tile in worldTiles)
                {
                    AddTile(worldData, tile.Value, tile.Key);
                }

                //AddFiller
                ProcessCommands(drlgparam, worldData, paramIndex);
            }
            //Coordinates are added after selection of tiles and map
            //Leave it for Defiler Crypt debugging
            //AddTile(world, tiles[132218], new Vector3D(720, 480, 0));
            //AddTile(world, tiles[132203], new Vector3D(480, 240, 0));
            //AddTile(world, tiles[132263], new Vector3D(240, 480, 0));
            //return world;
            return true;
        }


        /// <summary>
        /// Processes the commands for generating world
        /// </summary>
        /// <param name="drlgParams"></param>
        /// <param name="worldData"></param>
        /// <param name="levelIndex"></param>
        private static void ProcessCommands(DRLGParams drlgParams, Mooege.Common.MPQ.FileFormats.World worldData, int levelIndex)
        {
            //Process commands
            foreach (var command in drlgParams.Commands)
            {
                //Adds information about level
                if (command.CommandType == (int)CommandType.Group)
                {
                    //  command.TagMap
                    //{Mooege.Core.GS.Common.Types.TagMap.TagMap}
                    //    _tagMapEntries: Count = 6
                    //    TagMapEntries: Count = 6
                    //    TagMapSize: 0
                    //command.TagMap.TagMapEntries
                    //Count = 6
                    //    [0]: {851986 = -1}
                    //    [1]: {1015841 = 1}
                    //    [2]: {851987 = -1}
                    //    [3]: {851993 = -1}
                    //    [4]: {1015822 = 0}
                    //    [5]: {851983 = 19780} //19780 LevelArea A1_trDun_Level01
                    //hardcode this now until proper tagmap implementation is done
                    foreach (var chunk in worldData.SceneParams.SceneChunks)
                    {
                        if (command.TagMap.ContainsKey(DRLGCommandKeys.Group.Level))
                            chunk.SceneSpecification.SNOLevelAreas[levelIndex] = command.TagMap[DRLGCommandKeys.Group.Level].Id;
                    }


                }
                if (command.CommandType == (int)CommandType.AddExit)
                {
                    //drlgparam.Commands[6].TagMap.TagMapEntries
                    //[0]: {852000 = -1}    Type SNO (2)
                    //[1]: {851984 = 60713} Type SNO (2) [20:16] (snobot) [1] 60713 Worlds trDun_Cain_Intro, 
                    //[2]: {1020032 = 1}    (0)
                    //[3]: {852050 = 0}     //Starting location? ID (7)
                    //[4]: {1015841 = 1}    (0)
                    //[5]: {852051 = 172}   //Destination Actor Tag (7)
                    //[6]: {1015814 = 0}    (0)
                    //[7]: {854612 = -1}    Type SNO (2)
                    //[8]: {1015813 = 300}  (0) Tiletype (exit)
                    //[9]: {1020416 = 1}    (0)
                    //[10]: {854613 = -1}   Type SNO (2)
                    //[11]: {1015821 = -1}  (0)

                    //find all tiles of TileType
                    //foreach (var tile in worldTiles)
                    //{

                    //}
                }
            }
        }

        /// <summary>
        /// Adds filler tiles around the world
        /// </summary>
        /// <param name="worldTiles"></param>
        /// <param name="tiles"></param>
        private static void AddFillers(Dictionary<Vector3D, TileInfo> worldTiles, Dictionary<int, TileInfo> tiles)
        {

            Dictionary<Vector3D, TileInfo> fillersToAdd = new Dictionary<Vector3D, TileInfo>();
            foreach (var tile in worldTiles)
            {
                Dictionary<TileExits, Vector3D> adjacentPositions = GetAdjacentPositions(tile.Key);
                foreach (var position in adjacentPositions)
                {
                    //Add filler to all free tiles (all exits should have been filled and the blocked ones don't need anything else)
                    if (GetExistStatus(worldTiles, position.Value, position.Key) == ExitStatus.Free)
                    {
                        //random filler
                        if (!fillersToAdd.ContainsKey(position.Value))
                            fillersToAdd.Add(position.Value, GetTileInfo(tiles, 0));
                    }
                }
            }

            foreach (var tile in fillersToAdd)
            {
                worldTiles.Add(tile.Key, tile.Value);
            }
        }

        /// <summary>
        /// Adds tiles to all exits of a tile
        /// </summary>
        /// <param name="worldTiles">Contains a list of already added tiles.</param>
        /// <param name="tileInfo">Originating tile</param>
        /// <param name="tiles">List of tiles to choose from</param>
        /// <param name="counter">Contains how many tiles were added. When counter reached it will look for an exit.
        /// If exit was not found look for deadend(filler?). </param>
        /// <param name="position">Position of originating tile.</param>
        /// <param name="x">Originating tile world x position</param>
        private static int AddAdjacentTiles(Dictionary<Vector3D, TileInfo> worldTiles, TileInfo tileInfo, Dictionary<int, TileInfo> tiles, int counter, Vector3D position)
        {
            Logger.Debug("Counter: {0}, ExitDirectionbitsOfGivenTile: {1}", counter, tileInfo.ExitDirectionBits);
            var lookUpExits = GetLookUpExitBits(tileInfo.ExitDirectionBits);

            Dictionary<TileExits, Vector3D> randomizedExitTypes = GetAdjacentPositions(position, true);

            //add adjacent tiles for each randomized direction
            foreach (var exit in randomizedExitTypes)
            {
                if ((lookUpExits & (int)exit.Key) > 0 && !worldTiles.ContainsKey(exit.Value))
                {
                    counter = AddadjacentTileAtExit(worldTiles, tiles, counter, exit.Value);
                }
            }

            return counter;
        }

        /// <summary>
        /// Adds an adjacent tile in the given exit position
        /// </summary>
        /// <param name="worldTiles"></param>
        /// <param name="tiles"></param>
        /// <param name="counter"></param>
        /// <returns></returns>
        private static int AddadjacentTileAtExit(Dictionary<Vector3D, TileInfo> worldTiles, Dictionary<int, TileInfo> tiles, int counter, Vector3D position)
        {
            TileTypes tileTypeToFind = TileTypes.Normal;
            if (counter > 5)
            {
                if (!ContainsTileType(worldTiles, TileTypes.Exit)) tileTypeToFind = TileTypes.Exit;
                else tileTypeToFind = TileTypes.EventTile1;
            }
            //Find if other exits are in the area of the new tile to add
            Dictionary<TileExits, ExitStatus> exitStatus = GetadjacentExitStatus(worldTiles, position);
            TileInfo newTile = GetTileInfo(tiles, (int)tileTypeToFind, exitStatus);
            if (newTile == null) return counter;
            worldTiles.Add(position, newTile);
            Logger.Debug("Added tile: Type: {0}, SNOScene: {1}, ExitTypes: {2}", newTile.TileType, newTile.SNOScene, newTile.ExitDirectionBits);
            counter = AddAdjacentTiles(worldTiles, newTile, tiles, counter + 1, position);
            return counter;
        }

        /// <summary>
        /// Returns the status of all exits for a specified position
        /// </summary>
        /// <param name="worldTiles">Tiles already added to world</param>
        /// <param name="position">Position</param>
        private static Dictionary<TileExits, ExitStatus> GetadjacentExitStatus(Dictionary<Vector3D, TileInfo> worldTiles, Vector3D position)
        {
            Dictionary<TileExits, ExitStatus> exitStatusDict = new Dictionary<TileExits, ExitStatus>();
            //Compute East adjacent Location
            Vector3D positionEast = new Vector3D(position.X + 240, position.Y, position.Z);
            ExitStatus exitStatusEast = GetExistStatus(worldTiles, positionEast, TileExits.West);
            exitStatusDict.Add(TileExits.East, exitStatusEast);

            Vector3D positionWest = new Vector3D(position.X - 240, position.Y, position.Z);
            ExitStatus exitStatusWest = GetExistStatus(worldTiles, positionWest, TileExits.East);
            exitStatusDict.Add(TileExits.West, exitStatusWest);

            Vector3D positionNorth = new Vector3D(position.X, position.Y + 240, position.Z);
            ExitStatus exitStatusNorth = GetExistStatus(worldTiles, positionNorth, TileExits.South);
            exitStatusDict.Add(TileExits.North, exitStatusNorth);

            Vector3D positionSouth = new Vector3D(position.X, position.Y - 240, position.Z);
            ExitStatus exitStatusSouth = GetExistStatus(worldTiles, positionSouth, TileExits.North);
            exitStatusDict.Add(TileExits.South, exitStatusSouth);

            return exitStatusDict;
        }

        /// <summary>
        /// Returns a dictionary of all positions adjacent to a tile
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isRandom"></param>
        private static Dictionary<TileExits, Vector3D> GetAdjacentPositions(Vector3D position, bool isRandom = false)
        {
            Vector3D positionEast = new Vector3D(position.X - 240, position.Y, 0);
            Vector3D positionWest = new Vector3D(position.X + 240, position.Y, 0);
            Vector3D positionNorth = new Vector3D(position.X, position.Y - 240, 0);
            Vector3D positionSouth = new Vector3D(position.X, position.Y + 240, 0);

            //get a random direction
            Dictionary<TileExits, Vector3D> exitTypes = new Dictionary<TileExits, Vector3D>();
            exitTypes.Add(TileExits.East, positionEast);
            exitTypes.Add(TileExits.West, positionWest);
            exitTypes.Add(TileExits.North, positionNorth);
            exitTypes.Add(TileExits.South, positionSouth);

            if (!isRandom)
                return exitTypes;

            //randomize
            Dictionary<TileExits, Vector3D> randomExitTypes = new Dictionary<TileExits, Vector3D>();
            var count = exitTypes.Count;

            //Randomise exit directions
            for (int i = 0; i < count; i++)
            {
                //Chose a random exit to test
                Vector3D chosenExitPosition = RandomHelper.RandomValue(exitTypes);
                var chosenExitDirection = (from pair in exitTypes
                                           where pair.Value == chosenExitPosition
                                           select pair.Key).FirstOrDefault();
                randomExitTypes.Add(chosenExitDirection, chosenExitPosition);
                exitTypes.Remove(chosenExitDirection);
            }

            return randomExitTypes;
        }

        private static bool ContainsTileType(Dictionary<Vector3D, TileInfo> worldTiles, TileTypes tileType)
        {
            foreach (var tileInfo in worldTiles)
            {
                if (tileInfo.Value.TileType == (int)tileType) return true;
            }
            return false;
        }

        /// <summary>
        /// Provides the exit status given position and exit (NSEW)
        /// </summary>
        /// <param name="worldTiles"></param>
        /// <param name="position"></param>
        /// <param name="exit"></param>
        /// <returns></returns>
        private static ExitStatus GetExistStatus(Dictionary<Vector3D, TileInfo> worldTiles, Vector3D position, TileExits exit)
        {
            if (!worldTiles.ContainsKey(position)) return ExitStatus.Free;
            else
            {
                if ((worldTiles[position].ExitDirectionBits & (int)exit) > 0) return ExitStatus.Open;
                else return ExitStatus.Blocked;
            }
        }

        /// <summary>
        /// Provides what entrances to look-up based on an entrance set of bits
        /// N means look for S
        /// S means look for N
        /// W means look for E
        /// E means look for W
        /// basically switch first two bits and last two bits
        /// </summary>
        /// <param name="exitDirectionBits"></param>
        /// <returns></returns>
        private static int GetLookUpExitBits(int exitDirectionBits)
        {
            return (((exitDirectionBits & ~3) & (int)0x4U) << 1 | ((exitDirectionBits & ~3) & (int)0x8U) >> 1)
                + (((exitDirectionBits & ~12) & (int)0x1U) << 1 | ((exitDirectionBits & ~12) & (int)0x2U) >> 1);
        }

        /// <summary>
        /// Get tileInfo with specific requirements
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="exitDirectionBits"></param>
        /// <param name="tileType"></param>
        /// <param name="exitStatus"></param>
        /// <returns></returns>
        private static TileInfo GetTileInfo(Dictionary<int, TileInfo> tiles, int tileType, Dictionary<TileExits, ExitStatus> exitStatus)
        {
            //get all exits that need to be in the new tile
            int mustHaveExits = 0;
            Dictionary<int, TileInfo> acceptedTiles = new Dictionary<int, TileInfo>();
            //By default use all tiles
            acceptedTiles = tiles;
            foreach (TileExits exit in System.Enum.GetValues(typeof(TileExits)))
            {
                if (exitStatus[exit] == ExitStatus.Open) mustHaveExits += (int)exit;
                //delete from the pool of tiles those that do have exits that are blocked
                if (exitStatus[exit] == ExitStatus.Blocked)
                {
                    acceptedTiles = acceptedTiles.Where(pair => (pair.Value.ExitDirectionBits & (int)exit) == 0).ToDictionary(pair => pair.Key, pair => pair.Value);
                }
            }
            Logger.Debug("Looking for tile with Exits: {0}", mustHaveExits);
            return GetTileInfo(acceptedTiles.Where(pair => pair.Value.TileType == tileType).ToDictionary(pair => pair.Key, pair => pair.Value), mustHaveExits);
        }

        /// <summary>
        /// Returns a tileinfo from a list of tiles that has specific exit directions
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="exitDirectionBits"></param>
        /// <returns></returns>
        private static TileInfo GetTileInfo(Dictionary<int, TileInfo> tiles, int exitDirectionBits)
        {
            //if no exit direction bits return filler
            if (exitDirectionBits == 0)
            {
                //return filler
                return GetTileInfo(tiles, TileTypes.Filler);
            }

            List<TileInfo> tilesWithRightDirection = (from pair in tiles where ((pair.Value.ExitDirectionBits & exitDirectionBits) > 0) select pair.Value).ToList<TileInfo>();
            if (tilesWithRightDirection.Count == 0)
            {
                Logger.Debug("Did not find matching tile");
                //TODO: Never return null. Try to find other tiles that match entry pattern and rotate
                //There should be a field that defines if tile can be rotated
                return null;
            }

            return RandomHelper.RandomItem(tilesWithRightDirection, x => (x.Probability / 100));
        }

        /// <summary>
        /// Returns a tileinfo from a list of tiles that has a specific type
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="exitDirectionBits"></param>
        /// <returns></returns>
        private static TileInfo GetTileInfo(Dictionary<int, TileInfo> tiles, TileTypes tileType)
        {
            var tilesWithRightType = (from pair in tiles where (pair.Value.TileType == (int)tileType) select pair.Value);
            return RandomHelper.RandomItem(tilesWithRightType, x => 1);
        }

        private static void AddTile(Mooege.Common.MPQ.FileFormats.World worldData, TileInfo tileInfo, Vector3D location)
        {
            var sceneChunk = new SceneChunk();
            sceneChunk.SNOHandle = new SNOHandle(tileInfo.SNOScene);
            sceneChunk.PRTransform = new PRTransform();
            sceneChunk.PRTransform.Quaternion = new Quaternion();
            sceneChunk.PRTransform.Quaternion.W = 1.0f;
            sceneChunk.PRTransform.Quaternion.Vector3D = new Vector3D(0, 0, 0);
            sceneChunk.PRTransform.Vector3D = new Vector3D();
            sceneChunk.PRTransform.Vector3D = location;

            var spec = new SceneSpecification();
            //scene.Specification = spec;
            spec.Cell = new Vector2D() { X = 0, Y = 0 };
            spec.CellZ = 0;
            spec.SNOLevelAreas = new int[] { -1, -1, -1, -1 };
            spec.SNOMusic = -1;
            spec.SNONextLevelArea = -1;
            spec.SNONextWorld = -1;
            spec.SNOPresetWorld = -1;
            spec.SNOPrevLevelArea = -1;
            spec.SNOPrevWorld = -1;
            spec.SNOReverb = -1;
            spec.SNOWeather = 50542;
            spec.SNOCombatMusic = -1;
            spec.SNOAmbient = -1;
            spec.ClusterID = -1;
            spec.Unknown1 = 14;
            spec.Unknown3 = 5;
            spec.Unknown4 = -1;
            spec.Unknown5 = 0;
            spec.SceneCachedValues = new SceneCachedValues();
            spec.SceneCachedValues.Unknown1 = 63;
            spec.SceneCachedValues.Unknown2 = 96;
            spec.SceneCachedValues.Unknown3 = 96;
            var sceneFile = MPQStorage.Data.Assets[SNOGroup.Scene][tileInfo.SNOScene];
            var sceneData = (Mooege.Common.MPQ.FileFormats.Scene)sceneFile.Data;
            spec.SceneCachedValues.AABB1 = sceneData.AABBBounds;
            spec.SceneCachedValues.AABB2 = sceneData.AABBMarketSetBounds;
            spec.SceneCachedValues.Unknown4 = new int[4] { 0, 0, 0, 0 };

            sceneChunk.SceneSpecification = spec;

            worldData.SceneParams.SceneChunks.Add(sceneChunk);
            worldData.SceneParams.ChunkCount++;
        }

        /// <summary>
        /// Loads content for level areas. Call this after scenes have been generated and after scenes have their GizmoLocations
        /// set (this is done in Scene.LoadActors right now)
        /// </summary>
        /// <param name="levelAreas">Dictionary that for every level area has the scenes it consists of</param>
        /// <param name="world">The world to which to add loaded actors</param>
        private static void loadLevelAreas(Dictionary<int, List<Scene>> levelAreas, World world)
        {
            /// Each Scene has one to four level areas assigned to it. I dont know if that means
            /// the scene belongs to both level areas or if the scene is split
            /// Scenes marker tags have generic GizmoLocationA to Z that are used 
            /// to provide random spawning possibilities.
            /// For each of these 26 LocationGroups, the LevelArea has a entry in its SpawnType array that defines
            /// what type of actor/encounter/adventure could spawn there
            /// 
            /// It could for example define, that for a level area X, out of the four spawning options
            /// two are randomly picked and have barrels placed there

            // Create an array of mobs, used with the loadActor in the load monster area loop
            // Each monster are created in Mooege.Core.GS.Actors.Implementations.Monsters
            // By Poluxxx
            //int[] aSNO = new int[] {
            //       // 6652      // Zombie
            //         6443      // Ravenous
            //        //, 136943    // Ghost
            //};
            Dictionary<PRTransform, Mooege.Common.MPQ.FileFormats.Actor> dict = new Dictionary<PRTransform, Mooege.Common.MPQ.FileFormats.Actor>();
            foreach (int la in levelAreas.Keys)
            {
                SNOHandle levelAreaHandle = new SNOHandle(SNOGroup.LevelArea, la);
                if (!levelAreaHandle.IsValid)
                {
                    Logger.Warn("Level area {0} does not exist", la);
                    continue;
                }
                var levelArea = levelAreaHandle.Target as LevelArea;

                for (int i = 0; i < 26; i++)
                {
                    // Merge the gizmo starting locations from all scenes and
                    // their subscenes into a single list for the whole level area
                    List<PRTransform> gizmoLocations = new List<PRTransform>();
                    foreach (var scene in levelAreas[la])
                    {
                        if (scene.GizmoSpawningLocations[i] != null)
                            gizmoLocations.AddRange(scene.GizmoSpawningLocations[i]);
                        foreach (Scene subScene in scene.Subscenes)
                        {
                            if (subScene.GizmoSpawningLocations[i] != null)
                                gizmoLocations.AddRange(subScene.GizmoSpawningLocations[i]);
                        }
                    }

                    // Load all spawns that are defined for that location group 
                    foreach (GizmoLocSpawnEntry spawnEntry in levelArea.LocSet.SpawnType[i].SpawnEntry)
                    {
                        // Get a random amount of spawns ...
                        int amount = RandomHelper.Next(spawnEntry.Max, spawnEntry.Max);
                        if (amount > gizmoLocations.Count)
                        {
                            Logger.Trace("Breaking after spawnEntry {0} for LevelArea {1} because there are less locations ({2}) than spawn amount ({3}, {4} min)", spawnEntry.SNOHandle, levelAreaHandle, gizmoLocations.Count, amount, spawnEntry.Min);
                            break;
                        }

                        Logger.Trace("Spawning {0} ({3} - {4} {1} in {2}", amount, spawnEntry.SNOHandle, levelAreaHandle, spawnEntry.Min, spawnEntry.Max);

                        // ...and place each one on a random position within the location group
                        for (; amount > 0; amount--)
                        {
                            int location = RandomHelper.Next(gizmoLocations.Count - 1);

                            switch (spawnEntry.SNOHandle.Group)
                            {
                                case SNOGroup.Actor:
                                    //TODO: Why to pass tagmap here and not load it inside Actor
                                    loadActor(spawnEntry.SNOHandle, gizmoLocations[location], world, ((Mooege.Common.MPQ.FileFormats.Actor)spawnEntry.SNOHandle.Target).TagMap);
                                    break;
                                case SNOGroup.Encounter:
                                    var encounter = spawnEntry.SNOHandle.Target as Encounter;
                                    var actor = RandomHelper.RandomItem(encounter.Spawnoptions, x => x.Probability);
                                    var actorHandle = new SNOHandle(actor.SNOSpawn);
                                    loadActor(actorHandle, gizmoLocations[location], world, ((Mooege.Common.MPQ.FileFormats.Actor)actorHandle.Target).TagMap);
                                    break;
                                case SNOGroup.Adventure:
                                    // Adventure are basically made up of a markerSet that has relative PRTransforms
                                    // it has some other fields that are always 0 and a reference to a symbol actor
                                    // no idea what they are used for - farmy

                                    var adventure = spawnEntry.SNOHandle.Target as Adventure;
                                    var markerSet = new SNOHandle(adventure.SNOMarkerSet).Target as MarkerSet;

                                    foreach (var marker in markerSet.Markers)
                                    {
                                        // relative marker set coordinates to absolute world coordinates
                                        var absolutePRTransform = new PRTransform
                                        {
                                            Vector3D = marker.PRTransform.Vector3D + gizmoLocations[location].Vector3D,
                                            Quaternion = new Quaternion
                                            {
                                                Vector3D = new Vector3D(marker.PRTransform.Quaternion.Vector3D.X, marker.PRTransform.Quaternion.Vector3D.Y, marker.PRTransform.Quaternion.Vector3D.Z),
                                                W = marker.PRTransform.Quaternion.W
                                            }
                                        };
                                        switch (marker.Type)
                                        {
                                            case MarkerType.Actor:
                                                loadActor(marker.SNOHandle, absolutePRTransform, world, marker.TagMap);
                                                break;

                                            case MarkerType.Encounter:
                                                var encounter2 = marker.SNOHandle.Target as Encounter;
                                                var actor2 = RandomHelper.RandomItem(encounter2.Spawnoptions, x => x.Probability);
                                                loadActor(new SNOHandle(actor2.SNOSpawn), absolutePRTransform, world, marker.TagMap);
                                                break;

                                            default:
                                                Logger.Warn("Unhandled marker type {0} in actor loading", marker.Type);
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    if (spawnEntry.SNOHandle.Id != -1)
                                        Logger.Warn("Unknown sno handle in LevelArea spawn entries: {0}", spawnEntry.SNOHandle);
                                    break;
                            }

                            // dont use that location again
                            gizmoLocations.RemoveAt(location);

                        }
                    }
                }

                // Load monsters for level area
                foreach (var scene in levelAreas[la])
                {
                    // HACK: don't spawn monsters in tristram town scenes /mdz
                    if (MPQStorage.Data.Assets[SNOGroup.Scene][scene.SceneSNO.Id].Name.StartsWith("trOut_Tristram_"))
                        continue;

                    // a little variety in monsters spawned
                    int[] monsterActors = { 6652, 219725, 5346, 6356, 5393, 434, 4982 };

                    for (int i = 0; i < 100; i++)
                    {
                        if (RandomHelper.NextDouble() > 0.8)
                        {
                            // TODO Load correct spawn population
                            // 2.5 is units per square, TODO: Find out how to calculate units per square. Is it F1 * V0.I1 / SquareCount?
                            int x = RandomHelper.Next(scene.NavMesh.SquaresCountX);
                            int y = RandomHelper.Next(scene.NavMesh.SquaresCountY);

                            if ((scene.NavMesh.Squares[y * scene.NavMesh.SquaresCountX + x].Flags & Mooege.Common.MPQ.FileFormats.Scene.NavCellFlags.NoSpawn) == 0)
                            {
                                loadActor(
                                    new SNOHandle(monsterActors[RandomHelper.Next(monsterActors.Length)]),
                                    new PRTransform
                                    {
                                        Vector3D = new Vector3D
                                        {
                                            X = (float)(x * 2.5 + scene.Position.X),
                                            Y = (float)(y * 2.5 + scene.Position.Y),
                                            Z = scene.NavMesh.Squares[y * scene.NavMesh.SquaresCountX + x].Z + scene.Position.Z
                                        },
                                        Quaternion = Quaternion.FacingRotation((float)(RandomHelper.NextDouble() * System.Math.PI * 2))
                                    },
                                    world,
                                    new TagMap()
                                    );
                            }
                        }
                    }
                }
            }
        }

        //TODO: Move this out as loading actors can happen even after world was generated
        public static uint loadActor(SNOHandle actorHandle, PRTransform location, World world, TagMap tagMap)
        {
            var actor = Mooege.Core.GS.Actors.ActorFactory.Create(world, actorHandle.Id, tagMap);

            if (actor == null)
            {
                if (actorHandle.Id != -1)
                    Logger.Warn("ActorFactory did not load actor {0}", actorHandle);
                return 0;
            }

            actor.RotationW = location.Quaternion.W;
            actor.RotationAxis = location.Quaternion.Vector3D;
            actor.EnterWorld(location.Vector3D);
            return actor.DynamicID;
        }

        /// <summary>
        /// Loads all markersets of a scene and looks for the one with the subscene position
        /// </summary>
        private static Vector3D FindSubScenePosition(Mooege.Common.MPQ.FileFormats.SceneChunk sceneChunk)
        {
            var mpqScene = MPQStorage.Data.Assets[SNOGroup.Scene][sceneChunk.SNOHandle.Id].Data as Mooege.Common.MPQ.FileFormats.Scene;

            foreach (var markerSet in mpqScene.MarkerSets)
            {
                var mpqMarkerSet = MPQStorage.Data.Assets[SNOGroup.MarkerSet][markerSet].Data as Mooege.Common.MPQ.FileFormats.MarkerSet;
                foreach (var marker in mpqMarkerSet.Markers)
                    if (marker.Type == Mooege.Common.MPQ.FileFormats.MarkerType.SubScenePosition)
                        return marker.PRTransform.Vector3D;
            }
            return null;
        }
    }
}