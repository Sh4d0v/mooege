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
        private bool FinishedToMove = false;
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

            if (HadConversation)
            {
                HadConversation = false;
                Logger.Debug(" RESCUE CAIN QUEST STARTED ");
                Logger.Debug(" Quests.Advance(72095) ");
                world.Game.Quests.Advance(72095);
            }
            //Logger.Debug(" Conversation(190404) ");
            //StartConversation(world, 190404);
            //Берём Лею
            var LeahBrains = world.GetActorByDynamicId(83);
            LeahBrains.OnLeave(world);
            Hireling LeahFriend = new Hireling(world, LeahBrains.ActorSNO.Id, LeahBrains.Tags);
            LeahFriend.Brain = new MinionBrain(LeahFriend);
            var NewPoint = new Vector3D(LeahBrains.Position.X, LeahBrains.Position.Y + 5, LeahBrains.Position.Z);
            LeahFriend.Attributes[GameAttribute.Untargetable] = false;
            
            
            foreach (var player in world.Players)
            {
                if (player.Value.PlayerIndex == 0)
                {
                    LeahFriend.GBHandle.Type = 4;
                    LeahFriend.GBHandle.GBID = 717705071;

                    LeahFriend.Attributes[GameAttribute.Pet_Creator] = player.Value.PlayerIndex;
                    LeahFriend.Attributes[GameAttribute.Pet_Type] = 0;
                    LeahFriend.Attributes[GameAttribute.Pet_Owner] = player.Value.PlayerIndex;
                    LeahFriend.Position = RandomDirection(player.Value.Position, 3f, 8f);
                    LeahFriend.RotationW = LeahBrains.RotationW;
                    LeahFriend.RotationAxis = LeahBrains.RotationAxis;
                    LeahFriend.EnterWorld(NewPoint);
                    LeahFriend.Attributes[GameAttribute.Level] = 6;
                    player.Value.ActiveHireling = LeahFriend;
                    player.Value.SelectedNPC = null;
                    LeahFriend.Brain.Activate();

                }
            }
                
            foreach (var player in world.Players)
            {
                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);

                dbQuestProgress.ActiveQuest = 72095;
                dbQuestProgress.StepOfQuest = 1;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
            };
            

          //  hireling.Teleport(NewPoint);
            
            //LeahBrains.Destroy();
            //Берем портал
            //var NewTristramPortal = world.GetActorByDynamicId(34);

          /*  var WaitingOfWalk = Task<bool>.Factory.StartNew(() => OnKillListener(LeahBrains,NewTristramPortal, world));
            WaitingOfWalk.ContinueWith(delegate
            {
                //StartConversation(world, 151156);
                if (world.HasActor(83))
                {
                    LeahBrains.Destroy();
                    Logger.Debug(" Удаляем Лею ");
                }
                //world.Game.Quests.Advance(72095);
            });*/

            
            //Тупо кординаты портала
            //Vector3D ToPortal = new Vector3D(2981.73f, 2835.009f, 24.66344f);

            //Создаём направляющий угол для движения Леи
            //float Angle = MovementHelpers.GetFacingAngle(LeahBrains.Position, NewTristramPortal.Position);
            
            //Отправляю Лею к порталу под нужным углом)) Profit ;)
            //LeahBrains.Move(NewTristramPortal.Position, Angle);
            
                                    
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