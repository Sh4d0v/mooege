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
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Skills;
using Mooege.Core.GS.Powers;
using Mooege.Core.GS.Ticker;
using Mooege.Core.GS.Actors.Implementations.Minions;

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

             //client.Player.World.Leave(FalsePortal);

            //Тестовая проверка прохождения // Пока только синг.
            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(client.Player.Toon.PersistentID);
            var world = client.Player.World;
            //Остреливаем левый портал. НАХЕР ЭТО ГАВНО ИЗ ГОРОДА
            try
            {
                var FalsePortal = client.Player.World.GetActorBySNO(191492);
                FalsePortal.Destroy();
            } catch { }
            
            //[050453] [UI] Quests
            //[050454] [UI] QuestUpdate
            //[174218] [UI] QuestReward
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
                    Hireling LeahFriend = new Scoundrel(world, LeahBrains.ActorSNO.Id, LeahBrains.Tags);
                    LeahFriend.Brain = new HirelingBrain(LeahFriend);
                   // SetBrain(new MinionBrain(this));
                    LeahFriend.Attributes[GameAttribute.Untargetable] = false;
                    LeahFriend.GBHandle.Type = 4;
                    LeahFriend.GBHandle.GBID = 717705071;
                    LeahFriend.Master = client.Player;
                    LeahFriend.Attributes[GameAttribute.Pet_Creator] = client.Player.PlayerIndex;
                    LeahFriend.Attributes[GameAttribute.Pet_Type] = 0;
                    LeahFriend.Attributes[GameAttribute.Pet_Owner] = client.Player.PlayerIndex;
                    LeahFriend.RotationW = LeahBrains.RotationW;
                    LeahFriend.RotationAxis = LeahBrains.RotationAxis;
                    LeahFriend.EnterWorld(RandomDirection(client.Player.Position, 3f, 8f));
                    LeahFriend.Attributes[GameAttribute.Level] = 6;
                    client.Player.ActiveHireling = LeahFriend;
                    

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
                //world.Leave(world.GetActorByDynamicId(72));
                world.Leave(world.GetActorByDynamicId(75));
                #region Перемотка ко второму квесту
                for (int Rem = 0; Rem < 8; Rem++)
                {
                    world.Game.Quests.Advance(87700);
                }
                //world.Leave(world.GetActorByDynamicId(75));
                #endregion
                #region Перемотка ко третьему квесту
                for (int Rem = 0; Rem < 15; Rem++)
                {
                    world.Game.Quests.Advance(72095);
                }
                world.Leave(world.GetActorByDynamicId(25));
                #endregion
                if (dbQuestProgress.StepOfQuest >= 0 && dbQuestProgress.StepOfQuest < 5)
                {
                    var BlacksmithVendor = world.GetActorBySNO(56947);
                    Vector3D position = new Vector3D(BlacksmithVendor.Position);
                    //world.SpawnMonster(65036, position);// NonVendor - 65036
                    var BlacksmithQuest = world.GetActorBySNO(65036);
                    BlacksmithQuest.RotationAxis = BlacksmithVendor.RotationAxis;
                    BlacksmithQuest.RotationW = BlacksmithVendor.RotationW;
                    world.Leave(BlacksmithVendor);
                }
                if(dbQuestProgress.StepOfQuest == 6)
                {
                    var ListenerEnterToGraveyard = Task<bool>.Factory.StartNew(() => OnListenerToEnterGraveyard(client.Player, world));
                    ListenerEnterToGraveyard.ContinueWith(delegate
                    {
                        Logger.Debug("Enter to Road Objective done ");
                    });
                }
                

            }

            #endregion

            #region Акт 1 Квест 4 - Правление короля скелетов
            if (dbQuestProgress.ActiveQuest == 72061)
            {
                world.Leave(world.GetActorByDynamicId(75));
                #region Перемотка ко второму квесту
                for (int Rem = 0; Rem < 8; Rem++)
                {
                    world.Game.Quests.Advance(87700);
                }
                //world.Leave(world.GetActorByDynamicId(75));
                #endregion
                #region Перемотка ко третьему квесту
                for (int Rem = 0; Rem < 15; Rem++)
                {
                    world.Game.Quests.Advance(72095);
                }
                world.Leave(world.GetActorByDynamicId(25));
                #endregion
                #region Перемотка к четвертому квесту
                for (int Rem = 0; Rem < 10; Rem++)
                {
                    world.Game.Quests.Advance(72221);
                }
                var BlacksmithVendor = world.GetActorBySNO(56947);
                Vector3D position = new Vector3D(BlacksmithVendor.Position);
                world.SpawnMonster(56947, position);// NonVendor - 65036
                #endregion

                if (dbQuestProgress.StepOfQuest < 2)
                {
                    var ListenerUsePortalTask = Task<bool>.Factory.StartNew(() => OnEnterToParkListener(client.Player, world));
                    //Wait for portal to be used or player going to scene.
                    ListenerUsePortalTask.ContinueWith(delegate
                    {
                        Logger.Debug(" Waypoint_Park Objective done ");

                    });
                }

            }
            #endregion

            #region Акт 1 Квест 5 - Меч незнакомца
            if (dbQuestProgress.ActiveQuest == 117779)
            {
                world.Leave(world.GetActorByDynamicId(75));
                #region Перемотка ко второму квесту
                for (int Rem = 0; Rem < 8; Rem++)
                {
                    world.Game.Quests.Advance(87700);
                }
                //world.Leave(world.GetActorByDynamicId(75));
                #endregion
                #region Перемотка ко третьему квесту
                for (int Rem = 0; Rem < 15; Rem++)
                {
                    world.Game.Quests.Advance(72095);
                }
                world.Leave(world.GetActorByDynamicId(25));
                #endregion
                #region Перемотка к четвертому квесту
                for (int Rem = 0; Rem < 10; Rem++)
                {
                    world.Game.Quests.Advance(72221);
                }
                var BlacksmithVendor = world.GetActorBySNO(56947);
                world.Leave(BlacksmithVendor);
                Vector3D position = new Vector3D(BlacksmithVendor.Position);
                world.SpawnMonster(56947, position);// NonVendor - 65036
                

                #endregion
                #region Перемотка к пятому квесту
                for (int Rem = 0; Rem < 18; Rem++)
                {
                    world.Game.Quests.Advance(72061);
                }
                #endregion
                
                Vector3D PointToTyrael = new Vector3D(2940.182f, 2792.239f, 24.04533f);
                //world.SpawnMonster(117365, PointToTyrael);
                //Открытие ворот
                var ListenerNierDoorsTask = Task<bool>.Factory.StartNew(() => OnNierDoorsListener(client.Player, world));
                //Wait for portal to be used or player going to scene.
                ListenerNierDoorsTask.ContinueWith(delegate
                {
                    Logger.Debug(" Waypoint_Park Objective done ");
                    //No Lock 230324
                    //Lock 216574
                    var Locked = world.GetActorBySNO(216574);
                    var Unlocked = world.GetActorBySNO(230324);
                    world.Leave(Locked);
                    world.BroadcastIfRevealed(new PlayAnimationMessage
                    {
                        ActorID = Unlocked.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                    {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 1000,
                                AnimationSNO = Unlocked.AnimationSet.TagMapAnimDefault[Mooege.Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 1
                            }
                    }
                    }, Unlocked);

                    world.BroadcastIfRevealed(new SetIdleAnimationMessage
                    {
                        ActorID = Unlocked.DynamicID,
                        AnimationSNO = Unlocked.AnimationSet.TagMapAnimDefault[Mooege.Core.GS.Common.Types.TagMap.AnimationSetKeys.Open]
                    }, Unlocked);

                });

                if (dbQuestProgress.StepOfQuest < 2)
                {
                    var ListenerTristramFieldsTask = Task<bool>.Factory.StartNew(() => OnFieldsListener(client.Player, world));
                    ListenerTristramFieldsTask.ContinueWith(delegate
                    {
                        Logger.Debug("Добро пожаловать в гиблые поля");
                        var ListenerFoundCaveTask = Task<bool>.Factory.StartNew(() => OnFoundCaveListener(client.Player, world));
                        ListenerFoundCaveTask.ContinueWith(delegate
                        {
                            Logger.Debug("Пещера Хазра найдена");
                        });
                    });
                }
                if (dbQuestProgress.StepOfQuest == 2)
                {
                    //OnFoundCaveListener
                    var ListenerFoundCaveTask = Task<bool>.Factory.StartNew(() => OnFoundCaveListener(client.Player, world));
                    ListenerFoundCaveTask.ContinueWith(delegate
                    {
                        Logger.Debug("Пещера Хазра найдена");
                    });
                }
                //World - Fields_Cave_SwordOfJustice_Level01 [119888]

            }
            #endregion

            #region Акт 1 Квест 6 - Меч незнакомца
            if (dbQuestProgress.ActiveQuest == 72738)
            {
                world.Leave(world.GetActorByDynamicId(72));
                //world.Leave(world.GetActorByDynamicId(75));
                #region Перемотка ко второму квесту
                for (int Rem = 0; Rem < 7; Rem++)
                {
                    world.Game.Quests.Advance(87700);
                }
                #endregion
                #region Перемотка ко третьему квесту
                for (int Rem = 0; Rem < 15; Rem++)
                {
                    world.Game.Quests.Advance(72095);
                }
                //world.Leave(world.GetActorByDynamicId(25));
                #endregion
                #region Перемотка к четвертому квесту
                for (int Rem = 0; Rem < 9; Rem++)
                {
                    world.Game.Quests.Advance(72221);
                }
                /*var BlacksmithVendor = world.GetActorBySNO(56947);
                world.Leave(BlacksmithVendor);
                Vector3D position = new Vector3D(BlacksmithVendor.Position);
                world.SpawnMonster(56947, position);// NonVendor - 65036*/


                #endregion
                #region Перемотка к пятому квесту
                for (int Rem = 0; Rem < 18; Rem++)
                {
                    world.Game.Quests.Advance(72061);
                }
                #endregion
                #region Перемотка к шестому квесту
                for (int Rem = 0; Rem < 6; Rem++)
                {
                    world.Game.Quests.Advance(117779);
                }
                //118037
                
                #endregion

                Vector3D PointToTyrael = new Vector3D(2940.182f, 2792.239f, 24.04533f);
             //   world.SpawnMonster(117365, PointToTyrael);
                //Открытие ворот
                var ListenerNierDoorsTask = Task<bool>.Factory.StartNew(() => OnNierDoorsListener(client.Player, world));
                //Открытие ворот в гиблые поля.
                ListenerNierDoorsTask.ContinueWith(delegate
                {
                    Logger.Debug(" Waypoint_Park Objective done ");
                    //No Lock 230324
                    //Lock 216574
                    var Locked = world.GetActorBySNO(216574);
                    var Unlocked = world.GetActorBySNO(230324);
                    world.Leave(Locked);
                    world.BroadcastIfRevealed(new PlayAnimationMessage
                    {
                        ActorID = Unlocked.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                    {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 1000,
                                AnimationSNO = Unlocked.AnimationSet.TagMapAnimDefault[Mooege.Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 1
                            }
                    }
                    }, Unlocked);

                    world.BroadcastIfRevealed(new SetIdleAnimationMessage
                    {
                        ActorID = Unlocked.DynamicID,
                        AnimationSNO = Unlocked.AnimationSet.TagMapAnimDefault[Mooege.Core.GS.Common.Types.TagMap.AnimationSetKeys.Open]
                    }, Unlocked);

                });

                if (dbQuestProgress.StepOfQuest < 2)
                {
                    var ListenerFirstTask = Task<bool>.Factory.StartNew(() => OnNierZoneTFListener(client.Player, world));
                    //Подходим к подъему 78212
                    ListenerFirstTask.ContinueWith(delegate
                    {
                        Logger.Debug(" Waypoint_Park Objective done ");
                        world.Game.Quests.Advance(72738);


                        TickTimer Timeout = new SecondsTickTimer(world.Game, 5f);
                        var Waiter = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
                    //Ждём пока убьют
                    Waiter.ContinueWith(delegate
                        {
                        //ScoundrelNPC-80812 / Dyn.1088
                        var ScoundrelNPC = world.GetActorBySNO(80812);
                            Vector3D FinishToPath = new Vector3D(1568.468f, 839.8882f, 28.78354f);
                            var facingAngle = Core.GS.Actors.Movement.MovementHelpers.GetFacingAngle(ScoundrelNPC, FinishToPath);
                            ScoundrelNPC.Move(FinishToPath, facingAngle);

                            StartConversation(world, 111893);
                        //ScoundrelNPC.Attributes[GameAttribute.Unte] = true;
                        //MoveTo - 
                        Vector3D MoveTo = new Vector3D(1530.305f, 857.0227f, 39.23478f);
                            var facingAnglenew = Core.GS.Actors.Movement.MovementHelpers.GetFacingAngle(ScoundrelNPC, MoveTo);
                            ScoundrelNPC.Move(MoveTo, facingAnglenew);

                        //167677 - Перед боем
                    });



                    });
                }
                if (dbQuestProgress.StepOfQuest > 11)
                {
                    var ListenerToTempleTask = Task<bool>.Factory.StartNew(() => OnNierTempleListener(client.Player, world));
                    ListenerToTempleTask.ContinueWith(delegate
                    {
                        #region Дверь
                        var Door = client.Player.World.GetActorBySNO(100967);
                        client.Player.World.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                        {
                            ActorID = Door.DynamicID,
                            Field1 = 5,
                            Field2 = 0,
                            tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                                        {
                                            Duration = 300,
                                            AnimationSNO = Door.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                            PermutationIndex = 0,
                                            Speed = 0.9f
                                        }
                            }
                        }, Door);
                        client.Player.World.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                        {
                            ActorID = Door.DynamicID,
                            AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                        }, Door);
                        #endregion
                        #region Мост
                        var Bridge = client.Player.World.GetActorBySNO(144149);
                        client.Player.World.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                        {
                            ActorID = Bridge.DynamicID,
                            Field1 = 5,
                            Field2 = 0,
                            tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                                        {
                                            Duration = 300,
                                            AnimationSNO = Bridge.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                            PermutationIndex = 0,
                                            Speed = 0.9f
                                        }
                            }
                        }, Bridge);
                        client.Player.World.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                        {
                            ActorID = Bridge.DynamicID,
                            AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                        }, Bridge);
                        #endregion
                    });
                    
                }
                // */
            }


            #endregion

            #region Акт 1 Квест 7 - Судьба Вортема
            if (dbQuestProgress.ActiveQuest == 73236)
            {
                world.Leave(world.GetActorByDynamicId(72));
                //world.Leave(world.GetActorByDynamicId(75));
                #region Перемотка ко второму квесту
                for (int Rem = 0; Rem < 7; Rem++)
                {
                    world.Game.Quests.Advance(87700);
                }
                #endregion
                #region Перемотка ко третьему квесту
                for (int Rem = 0; Rem < 15; Rem++)
                {
                    world.Game.Quests.Advance(72095);
                }
                //world.Leave(world.GetActorByDynamicId(25));
                #endregion
                #region Перемотка к четвертому квесту
                for (int Rem = 0; Rem < 9; Rem++)
                {
                    world.Game.Quests.Advance(72221);
                }
                /*var BlacksmithVendor = world.GetActorBySNO(56947);
                world.Leave(BlacksmithVendor);
                Vector3D position = new Vector3D(BlacksmithVendor.Position);
                world.SpawnMonster(56947, position);// NonVendor - 65036*/


                #endregion
                #region Перемотка к пятому квесту
                for (int Rem = 0; Rem < 18; Rem++)
                {
                    world.Game.Quests.Advance(72061);
                }
                #endregion
                #region Перемотка к шестому квесту
                for (int Rem = 0; Rem < 6; Rem++)
                {
                    world.Game.Quests.Advance(117779);
                }
                #endregion
                #region Перемотка к седьмому квесту
                for (int Rem = 0; Rem < 18; Rem++)
                {
                    world.Game.Quests.Advance(72738);
                }
                #endregion

                //Открытие ворот
                var ListenerNierDoorsTask = Task<bool>.Factory.StartNew(() => OnNierDoorsListener(client.Player, world));
                //Открытие ворот в гиблые поля.
                ListenerNierDoorsTask.ContinueWith(delegate
                {
                    Logger.Debug(" Waypoint_Park Objective done ");
                    //No Lock 230324
                    //Lock 216574
                    var Locked = world.GetActorBySNO(216574);
                    var Unlocked = world.GetActorBySNO(230324);
                    world.Leave(Locked);
                    world.BroadcastIfRevealed(new PlayAnimationMessage
                    {
                        ActorID = Unlocked.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                    {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 1000,
                                AnimationSNO = Unlocked.AnimationSet.TagMapAnimDefault[Mooege.Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 1
                            }
                    }
                    }, Unlocked);

                    world.BroadcastIfRevealed(new SetIdleAnimationMessage
                    {
                        ActorID = Unlocked.DynamicID,
                        AnimationSNO = Unlocked.AnimationSet.TagMapAnimDefault[Mooege.Core.GS.Common.Types.TagMap.AnimationSetKeys.Open]
                    }, Unlocked);

                });

                // */
            }


            #endregion

            #endregion

            #region Основная проверка
            if (dbQuestProgress.ActiveQuest != -1)
            {
                #region Нижнии ворота тристрама
                var DownGate = world.GetActorBySNO(90419);
                DownGate.Attributes[GameAttribute.Gizmo_State] = 1;
                DownGate.Attributes.BroadcastChangedIfRevealed();
                world.BroadcastIfRevealed(new Message.Definitions.Animation.SetIdleAnimationMessage
                {
                    ActorID = DownGate.DynamicID,
                    AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID
                }, DownGate);
                #endregion
                //Убираем телегу
                var FactorToShoot = world.GetActorBySNO(81699);
                FactorToShoot.Destroy();
                #region Убираем телегу
                if (dbQuestProgress.ActiveQuest != 87700 && dbQuestProgress.ActiveQuest != 72095)
                {
                    var TELEGAS = world.GetActorsBySNO(112131);
                    Vector3D LastTelega = new Vector3D();
                    foreach (var TELEGA in TELEGAS)
                    {
                        TELEGA.Destroy();
                        LastTelega = TELEGA.Position;
                    }
                }
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
                    world.Game.Quests.CurrentQuest(dbQuestProgress.ActiveQuest);
                    try { world.Game.Quests.Advance(dbQuestProgress.ActiveQuest); }
                    catch{ }
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

            #region Покои Короля

            //Покои: 117405
            // To Tyrael Zone
            /*
            [148748] a1dun_Leor_Tyrael_Back_Skybox_01
            [135396] a1dun_Leor_Tyrael_jail_01
            [135521] a1dun_Leor_Tyrael_Stairs_A_01
            [135710] a1dun_Leor_Tyrael_Filler_02
            [117035] trDun_Crypt_W_Exit_Stranger_01
            
            */
            //  var Leor_Tyrael = client.Player.World.Game.GetWorld(117405);
            Vector3D PointToScene = new Vector3D(0f, 0f, 0f);
         //   Core.GS.Map.Scene CainScene = new Core.GS.Map.Scene(Leor_Tyrael, PointToScene, 135521, null);
            #endregion

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
                if (player.World.WorldSNO.Id == 71150)
                {
                    sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 90196) //90923 - Adria House
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
                        if (ActivePortal == true)
                        { StartConversation(world, 166678); }
                        else { world.Game.Quests.Advance(72095); StartConversation(world, 166678); }
                        try
                        {
                            var Gate = world.GetActorBySNO(108466);
                            world.BroadcastIfRevealed(new Message.Definitions.Animation.PlayAnimationMessage
                            {
                                ActorID = Gate.DynamicID,
                                Field1 = 5,
                                Field2 = 0,
                                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                            new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                            {
                                Duration = 50,
                                AnimationSNO = Gate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                                PermutationIndex = 0,
                                Speed = 1
                            }
                            }
                            }, Gate);

                            world.BroadcastIfRevealed(new Message.Definitions.Animation.SetIdleAnimationMessage
                            {
                                ActorID = Gate.DynamicID,
                                AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID
                            }, Gate);
                            if (player.ActiveHireling != null)
                            {
                                var HirelingToLeave = player.ActiveHireling;
                                world.Leave(HirelingToLeave);
                                Vector3D OutDoor = new Vector3D(1896.382f, 2782.988f, 32.85f);
                                Vector3D NearDoor = new Vector3D(1935.697f, 2792.971f, 40.23627f);
                                var Leah_Back = world.GetActorByDynamicId(83);
                                Leah_Back.EnterWorld(OutDoor);
                            }

                        } catch {}
                        break;
                    }
                }
            }

            return true;
        }
        private bool OnListenerToAndriaEnter(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
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
                        world.Game.Quests.Advance(72095); world.Game.Quests.Advance(72095);
                        break;
                    }
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

        #region Отслеживания для Акт 1 - Квест 3
        private bool OnListenerToEnterGraveyard(Core.GS.Players.Player player, Core.GS.Map.World world)
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

                        }

                        if (ActivePortal == true)
                        {
                            world.Game.Quests.Advance(72221);
                            DBSessions.AccountSession.Flush();
                        }
                        break;
                    }
                }
            }

            return true;
        }

        #endregion

        #region Отслеживания для Акт 1 - Квест 4
        private bool OnEnterToParkListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                try
                {
                    sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 93992)
                    {
                        world.Game.Quests.NotifyQuest(72061, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterLevelArea, 19938);
                        world.Game.Quests.Advance(72061);
                        foreach (var playerN in world.Players)
                        {
                            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                            dbQuestProgress.ActiveQuest = 72061;
                            dbQuestProgress.StepOfQuest = 2;
                            DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                            DBSessions.AccountSession.Flush();
                        }
                        break;
                    }
                }
                catch { }
            }
            return true;
        }
        #endregion

        #region Отслеживания для Акт 1 - Квест 5
        private bool OnNierDoorsListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                try
                {
                    sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 74617)
                    {
                        break;
                    }
                }
                catch { }
            }
            return true;
        }
        //OnFieldsListener
        private bool OnFieldsListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                try
                {
                    sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 56593)
                    {
                        world.Game.Quests.Advance(117779);
                        foreach (var playerN in world.Players)
                        {
                            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                            dbQuestProgress.ActiveQuest = 117779;
                            dbQuestProgress.StepOfQuest = 2;
                            DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                            DBSessions.AccountSession.Flush();
                        }
                        break;
                    }
                }
                catch { }
            }
            return true;
        }
        //OnFoundCave
        private bool OnFoundCaveListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            var scene = player.CurrentScene;
            while (true)
            {
                try
                {
                    scene = player.CurrentScene;
                    if (scene.Subscenes.Count > 0)
                    {
                        if (scene.Subscenes[0].SceneSNO.Id == 118195)
                        {
                            world.Game.Quests.Advance(117779);
                            foreach (var playerN in world.Players)
                            {
                                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                                dbQuestProgress.ActiveQuest = 117779;
                                dbQuestProgress.StepOfQuest = 3;
                                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                                DBSessions.AccountSession.Flush();
                                //
                                StartConversation(world, 130225);
                            }
                            break;
                        }
                    }
                }
                catch { }
            }
            return true;
        }
        #endregion

        #region Отслеживания для Акт 1 - Квест 6
        private bool OnNierZoneTFListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {
            int sceneID = player.CurrentScene.SceneSNO.Id;
            while (true)
            {
                try
                {
                    sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 78212)
                    {
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
        private bool OnNierTempleListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {

            while (true)
            {
                try
                {
                    int sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 60695)
                    {
                        break;
                    }
                }
                catch { }
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
