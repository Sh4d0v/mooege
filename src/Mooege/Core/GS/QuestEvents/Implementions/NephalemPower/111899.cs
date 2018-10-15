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

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _111899 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;
        List<uint> monstersAlive = new List<uint> { };
        private Player player;
        public _111899()
            : base(111899)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Conversation done ");
            if (HadConversation)
            {
                HadConversation = false;
            }

        
            //Открываем выход с фермы

            foreach (var playin in world.Players)
            {
                if (playin.Value.PlayerIndex == 0)
                    player = playin.Value;
            }

            var ListenerAwayTask = Task<bool>.Factory.StartNew(() => OnAwayZoneTFListener(player, world));
            //Подходим к поталу
            ListenerAwayTask.ContinueWith(delegate
            {
                world.Game.Quests.Advance(72738);
                var ListenerToTempleTask = Task<bool>.Factory.StartNew(() => OnNierTempleListener(player, world));
                ListenerToTempleTask.ContinueWith(delegate
                {
                    world.Game.Quests.Advance(72738);
                });
                StartConversation(world, 116881);
            });
               
           
        }

        private bool OnAwayZoneTFListener(Core.GS.Players.Player player, Core.GS.Map.World world)
        {

            while (true)
            {
                try
                {
                    int sceneID = player.CurrentScene.SceneSNO.Id;
                    if (sceneID != 78212)
                    {
                        break;
                    }
                }
                catch { }
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