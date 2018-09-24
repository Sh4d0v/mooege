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
    class _190404 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public _190404()
            : base(190404)
        {
        }

        public override void Execute(Map.World world)
        {
            Logger.Debug(" Разговор с Леей закончен ");
            world.Game.Quests.Advance(72095);

            //Logger.Debug(" Conversation(166678) ");
            //StartConversation(world, 166678); // "Открыть ворота" нужно использовать в Old Ruins
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