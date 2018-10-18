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
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Players;
using Mooege.Core.GS.Ticker;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _198925 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;
        List<uint> monstersAlive = new List<uint> { };
        private Player player;
        private TickTimer Timeout;
        public _198925()
            : base(198925)
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
                       
            Timeout = new SecondsTickTimer(world.Game, 2f);
            var WaitToWalk = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
            WaitToWalk.ContinueWith(delegate
            {
                world.SpawnMonster(6024, player.Position);
                world.SpawnMonster(6024, player.Position);
                world.SpawnMonster(6024, player.Position);
                world.SpawnMonster(6024, player.Position);
                world.SpawnMonster(6024, player.Position);
                List<uint> KillList = new List<uint> { };
                var Cultists = world.GetActorsBySNO(6024);
                foreach (var monste in Cultists)
                {
                    KillList.Add(monste.DynamicID);
                }
                var ListenerSkeletons = Task<bool>.Factory.StartNew(() => OnKillListener(KillList, world));
                world.Game.Quests.Advance(72738);
                //Ждём пока убьют
                ListenerSkeletons.ContinueWith(delegate
                {
                    world.Game.Quests.Advance(72738);
                    StartConversation(world, 120086);
                });

                StartConversation(world, 133487);

            });


        }

        private bool OnKillListener(List<uint> monstersAlive, Map.World world)
        {
            System.Int32 monstersKilled = 0;
            var monsterCount = monstersAlive.Count;
            while (monstersKilled != monsterCount)
            {
                
                for (int i = monstersAlive.Count - 1; i >= 0; i--)
                {
                    if (world.HasMonster(monstersAlive[i]))
                    {

                    }
                    else
                    {
                        Logger.Debug(monstersAlive[i] + " has been killed");
                        monstersAlive.RemoveAt(i);
                        monstersKilled++;
                    }
                }
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