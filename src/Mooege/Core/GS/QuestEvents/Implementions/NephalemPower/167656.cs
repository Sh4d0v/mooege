﻿/*
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
    class _167656 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;
        List<uint> monstersAlive = new List<uint> { };

        public _167656()
            : base(167656)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Conversation done ");
            if (HadConversation)
            {
                HadConversation = false;
            }
            
            var firgtRobber = world.GetActorBySNO(4373); //graveRobber_B
            var secondRobber = world.GetActorBySNO(4376); //graveRobber_C
            var thirdRobber = world.GetActorBySNO(4373); //graveRobber_D_NPC- 177539
            monstersAlive.Add(firgtRobber.DynamicID);
            monstersAlive.Add(secondRobber.DynamicID);
            monstersAlive.Add(thirdRobber.DynamicID);
            

            firgtRobber.Attributes[GameAttribute.Hitpoints_Max] = 20000000f;
            firgtRobber.Attributes[GameAttribute.Hitpoints_Cur] = 20000000f;

            secondRobber.Attributes[GameAttribute.Hitpoints_Max] = 20000000f;
            secondRobber.Attributes[GameAttribute.Hitpoints_Cur] = 20000000f;
            
            thirdRobber.Attributes[GameAttribute.Hitpoints_Max] = 20000000f;
            thirdRobber.Attributes[GameAttribute.Hitpoints_Cur] = 20000000f;
            var ListenerBandit = Task<bool>.Factory.StartNew(() => OnKillListener(monstersAlive, world));
            
            StartConversation(world, 167677);
            ListenerBandit.ContinueWith(delegate
            {
                world.Game.Quests.Advance(72738);
                
               StartConversation(world, 111899);
            });
        }

        
        private bool OnKillListener(List<uint> monstersAlive, Map.World world)
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