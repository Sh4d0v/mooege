/*
 * Copyright (C) 2011 mooege project
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
using Mooege.Common.Logging;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Players;
using Mooege.Core.GS.Ticker;
using Mooege.Net.GS.Message.Definitions.Effect;
using Mooege.Net.GS.Message.Definitions.World;

namespace Mooege.Core.GS.Powers
{
    public class PowerManager
    {
        static readonly Logger Logger = LogManager.CreateLogger();

        // list of all actively channeled skills
        private List<ChanneledSkill> _channeledSkills = new List<ChanneledSkill>();

        // list of all executing power scripts
        private class ExecutingScript
        {
            public IEnumerator<TickTimer> PowerEnumerator;
            public PowerScript Script;
        }
        private List<ExecutingScript> _executingScripts = new List<ExecutingScript>();

        // list of actors that were killed and are waiting to be deleted
        // rather ugly hack needed because deleting actors immediatly when they have visual buff effects
        // applied causes the effects to stay around forever.
        private Dictionary<Actor, TickTimer> _deletingActors = new Dictionary<Actor, TickTimer>();

        private bool UseActorOnKotel72095 = false;
        private bool UseDoor72095 = false;
        private bool Dialog72221 = false;
        public PowerManager()
        {
        }

        public void Update()
        {
            _UpdateDeletingActors();
            _UpdateExecutingScripts();
        }

        public bool RunPower(Actor user, PowerScript power, Actor target = null,
                             Vector3D targetPosition = null, TargetMessage targetMessage = null)
        {
            // replace power with existing channel instance if one exists
            if (power is ChanneledSkill)
            {
                var existingChannel = _FindChannelingSkill(user, power.PowerSNO);
                if (existingChannel != null)
                {
                    power = existingChannel;
                }
                else  // new channeled skill, add it to the list
                {
                    _channeledSkills.Add((ChanneledSkill)power);
                }
            }

            // copy in context params
            power.User = user;
            power.Target = target;
            power.World = user.World;
            power.TargetPosition = targetPosition;
            power.TargetMessage = targetMessage;

            _StartScript(power);
            return true;
        }

        public bool RunPower(Actor user, int powerSNO, uint targetId = uint.MaxValue, Vector3D targetPosition = null,
                               TargetMessage targetMessage = null)
        {
            Actor target;

            if (targetId == uint.MaxValue)
            {
                target = null;
            }
            else
            {
                target = user.World.GetActorByDynamicId(targetId);
                if (target == null)
                    return false;

                targetPosition = target.Position;
            }
            
            // find and run a power implementation
            var implementation = PowerLoader.CreateImplementationForPowerSNO(powerSNO);
            
            #region Южные ворота в тристрам.
            try
            {
                if(target.ActorSNO.Id == 90419)
                {
                    foreach(var player in user.World.Players)
                    {

                    }
                    
                }
            } catch { }
            
            #endregion

            #region Не Лекарь, а мужик неподалёку)
            try
            {
                if(target.ActorSNO.Id == 205665)
                {
                    
                    var playersAffected = user.GetPlayersInRange(26f);
                    foreach (Player player in playersAffected)
                    {
                        foreach (Player targetAffected in playersAffected)
                        {
                            player.InGameClient.SendMessage(new PlayEffectMessage()
                            {
                                ActorId = targetAffected.DynamicID,
                                Effect = Effect.HealthOrbPickup
                            });
                        }

                        //every summon and mercenary owned by you must broadcast their green text to you /H_DANILO
                        player.AddPercentageHP(100);
                    }
                    //player.UpdateExp(player.Attributes[Net.GS.Message.GameAttribute.Experience_Next]);
                }
            }catch { }
            #endregion

            #region Активация баннера игрока для телепортации
            try
            {
                var TeleportToPlayer = new Vector3D();
                if(target.ActorSNO.Name == "Banner_Player_1")
                {
                    foreach (var player in user.World.Players)
                    {
                        if (player.Value.PlayerIndex == 0)
                            if(player.Value.Position != user.Position)
                                TeleportToPlayer = player.Value.Position;
                                Logger.Warn("Перенос пользователя с помощью флага к игроку № {0}",player.Value.PlayerIndex);
                    }
                    if (TeleportToPlayer.Z != 0)
                        user.Teleport(TeleportToPlayer);
                        
                }
                else if (target.ActorSNO.Name == "Banner_Player_2")
                {
                    foreach (var player in user.World.Players)
                    {
                        if (player.Value.PlayerIndex == 1)
                            if (player.Value.Position != user.Position)
                                TeleportToPlayer = player.Value.Position;
                                Logger.Warn("Перенос пользователя с помощью флага к игроку № {0}", player.Value.Position);
                    }
                    if (TeleportToPlayer.Z != 0)
                        user.Teleport(TeleportToPlayer);
                }
                else if (target.ActorSNO.Name == "Banner_Player_3")
                {
                    foreach (var player in user.World.Players)
                    {
                        if (player.Value.PlayerIndex == 2)
                            if (player.Value.Position != user.Position)
                                TeleportToPlayer = player.Value.Position;
                                Logger.Warn("Перенос пользователя с помощью флага к игроку № {0}", player.Value.Position);  
                    }
                    if (TeleportToPlayer.Z != 0)
                        user.Teleport(TeleportToPlayer);
                }
                else if (target.ActorSNO.Name == "Banner_Player_4")
                {
                    foreach (var player in user.World.Players)
                    {
                        if (player.Value.PlayerIndex == 3)
                            if (player.Value.Position != user.Position)
                                TeleportToPlayer = player.Value.Position;
                                Logger.Warn("Перенос пользователя с помощью флага к игроку № {0}", player.Value.Position);
                    }
                    if (TeleportToPlayer.Z != 0)
                        user.Teleport(TeleportToPlayer);
                }
            }
            catch { }
            #endregion

            #region Квестовые события

            #region Северные ворота
            try
            {
                if (target.ActorSNO.Id == 121241)
                {
                    foreach (var player in user.World.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                        if (dbQuestProgress.ActiveQuest == 72221)
                        {
                            if (dbQuestProgress.StepOfQuest == 5)
                            {
                                dbQuestProgress.StepOfQuest = 6;
                                Dialog72221 = true;
                            }
                        }
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }
                    if (Dialog72221 == true)
                    {
                        //user.World.Game.Quests.Advance(72221);
                        user.World.Game.Quests.NotifyQuest(72221, Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.InteractWithActor, 121241);
                        Dialog72221 = false;
                    }
                }
            }
            catch { }
            #endregion

            #region Разговор с кузнецом
            try
            {
                if(target.ActorSNO.Id == 65036)
                {
                    foreach (var player in user.World.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                        if (dbQuestProgress.ActiveQuest == 72221)
                        {
                            dbQuestProgress.ActiveQuest = 72221;
                            if (dbQuestProgress.StepOfQuest == 1)
                            {
                                Dialog72221 = true;
                               dbQuestProgress.StepOfQuest = 2;
                            }

                        }
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }
                    if (Dialog72221 == true)
                    {
                        Dialog72221 = false;
                        StartConversation(target.World, 198292);
                    }
                }
            }
            catch { }
            #endregion

            #region Выход через шкаф)
            try
            {
                if (target.ActorSNO.Id == 188743)
                {

                    foreach (var player in user.World.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                        if (dbQuestProgress.ActiveQuest == 72095)
                        {
                            dbQuestProgress.ActiveQuest = 72095;
                            if (dbQuestProgress.StepOfQuest == 14)
                            {
                                dbQuestProgress.StepOfQuest = 15;
                                UseDoor72095 = true;
                            }

                        }
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }

                    if (this.UseDoor72095 == true)
                    {
                        user.World.Game.Quests.Advance(72095);
                        UseDoor72095 = false;
                    }
                    // 136291 - Houser_Door_trOut_newTristram
                }
            }
            catch { }
            #endregion

            #region Двери собора
            try
            {
                if (target.DynamicID == 1543 || target.ActorSNO.Id == 167289)
                {

                    foreach (var player in user.World.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                        if (dbQuestProgress.ActiveQuest == 72095)
                        {
                            dbQuestProgress.ActiveQuest = 72095;
                            if (dbQuestProgress.StepOfQuest == 9)
                            {
                                dbQuestProgress.StepOfQuest = 10;
                                UseDoor72095 = true;
                            }

                        }
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }

                    if (this.UseDoor72095 == true)
                    {
                        user.World.Game.Quests.Advance(72095);
                        UseDoor72095 = false;
                    }

                }
            }
            catch{}
            #endregion

            #region Котёл в хижине
            try
            {
                
                if (target.DynamicID == 1859 || target.ActorSNO.Id == 131123)
                {
                    //Отлавливаем котёл в потойном подвале
                    foreach (var player in user.World.Players)
                    {
                        var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                        if (dbQuestProgress.ActiveQuest == 72095)
                        {
                            dbQuestProgress.ActiveQuest = 72095;
                            if (dbQuestProgress.StepOfQuest == 6)
                            {
                                //dbQuestProgress.StepOfQuest = 7;
                                UseActorOnKotel72095 = true;
                            }

                        }
                        DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                        DBSessions.AccountSession.Flush();
                    }

                    if (this.UseActorOnKotel72095 == true)
                    {
                        StartConversation(user.World, 167115);
                        user.World.Game.Quests.Advance(72095);
                        UseActorOnKotel72095 = false;
                    }
                }
            }
            catch{}
            #endregion

            #region 
            #endregion
            #endregion

            #region Книги
            try
            {
                #region Книга Лии - первый журнал
                if (target.ActorSNO.Id == 86817)
                {
                    Vector3D PointToItem = new Vector3D(93.56282f, 111.3167f, 0.5335404f);
                    //user.World.
                    //LeahJorunal First Book
                    string Leah_Diary_in_room = "LeahJournal_PartOne";
                    target.Destroy();
                    foreach (var player in user.World.Players)
                    {
                        var item = Items.ItemGenerator.Cook(player.Value, Leah_Diary_in_room);
                        item.EnterWorld(PointToItem);
                    }
                }
                #endregion
                #region Книга Декарда - 
                else if(target.ActorSNO.Id == 80)
                {

                }
                #endregion
            }
            catch { }
            #endregion
            if (implementation != null)
            {
                return RunPower(user, implementation, target, targetPosition, targetMessage);
            }
            else
            {
                return false;
            }
        }

        private void _UpdateExecutingScripts()
        {
            // process all powers, removing from the list the ones that expire
            _executingScripts.RemoveAll(script =>
            {
                if (script.PowerEnumerator.Current.TimedOut)
                {
                    if (script.PowerEnumerator.MoveNext())
                        return script.PowerEnumerator.Current == PowerScript.StopExecution;
                    else
                        return true;
                }
                else
                {
                    return false;
                }
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

        public void CancelChanneledSkill(Actor user, int powerSNO)
        {
            var channeledSkill = _FindChannelingSkill(user, powerSNO);
            if (channeledSkill != null)
            {
                channeledSkill.CloseChannel();
                _channeledSkills.Remove(channeledSkill);
            }
            else
            {
                Logger.Debug("cancel channel for power {0}, but it doesn't have an open channel to cancel", powerSNO);
            }
        }

        private ChanneledSkill _FindChannelingSkill(Actor user, int powerSNO)
        {
            return _channeledSkills.FirstOrDefault(impl => impl.User == user &&
                                                           impl.PowerSNO == powerSNO &&
                                                           impl.IsChannelOpen);
        }

        private void _StartScript(PowerScript script)
        {
            var powerEnum = script.Run().GetEnumerator();
            if (powerEnum.MoveNext() && powerEnum.Current != PowerScript.StopExecution)
            {
                _executingScripts.Add(new ExecutingScript
                {
                    PowerEnumerator = powerEnum,
                    Script = script
                });
            }
        }

        private void _UpdateDeletingActors()
        {
            foreach (var key in _deletingActors.Keys.ToArray())
            {
                if (_deletingActors[key].TimedOut)
                {
                    key.Destroy();
                    _deletingActors.Remove(key);
                }
            }
        }

        public void AddDeletingActor(Actor actor)
        {
            _deletingActors.Add(actor, new SecondsTickTimer(actor.World.Game, 0.2f));
        }

        public bool IsDeletingActor(Actor actor)
        {
            return _deletingActors.ContainsKey(actor);
        }

        public void CancelAllPowers(Actor user)
        {
            _channeledSkills.RemoveAll(impl =>
            {
                if (impl.User == user && impl.IsChannelOpen)
                {
                    impl.CloseChannel();
                    return true;
                }
                return false;
            });

            _executingScripts.RemoveAll((script) => script.Script.User == user);
        }
    }
}
