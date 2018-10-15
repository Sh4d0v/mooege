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
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.GS.Actors.Interactions;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Players;
using Mooege.Core.GS.Ticker;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _133372 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;
        List<uint> monstersAlive = new List<uint> { };
        private Player player;
        private TickTimer Timeout;
        public _133372()
            : base(133372)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Conversation done ");
            if (HadConversation)
            {
                HadConversation = false;
            }

            foreach (var playin in world.Players)
            {
                if (playin.Value.PlayerIndex == 0)
                    player = playin.Value;
            }
            //world.Game.Quests.Advance(72738);
            #region Дверь
            var Gate = world.GetActorBySNO(177439);
            var WalkableGate = new Actors.Implementations.Door(world, 177439, Gate.Tags);
            WalkableGate.Field2 = 16;
            WalkableGate.RotationAxis = Gate.RotationAxis;
            WalkableGate.RotationW = Gate.RotationW;
            WalkableGate.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
            //NoDownGate.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
            WalkableGate.Attributes[GameAttribute.Gizmo_State] = 1;
            WalkableGate.Attributes[GameAttribute.Untargetable] = true;
            WalkableGate.Attributes.BroadcastChangedIfRevealed();
            WalkableGate.EnterWorld(Gate.Position);
            Gate.Destroy();

            world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
            {
                ActorID = WalkableGate.DynamicID,
                Field1 = 5,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                {
                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                        {
                            Duration = 300,
                            AnimationSNO = WalkableGate.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                            PermutationIndex = 0,
                            Speed = 0.9f
                        }
                }
            }, WalkableGate);
            world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
            {
                ActorID = WalkableGate.DynamicID,
                AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
            }, WalkableGate);
            #endregion

            Timeout = new SecondsTickTimer(world.Game, 6f);
            var WaitToWalk = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
            WaitToWalk.ContinueWith(delegate
            {
                StartConversation(world, 198925);
            });


        }

        private bool WaitToSpawn(TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

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