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


namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _166678 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();

        public _166678()
            : base(166678)
        {
        }
        
        public override void Execute(Map.World world)
        {
            Logger.Debug(" Ворота открыты ");
            world.Game.Quests.Advance(72095);
            Logger.Debug(" Quests.Advance(72095) ");
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