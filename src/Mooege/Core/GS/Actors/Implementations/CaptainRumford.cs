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

using Mooege.Common.Logging;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Core.GS.AI.Brains;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Core.GS.Objects;
using Mooege.Net.GS.Message;
using MonsterFF = Mooege.Common.MPQ.FileFormats.Monster;
using GameBalance = Mooege.Common.MPQ.FileFormats.GameBalance;

namespace Mooege.Core.GS.Actors.Implementations
{
    [HandledSNO(3739)]
    class CaptainRumford : InteractiveNPC, IUpdateable
    {
        public CaptainRumford(World world, int snoID, TagMap tags)
            : base(world, snoID, tags)
        {
            Brain = new AggressiveNPCBrain(this); // erekose             

            // lookup GameBalance MonsterLevels.gam asset
            var monsterLevels = (GameBalance)Mooege.Common.MPQ.MPQStorage.Data.Assets[SNOGroup.GameBalance][19760].Data;
            var monsterData = (Monster.Target as MonsterFF);

            // always use normal difficulty levels for now
            if (monsterData.Level.Normal >= 0 && monsterData.Level.Normal < monsterLevels.MonsterLevel.Count)
            {
                //this.Brain = new MonsterBrain(this);
                this.Attributes[GameAttribute.Level] = monsterData.Level.Normal;
                this.Attributes[GameAttribute.Hitpoints_Max] = monsterLevels.MonsterLevel[monsterData.Level.Normal].F0;
                this.Attributes[GameAttribute.Hitpoints_Cur] = this.Attributes[GameAttribute.Hitpoints_Max_Total];
                this.Attributes[GameAttribute.Attacks_Per_Second] = 1.0f;
                this.Attributes[GameAttribute.Damage_Weapon_Min, 0] = 5f;
                this.Attributes[GameAttribute.Damage_Weapon_Delta, 0] = 5f;
                this.WalkSpeed = 0.3f * monsterData.Floats[129];  // TODO: this is probably multiplied by something erekose the 0.3 is because he is way too fast otherwise
            }
        }

        protected override void ReadTags()
        {
            if (!Tags.ContainsKey(MarkerKeys.ConversationList))
            {
                Tags.Add(MarkerKeys.ConversationList, new TagMapEntry(MarkerKeys.ConversationList.ID, 108832, 2));
            }

            base.ReadTags();
        }

        public void Update(int tickCounter)
        {
            if (this.Brain == null)
                return;

            this.Brain.Update(tickCounter);
        }
    }
}