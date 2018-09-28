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
                //198312 - had Conv
                //156381 - Chancellor Eamon
            }
            foreach (var player in world.Players)
            {

                var dbQuestProgress = DBSessions.AccountSession.Get<DBProgressToon>(player.Value.Toon.PersistentID);
                dbQuestProgress.ActiveQuest = 72221;
                dbQuestProgress.StepOfQuest = 2;
                DBSessions.AccountSession.SaveOrUpdate(dbQuestProgress);
                DBSessions.AccountSession.Flush();
                Logger.Debug(" Progress Saved ");

            };
            // Нахер тележку!)
            //3026,339 2779,678 24,04532
            // 112131 - Телега
            var TELEGAS = world.GetActorsBySNO(112131);
            Vector3D LastTelega = new Vector3D();
            foreach (var TELEGA in TELEGAS)
            {
                TELEGA.Destroy();
                LastTelega = TELEGA.Position;
            }
            
            
            var BlacksmithQuest = world.GetActorBySNO(65036);
            Vector3D FirstPoint = new Vector3D(3026.339f, 2779.678f, 24.0453f);
            var FirstfacingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(BlacksmithQuest, FirstPoint);

            //BlacksmithQuest.Move(FirstPoint,FirstfacingAngle);

            //world.SpawnMonster(112131, LastTelega);
            StartConversation(world, 198307);

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
