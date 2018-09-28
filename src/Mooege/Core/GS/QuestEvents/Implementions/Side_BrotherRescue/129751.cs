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
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _129751 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _129751()
            : base(129751)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                world.Game.Quests.Advance(129801);
                Logger.Debug(" Quests.Advance(129801) Side Quest");
            }
            Logger.Debug(" Side quest progress save is not implement..");

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
