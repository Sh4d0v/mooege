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

using System.Text;
using D3.Quests;

namespace Mooege.Net.GS.Message.Definitions.Quest
{
    [Message(Opcodes.PlayerQuestMessage2)]
    public class QuestRewardMessage : GameMessage
    {
        //D3.Quests.QuestReward
        public QuestReward QuestReward;

        public QuestRewardMessage() : base(Opcodes.QuestMeterMessage) { }

        public override void Parse(GameBitBuffer buffer)
        {
            QuestReward = QuestReward.ParseFrom(buffer.ReadBlob(32));
        }

        public override void Encode(GameBitBuffer buffer)
        {
            buffer.WriteBlob(32, QuestReward.ToByteArray());
        }

        public override void AsText(StringBuilder b, int pad)
        {
            b.Append(' ', pad);
            b.AppendLine("QuestRewardMessage:");
            b.Append(' ', pad++);
            b.Append(QuestReward.ToString());
        }

    }
}