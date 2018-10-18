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
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.GS.Actors.Interactions;
using Mooege.Core.GS.Ticker;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _139823 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }


        public _139823()
            : base(139823)
        {

        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Death Of King Event ");
            
            //Берём участников сцены
            var LeoricGhost = world.GetActorBySNO(5365);
            var GhostKnights = world.GetActorsBySNO(4182);
            var LachdananGhost = world.GetActorBySNO(4183);
            var SwordPlace = world.GetActorBySNO(163449);
            //Вычесляем взгляд для участников
            float LeoricFacingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(LeoricGhost, LachdananGhost);
            float LachdananFacingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(LachdananGhost, LeoricGhost);


            LeoricGhost.SetFacingRotation(LeoricFacingAngle);
            LachdananGhost.SetFacingRotation(LachdananFacingAngle);
            //4 секунды до убийства
            //Убийство + запуск нового диалога

            
            foreach (var GKnight in GhostKnights)
            {
                float KnightFacing = Actors.Movement.MovementHelpers.GetFacingAngle(GKnight, world.GetActorBySNO(220219));
                GKnight.SetFacingRotation(KnightFacing);
            }
            LachdananGhost.Move(SwordPlace.Position, LachdananFacingAngle);

            TickTimer Timeout = new SecondsTickTimer(world.Game, 4f);
            var ListenerKingSkeletons = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
            ListenerKingSkeletons.ContinueWith(delegate
            {
                StartConversation(world, 139825);
            });
        }

        //Launch Conversations.
        private bool StartConversation(Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId); // this does the job of sending the proper stuff :p
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
    }
}