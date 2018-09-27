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

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _198292 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _198292() 
            : base(198292)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                Logger.Debug(" Quests.Advance(72221) ");
            }

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
