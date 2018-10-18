/*
 * Copyright (C) 2011-2012 mooege project
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
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Net.GS.Message;
using TreasureClass = Mooege.Common.MPQ.FileFormats.TreasureClass;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;

namespace Mooege.Core.GS.Actors.Implementations
{
    /// <summary>
    /// Class that implements behaviour for clickable door types.
    /// Play open animation on click, then set idle animation
    /// </summary>
    class Door : Gizmo
    {
        public Door(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Field2 = 0;
        }


        public override void OnTargeted(Players.Player player, Net.GS.Message.Definitions.World.TargetMessage message)
        {
            var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Toon.PersistentID);
            if (ActorSNO.Id == 104545 && dbQuestProgress.ActiveQuest == 72546 && dbQuestProgress.StepOfQuest == 4)
            {
                        //118037 - Конец квеста
                    World.Game.Quests.Advance(72546);
                    Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                    Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                    Attributes[GameAttribute.Gizmo_State] = 1;
                    Attributes[GameAttribute.Untargetable] = true;
                    Attributes.BroadcastChangedIfRevealed();
            }
            else
            {

                var OpenDoor = new Door(this.World, this.ActorSNO.Id, this.Tags);
                OpenDoor.Field2 = 16;
                OpenDoor.RotationAxis = this.RotationAxis;
                OpenDoor.RotationW = this.RotationW;
                OpenDoor.Attributes[GameAttribute.Gizmo_Has_Been_Operated] = true;
                OpenDoor.Attributes[GameAttribute.Gizmo_Operator_ACDID] = unchecked((int)player.DynamicID);
                OpenDoor.Attributes[GameAttribute.Gizmo_State] = 1;
                OpenDoor.Attributes[GameAttribute.Untargetable] = true;
                Attributes.BroadcastChangedIfRevealed();
                OpenDoor.EnterWorld(this.Position);

                World.BroadcastIfRevealed(new PlayAnimationMessage
                {
                    ActorID = OpenDoor.DynamicID,
                    Field1 = 5,
                    Field2 = 0,
                    tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                    {
                    new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                    {
                        Duration = 50,
                        AnimationSNO = AnimationSet.TagMapAnimDefault[AnimationSetKeys.Opening],
                        PermutationIndex = 0,
                        Speed = 1
                    }
                    }

                }, OpenDoor);

                World.BroadcastIfRevealed(new SetIdleAnimationMessage
                {
                    ActorID = OpenDoor.DynamicID,
                    AnimationSNO = AnimationSetKeys.Open.ID
                }, OpenDoor);


                Destroy();
            }
            DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
            DBSessions.AccountSession.Flush();

            base.OnTargeted(player, message);
            
        }
    }
}