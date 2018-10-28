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

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _81576 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;
        List<uint> monstersAlive = new List<uint> { };
        private Player player;
        public _81576()
            : base(81576)
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

            var ListenerAwayTempleTask = Task<bool>.Factory.StartNew(() => OnAwayTempleListener(player, world));
            ListenerAwayTempleTask.ContinueWith(delegate
            {
                #region Мост
                var Bridge = world.GetActorBySNO(100849);

                var WalkableBridge = new Actors.Implementations.Door(world, 100849, Bridge.Tags);
                WalkableBridge.Field2 = 16;
                WalkableBridge.RotationAxis = Bridge.RotationAxis;
                WalkableBridge.RotationW = Bridge.RotationW;
                WalkableBridge.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                //NoDownGate.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                WalkableBridge.Attributes[GameAttribute.Gizmo_State] = 1;
                WalkableBridge.Attributes[GameAttribute.Untargetable] = true;
                WalkableBridge.Attributes.BroadcastChangedIfRevealed();
                WalkableBridge.EnterWorld(Bridge.Position);
                Bridge.Destroy();

                world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                {
                    ActorID = WalkableBridge.DynamicID,
                    Field1 = 5,
                    Field2 = 0,
                    tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                    {
                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                        {
                            Duration = 300,
                            AnimationSNO = WalkableBridge.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Opening],
                            PermutationIndex = 0,
                            Speed = 0.9f
                        }
                    }
                }, WalkableBridge);
                world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                {
                    ActorID = WalkableBridge.DynamicID,
                    AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.Open.ID,
                }, WalkableBridge);
                #endregion

                var ListenerToWoodTask = Task<bool>.Factory.StartNew(() => OnWoodListener(player, world));
                ListenerToWoodTask.ContinueWith(delegate
                {


                    world.Game.Quests.Advance(72738);
                    foreach (var player in world.Players)
                    {

                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                        dbQuestProgress.ActiveQuest = 72738;
                        dbQuestProgress.StepOfQuest = 10;
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                        Logger.Debug(" Progress Saved ");

                    }

                });
            });


        }

        private bool OnAwayTempleListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {

            while (true)
            {
                try
                {
                    int sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 58974)
                    {
                        break;
                    }
                }
                catch { }
            }
            return true;
        }
        private bool OnWoodListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {

            while (true)
            {
                try
                {
                    int sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID == 79216)
                    {
                        break;
                    }
                }
                catch { }
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