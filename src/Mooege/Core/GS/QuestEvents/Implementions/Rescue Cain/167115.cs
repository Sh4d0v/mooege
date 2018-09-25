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

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _167115 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public _167115()
            : base(167115)
        {
        }

        static int CapitanDaltynAID = 156801; //Actor ID Капитана Далтина
        List<uint> CapitanDaltynKiller = new List<uint> { }; // Используем для отслеживания убийства
        

        public override void Execute(Map.World world)
        {

            var actor = world.GetActorBySNO(CapitanDaltynAID);
            if (actor == null)
            {
                Logger.Debug("Не найдено: Капитан Далтин - {0}", CapitanDaltynAID);
                Vector3D CapitanDaltyn = new Vector3D(156.3844f, 53.71516f, 3.051758E-05f);
                world.SpawnMonster(CapitanDaltynAID, CapitanDaltyn);
                actor = world.GetActorBySNO(CapitanDaltynAID);
                CapitanDaltynKiller.Add(actor.DynamicID);
            }
            else
            {
                CapitanDaltynKiller.Add(actor.DynamicID);
            }

            //Запуск отслеживания убийства
            var ListenerDaltyn = Task<bool>.Factory.StartNew(() => OnKillListener(CapitanDaltynKiller, world));
            //Ждём пока убьют
            ListenerDaltyn.ContinueWith(delegate
            {
                world.Game.Quests.Advance(72095);
            });

        }

        private bool StartConversation(Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
            }
            return true;
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
        }
}