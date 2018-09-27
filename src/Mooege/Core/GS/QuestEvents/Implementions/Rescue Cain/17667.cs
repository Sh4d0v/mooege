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
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;

namespace Mooege.Core.GS.QuestEvents.Implementations
{
    class _17667 : QuestEvent
    {

        private static readonly Logger Logger = LogManager.CreateLogger();
        private Boolean HadConversation = true;
        

        public _17667()
            : base(17667)
        {
        }

        public override void Execute(Map.World world)
        {

            if (HadConversation)
            {
                HadConversation = false;
                world.Game.Quests.Advance(72095);
                Logger.Debug(" Quests.Advance(72095) ");
                Logger.Debug(" Dialog with Cain ");
            }
            var CainBrains = world.GetActorBySNO(102386);
            Vector3D CainPath = new Vector3D(76.99389f, 155.145f, 0.0997252f);
            var facingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(CainBrains, CainPath);

            CainBrains.Move(CainPath, facingAngle);
            foreach (var player in world.Players)
            {

                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                dbQuestProgress.ActiveQuest = 72095;
                dbQuestProgress.StepOfQuest = 14;
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