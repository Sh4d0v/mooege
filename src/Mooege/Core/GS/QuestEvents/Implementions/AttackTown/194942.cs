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

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _194942 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        public List<ConversationInteraction> Conversations { get; private set; }
        private Boolean HadConversation = true;


        public _194942()
            : base(194942)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Conversation done ");
            if (HadConversation)
            {
                HadConversation = false;
            }
            world.Game.Quests.NotifyQuest(73236, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1);
            var Maghda = world.GetActorBySNO(129345);
            world.Leave(Maghda);
            Vector3D YorikSpawn = new Vector3D(Maghda.Position.X + 5, Maghda.Position.Y, Maghda.Position.Z);
            world.SpawnMonster(178619, YorikSpawn);
            var Yorik = world.GetActorBySNO(178619);
            Yorik.Attributes[GameAttribute.Hitpoints_Max] = 1500f;
            Yorik.Attributes[GameAttribute.Hitpoints_Cur] = 1500f;
            Yorik.Attributes[Net.GS.Message.GameAttribute.Damage_Weapon_Min, 0] = 70f;
            Yorik.Attributes[Net.GS.Message.GameAttribute.Damage_Weapon_Delta, 0] = 70f;
            Yorik.Attributes[Net.GS.Message.GameAttribute.Movement_Scalar_Reduction_Percent] -= 10f;
            Yorik.Attributes[Net.GS.Message.GameAttribute.Quest_Monster] = true;
            List<uint> CultistList = new List<uint> { };
            CultistList.Add(Yorik.DynamicID);

            var ListenerCultists = Task<bool>.Factory.StartNew(() => OnKillCultistInTownListener(CultistList, world));
            ListenerCultists.ContinueWith(delegate
            {
                world.Game.Quests.NotifyQuest(73236, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1);
                Vector3D FirstSpawn = new Vector3D(Maghda.Position.X + 5, Maghda.Position.Y, Maghda.Position.Z);
                Vector3D SecondSpawn = new Vector3D(Maghda.Position.X + 3, Maghda.Position.Y, Maghda.Position.Z);
                Vector3D ThirdSpawn = new Vector3D(Maghda.Position.X + 5, Maghda.Position.Y+2, Maghda.Position.Z);

                world.SpawnMonster(178300, FirstSpawn);
                world.SpawnMonster(178300, SecondSpawn);
                world.SpawnMonster(178300, ThirdSpawn);

                var AllBerserks = world.GetActorsBySNO(178300);
                List<uint> BerserkList = new List<uint> { };

                foreach (var Berserk in AllBerserks)
                {
                    BerserkList.Add(Berserk.DynamicID);
                    Berserk.Attributes[GameAttribute.Hitpoints_Max] = 900f;
                    Berserk.Attributes[GameAttribute.Hitpoints_Cur] = 900f;
                    Berserk.Attributes[Net.GS.Message.GameAttribute.Damage_Weapon_Min, 0] = 120f;
                    Berserk.Attributes[Net.GS.Message.GameAttribute.Damage_Weapon_Delta, 0] = 120f;
                    Berserk.Attributes[Net.GS.Message.GameAttribute.Movement_Scalar_Reduction_Percent] -= 10f;
                    Berserk.Attributes[Net.GS.Message.GameAttribute.Quest_Monster] = true;
                    world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
                    {
                        ActorID = Berserk.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                        {
                            Duration = 100,
                            AnimationSNO = 55652,
                            PermutationIndex = 0,
                            Speed = 0.9f
                        }
                            }
                    }, Berserk);
                }
                var ListenerBerserk = Task<bool>.Factory.StartNew(() => OnKillBerserkInTownListener(BerserkList, world));
                ListenerBerserk.ContinueWith(delegate
                {
                    world.Game.Quests.Advance(73236);
                    StartConversation(world, 120372);
                    world.Game.Quests.Advance(73236);
                    foreach (var playerN in world.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(playerN.Value.Toon.PersistentID);
                        dbQuestProgress.ActiveQuest = 73236;
                        dbQuestProgress.StepOfQuest = 7;
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }
                    var Church = world.GetActorBySNO(165475);
                    world.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.SetIdleAnimationMessage
                    {
                        ActorID = Church.DynamicID,
                        AnimationSNO = Core.GS.Common.Types.TagMap.AnimationSetKeys.GizmoState1.ID,
                    }, Church);
                });
            });
        }

        private bool OnKillCultistInTownListener(List<uint> monstersAlive, Map.World world)
        {
            System.Int32 monstersKilled = 0;
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
        private bool OnKillBerserkInTownListener(List<uint> monstersAlive, Map.World world)
        {
            System.Int32 monstersKilled = 0;
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
                        world.Game.Quests.NotifyQuest(73236, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived, -1);
                    }
                }
            }
            return true;
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