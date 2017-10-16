/*
 * Copyright (C) 2011 - 2012 mooege project - http://www.mooege.org
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
using System.Linq;
using System.Collections.Generic;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Players;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.World;
using Mooege.Core.GS.Actors.Interactions;
using Mooege.Core.GS.Actors.Implementations;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message.Definitions.NPC;
using Mooege.Net.GS;
using Mooege.Net.GS.Message.Definitions.Hireling;
using Mooege.Core.GS.Games;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Net.GS.Message.Definitions.Artisan;
using Mooege.Common.Logging;


namespace Mooege.Core.GS.Actors
{
    public class InteractiveNPC : NPC, IMessageConsumer
    {

        public static Logger Logger = new Logger("InteractiveNPC");


        public List<IInteraction> Interactions { get; private set; }
        public List<ConversationInteraction> Conversations { get; private set; }

        public InteractiveNPC(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            this.Attributes[GameAttribute.NPC_Has_Interact_Options, 0] = true;
            this.Attributes[GameAttribute.NPC_Is_Operatable] = true;
            //this.Attributes[GameAttribute.Buff_Visual_Effect, 0x00FFFFF] = true;
            Interactions = new List<IInteraction>();
            Conversations = new List<ConversationInteraction>();

            foreach (var quest in World.Game.Quests)
                quest.OnQuestProgress += new Games.Quest.QuestProgressDelegate(quest_OnQuestProgress);
            UpdateConversationList(); // show conversations with no quest dependency
        }

        protected override void quest_OnQuestProgress(Quest quest) // shadows Actors'Mooege.Core.GS.Actors.InteractiveNPC.quest_OnQuestProgress(Mooege.Core.GS.Games.Quest)'
        {
            // call base classe update range stuff            

            UpdateQuestRangeVisbility();
            // Logger.Debug(" (quesy_OnQuestProgress) has been called -> updatin conversaton list ");
            UpdateConversationList();
            UpdateConversationQuestList(quest);
            // brutalConversationAddOnQuestEnd(quest);
        }

        // first hack erekose
        private void brutalConversationAddOnQuestEnd(Quest quest)
        {
            if (ActorSNO.Id == 4580) // for now only leah is concerned :p
            {
                Logger.Debug(" (brutalCOnversationAssOnQuestEnd) called on quest {0} for actor with dynamic ID {1} ", quest.SNOHandle.Id, DynamicID);
                if (quest.IsDone())
                {
                    // check if NPC has the conversation in its inherited ConversationList
                    if ((ConversationList != null))
                        if (ConversationList.ConversationListEntries != null)
                            if (ConversationList.ConversationListEntries.Count > 0)
                            {
                                var convlistentries = ConversationList.ConversationListEntries;
                                bool l_found = false;
                                foreach (var convlistentry in convlistentries)
                                {
                                    if (convlistentry.SNOConv == 198541)
                                    {
                                        Logger.Debug(" (brutalCOnversationAssOnQuestEnd) NPC has the conversation 198541 in it inherited ConversationList ");
                                        l_found = true;
                                        break;
                                    }
                                }
                                if (!l_found)
                                    Logger.Debug(" (brutalCOnversationAssOnQuestEnd) NPC DOES NOT HAVE THE conversation 198541 in it inherited ConversationList !! THIS IS BAD !!");
                            }
                    //else
                    //{
                    //    Logger.Debug(" (brutalCOnversationAssOnQuestEnd) the inherited ConversationList IS NULL OR EMPTY WTF ????");
                    //}

                    Logger.Debug(" (brutalCOnversationAssOnQuestEnd) quest is marked as done trying to get the conversation lists ");
                    // looking for conversation in the whole conversation set !!! 
                    var conv_assets = Mooege.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.Conversation];
                    var convs = from conv_asset in conv_assets.Values
                                where (conv_asset.Data as Mooege.Common.MPQ.FileFormats.Conversation).SNOQuest == quest.SNOHandle.Id
                                select (conv_asset.Data as Mooege.Common.MPQ.FileFormats.Conversation);
                    foreach (var conv in convs)
                    {
                        int[] tab = { conv.SNOAltNpc1, conv.SNOAltNpc2, conv.SNOAltNpc3, conv.SNOAltNpc4, conv.SNOPrimaryNpc };
                        if (tab.Contains(ActorSNO.Id))
                        {
                            Logger.Debug(" (brutalCOnversationAssOnQuestEnd) Trying to add conversation : INTERACTIVE NPC {0} is present in Conversation {1} SNO alt NPC where : {2}", ActorSNO.Id, conv.Header.SNOId, tab);
                            if (Conversations.Exists(x => x.ConversationSNO == conv.Header.SNOId))
                            {
                                //RAS 
                                Logger.Debug(" (brutalCOnversationAssOnQuestEnd) already present doing nothing ");
                            }
                            else
                            {
                                Logger.Debug(" (brutalCOnversationAssOnQuestEnd) added !!");
                                Conversations.Add(new ConversationInteraction(conv.Header.SNOId));
                            }
                        }
                    }

                }
            }

        }

        private void UpdateConversationQuestList(Quest quest)
        {
            if ((ActorSNO.Id == 4580) && DynamicID == 83) // for now only the Leah near the stash
            {
                if (quest.IsDone())
                {
                    if (ConversationList != null) // this is from Actor
                    {
                        var ConversationsNew = new List<int>();
                        foreach (var entry in ConversationList.ConversationListEntries) // again on actor
                        {
                            if (entry.SNOQuestComplete == quest.SNOHandle.Id)   // we'll refine later...
                                ConversationsNew.Add(entry.SNOConv);
                        }

                        // remove outdates conversation options and add new ones
                        Conversations = Conversations.Where(x => ConversationsNew.Contains(x.ConversationSNO)).ToList(); // this is in the InteractiveNPC
                        foreach (var sno in ConversationsNew)
                            if (!Conversations.Select(x => x.ConversationSNO).Contains(sno))
                                Conversations.Add(new ConversationInteraction(sno));

                        // search for an unread questconversation
                        bool questConversation = false;
                        foreach (var conversation in Conversations) // this is in the InteractiveNPC
                            if (Mooege.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.Conversation].ContainsKey(conversation.ConversationSNO))
                                if ((Mooege.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.Conversation][conversation.ConversationSNO].Data as Mooege.Common.MPQ.FileFormats.Conversation).I0 == 1)
                                    if (conversation.Read == false)
                                    {
                                        // Logger.Debug(" (UpdateConversationList) for actor {0}-{1} has unread quest conversation no {2}: ", ActorSNO.Id, ActorSNO.Name, conversation.ConversationSNO);
                                        questConversation = true;
                                    }

                        // show the exclamation mark if actor has an unread quest conversation
                        Attributes[GameAttribute.Conversation_Icon, 0] = questConversation ? 1 : 0;
                        Attributes.BroadcastChangedIfRevealed();
                    }
                }
            }
        }


        private void UpdateConversationList()
        {
            // Logger.Debug(" (UpdateConversationList) has been called ");
            if (ConversationList != null) // this is from Actor
            {
                var ConversationsNew = new List<int>();
                foreach (var entry in ConversationList.ConversationListEntries) // again on actor
                {
                    if (entry.SNOConv == 198541)
                    {
                        Logger.Debug(" (UpdateConversationList) conv 198541 found for Actor {0} dyn actor {1}", ActorSNO, DynamicID);
                        Logger.Debug(" (UpdateConversationList) conv 198541 entry quest complete is {0}", entry.SNOQuestComplete);
                    }

                    if (entry.SNOLevelArea == -1 && entry.SNOQuestActive == -1 && entry.SNOQuestAssigned == -1 && entry.SNOQuestComplete == -1 && entry.SNOQuestCurrent == -1 && entry.SNOQuestRange == -1)
                        ConversationsNew.Add(entry.SNOConv);

                    if (Mooege.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.QuestRange].ContainsKey(entry.SNOQuestRange))
                        if (World.Game.Quests.IsInQuestRange(Mooege.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.QuestRange][entry.SNOQuestRange].Data as Mooege.Common.MPQ.FileFormats.QuestRange))
                            ConversationsNew.Add(entry.SNOConv);

                    if (World.Game.Quests.HasCurrentQuest(entry.SNOQuestCurrent, entry.I3))
                        ConversationsNew.Add(entry.SNOConv);
                }

                // remove outdates conversation options and add new ones
                Conversations = Conversations.Where(x => ConversationsNew.Contains(x.ConversationSNO)).ToList(); // this is in the InteractiveNPC
                foreach (var sno in ConversationsNew)
                    if (!Conversations.Select(x => x.ConversationSNO).Contains(sno))
                        Conversations.Add(new ConversationInteraction(sno));

                // search for an unread questconversation
                bool questConversation = false;
                foreach (var conversation in Conversations) // this is in the InteractiveNPC
                    if (Mooege.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.Conversation].ContainsKey(conversation.ConversationSNO))
                        if ((Mooege.Common.MPQ.MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.Conversation][conversation.ConversationSNO].Data as Mooege.Common.MPQ.FileFormats.Conversation).I0 == 1)
                            if (conversation.Read == false)
                            {
                                // Logger.Debug(" (UpdateConversationList) for actor {0}-{1} has unread quest conversation no {2}: ", ActorSNO.Id, ActorSNO.Name, conversation.ConversationSNO);
                                questConversation = true;
                            }

                // show the exclamation mark if actor has an unread quest conversation
                Attributes[GameAttribute.Conversation_Icon, 0] = questConversation ? 1 : 0;
                Attributes.BroadcastChangedIfRevealed();
            }
        }


        public override void OnTargeted(Player player, TargetMessage message)
        {
            Logger.Debug(" (OnTargeted) the npc has dynID {0}", DynamicID);

            player.SelectedNPC = this;
            
            // i am not sure whether this will work
            var vendor = player.SelectedNPC as Vendor;
            
            var count = Interactions.Count + Conversations.Count;
            if (count == 0)
                return;

            // If there is only one conversation option, immediatly select it without showing menu
            if (Interactions.Count == 0 && Conversations.Count == 1 && this != vendor)
            {
                player.Conversations.StartConversation(Conversations[0].ConversationSNO);
                Conversations[0].MarkAsRead();
                UpdateConversationList();
                return;
            }


            NPCInteraction[] npcInters = new NPCInteraction[count];

            var it = 0;
            foreach (var conv in Conversations)            
            {               
              if (this == vendor)                    
                  return;                
              else                
              {                    
                  npcInters[it] = conv.AsNPCInteraction(this, player);                    
                  it++;                
              }            
            }

            foreach (var inter in Interactions)
            {
                npcInters[it] = inter.AsNPCInteraction(this, player);
                it++;
            }


            player.InGameClient.SendMessage(new NPCInteractOptionsMessage()
            {
                ActorID = this.DynamicID,
                tNPCInteraction = npcInters,
                Type = NPCInteractOptionsType.Normal
            });

            // TODO: this has no effect, why is it sent?
            player.InGameClient.SendMessage(new Mooege.Net.GS.Message.Definitions.Effect.PlayEffectMessage()
            {
                ActorId = this.DynamicID,
                Effect = Net.GS.Message.Definitions.Effect.Effect.Unknown36
            });

            UpdateConversationList();
        }

        public void Consume(GameClient client, GameMessage message)
        {
            if (message is NPCSelectConversationMessage) OnSelectConversation(client.Player, message as NPCSelectConversationMessage);
            if (message is HirelingHireMessage) OnHire(client.Player);
            if (message is HirelingInventoryMessage) OnInventory(client.Player);
            if (message is CraftInteractionMessage) OnCraft(client.Player);
            else return;
        }

        public virtual void OnCraft(Player player)
        {
            throw new NotImplementedException();
        }

        public virtual void OnInventory(Player player)
        {
            throw new NotImplementedException();
        }

        public virtual void OnHire(Player player)
        {
            throw new NotImplementedException();
        }

        private void OnSelectConversation(Player player, NPCSelectConversationMessage message)
        {
            var conversation = Conversations.FirstOrDefault(conv => conv.ConversationSNO == message.ConversationSNO);
            if (conversation == null)
                return;

            player.Conversations.StartConversation(conversation.ConversationSNO);
            conversation.MarkAsRead();

            UpdateConversationList(); // erekose now the dialogs shit are updated properly :D yay !

        }
    }
}
