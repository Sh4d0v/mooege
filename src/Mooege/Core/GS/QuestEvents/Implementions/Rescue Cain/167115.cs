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
using Mooege.Net.GS.Message;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _167115 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public _167115()
            : base(167115)
        {
        }

        static int RiseZombieAID = 6644;
        static int CapitanDaltynAID = 156801; //Actor ID Капитана Далтина
        List<uint> CapitanDaltynKiller = new List<uint> { }; // Используем для отслеживания убийства
        

        public override void Execute(Map.World world)
        {

            var boss = world.GetActorBySNO(CapitanDaltynAID);
            var minions = world.GetActorsBySNO(RiseZombieAID);
            if (boss == null)
            {
                Logger.Debug("Не найдено: Капитан Далтин - {0}", CapitanDaltynAID);
                Vector3D CapitanDaltyn = new Vector3D(51.12595f, 100.2664f, 0.1000305f);
                Vector3D[] Zombie = new Vector3D[4];
                Zombie[0] = new Vector3D(50.00065f, 125.4087f, 0.1000305f);
                Zombie[1] = new Vector3D(54.88688f, 62.24541f, 0.1000305f);
                Zombie[2] = new Vector3D(86.45869f, 77.09571f, 0.1000305f);
                Zombie[3] = new Vector3D(102.117f, 97.59058f, 0.1000305f);
                world.SpawnMonster(CapitanDaltynAID, CapitanDaltyn);
                foreach (Vector3D point in Zombie)
                {
                    world.SpawnMonster(RiseZombieAID, point);
                }
                boss = world.GetActorBySNO(CapitanDaltynAID);
                CapitanDaltynKiller.Add(boss.DynamicID);
                minions = world.GetActorsBySNO(RiseZombieAID);
                
                foreach(var minion in minions)
                {
                    CapitanDaltynKiller.Add(minion.DynamicID);
                }
                
            }
            else
            {
                CapitanDaltynKiller.Add(boss.DynamicID);
            }
            // Пытаемся привязать статус босса!
            boss = world.GetActorBySNO(CapitanDaltynAID);
            boss.Attributes[Net.GS.Message.GameAttribute.Using_Bossbar] = true;
            boss.Attributes[Net.GS.Message.GameAttribute.InBossEncounter] = true;
            // DOES NOT WORK it should be champion affixes or shit of this kind ...
            // Увеличиваем здоровье босса!
            boss.Attributes[GameAttribute.Hitpoints_Max] = 200f;
            boss.Attributes[GameAttribute.Hitpoints_Cur] = 200f;
            boss.Attributes[GameAttribute.Movement_Scalar_Reduction_Percent] -= 10f;
            boss.Attributes[GameAttribute.Quest_Monster] = true;
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
                var monsterCount = monstersAlive.Count;
                while (monstersKilled != monsterCount)
                {
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