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
    class _196043 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;

        public _196043()
            : base(196043)
        {
        }

        public override void Execute(Map.World world)
        {
            if (HadConversation)
            {
                HadConversation = false;
                Logger.Debug(" Quests.Advance(72221) ");
                world.Game.Quests.Advance(72221);
            }
            foreach (var player in world.Players)
            {

                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                dbQuestProgress.LastQuest = 72221;
                dbQuestProgress.ActiveQuest = 72061;
                dbQuestProgress.StepOfQuest = 0;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
                Logger.Debug(" Progress Saved ");
                

            };


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
