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

using Mooege.Common.Logging;
using Mooege.Core.GS.Games;
using Mooege.Core.GS.Players;
using Mooege.Core.MooNet.Toons;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Act;
using Mooege.Net.GS.Message.Definitions.Connection;
using Mooege.Net.GS.Message.Definitions.Game;
using Mooege.Net.GS.Message.Definitions.Hero;
using Mooege.Net.GS.Message.Definitions.Misc;
using System;
using Mooege.Common.Extensions;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.GS.Actors.Implementations.Hirelings;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.AI.Brains;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mooege.Net.GS
{
    public class ClientManager : IMessageConsumer
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();
        private static readonly ClientManager _instance = new ClientManager();
        public static ClientManager Instance { get { return _instance; } }
        private static ThreadLocal<Random> _threadRand = new ThreadLocal<Random>(() => new Random());
        public static Random Rand { get { return _threadRand.Value; } }


        public void OnConnect(object sender, ConnectionEventArgs e)
        {
            Logger.Trace("Game-Client connected: {0}", e.Connection.ToString());

            var gameClient = new GameClient(e.Connection);
            e.Connection.Client = gameClient;
        }

        public void OnDisconnect(object sender, ConnectionEventArgs e)
        {
            Logger.Trace("Client disconnected: {0}", e.Connection.ToString());
            GameManager.RemovePlayerFromGame((GameClient)e.Connection.Client);
        }

        public void Consume(GameClient client, GameMessage message)
        {
            if (message is JoinBNetGameMessage) OnJoinGame(client, (JoinBNetGameMessage)message);

            //Остреливаем левый портал
            var FalsePortal = client.Player.World.GetActorBySNO(5648);
            //client.Player.World.Leave(FalsePortal);
            //FalsePortal.Destroy();
            //Тестовая проверка прохождения // Пока только синг.
            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(client.Player.Toon.PersistentID);
            var world = client.Player.World;
          
            #region Акт 1

            #region Акт 1 Квест 2 - Наследие декарда каина
            if (dbQuestProgress.ActiveQuest == 72095)
            {
                #region Перемотка ко второму квесту
                for (int Rem = 0; Rem < 8; Rem++)
                {
                    world.Game.Quests.Advance(87700);
                }
                #endregion
                var LeahBrains = world.GetActorByDynamicId(72);

                if (LeahBrains != null)
                {
                    Logger.Debug("Вышибаем SNO {0}, мир содершит {1} ", LeahBrains.ActorSNO, world.GetActorsBySNO(3739).Count);
                    world.Leave(LeahBrains);
                    world.Leave(world.GetActorByDynamicId(75));

                }
                if (dbQuestProgress.StepOfQuest == -1 || dbQuestProgress.StepOfQuest == 0 || dbQuestProgress.StepOfQuest == 1 || dbQuestProgress.StepOfQuest == 2)
                {
                    world.Leave(world.GetActorByDynamicId(83));
                    Hireling LeahFriend = new Hireling(world, LeahBrains.ActorSNO.Id, LeahBrains.Tags);
                    var NewPoint = new Vector3D(LeahBrains.Position.X, LeahBrains.Position.Y + 5, LeahBrains.Position.Z);
                    LeahFriend.Brain = new MinionBrain(LeahFriend);
                    LeahFriend.Attributes[GameAttribute.Untargetable] = false;
                    LeahFriend.GBHandle.Type = 4;
                    LeahFriend.GBHandle.GBID = 717705071;
                    LeahFriend.Attributes[GameAttribute.Pet_Creator] = client.Player.PlayerIndex;
                    LeahFriend.Attributes[GameAttribute.Pet_Type] = 0;
                    LeahFriend.Attributes[GameAttribute.Pet_Owner] = client.Player.PlayerIndex;
                    LeahFriend.Position = RandomDirection(client.Player.Position, 3f, 8f);
                    LeahFriend.RotationW = LeahBrains.RotationW;
                    LeahFriend.RotationAxis = LeahBrains.RotationAxis;
                    LeahFriend.EnterWorld(NewPoint);
                    LeahFriend.Attributes[GameAttribute.Level] = 6;
                    client.Player.ActiveHireling = LeahFriend;
                    client.Player.SelectedNPC = null;
                    LeahFriend.Brain.Activate();
                }
                if (dbQuestProgress.StepOfQuest == -1 || dbQuestProgress.StepOfQuest == 0 || dbQuestProgress.StepOfQuest == 1)
                {
                    var NewTristramPortal = world.GetActorByDynamicId(34);
                    Player MasterPlayer = client.Player;
                    var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnUseTeleporterListener(NewTristramPortal.DynamicID, world));

                }
                if (dbQuestProgress.StepOfQuest == -1 || dbQuestProgress.StepOfQuest == 0 || dbQuestProgress.StepOfQuest == 1 || dbQuestProgress.StepOfQuest == 2)
                {

                    Player MasterPlayer = client.Player;
                    var ListenerEnterToOldTristram = Task<bool>.Factory.StartNew(() => OnListenerToEnter(MasterPlayer, world));

                    ListenerEnterToOldTristram.ContinueWith(delegate //Once killed:
                    {
                        Logger.Debug("Enter to Road Objective done "); // Waypoint_OldTristram
                        var ListenerEnterToAdriaEnter = Task<bool>.Factory.StartNew(() => OnListenerToAndriaEnter(MasterPlayer, world));
                    });
                }
                if (dbQuestProgress.StepOfQuest == 3)
                {
                    Player MasterPlayer = client.Player;
                    var ListenerEnterToAdriaEnter = Task<bool>.Factory.StartNew(() => OnListenerToAndriaEnter(MasterPlayer, world));
                }
                if (dbQuestProgress.StepOfQuest > 3 && dbQuestProgress.StepOfQuest < 11)
                {
                    LeahBrains.EnterWorld(LeahBrains.Position);
                }
                
                if (dbQuestProgress.StepOfQuest == 12)
                {
                    Player MasterPlayer = client.Player;
                    var CainIntroWorld = client.Player.World.Game.GetWorld(60713);
                    //var ListenerEnterToAdriaEnter = Task<bool>.Factory.StartNew(() => OnListenerToAndriaEnter(MasterPlayer, CainIntroWorld));
                    var minions = CainIntroWorld.GetActorsBySNO(80652);
                    List<uint> SkilletKiller = new List<uint> { };

                    foreach (var minion in minions)
                    {
                        SkilletKiller.Add(minion.DynamicID);
                    }
                    var CainKillerEvent = Task<bool>.Factory.StartNew(() => OnKillListenerCain(SkilletKiller, CainIntroWorld));
                    CainKillerEvent.ContinueWith(delegate
                    {
                        world.Game.Quests.Advance(72095);
                        dbQuestProgress.StepOfQuest = 13;
                    });
                }
            }

            #endregion

            #region Акт 1 Квест 3 - Сломанная корона
            if(dbQuestProgress.ActiveQuest == 72221)
            {
                #region Перемотка ко второму квесту
                for (int Rem = 0; Rem < 8; Rem++)
                {
                    world.Game.Quests.Advance(87700);
                }
                world.Leave(world.GetActorByDynamicId(75));
                #endregion
                #region Перемотка ко третьему квесту
                for (int Rem = 0; Rem < 15; Rem++)
                {
                    world.Game.Quests.Advance(72095);
                }
                world.Leave(world.GetActorByDynamicId(25));
                //world.Game.Quests.Advance(72095);
                #endregion
            }
            #endregion

            #endregion
            #region Основная проверка
            if (dbQuestProgress.ActiveQuest != -1)
            {
                #region Нижнии ворота тристрама
                var DownGate = world.GetActorBySNO(90419);
                world.BroadcastIfRevealed(new Message.Definitions.Animation.PlayAnimationMessage
                {
                    ActorID = DownGate.DynamicID,
                    Field1 = 5,
                    Field2 = 0,
                    tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                                {
                    new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                    {
                        Duration = 50,
                        AnimationSNO = DownGate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                        PermutationIndex = 0,
                        Speed = 1
                    }
                                }

                }, DownGate);

                world.BroadcastIfRevealed(new Message.Definitions.Animation.SetIdleAnimationMessage
                {
                    ActorID = DownGate.DynamicID,
                    AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID
                }, DownGate);
                DownGate.Attributes[GameAttribute.Gizmo_State] = 1;
                DownGate.Attributes.BroadcastChangedIfRevealed();
                #endregion

                if (dbQuestProgress.StepOfQuest > 0)
                {
                    // Вышибаем лею                      
                    var actorToShoot = world.GetActorByDynamicId(72);
                    if (dbQuestProgress.ActiveQuest == 87700)
                    {
                        if (actorToShoot != null)
                        {
                            Logger.Debug("Вышибаем SNO {0}, мир содершит {1} ", actorToShoot.ActorSNO, world.GetActorsBySNO(3739).Count);
                            world.Leave(actorToShoot);
                        }
                        else
                        {
                            Logger.Debug("Вышибать некого");
                        }
                    }

                    Logger.Warn("Обнаружен начатый квест {0}", dbQuestProgress.ActiveQuest);
                    for (int CS = 0; CS < dbQuestProgress.StepOfQuest; CS++)
                    {
                        world.Game.Quests.Advance(dbQuestProgress.ActiveQuest);
                        //Logger.Warn("Обнаруженно прохождение квеста {0}, шаг квеста {1]", dbQuestProgress.ActiveQuest, dbQuestProgress.StepOfQuest);
                    }
                    Logger.Warn("Обнаружено Прохождение квеста {0}, шаг - {1}", dbQuestProgress.ActiveQuest, dbQuestProgress.StepOfQuest);
                }
                else
                {
                    world.Game.Quests.Advance(dbQuestProgress.ActiveQuest);
                    Logger.Warn("Обнаружен начатый квест {0}", dbQuestProgress.ActiveQuest);
                }
                if (dbQuestProgress.ActiveQuest == 87700)
                {
                    if (dbQuestProgress.StepOfQuest == 8)
                    {
                        world.Game.Quests.NotifyQuest(87700, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.InteractWithActor, 192164);
                    }
                }
            }
            #endregion

            #region Сырые локации

          

            #endregion
            #region Заполним немного локации))
            Vector3D[] Points = new Vector3D[4];
            Points[0] = new Vector3D(2427.788f, 2852.193f, 27.1f);
            Points[1] = new Vector3D(2356.931f, 2528.715f, 27.1f);
            Points[2] = new Vector3D(2119.563f, 2489.693f, 27.1f);
            Points[3] = new Vector3D(2021.855f, 2774.645f, 40.05685f);
            int FatZombieAID = 6652;
            int RisenZombieAID = 6644;
            //Ugly add monster, скучно))
            Vector3D[] MobPosSpawn = new Vector3D[50];
            foreach (Vector3D Point in Points)
            {
                for (int a = 0; a < 6; a++)
                {
                    world.SpawnMonster(RisenZombieAID, RandomDirection(Point, 10f, 80f));
                }
                for (int a = 0; a < 4; a++)
                {
                    world.SpawnMonster(FatZombieAID, RandomDirection(Point, 10f, 80f));                    
                }
            }

            #endregion
            DBSessions.AccountSession.Flush();
        }
        public Vector3D RandomDirection(Vector3D position, float minRadius, float maxRadius)
        {
            float angle = (float)(Mooege.Core.GS.QuestEvents.Implementations._198541.Rand.NextDouble() * Math.PI * 2);
            float radius = minRadius + (float)Mooege.Core.GS.QuestEvents.Implementations._198541.Rand.NextDouble() * (maxRadius - minRadius);
            return new Vector3D(position.X + (float)Math.Cos(angle) * radius,
                                position.Y + (float)Math.Sin(angle) * radius,
                                position.Z);
        }

        #region Отслеживания для Акт 1 - Квест 2
        private bool OnUseTeleporterListener(uint actorDynID, Core.GS.Map.World world)
        {
            if (world.HasActor(actorDynID))
            {
                var actor = world.GetActorByDynamicId(actorDynID); // it is not null :p

                //Logger.Debug(" supposed portal has type {3} has name {0} and state {1} , has gizmo  been operated ? {2} ", actor.NameSNOId, actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_State], actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated], actor.GetType());

                while (true)
                {
                    if (actor.Attributes[Net.GS.Message.GameAttribute.Gizmo_Has_Been_Operated])
                    {
                        world.Game.Quests.Advance(72095);
                        foreach (var playerN in world.Players)
                        {
                            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
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
        private bool OnListenerToEnter(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                sceneID = player.CurrentScene.SceneSNO.Id;
                if (sceneID == 90198) //90923 - Adria House
                {
                    bool ActivePortal = true;
                    
                    foreach (var playerN in world.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                        if (dbQuestProgress.StepOfQuest < 2)
                            ActivePortal = false;
                        dbQuestProgress.ActiveQuest = 72095;
                        dbQuestProgress.StepOfQuest = 3;
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }
                    if(ActivePortal == true)
                        world.Game.Quests.Advance(72095);
                    else { world.Game.Quests.Advance(72095); world.Game.Quests.Advance(72095); }
                    StartConversation(world, 166678);
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
                    world.Game.Quests.Advance(72095); world.Game.Quests.Advance(72095);
                    break;
                }
            }

            return true;
        }
            private bool OnKillListenerCain(List<uint> monstersAlive, Core.GS.Map.World world)
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
        #endregion

        private bool StartConversation(Core.GS.Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
            }
            return true;
        }
        private void OnJoinGame(GameClient client, JoinBNetGameMessage message)
        {
            var game = GameManager.GetGameById(message.GameId);
            if (game == null)
            {
                Logger.Warn("Client provided message.GameId doesnt exists, dropping him..");
                client.Connection.Disconnect();
                return;
            }
            lock (game)
            {
                var toon = ToonManager.GetToonByLowID((ulong)message.ToonEntityId.Low);

                client.Game = game;

                if (toon.GameAccount.LoggedInClient == null || toon.Dead)
                {
                    Logger.Warn("Client doesn't seem to be connected to moonet, dropping him..");
                    client.Connection.Disconnect();
                    return; // if moonet connection is lost, don't allow him to get in.
                }

                // Set references between MooNetClient and GameClient.
                client.BnetClient = toon.GameAccount.LoggedInClient;
                client.BnetClient.InGameClient = client;

                client.Player = new Player(game.StartingWorld, client, toon);
                Logger.Info("Player {0}[PlayerIndex: {1}] connected.", client.Player.Toon.Name, client.Player.PlayerIndex);

                client.SendMessage(new VersionsMessage(message.SNOPackHash));

                client.SendMessage(new ConnectionEstablishedMessage
                {
                    PlayerIndex = client.Player.PlayerIndex,
                    Field1 = 0x4BB91A16,
                    SNOPackHash = message.SNOPackHash,
                });

                client.SendMessage(new GameSetupMessage // should be the current tick for the game /raist.
                {
                    Field0 = game.TickCounter,
                });

                client.SendMessage(new SavePointInfoMessage
                {
                    snoLevelArea = -1,
                });

                client.SendMessage(new HearthPortalInfoMessage
                {
                    snoLevelArea = -1,
                    Field1 = -1,
                });

                // transition player to act so client can load act related data? /raist
                client.SendMessage(new ActTransitionMessage
                {
                    Field0 = 0x00000000,
                    Field1 = true,
                });

                toon.LoginTime = DateTimeExtensions.ToUnixTime(DateTime.UtcNow);
                Logger.Trace("Log in time:" + toon.LoginTime.ToString());

                game.Enter(client.Player);
            }
        }
    }
}
