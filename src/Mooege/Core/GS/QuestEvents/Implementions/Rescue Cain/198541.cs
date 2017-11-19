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


namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _198541 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();

        public _198541()
            : base(198541)
        {
        }

        public override void Execute(Map.World world)
        {
            // test kill leah
            if (world.HasActor(72))
            {
                world.GetActorByDynamicId(72).Destroy();
            }
            Logger.Debug(" RESCUE CAIN QUEST STARTED ");
            //Logger.Debug(" Quests.Advance(72095) ");
            //world.Game.Quests.Advance(72095);
            Logger.Debug(" Conversation(190404) ");
            StartConversation(world, 190404);
            Logger.Debug(" Conversation(166678) ");
            StartConversation(world, 166678); // "let me open the gate" need if in Old Ruins
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