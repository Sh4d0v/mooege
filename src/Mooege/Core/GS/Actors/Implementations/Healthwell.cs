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

using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Net.GS.Message;

namespace Mooege.Core.GS.Actors.Implementations
{
    /// <summary>
    /// Class that implements healthwell, run power on click and change gizmo state
    /// </summary>
    class Healthwell : Gizmo
    {
        public Healthwell(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Attributes[GameAttribute.MinimapActive] = true;
            Attributes[GameAttribute.Gizmo_State] = 0;
            Field2 = 0;
        }

        private bool WaitToSpawn(Ticker.TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

            }
            return true;
        }
       
        public override void OnTargeted(Players.Player player, Net.GS.Message.Definitions.World.TargetMessage message)
        {
            Logger.Warn("Healthwell Implementaion ver 1.1, Range 26f, refill 5 second");
            var playersAffected = player.GetPlayersInRange(26f);
            foreach (Players.Player playerN in playersAffected)
            {
                foreach (Players.Player targetAffected in playersAffected)
                {
                    player.InGameClient.SendMessage(new Net.GS.Message.Definitions.Effect.PlayEffectMessage()
                    {
                        ActorId = targetAffected.DynamicID,
                        Effect = Net.GS.Message.Definitions.Effect.Effect.HealthOrbPickup
                    });
                }

                player.AddPercentageHP(100);
            }
            this.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
            this.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
            this.Attributes[GameAttribute.Gizmo_State] = 1;
            Attributes.BroadcastChangedIfRevealed();

            Ticker.TickTimer Timeout = new Ticker.SecondsTickTimer(player.World.Game, 5f);
            var ListenerWaiting = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
            ListenerWaiting.ContinueWith(delegate {
                this.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = false;
                //this.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                this.Attributes[GameAttribute.Gizmo_State] = 0;
                Attributes.BroadcastChangedIfRevealed();
            });

            player.Attributes[GameAttribute.Skill, 0x0002EC66] = 1;
            player.Attributes[GameAttribute.Skill_Total, 0x0002EC66] = 1;

            player.Attributes.SendChangedMessage(player.InGameClient);

        }
    }
}
