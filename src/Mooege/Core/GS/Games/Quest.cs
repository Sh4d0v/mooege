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

using System;
using System.Collections.Generic;
using System.Linq;
using Mooege.Net.GS.Message.Definitions.Quest;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Common.Logging;

namespace Mooege.Core.GS.Games
{
    public interface QuestProgressHandler
    {
        void Notify(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType type, int value);
        void NotifyBonus(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType type, int value);
    }

    public class Quest : QuestProgressHandler
    {

        private static readonly Logger Logger = LogManager.CreateLogger(); // add for debugging purposes

        /// <summary>
        /// Keeps track of a single quest step
        /// </summary>
        public class QuestStep : QuestProgressHandler
        {

            private static readonly Logger Logger = LogManager.CreateLogger(); // add for debugging purposes

            /// <summary>
            /// Keeps track of a single quest step objective
            /// </summary>
            public class QuestObjective : QuestProgressHandler
            {

                private static readonly Logger Logger = LogManager.CreateLogger(); // add for debugging purposes

                public int Counter { get; private set; }
                public bool Done { get { return (objective.CounterTarget == 0 && Counter > 0) || Counter == objective.CounterTarget; } }
                public int ID { get; private set; }

                // these are only needed to show information in console
                public Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType ObjectiveType { get { return objective.ObjectiveType; } }
                public int ObjectiveValue { get { return objective.SNOName1.Id; } }

                private Mooege.Common.MPQ.FileFormats.QuestStepObjective objective;
                public QuestStep questStep;

                public QuestObjective(Mooege.Common.MPQ.FileFormats.QuestStepObjective objective, QuestStep questStep, int id)
                {
                    Logger.Debug(" (QuestObjective ctor) creating an objective with ID {0}, QuestStepObjective {1} and QuestStep ID {2}", id, objective.Group1Name, questStep.QuestStepID);
                    ID = id;
                    this.objective = objective;
                    this.questStep = questStep;
                }

                /// <summary>
                /// Notifies the objective (if it is flagged as abonus objective), that an event occured. The objective checks if that event matches the event it waits for
                /// </summary>
                public void NotifyBonus(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType type, int value)
                {
                    Logger.Debug(" (NotifyBonus) objective details SNOName 1 : {0}, ID {1} \n SNOName 2  : {2},ID {3} ", objective.SNOName1.Name, objective.SNOName1.Id, objective.SNOName2.Name, objective.SNOName2.Id);
                    Logger.Debug(" (NotifyBonus) in QuestObjective for type {0} and value {1} and objective.ObjectiveType is {2}", type, value, objective.ObjectiveType);
                    //if (type != objective.ObjectiveType) return;
                    switch (type)
                    {

                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.BonusStep:
                            {
                                Counter++;
                                questStep.UpdateBonusCounter(this);
                            }
                            break;
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterScene:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.InteractWithActor:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.KillMonster:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.CompleteQuest:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.HadConversation:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterLevelArea:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterTrigger:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.GameFlagSet:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.KillGroup:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.PlayerFlagSet:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.PossessItem:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.TimedEventExpired:
                            throw new NotImplementedException();
                    }
                }

                /// <summary>
                /// Notifies the objective, that an event occured. The objective checks if that event matches the event it waits for
                /// </summary>
                public void Notify(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType type, int value)
                {

                    if (type != objective.ObjectiveType) return;
                    switch (type)
                    {
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterWorld:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterScene:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.InteractWithActor:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.KillMonster:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.CompleteQuest:
                            {
                                if (value == objective.SNOName1.Id)
                                {
                                    Counter++;
                                    questStep.UpdateCounter(this);
                                    
                                }
                                break;
                            }
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.HadConversation:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterLevelArea:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EventReceived:
                            {
                                if (value == objective.SNOName1.Id)
                                {
                                    Logger.Debug(" %%%%%%% AN EVENT OCCURED %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% ");
                                    Logger.Debug(" (Notify) objective SNOName1  Name : {0}, Id {1}, Valid {2} ", objective.SNOName1.Name, objective.SNOName1.Id, objective.SNOName1.IsValid);
                                    Logger.Debug(" (Notify) objective SNOName2  Name : {0}, Id {1}, Valid {2} ", objective.SNOName2.Name, objective.SNOName2.Id, objective.SNOName2.IsValid);
                                    Logger.Debug(" (Notify) objective Group1Name : {0} ", objective.Group1Name);
                                    Logger.Debug(" (Notify) objective I0 : {0} ", objective.I0);
                                    Logger.Debug(" (Notify) objective I2 : {0} ", objective.I2);
                                    Logger.Debug(" (Notify) objective I4 : {0} ", objective.I4);
                                    Logger.Debug(" (Notify) objective I5 : {0} ", objective.I5);
                                    Logger.Debug(" -> (Notify) objectiveType : {0} ", objective.ObjectiveType);
                                    Logger.Debug(" (Notify) objective GBID1 : {0} ", objective.GBID1);
                                    Logger.Debug(" (Notify) objective GBID2 : {0} ", objective.GBID2);
                                    Logger.Debug(" (Notify) NOW CALLING UPDATE COUNTER ");

                                    Counter++;
                                    questStep.UpdateCounter(this);
                                }
                                break;
                            }
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.EnterTrigger:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.GameFlagSet:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.KillGroup:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.PlayerFlagSet:
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.PossessItem:
                            {
                                if (value == objective.SNOName1.Id)
                                {
                                    Logger.Debug(" %%%%%%% AN EVENT OCCURED %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% ");
                                    Logger.Debug(" (Notify) objective SNOName1  Name : {0}, Id {1}, Valid {2} ", objective.SNOName1.Name, objective.SNOName1.Id, objective.SNOName1.IsValid);
                                    Logger.Debug(" (Notify) objective SNOName2  Name : {0}, Id {1}, Valid {2} ", objective.SNOName2.Name, objective.SNOName2.Id, objective.SNOName2.IsValid);
                                    Logger.Debug(" (Notify) objective Group1Name : {0} ", objective.Group1Name);
                                    Logger.Debug(" (Notify) objective I0 : {0} ", objective.I0);
                                    Logger.Debug(" (Notify) objective I2 : {0} ", objective.I2);
                                    Logger.Debug(" (Notify) objective I4 : {0} ", objective.I4);
                                    Logger.Debug(" (Notify) objective I5 : {0} ", objective.I5);
                                    Logger.Debug(" -> (Notify) objectiveType : {0} ", objective.ObjectiveType);
                                    Logger.Debug(" (Notify) objective GBID1 : {0} ", objective.GBID1);
                                    Logger.Debug(" (Notify) objective GBID2 : {0} ", objective.GBID2);
                                    Logger.Debug(" (Notify) NOW CALLING UPDATE COUNTER ");

                                    Counter++;


                                    //*/
                                    questStep.UpdateCounter(this);
                                }
                                if (value == 61441)
                                {
                                    if (this.questStep.ObjectivesSets[0].Objectives[0].Counter == 0)
                                    {
                                        this.questStep.ObjectivesSets[0].Objectives[0].Counter++;
                                        this.questStep.ObjectivesSets[1].Objectives[0].Counter++;
                                        questStep.UpdateCounter(this);
                                    }
                                }
                                if (value == 62989)
                                {
                                    if (this.questStep.ObjectivesSets[0].Objectives[1].Counter == 0)
                                    {
                                        this.questStep.ObjectivesSets[0].Objectives[1].Counter++;
                                        this.questStep.ObjectivesSets[1].Objectives[1].Counter++;
                                        questStep.UpdateCounter(this);
                                    }
                                }
                                
                                break;
                            }
                        case Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType.TimedEventExpired:
                            throw new NotImplementedException();
                    }
                }
            }

            // this is only public for GameCommand / Debug
            public struct ObjectiveSet
            {
                public List<QuestObjective> Objectives;
                public int FollowUpStepID;
            }

            public List<ObjectiveSet> ObjectivesSets = new List<ObjectiveSet>(); // this is only public for GameCommand / Debug
            public List<List<QuestObjective>> bonusObjectives = new List<List<QuestObjective>>(); // this is now public for GameCOmmand /Debug
            private Mooege.Common.MPQ.FileFormats.IQuestStep _questStep = null;
            private Quest _quest = null;
            public int QuestStepID { get { return _questStep.ID; } }

            // working on it erekose 
            private void UpdateBonusCounter(QuestObjective objective)
            {
                Logger.Debug(" calling updateBonus Counter snoQuest {0}, step ID {1}, TaskIndex {2}, counter {3}, Checked {4}", _quest.SNOHandle.Id, _questStep.ID, objective.ID + 1, objective.Counter, objective.Done ? 1 : 0);
                foreach (var player in _quest.game.Players.Values)
                    player.InGameClient.SendMessage(new QuestCounterMessage()
                    {
                        snoQuest = _quest.SNOHandle.Id,
                        snoLevelArea = -1,
                        StepID = _questStep.ID,
                        TaskIndex = objective.ID + 1, // TODO really ugly what about counting the legit obj + the bonus obj ?
                        Counter = objective.Counter,
                        Checked = objective.Done ? 1 : 0,
                    });

            }

            private void UpdateCounter(QuestObjective objective)
            {
                Logger.Debug(" (UpdateCounter) is _questStep unassiagned ? {0}", (_questStep is Mooege.Common.MPQ.FileFormats.QuestUnassignedStep == false));
                if (_questStep is Mooege.Common.MPQ.FileFormats.QuestUnassignedStep == false)
                {
                    Logger.Debug(" (UpdateCounter) calling updateCounter snoQuest {0}, step ID {1}, TaskIndex {2}, counter {3}, Checked {4}", _quest.SNOHandle.Id, _questStep.ID, objective.ID, objective.Counter, objective.Done ? 1 : 0);
                    foreach (var player in _quest.game.Players.Values)
                        player.InGameClient.SendMessage(new QuestCounterMessage()
                        {
                            snoQuest = _quest.SNOHandle.Id,
                            snoLevelArea = -1,
                            StepID = _questStep.ID,
                            TaskIndex = objective.ID,
                            Counter = objective.Counter,
                            Checked = objective.Done ? 1 : 0,
                        });
                }

                //EREKOSE WARNING quest 87700 has at least 2 objective in stepID 55 

                var completedObjectiveList = from objectiveSet in ObjectivesSets
                                             where (from o in objectiveSet.Objectives select o.Done).Aggregate((r, o) => r && o)
                                             select objectiveSet.FollowUpStepID;

                Logger.Debug(" (UpdateCounter) contains {0} accomplished objectives ", completedObjectiveList.Count());
                Logger.Debug(" (UpdateCounter) Current quest step contains {0} objectiveSet ", ObjectivesSets.Count());
                foreach (var objectiveSet in ObjectivesSets)
                {
                    foreach (var l_objective in objectiveSet.Objectives)
                    {
                        Logger.Debug(" (UpdateCounter) objective in ObjSets contains objective ID {0}, type {1}, value {2}, isDone {3} ", l_objective.ID, l_objective.ObjectiveType, l_objective.ObjectiveValue, l_objective.Done);
                    }
                }

                if (completedObjectiveList.Count() == ObjectivesSets.Count())
                {
                    Logger.Debug(" (Update Counter) Now calling StepCompleted with followupID {0} ", completedObjectiveList.First());
                    _quest.StepCompleted(completedObjectiveList.First());
                }
            }

            /// <summary>
            /// Debug method, completes a given objective set
            /// </summary>
            /// <param name="index"></param>
            public void CompleteObjectiveSet(int index)
            {
                _quest.StepCompleted(_questStep.StepObjectiveSets[index].FollowUpStepID);
            }

            public QuestStep(Mooege.Common.MPQ.FileFormats.IQuestStep assetQuestStep, Quest quest)
            {
                _questStep = assetQuestStep;
                _quest = quest;
                int c = 0;

                foreach (var objectiveSet in assetQuestStep.StepObjectiveSets)
                {
                    ObjectivesSets.Add(new ObjectiveSet()
                    {
                        FollowUpStepID = objectiveSet.FollowUpStepID,
                        Objectives = new List<QuestObjective>(from objective in objectiveSet.StepObjectives select new QuestObjective(objective, this, c++))
                    });
                }
                c = 0;

                if (assetQuestStep is Mooege.Common.MPQ.FileFormats.QuestStep)
                {
                    var step = assetQuestStep as Mooege.Common.MPQ.FileFormats.QuestStep;

                    if (step.StepBonusObjectiveSets != null)
                        foreach (var objectiveSet in step.StepBonusObjectiveSets)
                        {
                            bonusObjectives.Add(new List<QuestObjective>(from objective in objectiveSet.StepBonusObjectives select new QuestObjective(objective, this, c++)));
                        }

                }

                Logger.Debug(" (questStep ctor) Displaying objectives sets for quest {0} in step {1} (if any) ", quest.SNOHandle, quest.CurrentStep);

                int i = 0;
                foreach (var objective_set in ObjectivesSets)
                {
                    Logger.Debug(" ObjectiveSet number {0}", i++);
                    foreach (var objective in objective_set.Objectives)
                    {
                        Logger.Debug("(questStep ctor) % objective has ID {0}, type {1}, value {2}, counter {4}, sub quest step {3} ", objective.ID, objective.ObjectiveType, objective.ObjectiveValue, objective.questStep.QuestStepID, objective.Counter);
                        Logger.Debug("(questStep ctor) % objective in string is {0}", objective.ToString());
                    }
                }

                Logger.Debug(" (questStep ctor) Displaying bonus objectives for quest {0} in step {1} (if any) ", quest.SNOHandle, quest.CurrentStep);

                i = 0;
                foreach (var bonus_objective_set in bonusObjectives)
                {
                    Logger.Debug(" Bonus Objective list number {0}", i++);
                    foreach (var bonus_objective in bonus_objective_set)
                    {
                        Logger.Debug("(questStep ctor) % bonus objective has ID {0}, type {1}, value {2}, counter {4}, sub quest step {3} ", bonus_objective.ID, bonus_objective.ObjectiveType, bonus_objective.ObjectiveValue, bonus_objective.questStep.QuestStepID, bonus_objective.Counter);
                        Logger.Debug("(questStep ctor) % bonus objective in string is {0}", bonus_objective.ToString());
                    }
                }
                Logger.Debug("(questStep ctor)  _questStep ID{0} ", _questStep.ID);
                //Logger.Debug("(questStep ctor)  _quest SNOHandle{0} CurrentStep Q", _quest.SNOHandle, _quest.CurrentStep.QuestStepID);
            }

            public void NotifyBonus(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType type, int value)
            {
                foreach (var bonus_objective_set in bonusObjectives)
                    foreach (var bonus_objective in bonus_objective_set)
                    {
                        //Logger.Debug(" NotifyBonus through QuestStep impl for type {0} and value {1} ", type, value);
                        bonus_objective.NotifyBonus(type, value);
                    }
            }

            public void Notify(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType type, int value)
            {
                foreach (var objectiveSet in ObjectivesSets)
                    foreach (var objective in objectiveSet.Objectives)
                    {
                        //Logger.Debug(" Notify through QuestStep impl for type {0} and value {1} ", type, value);
                        if (objective != null)
                            objective.Notify(type, value);
                        else
                        {
                            Logger.Debug(" an objective was null ...");

                        }
                    }
            }
        }

        //erekose start
        public class QuestCompletionStep
        {
            private static readonly Logger Logger = LogManager.CreateLogger(); // add for debugging purposes

            public string Name { get; private set; }
            public int StepId { get; private set; }
            public int I2 { get; private set; }
            public QuestCompletionStep(Mooege.Common.MPQ.FileFormats.QuestCompletionStep QCSasset)
            {

                Name = QCSasset.Unknown;
                StepId = QCSasset.ID;
                I2 = QCSasset.I2;
                Logger.Debug(" (ctor) asset contains {0} {1} {2}", QCSasset.Unknown, QCSasset.ID, QCSasset.I2);
            }
        }
        //erekose end


        public delegate void QuestProgressDelegate(Quest quest);
        public event QuestProgressDelegate OnQuestProgress;
        public SNOHandle SNOHandle { get; set; }
        public QuestStep CurrentStep { get; set; }

        public List<QuestCompletionStep> QuestCompletionSteps = new List<QuestCompletionStep>(); // erekose

        private List<int> completedSteps = new List<int>();           // this list has to be saved if quest progress should be saved. It is required to keep track of questranges
        private Mooege.Common.MPQ.FileFormats.Quest asset = null;
        private Game game { get; set; }


        public Quest(Game game, int SNOQuest)
        {
            this.game = game;
            SNOHandle = new SNOHandle(SNOGroup.Quest, SNOQuest);
            asset = SNOHandle.Target as Mooege.Common.MPQ.FileFormats.Quest;
            CurrentStep = new QuestStep(asset.QuestUnassignedStep, this);

            //erekose
            foreach (var QCSasset in asset.QuestCompletionSteps)
            {
                if (QCSasset != null)
                {
                    var nQCS = new QuestCompletionStep(QCSasset);
                    QuestCompletionSteps.Add(nQCS);
                    Logger.Debug(" (quest ctor)  adding a completion step {0}, {1}, {2} ", nQCS.Name, nQCS.StepId, nQCS.I2);
                }
                else
                {
                    Logger.Debug(" (quest ctor)  asset null problem in QCS ");
                }

            }

            foreach (var aQuestStep in asset.QuestSteps)
            {
                Logger.Debug(" (quest ctor) steps ID contained in quest is : {0} ", aQuestStep.ID);
            }


            Logger.Debug(" (quest ctor)  SNOHandle ID {0} Name {1}  Group {2} isValid ?{3} ", SNOHandle.Id, SNOHandle.Name, SNOHandle.Group, SNOHandle.IsValid);
            Logger.Debug(" (quest ctor)  from assets  numSteps {0}, SNO ID {1} num od completion step  ", asset.NumberOfSteps, asset.Header.SNOId, asset.NumberOfCompletionSteps);
            Logger.Debug(" (quest ctor)  CurrentStep ID ", CurrentStep.QuestStepID);
        }

        // 
        public bool HasStepCompleted(int stepID)
        {
            return completedSteps.Contains(stepID); // || CurrentStep.ObjectivesSets.Select(x => x.FollowUpStepID).Contains(stepID);
        }

        //erekose
        public bool IsDone()
        {
            //Logger.Debug(" (IsDone) called for quest SNO {0}", SNOHandle.Id);
            //Logger.Debug(" (IsDone) Completed Steps are : {0} ", string.Concat<string>( completedSteps.Cast<String>()));
            foreach (var qcs in QuestCompletionSteps)
            {
                if (!completedSteps.Contains(qcs.StepId))
                {
                    // Logger.Debug(" (IsDone) can't find {0} in completedSTeps", qcs.StepId);
                    return false;
                }
            }
            return true;
        }

        public void Advance()
        {
            ////Logger.Debug(" Advancing Current step  {0}", CurrentStep.QuestStepID);
            //foreach (var objsetelm in CurrentStep.ObjectivesSets)
            //{
            //    //Logger.Debug(" Current step  Objective sets type {0}", objsetelm.GetType());
            //}
            CurrentStep.CompleteObjectiveSet(0);
        }

        public void StepCompleted(int FollowUpStepID)
        {
            Logger.Debug(" (StepCompleted) snoQUest {0} StepID {1}", SNOHandle.Id, FollowUpStepID);

            foreach (var player in game.Players.Values)
                player.InGameClient.SendMessage(new QuestUpdateMessage()
                {
                    snoQuest = SNOHandle.Id,
                    snoLevelArea = -1,
                    StepID = FollowUpStepID,
                    Field3 = true,
                    Failed = false
                });

            Logger.Debug(" (StepCompleted) adding step {0} to completed step ", CurrentStep.QuestStepID);
            completedSteps.Add(CurrentStep.QuestStepID);

            if (IsDone())
            {
                Logger.Debug(" (StepCompleted) All objective have been reached Quest is Done ");
                CurrentStep = null;
                // somehow we should remove something from somehting else
            }
            else
            {
                if (QuestCompletionSteps.Exists(qc => qc.StepId == FollowUpStepID))
                {
                    Logger.Debug(" (StepCompleted) addin the EndingQuestStep ");
                    CurrentStep = null;
                    completedSteps.Add(FollowUpStepID);
                    Logger.Debug(" (StepCompleted) shooting the event handler");
                    OnQuestProgress -= OnQuestProgress;
                }
                else
                {
                    CurrentStep = (from step in asset.QuestSteps where step.ID == FollowUpStepID select new QuestStep(step, this)).FirstOrDefault();
                    OnQuestProgress(this);
                }
            }


            //Logger.Debug(" (StepCompleted) Choosing new current step");

            //if (CurrentStep == null)
            //{
            //    Logger.Debug(" (StepCompleted) No more step :p quest DONE !! ");
            //}
            //else
            //{
            //    Logger.Debug(" (StepCompleted) current step is {0}", CurrentStep.QuestStepID);

            //}
        }

        public void NotifyBonus(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType type, int value)
        {
            if (CurrentStep != null)
            {
                //Logger.Debug(" NotifyBonus through Quest impl for type {0} and value {1} ", type, value);
                CurrentStep.NotifyBonus(type, value);
            }
        }

        public void Notify(Mooege.Common.MPQ.FileFormats.QuestStepObjectiveType type, int value)
        {
            // Logger.Debug(" Current QuestStepID is  {0} ", CurrentStep.QuestStepID);
            if (CurrentStep != null)
            {
                //Logger.Debug(" Notify through Quest impl for type {0} and value {1} ", type, value);
                CurrentStep.Notify(type, value);
            }
            else
            {
                Logger.Debug(" CurrentStep is NULL !!");

            }
        }
    }

}