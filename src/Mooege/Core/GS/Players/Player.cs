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
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mooege.Common.Helpers.Math;
using Mooege.Common.Logging;
using Mooege.Common.MPQ;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Items;
using Mooege.Core.GS.Objects;
using Mooege.Core.GS.Map;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Powers;
using Mooege.Core.GS.Skills;
using Mooege.Core.MooNet.Toons;
using Mooege.Net.GS;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Misc;
using Mooege.Net.GS.Message.Definitions.Pet;
using Mooege.Net.GS.Message.Definitions.Waypoint;
using Mooege.Net.GS.Message.Definitions.World;
using Mooege.Net.GS.Message.Fields;
using Mooege.Net.GS.Message.Definitions.Hero;
using Mooege.Net.GS.Message.Definitions.Player;
using Mooege.Net.GS.Message.Definitions.Skill;
using Mooege.Net.GS.Message.Definitions.Effect;
using Mooege.Net.GS.Message.Definitions.Trade;
using Mooege.Core.GS.Actors.Implementations;
using Mooege.Net.GS.Message.Definitions.Artisan;
using Mooege.Core.GS.Actors.Implementations.Hirelings;
using Mooege.Net.GS.Message.Definitions.Hireling;
using Mooege.Common.Helpers;
using Mooege.Net.GS.Message.Definitions.ACD;
using Mooege.Net.GS.Message.Definitions.Animation;
using Mooege.Net.GS.Message.Definitions.Tutorial;


namespace Mooege.Core.GS.Players
{
    public class Player : Actor, IMessageConsumer, IUpdateable
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        /// <summary>
        /// The ingame-client for player.
        /// </summary>
        public GameClient InGameClient { get; set; }

        /// <summary>
        /// The player index.
        /// </summary>
        public int PlayerIndex { get; private set; }

        /// <summary>
        /// The player's toon.
        /// We need a better name /raist.
        /// </summary>
        public Toon Toon { get; private set; }

        /// <summary>
        /// Skillset for the player (or actually for player's toons class).
        /// </summary>
        public SkillSet SkillSet { get; private set; }

        /// <summary>
        /// The inventory of player's toon.
        /// </summary>
        public Inventory Inventory { get; private set; }

        /// <summary>
        /// ActorType = Player.
        /// </summary>
        public override ActorType ActorType { get { return ActorType.Player; } }

        /// <summary>
        /// Revealed objects to player.
        /// </summary>
        public Dictionary<uint, IRevealable> RevealedObjects = new Dictionary<uint, IRevealable>();

        public ConversationManager Conversations { get; private set; }

        // Collection of items that only the player can see. This is only used when items drop from killing an actor
        // TODO: Might want to just have a field on the item itself to indicate whether it is visible to only one player
        /// <summary>
        /// Dropped items for the player
        /// </summary>
        public Dictionary<uint, Item> GroundItems { get; private set; }

        /// <summary>
        /// Everything connected to ExpBonuses.
        /// </summary>
        public ExpBonusData ExpBonusData { get; private set; }

        /// <summary>
        /// NPC currently interaced with
        /// </summary>
        public InteractiveNPC SelectedNPC { get; set; }

        private Hireling _activeHireling = null;
        public Hireling ActiveHireling
        {
            get { return _activeHireling; }
            set
            {
                if (value == _activeHireling)
                    return;

                if (_activeHireling != null)
                {
                    _activeHireling.Dismiss(this);
                }

                _activeHireling = value;

                if (value != null)
                {
                    InGameClient.SendMessage(new PetMessage()
                    {
                        Field0 = 0,
                        Field1 = 0,
                        PetId = value.DynamicID,
                        Field3 = 0,
                    });
                }
            }
        }

        private Hireling _activeHirelingProxy = null;
        public Hireling ActiveHirelingProxy
        {
            get { return _activeHirelingProxy; }
            set
            {
                if (value == _activeHirelingProxy)
                    return;

                if (_activeHirelingProxy != null)
                {
                    _activeHirelingProxy.Dismiss(this);
                }

                _activeHirelingProxy = value;

                if (value != null)
                {
                    InGameClient.SendMessage(new PetMessage()
                    {
                        Field0 = 0,
                        Field1 = 0,
                        PetId = value.DynamicID,
                        Field3 = 22,
                    });
                }
            }
        }

        // Resource generation timing /mdz
        private int _lastResourceUpdateTick;

        // number of seconds to use for the cooldown that is started after changing a skill.
        private const float SkillChangeCooldownLength = 5f;  // TODO: this needs to vary based on difficulty

        #region Just a testing function, never called. Add this to the End of SetNonDefaultStats to get All Equipped items attributes written to a file.
        private string TestOutputAttributes(GameAttributeMap map)
        {
            var resultStringBuilder = new StringBuilder();
            foreach (GameAttributeF ga in GameAttribute.Attributes.Where(ga => ga is GameAttributeF))
            {
                var keys = map.AttributeKeys(ga);
                if (keys.Length == 0 || (keys.Length == 1 && !keys[0].HasValue))
                {
                    var curVal = Convert.ToDouble(map[ga]);
                    if (curVal.CompareTo(Convert.ToDouble(ga.DefaultValue)) == 0)
                        continue;
                    resultStringBuilder.AppendFormat("{0}:\t{1}\r\n", ga.Name, curVal);
                }
                else
                {
                    foreach (var key in keys)
                    {
                        var curVal = Convert.ToDouble(map[ga, key]);
                        if (curVal.CompareTo(Convert.ToDouble(ga.DefaultValue)) == 0)
                            continue;
                        resultStringBuilder.AppendFormat("{0}|{1}:\t{2}\r\n", ga.Name, key, curVal);

                    }
                }
            }

            foreach (GameAttributeI ga in GameAttribute.Attributes.Where(ga => ga is GameAttributeI))
            {
                var keys = map.AttributeKeys(ga);
                if (keys.Length == 0 || (keys.Length == 1 && !keys[0].HasValue))
                {
                    var curVal = map[ga];
                    if (curVal == ga.DefaultValue)
                        continue;
                    resultStringBuilder.AppendFormat("{0}:\t{1}\r\n", ga.Name, curVal);
                }
                else
                {
                    foreach (var key in keys)
                    {
                        var curVal = map[ga];
                        if (curVal == ga.DefaultValue)
                            continue;
                        resultStringBuilder.AppendFormat("{0}|{1}:\t{2}\r\n", ga.Name, key, curVal);

                    }
                }
            }
            return resultStringBuilder.ToString();
        }

        private void TestOutPutItemAttributes()
        {
            if (this.Inventory == null || !this.Inventory.Loaded) return;
            const string filename = "c:/attrtest.txt";
            File.Delete(filename);
            foreach (var item in this.Inventory.GetEquippedItems())
            {
                File.AppendAllText(filename, string.Format("======{0}=========\r\n", item.EquipmentSlot));

                File.AppendAllText(filename, TestOutputAttributes(item.Attributes));
                File.AppendAllText(filename, "===============\r\n\r\n");
            }
        }
        #endregion

        /// <summary>
        /// Creates a new player.
        /// </summary>
        /// <param name="world">The initial world player joins in.</param>
        /// <param name="client">The gameclient for the player.</param>
        /// <param name="bnetToon">Toon of the player.</param>
        public Player(World world, GameClient client, Toon bnetToon)
            : base(world, bnetToon.Gender == 0 ? bnetToon.HeroTable.SNOMaleActor : bnetToon.HeroTable.SNOFemaleActor)
        {
            this.InGameClient = client;
            this.PlayerIndex = Interlocked.Increment(ref this.InGameClient.Game.PlayerIndexCounter); // get a new playerId for the player and make it atomic.
            this.Toon = bnetToon;
            this.GBHandle.Type = (int)GBHandleType.Player;
            this.GBHandle.GBID = this.Toon.ClassID;

            this.Field2 = 0x00000009;
            this.Scale = this.ModelScale;
            this.RotationW = 0.05940768f;
            this.RotationAxis = new Vector3D(0f, 0f, 0.9982339f);
            this.Field7 = -1;
            this.NameSNOId = -1;
            this.Field10 = 0x0;

            this.SkillSet = new SkillSet(this.Toon.Class, this.Toon);
            this.GroundItems = new Dictionary<uint, Item>();
            this.Conversations = new ConversationManager(this, this.World.Game.Quests);
            this.ExpBonusData = new ExpBonusData(this);
            this.SelectedNPC = null;

            this._lastResourceUpdateTick = 0;

            // TODO SavePoint from DB
            this.SavePointData = new SavePointData() { snoWorld = -1, SavepointId = -1 };

            // Attributes
            SetAllStatsInCorrectOrder();
            // Enabled stone of recall
            EnableStoneOfRecall();

            //this only need to be set on Player load
            this.Attributes[GameAttribute.Hitpoints_Cur] = this.Attributes[GameAttribute.Hitpoints_Max_Total];
            this.Attributes.BroadcastChangedIfRevealed();
        }

        #region Attribute Setters
        public void SetAllStatsInCorrectOrder()
        {
            SetAttributesSkills();
            SetAttributesBuffs();
            SetAttributesDamage();
            SetAttributesRessources();
            SetAttributesClassSpecific();
            SetAttributesMovement();
            SetAttributesMisc();
            SetAttributesSkillSets();
            SetAttributesOther();
            SetAttributesPassiveSkills();
            if (this.Inventory == null)
                this.Inventory = new Inventory(this);
            SetAttributesByItems();//needs the Inventory
        }

        public void SetAttributesSkills()
        {
            //Skills
            this.Attributes[GameAttribute.SkillKit] = Toon.HeroTable.SNOSKillKit0;
            //scripted //this.Attributes[GameAttribute.Skill_Total, 0x7545] = 1; //Axe Operate Gizmo

            this.Attributes[GameAttribute.Skill, 0x7545] = 1;
            //scripted //this.Attributes[GameAttribute.Skill_Total, 0x76B7] = 1; //Punch!
            this.Attributes[GameAttribute.Skill, 0x76B7] = 1;
            //scripted //this.Attributes[GameAttribute.Skill_Total, 0x6DF] = 1; //Use Item
            this.Attributes[GameAttribute.Skill, 0x6DF] = 1;
            //scripted //this.Attributes[GameAttribute.Skill_Total, 0x7780] = 1; //Basic Attack
            this.Attributes[GameAttribute.Skill, 0x7780] = 1;
            //scripted //this.Attributes[GameAttribute.Skill_Total, 0x0002EC66] = 0; //stone of recall
            //scripted //this.Attributes[GameAttribute.Skill_Total, 0xFFFFF] = 1;
            this.Attributes[GameAttribute.Skill, 0xFFFFF] = 1;
        }
        public void SetAttributesBuffs()
        {
            //Buffs
            this.Attributes[GameAttribute.Buff_Active, 0x33C40] = true;
            this.Attributes[GameAttribute.Buff_Icon_End_Tick0, 0x00033C40] = 0x000003FB;
            this.Attributes[GameAttribute.Buff_Icon_Start_Tick0, 0x00033C40] = 0x00000077;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 0x00033C40] = 1;
            this.Attributes[GameAttribute.Buff_Active, 0xCE11] = true;
            this.Attributes[GameAttribute.Buff_Icon_Count0, 0x0000CE11] = 1;
            this.Attributes[GameAttribute.Buff_Visual_Effect, 0xFFFFF] = true;
        }
        public void SetAttributesDamage()
        {
            this.Attributes[GameAttribute.Primary_Damage_Attribute] = (int)Toon.HeroTable.CoreAttribute;
        }
        public void SetAttributesRessources()
        {
            //Resource
            this.Attributes[GameAttribute.Resource_Max, (int)Toon.HeroTable.PrimaryResource] = Toon.HeroTable.PrimaryResourceMax;
            this.Attributes[GameAttribute.Resource_Factor_Level, (int)Toon.HeroTable.PrimaryResource] = Toon.HeroTable.PrimaryResourceFactorLevel;
            //scripted //this.Attributes[GameAttribute.Resource_Max_Total, (int)data.PrimaryResource] = GetMaxResource((int)data.PrimaryResource);
            //scripted //this.Attributes[GameAttribute.Resource_Effective_Max, (int)data.PrimaryResource] = GetMaxResource((int)data.PrimaryResource);

            if (this.Toon.Class == ToonClass.Barbarian) // Barbarian Starts with 0 fury always [Necrosummon]
                this.Attributes[GameAttribute.Resource_Cur, (int)Toon.HeroTable.PrimaryResource] = 0;
            else
                this.Attributes[GameAttribute.Resource_Cur, (int)Toon.HeroTable.PrimaryResource] = GetMaxResource((int)Toon.HeroTable.PrimaryResource);

            this.Attributes[GameAttribute.Resource_Regen_Per_Second, (int)Toon.HeroTable.PrimaryResource] = Toon.HeroTable.PrimaryResourceRegenPerSecond;
            //scripted //this.Attributes[GameAttribute.Resource_Regen_Total, (int)data.PrimaryResource] = data.PrimaryResourceRegenPerSecond;
            this.Attributes[GameAttribute.Resource_Type_Primary] = (int)Toon.HeroTable.PrimaryResource;
            if (Toon.HeroTable.SecondaryResource != Mooege.Common.MPQ.FileFormats.HeroTable.Resource.None)
            {
                this.Attributes[GameAttribute.Resource_Type_Secondary] = (int)Toon.HeroTable.SecondaryResource;
                this.Attributes[GameAttribute.Resource_Max, (int)Toon.HeroTable.SecondaryResource] = Toon.HeroTable.SecondaryResourceMax;
                this.Attributes[GameAttribute.Resource_Factor_Level, (int)Toon.HeroTable.SecondaryResource] = Toon.HeroTable.SecondaryResourceFactorLevel;
                this.Attributes[GameAttribute.Resource_Cur, (int)Toon.HeroTable.SecondaryResource] = GetMaxResource((int)Toon.HeroTable.SecondaryResource);
                //scripted //this.Attributes[GameAttribute.Resource_Max_Total, (int)data.SecondaryResource] = GetMaxResource((int)data.SecondaryResource);
                //scripted //this.Attributes[GameAttribute.Resource_Effective_Max, (int)data.SecondaryResource] = GetMaxResource((int)data.SecondaryResource);
                this.Attributes[GameAttribute.Resource_Regen_Per_Second, (int)Toon.HeroTable.SecondaryResource] = Toon.HeroTable.SecondaryResourceRegenPerSecond;
                //scripted //this.Attributes[GameAttribute.Resource_Regen_Total, (int)data.SecondaryResource] = data.SecondaryResourceRegenPerSecond;
                this.Attributes[GameAttribute.Resource_Type_Secondary] = (int)Toon.HeroTable.SecondaryResource;
            }

            //scripted //this.Attributes[GameAttribute.Get_Hit_Recovery] = 6f;
            this.Attributes[GameAttribute.Get_Hit_Recovery_Per_Level] = Toon.HeroTable.GetHitRecoveryPerLevel;
            this.Attributes[GameAttribute.Get_Hit_Recovery_Base] = Toon.HeroTable.GetHitRecoveryBase;
            //scripted //this.Attributes[GameAttribute.Get_Hit_Max] = 60f;
            this.Attributes[GameAttribute.Get_Hit_Max_Per_Level] = Toon.HeroTable.GetHitMaxPerLevel;
            this.Attributes[GameAttribute.Get_Hit_Max_Base] = Toon.HeroTable.GetHitMaxBase;
        }
        public void SetAttributesResistance()
        {
            this.Attributes[GameAttribute.Resistance, 0xDE] = 0.5f;
            this.Attributes[GameAttribute.Resistance, 0x226] = 0.5f;
        }
        public void SetAttributesClassSpecific()
        {
            // Class specific
            switch (this.Toon.Class)
            {
                case ToonClass.Barbarian:
                    //scripted //this.Attributes[GameAttribute.Skill_Total, 30078] = 1;  //Fury Trait
                    this.Attributes[GameAttribute.Skill, 30078] = 1;
                    this.Attributes[GameAttribute.Trait, 30078] = 1;
                    this.Attributes[GameAttribute.Buff_Active, 30078] = true;
                    this.Attributes[GameAttribute.Buff_Icon_Count0, 30078] = 1;
                    break;
                case ToonClass.DemonHunter:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  // Hatred Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                    this.Attributes[GameAttribute.Trait, ] = 1;
                    this.Attributes[GameAttribute.Buff_Active, ] = true;
                    this.Attributes[GameAttribute.Buff_Icon_Count0, ] = 1;
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  // Discipline Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                    this.Attributes[GameAttribute.Trait, ] = 1;
                    this.Attributes[GameAttribute.Buff_Active, ] = true;
                    this.Attributes[GameAttribute.Buff_Icon_Count0, ] = 1;
                     */
                    break;
                case ToonClass.Monk:
                    //scripted //this.Attributes[GameAttribute.Skill_Total, 0x0000CE11] = 1;  //Spirit Trait
                    this.Attributes[GameAttribute.Skill, 0x0000CE11] = 1;
                    this.Attributes[GameAttribute.Trait, 0x0000CE11] = 1;
                    this.Attributes[GameAttribute.Buff_Active, 0xCE11] = true;
                    this.Attributes[GameAttribute.Buff_Icon_Count0, 0x0000CE11] = 1;
                    break;
                case ToonClass.WitchDoctor:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  //Mana Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                    this.Attributes[GameAttribute.Buff_Active, ] = true;
                    this.Attributes[GameAttribute.Buff_Icon_Count0, ] = 1;
                     */
                    break;
                case ToonClass.Wizard:
                    /* // unknown
                    this.Attributes[GameAttribute.Skill_Total, ] = 1;  //Arcane Power Trait
                    this.Attributes[GameAttribute.Skill, ] = 1;
                    this.Attributes[GameAttribute.Trait, ] = 1;
                    this.Attributes[GameAttribute.Buff_Active, ] = true;
                    this.Attributes[GameAttribute.Buff_Icon_Count0, ] = 1;
                     */
                    break;
            }
        }
        public void SetAttributesMovement()
        {
            //Movement
            //scripted //this.Attributes[GameAttribute.Movement_Scalar_Total] = 1f;
            //scripted //this.Attributes[GameAttribute.Movement_Scalar_Capped_Total] = 1f;
            //scripted //this.Attributes[GameAttribute.Movement_Scalar_Subtotal] = 1f;
            this.Attributes[GameAttribute.Movement_Scalar] = 1f;
            //scripted //this.Attributes[GameAttribute.Walking_Rate_Total] = data.WalkingRate;
            this.Attributes[GameAttribute.Walking_Rate] = Toon.HeroTable.WalkingRate;
            //scripted //this.Attributes[GameAttribute.Running_Rate_Total] = data.RunningRate;
            this.Attributes[GameAttribute.Running_Rate] = Toon.HeroTable.RunningRate;
            //scripted //this.Attributes[GameAttribute.Sprinting_Rate_Total] = data.F17; //These two are guesses -Egris
            //scripted //this.Attributes[GameAttribute.Strafing_Rate_Total] = data.F18;
            this.Attributes[GameAttribute.Sprinting_Rate] = Toon.HeroTable.F17; //These two are guesses -Egris
            this.Attributes[GameAttribute.Strafing_Rate] = Toon.HeroTable.F18;
        }
        public void SetAttributesMisc()
        {
            //Miscellaneous
            this.Attributes[GameAttribute.Disabled] = true; // we should be making use of these ones too /raist.
            this.Attributes[GameAttribute.Loading] = true;
            this.Attributes[GameAttribute.Invulnerable] = true;
            this.Attributes[GameAttribute.Hidden] = false;
            this.Attributes[GameAttribute.Immobolize] = true;
            this.Attributes[GameAttribute.Untargetable] = true;
            this.Attributes[GameAttribute.CantStartDisplayedPowers] = true;
            this.Attributes[GameAttribute.IsContentRestrictedActor] = true;
            this.Attributes[GameAttribute.Trait, 0x0000CE11] = 1;
            this.Attributes[GameAttribute.TeamID] = 2;
            //this.Attributes[GameAttribute.Shared_Stash_Slots] = 14;
            this.Attributes[GameAttribute.Backpack_Slots] = 60;
            this.Attributes[GameAttribute.General_Cooldown] = 0;
        }
        public void SetAttributesByItems()
        {
            const float nonPhysDefault = 0f; //was 3.051758E-05f
            var damageAttributeMinValues = new Dictionary<DamageType, float[]>
                                               {
                                                   {DamageType.Physical, new[] {2f, 2f}},
                                                   {DamageType.Arcane, new[] {nonPhysDefault, nonPhysDefault}},
                                                   {DamageType.Cold, new[] {nonPhysDefault, nonPhysDefault}},
                                                   {DamageType.Fire, new[] {nonPhysDefault, nonPhysDefault}},
                                                   {DamageType.Holy, new[] {nonPhysDefault, nonPhysDefault}},
                                                   {DamageType.Lightning, new[] {nonPhysDefault, nonPhysDefault}},
                                                   {DamageType.Poison, new[] {nonPhysDefault, nonPhysDefault}}
                                               };

            foreach (var damageType in DamageType.AllTypes)
            {
                var weaponDamageMin = Math.Max(this.Inventory.GetItemBonus(GameAttribute.Damage_Weapon_Min, damageType.AttributeKey), damageAttributeMinValues[damageType][0]);
                var weaponDamageDelta = Math.Max(this.Inventory.GetItemBonus(GameAttribute.Damage_Weapon_Delta, damageType.AttributeKey), damageAttributeMinValues[damageType][1]);
                var weaponDamageBonusMin = this.Inventory.GetItemBonus(GameAttribute.Damage_Weapon_Bonus_Min, damageType.AttributeKey);
                var weaponDamageBonusDelta = this.Inventory.GetItemBonus(GameAttribute.Damage_Weapon_Bonus_Delta, damageType.AttributeKey);

                this.Attributes[GameAttribute.Damage_Weapon_Min, damageType.AttributeKey] = weaponDamageMin;
                this.Attributes[GameAttribute.Damage_Weapon_Delta, damageType.AttributeKey] = weaponDamageDelta;
                this.Attributes[GameAttribute.Damage_Weapon_Bonus_Min, damageType.AttributeKey] = weaponDamageBonusMin;
                this.Attributes[GameAttribute.Damage_Weapon_Bonus_Delta, damageType.AttributeKey] = weaponDamageBonusDelta;

                this.Attributes[GameAttribute.Resistance, damageType.AttributeKey] = this.Inventory.GetItemBonus(GameAttribute.Resistance, damageType.AttributeKey);
            }





            this.Attributes[GameAttribute.Armor_Item_Percent] = this.Inventory.GetItemBonus(GameAttribute.Armor_Item_Percent);
            this.Attributes[GameAttribute.Armor_Item] = this.Inventory.GetItemBonus(GameAttribute.Armor_Item);
            this.Attributes[GameAttribute.Strength_Item] = this.Inventory.GetItemBonus(GameAttribute.Strength_Item);
            this.Attributes[GameAttribute.Dexterity_Item] = this.Inventory.GetItemBonus(GameAttribute.Dexterity_Item);
            this.Attributes[GameAttribute.Intelligence_Item] = this.Inventory.GetItemBonus(GameAttribute.Intelligence_Item);




            this.Attributes[GameAttribute.Hitpoints_Max_Percent_Bonus_Item] = this.Inventory.GetItemBonus(GameAttribute.Hitpoints_Max_Percent_Bonus_Item);
            this.Attributes[GameAttribute.Hitpoints_Max_Bonus] = this.Inventory.GetItemBonus(GameAttribute.Hitpoints_Max_Bonus);

            this.Attributes[GameAttribute.Attacks_Per_Second_Item] = this.Inventory.GetItemBonus(GameAttribute.Attacks_Per_Second_Item);


            this.Attributes[GameAttribute.Resistance_Freeze] = this.Inventory.GetItemBonus(GameAttribute.Resistance_Freeze);
            this.Attributes[GameAttribute.Resistance_Penetration] = this.Inventory.GetItemBonus(GameAttribute.Resistance_Penetration);
            this.Attributes[GameAttribute.Resistance_Percent] = this.Inventory.GetItemBonus(GameAttribute.Resistance_Percent);
            this.Attributes[GameAttribute.Resistance_Root] = this.Inventory.GetItemBonus(GameAttribute.Resistance_Root);
            this.Attributes[GameAttribute.Resistance_Stun] = this.Inventory.GetItemBonus(GameAttribute.Resistance_Stun);
            this.Attributes[GameAttribute.Resistance_StunRootFreeze] = this.Inventory.GetItemBonus(GameAttribute.Resistance_StunRootFreeze);

            this.Attributes[GameAttribute.Hitpoints_Regen_Per_Second] = this.Inventory.GetItemBonus(GameAttribute.Hitpoints_Regen_Per_Second); //this.Toon.HeroTable.GetHitRecoveryBase +(this.Toon.HeroTable.GetHitRecoveryPerLevel *this.Toon.Level);

        }
        public void SetAttributesSkillSets()
        {
            // unlocking assigned skills
            for (int i = 0; i < this.SkillSet.ActiveSkills.Length; i++)
            {
                if (this.SkillSet.ActiveSkills[i].snoSkill != -1)
                {
                    this.Attributes[GameAttribute.Skill, this.SkillSet.ActiveSkills[i].snoSkill] = 1;
                    //scripted //this.Attributes[GameAttribute.Skill_Total, this.SkillSet.ActiveSkills[i].snoSkill] = 1;
                    // update rune attributes for new skill
                    this.Attributes[GameAttribute.Rune_A, this.SkillSet.ActiveSkills[i].snoSkill] = this.SkillSet.ActiveSkills[i].snoRune == 0 ? 1 : 0;
                    this.Attributes[GameAttribute.Rune_B, this.SkillSet.ActiveSkills[i].snoSkill] = this.SkillSet.ActiveSkills[i].snoRune == 1 ? 1 : 0;
                    this.Attributes[GameAttribute.Rune_C, this.SkillSet.ActiveSkills[i].snoSkill] = this.SkillSet.ActiveSkills[i].snoRune == 2 ? 1 : 0;
                    this.Attributes[GameAttribute.Rune_D, this.SkillSet.ActiveSkills[i].snoSkill] = this.SkillSet.ActiveSkills[i].snoRune == 3 ? 1 : 0;
                    this.Attributes[GameAttribute.Rune_E, this.SkillSet.ActiveSkills[i].snoSkill] = this.SkillSet.ActiveSkills[i].snoRune == 4 ? 1 : 0;
                }
            }
            for (int i = 0; i < this.SkillSet.PassiveSkills.Length; ++i)
            {
                if (this.SkillSet.PassiveSkills[i] != -1)
                {
                    // switch on passive skill
                    this.Attributes[GameAttribute.Trait, this.SkillSet.PassiveSkills[i]] = 1;
                    this.Attributes[GameAttribute.Skill, this.SkillSet.PassiveSkills[i]] = 1;
                    //scripted //this.Attributes[GameAttribute.Skill_Total, this.SkillSet.PassiveSkills[i]] = 1;
                }
            }
        }
        public void SetAttributesOther()
        {
            //Bonus stats
            this.Attributes[GameAttribute.Hit_Chance] = 1f;

            this.Attributes[GameAttribute.Attacks_Per_Second] = 1.2f;
            //this.Attributes[GameAttribute.Attacks_Per_Second_Item] = 1.199219f;
            this.Attributes[GameAttribute.Crit_Percent_Base] = 0.05f; //5% Critical Chance Base of all classes [Necrosummon]
            this.Attributes[GameAttribute.Crit_Percent_Cap] = Toon.HeroTable.CritPercentCap;
            this.Attributes[GameAttribute.Crit_Damage_Percent] = (int)0.5; // Always starts with 50% CD, if this isn't here, Ruthless passive bonus will double his CD bonus. [Necrosummon]
            //scripted //this.Attributes[GameAttribute.Casting_Speed_Total] = 1f;
            this.Attributes[GameAttribute.Casting_Speed] = 1f;

            //Basic stats
            this.Attributes[GameAttribute.Level_Cap] = 60;
            this.Attributes[GameAttribute.Level] = this.Toon.Level;
            this.Attributes[GameAttribute.Experience_Next] = this.Toon.ExperienceNext;
            this.Attributes[GameAttribute.Experience_Granted] = 1000;
            this.Attributes[GameAttribute.Armor] = 0;
            //scripted //this.Attributes[GameAttribute.Armor_Total]


            this.Attributes[GameAttribute.Strength] = this.Strength;
            this.Attributes[GameAttribute.Dexterity] = this.Dexterity;
            this.Attributes[GameAttribute.Vitality] = this.Vitality;
            this.Attributes[GameAttribute.Intelligence] = this.Intelligence;

            //Hitpoints have to be calculated after Vitality
            this.Attributes[GameAttribute.Hitpoints_Factor_Level] = Toon.HeroTable.HitpointsFactorLevel;
            this.Attributes[GameAttribute.Hitpoints_Factor_Vitality] = 10f;
            this.Attributes[GameAttribute.Hitpoints_Max] = 40f;

            //TestOutPutItemAttributes(); //Activate this only for finding item stats.
            this.Attributes.BroadcastChangedIfRevealed();
        }

        public void SetAttributesPassiveSkills()
        {
            // Passive Bonus activate when you enter in the game [Necrosummon]
            this.SkillSet.PassiveSkillsEffects(this);
        }


        public void AllTheScriptedStats()
        {
            //scripted //this.Attributes[GameAttribute.Damage_Delta_Total, 0] = 1f;
            //scripted //this.Attributes[GameAttribute.Damage_Delta_Total, 1] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Delta_Total, 2] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Delta_Total, 3] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Delta_Total, 4] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Delta_Total, 5] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Delta_Total, 6] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Total, 0] = 2f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Total, 1] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Total, 2] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Total, 3] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Total, 4] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Total, 5] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Total, 6] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Total, 0xFFFFF] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Subtotal, 0] = 2f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Subtotal, 1] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Subtotal, 2] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Subtotal, 3] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Subtotal, 4] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Subtotal, 5] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Subtotal, 6] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Min_Subtotal, 0xFFFFF] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 0] = 2f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 1] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 2] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 3] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 4] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 5] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 6] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total, 0] = 2f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_All] = 2f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_MainHand, 0] = 2f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Min_Total_CurrentHand, 0xFFFFF] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_SubTotal, 0] = 1f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 0] = 1f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 1] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 2] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 3] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 4] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 5] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_CurrentHand, 6] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total, 0] = 1f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_All] = 1f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Delta_Total_MainHand, 0] = 1f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Max, 0] = 3f;
            //scripted //this.Attributes[GameAttribute.Damage_Weapon_Max_Total, 0] = 3f; 

            //scripted //this.Attributes[GameAttribute.Attacks_Per_Second_Item_CurrentHand] = 1.199219f;
            //scripted //this.Attributes[GameAttribute.Attacks_Per_Second_Item_Total_MainHand] = 1.199219f;
            //scripted //this.Attributes[GameAttribute.Attacks_Per_Second_Total] = 1.199219f;

            //scripted //this.Attributes[GameAttribute.Attacks_Per_Second_Item_MainHand] = 1.199219f;
            //scripted //this.Attributes[GameAttribute.Attacks_Per_Second_Item_Total] = 1.199219f;
            //scripted //this.Attributes[GameAttribute.Attacks_Per_Second_Item_Subtotal] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Attacks_Per_Second_Item] = 3.051758E-05f;

            //scripted //this.Attributes[GameAttribute.Strength_Total] = this.StrengthTotal;
            //scripted //this.Attributes[GameAttribute.Intelligence_Total] = this.IntelligenceTotal;
            //scripted //this.Attributes[GameAttribute.Dexterity_Total] = this.DexterityTotal;
            //scripted //this.Attributes[GameAttribute.Vitality_Total] = this.VitalityTotal;

            //scripted //this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = 3.051758E-05f;
            //scripted //this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = 40f; // For now, this just adds 40 hitpoints to the hitpoints gained from vitality
            //scripted //this.Attributes[GameAttribute.Hitpoints_Total_From_Vitality] = this.Attributes[GameAttribute.Vitality] * this.Attributes[GameAttribute.Hitpoints_Factor_Vitality];
            //this.Attributes[GameAttribute.Hitpoints_Max] = this.Attributes[GameAttribute.Hitpoints_Total_From_Level] + this.Attributes[GameAttribute.Hitpoints_Total_From_Vitality];

            //scripted //this.Attributes[GameAttribute.Hitpoints_Max_Total] = GetMaxTotalHitpoints();

            //Resistance
            //scripted //this.Attributes[GameAttribute.Resistance_From_Intelligence] = this.Attributes[GameAttribute.Intelligence] * 0.1f;
            //scripted //this.Attributes[GameAttribute.Resistance_Total, 0] = this.Attributes[GameAttribute.Resistance_From_Intelligence]; // im pretty sure key = 0 doesnt do anything since the lookup is (attributeId | (key << 12)), maybe this is some base resistance? /cm
            // likely the physical school of damage, it probably doesn't actually do anything in this case (or maybe just not for the player's hero)
            // but exists for the sake of parity with weapon damage schools
            //scripted //this.Attributes[GameAttribute.Resistance_Total, 1] = this.Attributes[GameAttribute.Resistance_From_Intelligence]; //Fire
            //scripted //this.Attributes[GameAttribute.Resistance_Total, 2] = this.Attributes[GameAttribute.Resistance_From_Intelligence]; //Lightning
            //scripted //this.Attributes[GameAttribute.Resistance_Total, 3] = this.Attributes[GameAttribute.Resistance_From_Intelligence]; //Cold
            //scripted //this.Attributes[GameAttribute.Resistance_Total, 4] = this.Attributes[GameAttribute.Resistance_From_Intelligence]; //Poison
            //scripted //this.Attributes[GameAttribute.Resistance_Total, 5] = this.Attributes[GameAttribute.Resistance_From_Intelligence]; //Arcane
            //scripted //this.Attributes[GameAttribute.Resistance_Total, 6] = this.Attributes[GameAttribute.Resistance_From_Intelligence]; //Holy

            //scripted //this.Attributes[GameAttribute.Resistance_Total, 0xDE] = 0.5f;
            //scripted //this.Attributes[GameAttribute.Resistance_Total, 0x226] = 0.5f;


        }


        #endregion

        #region game-message handling & consumers

        /// <summary>
        /// Consumes the given game-message.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="message">The GameMessage.</param>
        public void Consume(GameClient client, GameMessage message)
        {
            if (message is AssignActiveSkillMessage) OnAssignActiveSkill(client, (AssignActiveSkillMessage)message);
            else if (message is AssignTraitsMessage) OnAssignPassiveSkills(client, (AssignTraitsMessage)message);
            //else if (message is PlayerChangeHotbarButtonMessage) OnPlayerChangeHotbarButtonMessage(client, (PlayerChangeHotbarButtonMessage)message);
            else if (message is TargetMessage) OnObjectTargeted(client, (TargetMessage)message);
            else if (message is ACDClientTranslateMessage) OnPlayerMovement(client, (ACDClientTranslateMessage)message);
            else if (message is TryWaypointMessage) OnTryWaypoint(client, (TryWaypointMessage)message);
            else if (message is RequestBuyItemMessage) OnRequestBuyItem(client, (RequestBuyItemMessage)message);
            else if (message is RequestSellItemMessage) OnRequestSellItem(client, (RequestSellItemMessage)message);
            //else if (message is RequestAddSocketMessage) OnRequestAddSocket(client, (RequestAddSocketMessage)message);
            else if (message is HirelingDismissMessage) OnHirelingDismiss();
            //else if (message is SocketSpellMessage) OnSocketSpell(client, (SocketSpellMessage)message);
            else if (message is PlayerTranslateFacingMessage) OnTranslateFacing(client, (PlayerTranslateFacingMessage)message);
            else if (message is SecondaryAnimationPowerMessage) OnSecondaryPowerMessage(client, (SecondaryAnimationPowerMessage)message);
            else if (message is RequestBuffCancelMessage) OnRequestBuffCancel(client, (RequestBuffCancelMessage)message);
            else if (message is CancelChanneledSkillMessage) OnCancelChanneledSkill(client, (CancelChanneledSkillMessage)message);
            else if (message is TutorialShownMessage) OnTutorialShown(client, (TutorialShownMessage)message);
            else return;
        }

        private void OnTutorialShown(GameClient client, TutorialShownMessage message)
        {
            // Server has to save what tutorials are shown, so the player
            // does not have to see them over and over...
            for (int i = 0; i < this.SeenTutorials.Length; i++)
            {
                if (this.SeenTutorials[i] == -1)
                {
                    this.SeenTutorials[i] = message.SNOTutorial;
                    break;
                }
            }
        }

        private void OnTranslateFacing(GameClient client, PlayerTranslateFacingMessage message)
        {
            this.SetFacingRotation(message.Angle);

            World.BroadcastExclusive(new ACDTranslateFacingMessage
            {
                ActorId = this.DynamicID,
                Angle = message.Angle,
                TurnImmediately = message.TurnImmediately
            }, this);
        }

        private void OnAssignActiveSkill(GameClient client, AssignActiveSkillMessage message)
        {
            var oldSNOSkill = this.SkillSet.ActiveSkills[message.SkillIndex].snoSkill; // find replaced skills SNO.
            if (oldSNOSkill != -1)
            {
                //// if old power was socketted, pickup rune
                //Item oldRune = this.Inventory.RemoveRune(message.SkillIndex);
                //if (oldRune != null)
                //{
                //    if (!this.Inventory.PickUp(oldRune))
                //    {
                //        // full inventory, cancel socketting
                //        this.Inventory.SetRune(oldRune, oldSNOSkill, message.SkillIndex); // readd old rune
                //        return;
                //    }
                //}
                this.Attributes[GameAttribute.Skill, oldSNOSkill] = 0;
                //scripted //this.Attributes[GameAttribute.Skill_Total, oldSNOSkill] = 0;
            }

            this.Attributes[GameAttribute.Skill, message.SNOSkill] = 1;
            //scripted //this.Attributes[GameAttribute.Skill_Total, message.SNOSkill] = 1;
            this.SkillSet.ActiveSkills[message.SkillIndex].snoSkill = message.SNOSkill;
            this.SkillSet.ActiveSkills[message.SkillIndex].snoRune = message.RuneIndex;
            this.SkillSet.SwitchUpdateSkills(oldSNOSkill, message.SNOSkill, message.RuneIndex, this.Toon);
            this.SetAttributesSkillSets();

            this.Attributes.BroadcastChangedIfRevealed();
            this.UpdateHeroState();

            if (oldSNOSkill != -1)  // don't do cooldown when first skill put in slot
                _StartSkillCooldown(message.SNOSkill, SkillChangeCooldownLength);
        }

        private void OnAssignPassiveSkills(GameClient client, AssignTraitsMessage message)
        {
            for (int i = 0; i < message.SNOPowers.Length; ++i)
            {
                int oldSNOSkill = this.SkillSet.PassiveSkills[i]; // find replaced skills SNO.
                if (message.SNOPowers[i] != oldSNOSkill)
                {
                    if (oldSNOSkill != -1)
                    {
                        // switch off old passive skill
                        this.Attributes[GameAttribute.Trait, oldSNOSkill] = 0;
                        this.Attributes[GameAttribute.Skill, oldSNOSkill] = 0;
                        //scripted //this.Attributes[GameAttribute.Skill_Total, oldSNOSkill] = 0;
                        this.SkillSet.PassiveSkillsRemoveEffect(this); // Remove effect when you quit the passive [Necrosummon]
                    }

                    if (message.SNOPowers[i] != -1)
                    {
                        // switch on new passive skill
                        this.Attributes[GameAttribute.Trait, message.SNOPowers[i]] = 1;
                        this.Attributes[GameAttribute.Skill, message.SNOPowers[i]] = 1;
                        //scripted //this.Attributes[GameAttribute.Skill_Total, message.SNOPowers[i]] = 1;
                    }

                    this.SkillSet.PassiveSkills[i] = message.SNOPowers[i];

                    if (oldSNOSkill != -1)  // don't do cooldown when first skill put in slot
                        _StartSkillCooldown(message.SNOPowers[i], SkillChangeCooldownLength);
                }
            }

            this.SkillSet.UpdatePassiveSkills(this.Toon, this);
            this.Attributes.BroadcastChangedIfRevealed();
            this.UpdateHeroState();
        }

        private void _StartSkillCooldown(int snoPower, float seconds)
        {
            this.World.BuffManager.AddBuff(this, this,
                new Powers.Implementations.CooldownBuff(snoPower, seconds));
        }

        //private void OnPlayerChangeHotbarButtonMessage(GameClient client, PlayerChangeHotbarButtonMessage message)
        //{
        //    this.SkillSet.HotBarSkills[message.BarIndex] = message.ButtonData;
        //}

        /// <summary>
        /// Sockets skill with rune.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="socketSpellMessage"></param>
        //private void OnSocketSpell(GameClient client, SocketSpellMessage socketSpellMessage)
        //{
        //    Item rune = this.Inventory.GetItem(unchecked((uint)socketSpellMessage.RuneDynamicId));
        //    int PowerSNOId = socketSpellMessage.PowerSNOId;
        //    int skillIndex = -1; // find index of power in skills
        //    for (int i = 0; i < this.SkillSet.ActiveSkills.Length; i++)
        //    {
        //        if (this.SkillSet.ActiveSkills[i] == PowerSNOId)
        //        {
        //            skillIndex = i;
        //            break;
        //        }
        //    }
        //    if (skillIndex == -1)
        //    {
        //        // validity of message is controlled on client side, this shouldn't happen
        //        return;
        //    }
        //    Item oldRune = this.Inventory.RemoveRune(skillIndex); // removes old rune (if present)
        //    if (rune.Attributes[GameAttribute.Rune_Rank] != 0)
        //    {
        //        // unattuned rune: pick random color, create new rune, set attunement to new rune and destroy unattuned one
        //        int rank = rune.Attributes[GameAttribute.Rune_Rank];
        //        int colorIndex = RandomHelper.Next(0, 5);
        //        Item newRune = ItemGenerator.Cook(this, "Runestone_" + (char)('A' + colorIndex) + "_0" + rank); // TODO: quite of hack, find better solution /xsochor
        //        newRune.Attributes[GameAttribute.Rune_Attuned_Power] = PowerSNOId;
        //        switch (colorIndex)
        //        {
        //            case 0:
        //                newRune.Attributes[GameAttribute.Rune_A] = rank;
        //                break;
        //            case 1:
        //                newRune.Attributes[GameAttribute.Rune_B] = rank;
        //                break;
        //            case 2:
        //                newRune.Attributes[GameAttribute.Rune_C] = rank;
        //                break;
        //            case 3:
        //                newRune.Attributes[GameAttribute.Rune_D] = rank;
        //                break;
        //            case 4:
        //                newRune.Attributes[GameAttribute.Rune_E] = rank;
        //                break;
        //        }
        //        newRune.Owner = this;
        //        newRune.InventoryLocation.X = rune.InventoryLocation.X; // sets position of original
        //        newRune.InventoryLocation.Y = rune.InventoryLocation.Y; // sets position of original
        //        this.Inventory.DestroyInventoryItem(rune); // destroy unattuned rune
        //        newRune.EnterWorld(this.Position);
        //        newRune.Reveal(this);
        //        this.Inventory.SetRune(newRune, PowerSNOId, skillIndex);
        //    }
        //    else
        //    {
        //        this.Inventory.SetRune(rune, PowerSNOId, skillIndex);
        //    }
        //    if (oldRune != null)
        //    {
        //        this.Inventory.PickUp(oldRune); // pick removed rune
        //    }
        //    this.Attributes.BroadcastChangedIfRevealed();
        //    UpdateHeroState();
        //}

        private void OnObjectTargeted(GameClient client, TargetMessage message)
        {
            bool powerHandled = this.World.PowerManager.RunPower(this, message.PowerSNO, message.TargetID, message.Field2.Position, message);

            if (!powerHandled)
            {
                Actor actor = this.World.GetActorByDynamicId(message.TargetID);
                if (actor == null) return;

                if ((actor.GBHandle.Type == 1) && (actor.Attributes[GameAttribute.TeamID] == 10))
                {
                    this.ExpBonusData.MonsterAttacked(this.InGameClient.Game.TickCounter);
                }

                actor.OnTargeted(this, message);
            }

            this.ExpBonusData.Check(2);
        }

        private void OnPlayerMovement(GameClient client, ACDClientTranslateMessage message)
        {
            // here we should also be checking the position and see if it's valid. If not we should be resetting player to a good position with ACDWorldPositionMessage
            // so we can have a basic precaution for hacks & exploits /raist.
            if (message.Position != null)
                this.Position = message.Position;

            this.SetFacingRotation(message.Angle);

            var msg = new ACDTranslateNormalMessage
            {
                ActorId = (int)this.DynamicID,
                Position = this.Position,
                Angle = message.Angle,
                TurnImmediately = false,
                Speed = message.Speed,
                AnimationTag = message.AnimationTag
            };

            this.RevealScenesToPlayer();
            this.RevealActorsToPlayer();

            this.World.BroadcastExclusive(msg, this); // TODO: We should be instead notifying currentscene we're in. /raist.

            foreach (var actor in GetActorsInRange())
                actor.OnPlayerApproaching(this);

            this.CollectGold();
            this.CollectHealthGlobe();
        }

        private void OnCancelChanneledSkill(GameClient client, CancelChanneledSkillMessage message)
        {
            this.World.PowerManager.CancelChanneledSkill(this, message.PowerSNO);
        }

        private void OnRequestBuffCancel(GameClient client, RequestBuffCancelMessage message)
        {
            this.World.BuffManager.RemoveBuffs(this, message.PowerSNOId);
        }

        private void OnSecondaryPowerMessage(GameClient client, SecondaryAnimationPowerMessage message)
        {
            this.World.PowerManager.RunPower(this, message.PowerSNO);
        }

        private void OnTryWaypoint(GameClient client, TryWaypointMessage tryWaypointMessage)
        {
            var wayPoint = this.World.GetWayPointById(tryWaypointMessage.Field1);
            if (wayPoint == null)
            {
                var actData = (Mooege.Common.MPQ.FileFormats.Act)MPQStorage.Data.Assets[Common.Types.SNO.SNOGroup.Act][70015].Data;
                var wayPointInfo = actData.WayPointInfo;
                var AlterWayPoint = this.World.Game.GetWorld(wayPointInfo[tryWaypointMessage.Field1].SNOWorld).GetWayPointById(tryWaypointMessage.Field1);

                if (this.World.WorldSNO.Id != wayPointInfo[tryWaypointMessage.Field1].SNOWorld)
                    this.ChangeWorld(this.World.Game.GetWorld(wayPointInfo[tryWaypointMessage.Field1].SNOWorld), AlterWayPoint.Position);
            }
            if (wayPoint == null) return;
            this.Teleport(wayPoint.Position);
        }

        private void OnRequestBuyItem(GameClient client, RequestBuyItemMessage requestBuyItemMessage)
        {
            var vendor = this.SelectedNPC as Vendor;
            if (vendor == null)
                return;
            vendor.OnRequestBuyItem(this, requestBuyItemMessage.ItemId);
        }

        private void OnRequestSellItem(GameClient client, RequestSellItemMessage requestSellItemMessage)
        {
            var player = this.InGameClient.Player;
            var vendor = this.SelectedNPC as Vendor;

            var item = this.Inventory.GetItem(requestSellItemMessage.ItemId);
            if (item == null)
                return;

            vendor.OnRequestSellItem(player, requestSellItemMessage.ItemId);
        }

        //private void OnRequestAddSocket(GameClient client, RequestAddSocketMessage requestAddSocketMessage)
        //{
        //    var item = World.GetItem(requestAddSocketMessage.ItemID);
        //    if (item == null || item.Owner != this)
        //        return;
        //    var jeweler = World.GetActorInstance<Jeweler>();
        //    if (jeweler == null)
        //        return;

        //    jeweler.OnAddSocket(this, item);
        //}

        private void OnHirelingDismiss()
        {
            ActiveHireling = null;
        }

        #endregion

        #region update-logic

        public void Update(int tickCounter)
        {
            // Check the Killstreaks
            this.ExpBonusData.Check(0);
            this.ExpBonusData.Check(1);

            // Check if there is an conversation to close in this tick
            Conversations.Update(this.World.Game.TickCounter);

            _UpdateResources();
        }

        public bool IsPlayerDead()
        {
            if (this.Attributes[GameAttribute.Hitpoints_Cur] == 0)
                return true;
            else
                return false;
        }
        #endregion

        #region enter, leave, reveal handling

        /// <summary>
        /// Revals scenes in player's proximity.
        /// </summary>
        public void RevealScenesToPlayer()
        {
            var scenes = this.GetScenesInRegion(DefaultQueryProximityLenght * 2);

            foreach (var scene in scenes) // reveal scenes in player's proximity.
            {
                if (scene.IsRevealedToPlayer(this)) // if the actors is already revealed skip it.
                    continue; // if the scene is already revealed, skip it.

                if (scene.Parent != null) // if it's a subscene, always make sure it's parent get reveals first and then it reveals his childs.
                    scene.Parent.Reveal(this);
                else
                    scene.Reveal(this);
            }
        }

        /// <summary>
        /// Reveals actors in player's proximity.
        /// </summary>
        public void RevealActorsToPlayer()
        {
            var actors = this.GetActorsInRange();

            foreach (var actor in actors) // reveal actors in player's proximity.
            {
                if (actor.Visible == false || actor.IsRevealedToPlayer(this)) // if the actors is already revealed, skip it.
                    continue;

                if (actor.ActorType == ActorType.Gizmo || actor.ActorType == ActorType.Player
                    || actor.ActorType == ActorType.Monster || actor.ActorType == ActorType.Enviroment
                    || actor.ActorType == ActorType.Critter || actor.ActorType == ActorType.Item || actor.ActorType == ActorType.ServerProp)
                    actor.Reveal(this);
            }
        }

        public override void OnEnter(World world)
        {
            this.World.Reveal(this);

            this.RevealScenesToPlayer(); // reveal scenes in players proximity.
            this.RevealActorsToPlayer(); // reveal actors in players proximity.

            // load all inventory items
            if (!this.Inventory.Loaded)//why reload if already loaded?
                this.Inventory.LoadFromDB();
            else
                this.Inventory.RefreshInventoryToClient();

            // generate visual update message
            this.Inventory.SendVisualInventory(this);

            SetAllStatsInCorrectOrder();
        }

        public override void OnTeleport()
        {
            this.RevealScenesToPlayer(); // reveal scenes in players proximity.
            this.RevealActorsToPlayer(); // reveal actors in players proximity.
        }

        public override void OnLeave(World world)
        {
            this.Conversations.StopAll();
            this.Inventory.RefreshInventoryToClient();

            // save visual equipment
            this.Toon.HeroNameField.Value = this.Toon.Name; // Refresh Character Name when is changed for the !changename command [Necrosummon]
            this.Toon.HeroFlagsField.Value = this.Toon.Gender; // Refresh character gender when is changed with the !changesex command [Necrosummon]
            this.Toon.HeroVisualEquipmentField.Value = this.Inventory.GetVisualEquipment(); // Refresh the character visual equipment when you logout [Necrosummon]
            this.Toon.GameAccount.ChangedFields.SetPresenceFieldValue(this.Toon.HeroVisualEquipmentField);
            this.Toon.GameAccount.ChangedFields.SetPresenceFieldValue(this.Toon.HeroLevelField);
            this.Toon.GameAccount.ChangedFields.SetPresenceFieldValue(this.Toon.HeroNameField); // Refresh character name when is changed with the !changename command [Necrosummon]
            this.Toon.GameAccount.ChangedFields.SetPresenceFieldValue(this.Toon.HeroFlagsField); // Refresh character gender when is changed with the !changesex command [Necrosummon]
            // save all inventory items
            this.Inventory.SaveToDB();
            world.CleanupItemInstances();
        }

        public override bool Reveal(Player player)
        {
            if (!base.Reveal(player))
                return false;

            if (this == player) // only send this when player's own actor being is revealed. /raist.
            {
                player.InGameClient.SendMessage(new PlayerWarpedMessage()
                {
                    Field0 = 9,
                    Field1 = 0f,
                });
            }

            player.InGameClient.SendMessage(new PlayerEnterKnownMessage()
            {
                PlayerIndex = this.PlayerIndex,
                ActorId = this.DynamicID,
            });

            this.Inventory.SendVisualInventory(player);

            if (this == player) // only send this to player itself. Warning: don't remove this check or you'll make the game start crashing! /raist.
            {
                player.InGameClient.SendMessage(new PlayerActorSetInitialMessage()
                {
                    ActorId = this.DynamicID,
                    PlayerIndex = this.PlayerIndex,
                });
            }

            this.Inventory.Reveal(player);

            return true;
        }

        public override bool Unreveal(Player player)
        {
            if (!base.Unreveal(player))
                return false;

            this.Inventory.Unreveal(player);

            return true;
        }

        public override void BeforeChangeWorld()
        {
            this.Inventory.Unreveal(this);
        }

        public override void AfterChangeWorld()
        {
            this.Inventory.Reveal(this);
        }

        #endregion

        #region hero-state

        /// <summary>
        /// Allows hero state message to be sent when hero's some property get's updated.
        /// </summary>
        public void UpdateHeroState()
        {
            this.InGameClient.SendMessage(new HeroStateMessage
            {
                State = this.GetStateData()
            });
        }

        public HeroStateData GetStateData()
        {
            return new HeroStateData()
            {
                Field0 = 0x00000000,
                Field1 = 0x00000000,
                Field2 = 0x00000000,
                Field3 = -1,
                PlayerFlags = (int)Toon.Flags,
                PlayerSavedData = this.GetSavedData(),
                QuestRewardHistoryEntriesCount = 0x00000000,
                tQuestRewardHistory = QuestRewardHistory,
            };
        }

        #endregion

        #region player attribute handling



        public float Strength
        {
            get
            {
                var baseStrength = 0.0f;


                if (Toon.HeroTable.CoreAttribute == Mooege.Common.MPQ.FileFormats.PrimaryAttribute.Strength)
                    baseStrength = Toon.HeroTable.Strength + ((this.Toon.Level - 1) * 3);
                else
                    baseStrength = Toon.HeroTable.Strength + (this.Toon.Level - 1);

                return baseStrength;
            }
        }

        public float Dexterity
        {
            get
            {
                if (Toon.HeroTable.CoreAttribute == Mooege.Common.MPQ.FileFormats.PrimaryAttribute.Dexterity)
                    return Toon.HeroTable.Dexterity + ((this.Toon.Level - 1) * 3);
                else
                    return Toon.HeroTable.Dexterity + (this.Toon.Level - 1);
            }
        }

        public float Vitality
        {
            get
            {
                return Toon.HeroTable.Vitality + ((this.Toon.Level - 1) * 2);
            }
        }

        public float Intelligence
        {
            get
            {
                if (Toon.HeroTable.CoreAttribute == Mooege.Common.MPQ.FileFormats.PrimaryAttribute.Intelligence)
                    return Toon.HeroTable.Intelligence + ((this.Toon.Level - 1) * 3);
                else
                    return Toon.HeroTable.Intelligence + (this.Toon.Level - 1);
            }
        }

        #endregion

        #region saved-data

        private PlayerSavedData GetSavedData()
        {
            return new PlayerSavedData()
            {
                HotBarButtons = this.SkillSet.HotBarSkills,
                HotBarButton = new HotbarButtonData { SNOSkill = -1, Field1 = -1, ItemGBId = -1 },
                Field2 = 0xB4,
                PlaytimeTotal = (int)this.Toon.TimePlayed,
                WaypointFlags = 0x000005F7,
                #region Флаги для порталов 0 - 100
                //WaypointFlags = 0x00000003, // Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000005, // Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000007, // Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000009, // Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000000B, // Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000000D, // Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000000F, // Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000011, // Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000011, // Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000015, // Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000017, // Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000019, // Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000001B, // Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000001D, // Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000001F, // Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000021, // Уединенные покои и новый Тристрам
                //WaypointFlags = 0x00000023, // Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000025, // Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000027, // Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000029, // Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000002B, // Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000002D, // Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000002F, // Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000031, // Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000033, // Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000035, // Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000037, // Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000039, // Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000003B, // Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000003D, // Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000003F, // Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000041, // Кладбище проклятых и новый Тристрам
                //WaypointFlags = 0x00000043, // Кладбище проклятых,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000045, // Кладбище проклятых,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000047, // Кладбище проклятых,Соборный сад,Старый Тристрам  и новый Тристрам
                //WaypointFlags = 0x00000049, // Кладбище проклятых,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000000B, // Кладбище проклятых,Собор 3,Старый Тристрам и новый Тристрам
                //WaypointFlags = 0x0000000D, // Кладбище проклятых,Собор 3,Соборный сад, и новый Тристрам
                //WaypointFlags = 0x0000000F, // Кладбище проклятых,Собор 3,Соборный сад,Старый Тристрам и новый Тристрам

                //WaypointFlags = 0x00000051, // Кладбище проклятых,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000053, // Кладбище проклятых,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000055, // Кладбище проклятых,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000057, // Кладбище проклятых,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000059, // Кладбище проклятых,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000000B, // Кладбище проклятых,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000000D, // Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000000F, // Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000061, // Кладбище проклятых,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x00000063, // Кладбище проклятых,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000065, // Кладбище проклятых,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000067, // Кладбище проклятых,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000069, // Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000000B, // Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000000D, // Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000000F, // Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам

                //WaypointFlags = 0x00000071, // Кладбище проклятых,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000073, // Кладбище проклятых,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000075, // Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000077, // Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000079, // Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000000B, // Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000000D, // Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000000F, // Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000081, // Гиблые поля и новый Тристрам
                //WaypointFlags = 0x00000083, // Гиблые поля,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000085, // Гиблые поля,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000087, // Гиблые поля,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000089, // Гиблые поля,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000000B, // Гиблые поля,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000000D, // Гиблые поля,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000000F, // Гиблые поля,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000091, // Гиблые поля,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000093, // Гиблые поля,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000095, // Гиблые поля,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000097, // Гиблые поля,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000099, // Гиблые поля,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000009B, // Гиблые поля,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000009D, // Гиблые поля,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000009F, // Гиблые поля,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000000A1, // 
                //WaypointFlags = 0x000000A3, // 
                //WaypointFlags = 0x000000A5, // 
                //WaypointFlags = 0x000000A7, // 
                //WaypointFlags = 0x000000A9, // 
                //WaypointFlags = 0x000000AB, // 
                //WaypointFlags = 0x000000AD, // 
                //WaypointFlags = 0x000000AF, //

                //WaypointFlags = 0x000000B1, // 
                //WaypointFlags = 0x000000B3, // 
                //WaypointFlags = 0x000000B5, // 
                //WaypointFlags = 0x000000B7, // 
                //WaypointFlags = 0x000000B9, // 
                //WaypointFlags = 0x000000BB, // 
                //WaypointFlags = 0x000000BD, // 
                //WaypointFlags = 0x000000BF, //

                //WaypointFlags = 0x000000C1, // 
                //WaypointFlags = 0x000000C3, // 
                //WaypointFlags = 0x000000C5, // 
                //WaypointFlags = 0x000000C7, // 
                //WaypointFlags = 0x000000C9, // 
                //WaypointFlags = 0x000000CB, // 
                //WaypointFlags = 0x000000CD, // 
                //WaypointFlags = 0x000000CF, // 

                //WaypointFlags = 0x000000D1, // 
                //WaypointFlags = 0x000000D3, // 
                //WaypointFlags = 0x000000D5, // 
                //WaypointFlags = 0x000000D7, // 
                //WaypointFlags = 0x000000D9, // 
                //WaypointFlags = 0x000000DB, // 
                //WaypointFlags = 0x000000DD, // 
                //WaypointFlags = 0x000000DF, // 

                //WaypointFlags = 0x000000E1, // 
                //WaypointFlags = 0x000000E3, // 
                //WaypointFlags = 0x000000E5, // 
                //WaypointFlags = 0x000000E7, // 
                //WaypointFlags = 0x000000E9, // 
                //WaypointFlags = 0x000000EB, // 
                //WaypointFlags = 0x000000ED, // 
                //WaypointFlags = 0x000000EF, // 

                //WaypointFlags = 0x000000F1, // 
                //WaypointFlags = 0x000000F3, // 
                //WaypointFlags = 0x000000F5, // 
                //WaypointFlags = 0x000000F7, // Гиблые поля, Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000000F9, // Гиблые поля, Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000000FB, // Гиблые поля, Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000000FD, // Гиблые поля, Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000000FF, // Гиблые поля, Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                #endregion
                #region Флаги для порталов 100 - 200
                //WaypointFlags = 0x00000101, // Затопленный храм и новый Тристрам
                //WaypointFlags = 0x00000103, // Затопленный храм,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000105, // Затопленный храм,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000107, // Затопленный храм,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000109, // Затопленный храм,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000010B, // Затопленный храм,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000010D, // Затопленный храм,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000010F, // Затопленный храм,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000111, // Затопленный храм,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000113, // Затопленный храм,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000115, // Затопленный храм,Королевские гробницы,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x00000117, // Затопленный храм,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000119, // Затопленный храм,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000011B, // Затопленный храм,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000011D, // Затопленный храм,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000011F, // Затопленный храм,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000121, // Затопленный храм,Уединенные покои и новый Тристрам 
                //WaypointFlags = 0x00000123, // Затопленный храм,Уединенные покои,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000125, // Затопленный храм,Уединенные покои,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x00000127, // Затопленный храм,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000129, // Затопленный храм,Уединенные покои,Собор 3 и новый Тристрам  
                //WaypointFlags = 0x0000012B, // Затопленный храм,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x0000012D, // Затопленный храм,Уединенные покои,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x0000012F, // Затопленный храм,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 

                //WaypointFlags = 0x00000131, // Затопленный храм,Уединенные покои,Королевские гробницы и новый Тристрам  
                //WaypointFlags = 0x00000133, // Затопленный храм,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000135, // Затопленный храм,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x00000137, // Затопленный храм,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x00000139, // Затопленный храм,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000013B, // Затопленный храм,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x0000013D, // Затопленный храм,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x0000013F, // Затопленный храм,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 

                //WaypointFlags = 0x00000141, // Затопленный храм,Кладбище проклятых и новый Тристрам  
                //WaypointFlags = 0x00000143, // Затопленный храм,Кладбище проклятых,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000145, // Затопленный храм,Кладбище проклятых,Соборный сад и новый Тристрам  
                //WaypointFlags = 0x00000147, // Затопленный храм,Кладбище проклятых,Соборный сад,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x00000149, // Затопленный храм,Кладбище проклятых,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000014B, // Затопленный храм,Кладбище проклятых,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000014D, // Затопленный храм,Кладбище проклятых,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000014F, // Затопленный храм,Кладбище проклятых,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000151, // Затопленный храм,Кладбище проклятых,Королевские гробницы и новый Тристрам 
                //WaypointFlags = 0x00000153, // Затопленный храм,Кладбище проклятых,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000155, // Затопленный храм,Кладбище проклятых,Королевские гробницы,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x00000157, // Затопленный храм,Кладбище проклятых,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000159, // Затопленный храм,Кладбище проклятых,Королевские гробницы,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000015B, // Затопленный храм,Кладбище проклятых,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x0000015D, // Затопленный храм,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x0000015F, // Затопленный храм,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 

                //WaypointFlags = 0x00000161, // Затопленный храм,Кладбище проклятых,Уединенные покои и новый Тристрам  
                //WaypointFlags = 0x00000163, // Затопленный храм,Кладбище проклятых,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000165, // Затопленный храм,Кладбище проклятых,Уединенные покои,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x00000167, // Затопленный храм,Кладбище проклятых,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000169, // Затопленный храм,Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000016B, // Затопленный храм,Кладбище проклятых,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000016D, // Затопленный храм,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000016F, // Затопленный храм,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000171, // Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы и новый Тристрам  
                //WaypointFlags = 0x00000173, // Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x00000175, // Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам   
                //WaypointFlags = 0x00000177, // Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам   
                //WaypointFlags = 0x00000179, // Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам   
                //WaypointFlags = 0x0000017B, // Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x0000017D, // Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам  
                //WaypointFlags = 0x0000017F, // Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам  

                //WaypointFlags = 0x00000181, // Затопленный храм,Гиблые поля и новый Тристрам
                //WaypointFlags = 0x00000183, // Затопленный храм,Гиблые поля,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000185, // Затопленный храм,Гиблые поля,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000187, // Затопленный храм,Гиблые поля,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000189, // Затопленный храм,Гиблые поля,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000018B, // Затопленный храм,Гиблые поля,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000018D, // Затопленный храм,Гиблые поля,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000018F, // Затопленный храм,Гиблые поля,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000191, // Затопленный храм,Гиблые поля,Королевские гробницы и новый Тристрам 
                //WaypointFlags = 0x00000193, // Затопленный храм,Гиблые поля,Королевские гробницы,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000195, // Затопленный храм,Гиблые поля,Королевские гробницы,Соборный сад и новый Тристрам  
                //WaypointFlags = 0x00000197, // Затопленный храм,Гиблые поля,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x00000199, // Затопленный храм,Гиблые поля,Королевские гробницы,Собор 3 и новый Тристрам  
                //WaypointFlags = 0x0000019B, // Затопленный храм,Гиблые поля,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x0000019D, // Затопленный храм,Гиблые поля,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x0000019F, // Затопленный храм,Гиблые поля,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 

                //WaypointFlags = 0x000001A1, // Затопленный храм,Гиблые поля,Уединенные покои и новый Тристрам  
                //WaypointFlags = 0x000001A3, // Затопленный храм,Гиблые поля,Уединенные покои,Руины Тристрама и новый Тристрам   
                //WaypointFlags = 0x000001A5, // Затопленный храм,Гиблые поля,Уединенные покои,Соборный сад и новый Тристрам    
                //WaypointFlags = 0x000001A7, // Затопленный храм,Гиблые поля,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам    
                //WaypointFlags = 0x000001A9, // Затопленный храм,Гиблые поля,Уединенные покои,Собор 3 и новый Тристрам    
                //WaypointFlags = 0x000001AB, // Затопленный храм,Гиблые поля,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам   
                //WaypointFlags = 0x000001AD, // Затопленный храм,Гиблые поля,Уединенные покои,Собор 3,Соборный сад и новый Тристрам   
                //WaypointFlags = 0x000001AF, // Затопленный храм,Гиблые поля,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам   

                //WaypointFlags = 0x000001B1, // Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы и новый Тристрам  
                //WaypointFlags = 0x000001B3, // Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001B5, // Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам  
                //WaypointFlags = 0x000001B7, // Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x000001B9, // Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам  
                //WaypointFlags = 0x000001BB, // Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001BD, // Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x000001BF, // Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 

                //WaypointFlags = 0x000001C1, // Затопленный храм,Гиблые поля,Кладбище проклятых и новый Тристрам   
                //WaypointFlags = 0x000001C3, // Затопленный храм,Гиблые поля,Кладбище проклятых,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001C5, // Затопленный храм,Гиблые поля,Кладбище проклятых,Соборный сад и новый Тристрам  
                //WaypointFlags = 0x000001C7, // Затопленный храм,Гиблые поля,Кладбище проклятых,Соборный сад,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x000001C9, // Затопленный храм,Гиблые поля,Кладбище проклятых,Собор 3 и новый Тристрам  
                //WaypointFlags = 0x000001CB, // Затопленный храм,Гиблые поля,Кладбище проклятых,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001CD, // Затопленный храм,Гиблые поля,Кладбище проклятых,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x000001CF, // Затопленный храм,Гиблые поля,Кладбище проклятых,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 

                //WaypointFlags = 0x000001D1, // Затопленный храм,Гиблые поля,Кладбище проклятых,Королевские гробницы и новый Тристрам   
                //WaypointFlags = 0x000001D3, // Затопленный храм,Гиблые поля,Кладбище проклятых,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000001D5, // Затопленный храм,Гиблые поля,Кладбище проклятых,Королевские гробницы,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x000001D7, // Затопленный храм,Гиблые поля,Кладбище проклятых,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001D9, // Затопленный храм,Гиблые поля,Кладбище проклятых,Королевские гробницы,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x000001DB, // Затопленный храм,Гиблые поля,Кладбище проклятых,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000001DD, // Затопленный храм,Гиблые поля,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000001DF, // Затопленный храм,Гиблые поля,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000001E1, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои и новый Тристрам 
                //WaypointFlags = 0x000001E3, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001E5, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Соборный сад и новый Тристрам  
                //WaypointFlags = 0x000001E7, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x000001E9, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам  
                //WaypointFlags = 0x000001EB, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001ED, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x000001EF, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 

                //WaypointFlags = 0x000001F1, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы и новый Тристрам 
                //WaypointFlags = 0x000001F3, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001F5, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам  
                //WaypointFlags = 0x000001F7, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x000001F9, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам  
                //WaypointFlags = 0x000001FB, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000001FD, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x000001FF, // Затопленный храм,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 
                #endregion
                #region Флаги для порталов 200 - 300
                //WaypointFlags = 0x00000201, // Гниющий Лес и новый Тристрам
                //WaypointFlags = 0x00000203, // Гниющий Лес,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000205, // Гниющий Лес,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000207, // Гниющий Лес,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000209, // Гниющий Лес,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000020B, // Гниющий Лес,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000020D, // Гниющий Лес,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000020F, // Гниющий Лес,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000211, // Гниющий Лес,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000213, // Гниющий Лес,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000215, // Гниющий Лес,Королевские гробницы,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x00000217, // Гниющий Лес,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000219, // Гниющий Лес,Королевские гробницы,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000021B, // Гниющий Лес,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000021D, // Гниющий Лес,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000021F, // Гниющий Лес,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000221, // Гниющий Лес,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x00000223, // Гниющий Лес,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000225, // Гниющий Лес,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000227, // Гниющий Лес,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000229, // Гниющий Лес,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000022B, // Гниющий Лес,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000022D, // Гниющий Лес,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000022F, // Гниющий Лес,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000231, // Гниющий Лес,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000233, // Гниющий Лес,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000235, // Гниющий Лес,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000237, // Гниющий Лес,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000239, // Гниющий Лес,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000023B, // Гниющий Лес,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000023D, // Гниющий Лес,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000023F, // Гниющий Лес,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000241, // Гниющий Лес,Кладбище проклятых и новый Тристрам
                //WaypointFlags = 0x00000243, // Гниющий Лес,Кладбище проклятых,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000245, // Гниющий Лес,Кладбище проклятых,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000247, // Гниющий Лес,Кладбище проклятых,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000249, // Гниющий Лес,Кладбище проклятых,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000024B, // Гниющий Лес,Кладбище проклятых,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000024D, // Гниющий Лес,Кладбище проклятых,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000024F, // Гниющий Лес,Кладбище проклятых,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000251, // Гниющий Лес,Кладбище проклятых,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000253, // Гниющий Лес,Кладбище проклятых,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000255, // Гниющий Лес,Кладбище проклятых,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000257, // Гниющий Лес,Кладбище проклятых,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000259, // Гниющий Лес,Кладбище проклятых,Королевские гробницы,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000025B, // Гниющий Лес,Кладбище проклятых,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000025D, // Гниющий Лес,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000025F, // Гниющий Лес,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000261, // Гниющий Лес,Кладбище проклятых,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x00000263, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000265, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000267, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000269, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000026B, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000026D, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000026F, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000271, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000273, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000275, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000277, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000279, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам 
                //WaypointFlags = 0x0000027B, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000027D, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000027F, // Гниющий Лес,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000281, // Гниющий Лес,Гиблые поля и новый Тристрам
                //WaypointFlags = 0x00000283, // Гниющий Лес,Гиблые поля,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000285, // Гниющий Лес,Гиблые поля,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000287, // Гниющий Лес,Гиблые поля,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000289, // Гниющий Лес,Гиблые поля,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000028B, // Гниющий Лес,Гиблые поля,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000028D, // Гниющий Лес,Гиблые поля,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000028F, // Гниющий Лес,Гиблые поля,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000291, // Гниющий Лес,Гиблые поля,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000293, // Гниющий Лес,Гиблые поля,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000295, // Гниющий Лес,Гиблые поля,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000297, // Гниющий Лес,Гиблые поля,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000299, // Гниющий Лес,Гиблые поля,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000029B, // Гниющий Лес,Гиблые поля,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000029D, // Гниющий Лес,Гиблые поля,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000029F, // Гниющий Лес,Гиблые поля,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000002A1, // Гниющий Лес,Гиблые поля,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x000002A3, // Гниющий Лес,Гиблые поля,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002A5, // Гниющий Лес,Гиблые поля,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002A7, // Гниющий Лес,Гиблые поля,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000002A9, // Гниющий Лес,Гиблые поля,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000002AB, // Гниющий Лес,Гиблые поля,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002AD, // Гниющий Лес,Гиблые поля,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002AF, // Гниющий Лес,Гиблые поля,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000002B1, // Гниющий Лес,Гиблые поля,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x000002B3, // Гниющий Лес,Гиблые поля,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002B5, // Гниющий Лес,Гиблые поля,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002B7, // Гниющий Лес,Гиблые поля,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000002B9, // Гниющий Лес,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000002BB, // Гниющий Лес,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002BD, // Гниющий Лес,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002BF, // Гниющий Лес,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000002C1, // Гниющий Лес,Гиблые поля,Кладбище проклятых и новый Тристрам 
                //WaypointFlags = 0x000002C3, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000002C5, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Соборный сад и новый Тристрам  
                //WaypointFlags = 0x000002C7, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Соборный сад,Руины Тристрама и новый Тристрам  
                //WaypointFlags = 0x000002C9, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Собор 3 и новый Тристрам  
                //WaypointFlags = 0x000002CB, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Собор 3,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000002CD, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Собор 3,Соборный сад и новый Тристрам 
                //WaypointFlags = 0x000002CF, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам 

                //WaypointFlags = 0x000002D1, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x000002D3, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002D5, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002D7, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000002D9, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000002DB, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002DD, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002DF, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000002E1, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x000002E3, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002E5, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002E7, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000002E9, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000002EB, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002ED, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002EF, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000002F1, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x000002F3, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002F5, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002F7, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000002F9, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000002FB, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000002FD, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000002FF, // Гниющий Лес,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам
                #endregion
                #region Флаги для порталов 300 - 400
                //,Гиблые поля,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама
                //WaypointFlags = 0x00000301, // Гниющий Лес,Затопленный храм и новый Тристрам
                //WaypointFlags = 0x00000303, // Гниющий Лес,Затопленный храм,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000305, // Гниющий Лес,Затопленный храм,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000307, // Гниющий Лес,Затопленный храм,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000309, // Гниющий Лес,Затопленный храм,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000030B, // Гниющий Лес,Затопленный храм,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000030D, // Гниющий Лес,Затопленный храм,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000030F, // Гниющий Лес,Затопленный храм,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000311, // Гниющий Лес,Затопленный храм,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000313, // Гниющий Лес,Затопленный храм,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000315, // Гниющий Лес,Затопленный храм,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000317, // Гниющий Лес,Затопленный храм,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000319, // Гниющий Лес,Затопленный храм,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000031B, // Гниющий Лес,Затопленный храм,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000031D, // Гниющий Лес,Затопленный храм,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000031F, // Гниющий Лес,Затопленный храм,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000321, // Гниющий Лес,Затопленный храм,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x00000323, // Гниющий Лес,Затопленный храм,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000325, // Гниющий Лес,Затопленный храм,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000327, // Гниющий Лес,Затопленный храм,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000329, // Гниющий Лес,Затопленный храм,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000032B, // Гниющий Лес,Затопленный храм,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000032D, // Гниющий Лес,Затопленный храм,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000032F, // Гниющий Лес,Затопленный храм,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000331, // Гниющий Лес,Затопленный храм,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000333, // Гниющий Лес,Затопленный храм,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000335, // Гниющий Лес,Затопленный храм,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000337, // Гниющий Лес,Затопленный храм,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000339, // Гниющий Лес,Затопленный храм,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000033B, // Гниющий Лес,Затопленный храм,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000033D, // Гниющий Лес,Затопленный храм,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000033F, // Гниющий Лес,Затопленный храм,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000341, // Гниющий Лес,Затопленный храм,Кладбище проклятых и новый Тристрам
                //WaypointFlags = 0x00000343, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000345, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000347, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000349, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000034B, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000034D, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000034F, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000351, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000353, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000355, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000357, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000359, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000035B, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000035D, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000035F, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000361, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x00000363, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000365, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000367, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000369, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000036B, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000036D, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000036F, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000371, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000373, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000375, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000377, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000379, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000037B, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000037D, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000037F, // Гниющий Лес,Затопленный храм,Кладбище проклятых,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000381, // Гниющий Лес,Затопленный храм,Гиблые поля и новый Тристрам
                //WaypointFlags = 0x00000383, // Гниющий Лес,Затопленный храм,Гиблые поля,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000385, // Гниющий Лес,Затопленный храм,Гиблые поля,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000387, // Гниющий Лес,Затопленный храм,Гиблые поля,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000389, // Гниющий Лес,Затопленный храм,Гиблые поля,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000038B, // Гниющий Лес,Затопленный храм,Гиблые поля,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000038D, // Гниющий Лес,Затопленный храм,Гиблые поля,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000038F, // Гниющий Лес,Затопленный храм,Гиблые поля,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x00000391, // Гниющий Лес,Затопленный храм,Гиблые поля,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x00000393, // Гниющий Лес,Затопленный храм,Гиблые поля,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000395, // Гниющий Лес,Затопленный храм,Гиблые поля,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000397, // Гниющий Лес,Затопленный храм,Гиблые поля,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x00000399, // Гниющий Лес,Затопленный храм,Гиблые поля,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000039B, // Гниющий Лес,Затопленный храм,Гиблые поля,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000039D, // Гниющий Лес,Затопленный храм,Гиблые поля,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000039F, // Гниющий Лес,Затопленный храм,Гиблые поля,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000003A1, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x000003A3, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003A5, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003A7, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000003A9, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000003AB, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003AD, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003AF, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000003B1, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x000003B3, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003B5, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003B7, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000003B9, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000003BB, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003BD, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003BF, // Гниющий Лес,Затопленный храм,Гиблые поля,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000003C1, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты и новый Тристрам
                //WaypointFlags = 0x000003C3, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003C5, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003C7, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000003C9, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000003CB, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003CD, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003CF, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000003D1, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x000003D3, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003D5, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003D7, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000003D9, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000003DB, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003DD, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003DF, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000003E1, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои и новый Тристрам
                //WaypointFlags = 0x000003E3, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003E5, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003E7, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000003E9, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000003EB, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003ED, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003EF, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                //WaypointFlags = 0x000003F1, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы и новый Тристрам
                //WaypointFlags = 0x000003F3, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003F5, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003F7, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы,Соборный сад,Руины Тристрама и новый Тристрам 
                //WaypointFlags = 0x000003F9, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы,Собор 3 и новый Тристрам
                //WaypointFlags = 0x000003FB, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x000003FD, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x000003FF, // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам
                #endregion
                #region Флаги для порталов 400 - 500
                // Гниющий Лес,Затопленный храм,Гиблые поля,Кладбище прокляты,Уединенные покои,Королевские гробницы,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000401, // Подвал часовни Вортема и новый Тристрам
                //WaypointFlags = 0x00000403, // Подвал часовни Вортема,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000405, // Подвал часовни Вортема,Соборный сад и новый Тристрам
                //WaypointFlags = 0x00000407, // Подвал часовни Вортема,Соборный сад,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x00000409, // Подвал часовни Вортема,Собор 3 и новый Тристрам
                //WaypointFlags = 0x0000040B, // Подвал часовни Вортема,Собор 3,Руины Тристрама и новый Тристрам
                //WaypointFlags = 0x0000040D, // Подвал часовни Вортема,Собор 3,Соборный сад и новый Тристрам
                //WaypointFlags = 0x0000040F, // Подвал часовни Вортема,Собор 3,Соборный сад,Руины Тристрама и новый Тристрам

                #endregion
                #region Флаги для порталов 500 - 600
                //WaypointFlags = 0x00000501, // 
                //WaypointFlags = 0x00000503, // 
                //WaypointFlags = 0x00000505, // 
                //WaypointFlags = 0x00000507, //  
                //WaypointFlags = 0x00000509, // 
                //WaypointFlags = 0x0000050B, // 
                //WaypointFlags = 0x0000050D, // 
                //WaypointFlags = 0x0000050F, // 
                #endregion
                Field4 = new HirelingSavedData()
                {
                    HirelingInfos = this.HirelingInfo,
                    Field1 = 0x00000000,
                    Field2 = 0x00000002,
                },

                Field5 = 0x00000726,

                LearnedLore = this.LearnedLore,
                ActiveSkills = this.SkillSet.ActiveSkills,
                snoTraits = this.SkillSet.PassiveSkills,
                SavePointData = new SavePointData { snoWorld = -1, SavepointId = -1, },
            };
        }

        public SavePointData SavePointData { get; set; }

        public LearnedLore LearnedLore = new LearnedLore()
        {
            Count = 0x00000000,
            m_snoLoreLearned = new int[256]
             {
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,
                0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000,0x00000000
             },
        };

        public int[] SeenTutorials = new int[64]
        {
            1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        };

        public PlayerQuestRewardHistoryEntry[] QuestRewardHistory = new PlayerQuestRewardHistoryEntry[0] { };

        //Система найма
        public HirelingInfo[] HirelingInfo = new HirelingInfo[4]
        {
            new HirelingInfo { HirelingIndex = 0x00000000, Field1 = -1, Level = 0, Field3 = 0x0000, Field4 = false, Skill1SNOId = -1, Skill2SNOId = -1, Skill3SNOId = -1, Skill4SNOId = -1, },
            new HirelingInfo { HirelingIndex = 0x00000000, Field1 = -1, Level = 0, Field3 = 0x0000, Field4 = false, Skill1SNOId = -1, Skill2SNOId = -1, Skill3SNOId = -1, Skill4SNOId = -1, },
            new HirelingInfo { HirelingIndex = 0x00000000, Field1 = -1, Level = 0, Field3 = 0x0000, Field4 = false, Skill1SNOId = -1, Skill2SNOId = -1, Skill3SNOId = -1, Skill4SNOId = -1, },
            new HirelingInfo { HirelingIndex = 0x00000000, Field1 = -1, Level = 0, Field3 = 0x0000, Field4 = false, Skill1SNOId = -1, Skill2SNOId = -1, Skill3SNOId = -1, Skill4SNOId = -1, },
        };

        public SkillKeyMapping[] SkillKeyMappings = new SkillKeyMapping[15]
        {
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
            new SkillKeyMapping { Power = -1, Field1 = -1, Field2 = 0x00000000, },
        };

        #endregion

        #region cooked messages

        public PlayerBannerMessage GetPlayerBanner()
        {
            var playerBanner = D3.GameMessage.PlayerBanner.CreateBuilder()
                .SetPlayerIndex((uint)this.PlayerIndex)
                .SetBanner(this.Toon.GameAccount.BannerConfigurationField.Value)
                .Build();

            return new PlayerBannerMessage() { PlayerBanner = playerBanner };
        }

        public BlacksmithDataInitialMessage GetBlacksmithData()
        {
            var blacksmith = D3.ItemCrafting.CrafterData.CreateBuilder()
                .SetLevel(45)
                .SetCooldownEnd(0)
                .Build();
            return new BlacksmithDataInitialMessage() { CrafterData = blacksmith };
        }

        public JewelerDataInitialMessage GetJewelerData()
        {
            var jeweler = D3.ItemCrafting.CrafterData.CreateBuilder()
                .SetLevel(9)
                .SetCooldownEnd(0)
                .Build();
            return new JewelerDataInitialMessage() { CrafterData = jeweler };
        }

        public MysticDataInitialMessage GetMysticData()
        {
            var mystic = D3.ItemCrafting.CrafterData.CreateBuilder()
                .SetLevel(45)
                .SetCooldownEnd(0)
                .Build();
            return new MysticDataInitialMessage() { CrafterData = mystic };
        }

        #endregion

        #region generic properties

        public int ClassSNO
        {
            get
            {

                if (this.Toon.Gender == 0)
                {
                    return Toon.HeroTable.SNOMaleActor;
                }
                else
                {
                    return Toon.HeroTable.SNOFemaleActor;
                }
            }
        }

        public float ModelScale
        {
            get
            {
                switch (this.Toon.Class)
                {
                    case ToonClass.Barbarian:
                        return 1.2f;
                    case ToonClass.DemonHunter:
                        return 1.35f;
                    case ToonClass.Monk:
                        return 1.43f;
                    case ToonClass.WitchDoctor:
                        return 1.1f;
                    case ToonClass.Wizard:
                        return 1.3f;
                }
                return 1.43f;
            }
        }

        public int PrimaryResourceID
        {
            get
            {
                return (int)Toon.HeroTable.PrimaryResource;
            }
        }

        public int SecondaryResourceID
        {
            get
            {
                return (int)Toon.HeroTable.SecondaryResource;
            }
        }

        #endregion

        #region queries

        public List<T> GetRevealedObjects<T>() where T : class, IRevealable
        {
            return this.RevealedObjects.Values.OfType<T>().Select(@object => @object).ToList();
        }

        #endregion

        #region experience handling

        //Max((Hitpoints_Max + Hitpoints_Total_From_Level + Hitpoints_Total_From_Vitality + Hitpoints_Max_Bonus) * (Hitpoints_Max_Percent_Bonus + Hitpoints_Max_Percent_Bonus_Item + 1), 1)
        private float GetMaxTotalHitpoints()
        {
            return (Math.Max((this.Attributes[GameAttribute.Hitpoints_Max] + this.Attributes[GameAttribute.Hitpoints_Total_From_Level] +
                this.Attributes[GameAttribute.Hitpoints_Max_Bonus]) *
                (this.Attributes[GameAttribute.Hitpoints_Max_Percent_Bonus] + this.Attributes[GameAttribute.Hitpoints_Max_Percent_Bonus_Item] + 1), 1));
        }

        //Max((Resource_Max + ((Level#NONE - 1) * Resource_Factor_Level) + Resource_Max_Bonus) * (Resource_Max_Percent_Bonus + 1), 0)
        private float GetMaxResource(int resourceId)
        {
            return (Math.Max((this.Attributes[GameAttribute.Resource_Max, resourceId] + ((this.Attributes[GameAttribute.Level] - 1) * this.Attributes[GameAttribute.Resource_Factor_Level, resourceId]) + this.Attributes[GameAttribute.Resource_Max_Bonus, resourceId]) * (this.Attributes[GameAttribute.Resource_Max_Percent_Bonus, resourceId] + 1), 0));
        }

        public static int[] LevelBorders =
        {
            0, 1200, 2700, 4500, 6600, 9000, 11700, 14700, 17625, 20800, 24225, /* Level 0-10 */
            27900, 31825, 36000, 41475, 38500, 40250, 42000, 43750, 45500, 47250, /* Level 11-20 */
            49000, 58800, 63750, 73625, 84000, 94875, 106250, 118125, 130500, 134125, /* Level 21-30 */
            77700, 81700, 85800, 90000, 94300, 98700, 103200, 107800, 112500, 117300, /* Level 31-40 */
            122200, 127200, 132300, 137500, 142800, 148200, 153700, 159300, 165000, 170800, /* Level 41-50 */
            176700, 182700, 188800, 195000, 201300, 207700, 214200, 220800, 227500, 234300, /* Level 51-60 */
            241200, 248200, 255300, 262500, 269800, 277200, 284700, 292300, 300000, 307800, /* Level 61-70 */
            315700, 323700, 331800, 340000, 348300, 356700, 365200, 373800, 382500, 391300, /* Level 71-80 */
            400200, 409200, 418300, 427500, 436800, 446200, 455700, 465300, 475000, 484800, /* Level 81-90 */
            494700, 504700, 514800, 525000, 535300, 545700, 556200, 566800, 577500 /* Level 91-99 */
        };

        public static int[] LevelUpEffects =
        {
            85186, 85186, 85186, 85186, 85186, 85190, 85190, 85190, 85190, 85190, /* Level 1-10 */
            85187, 85187, 85187, 85187, 85187, 85187, 85187, 85187, 85187, 85187, /* Level 11-20 */
            85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, /* Level 21-30 */
            85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, /* Level 31-40 */
            85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, 85192, /* Level 41-50 */
            85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, /* Level 51-60 */
            85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, /* Level 61-70 */
            85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, 85194, /* Level 71-80 */
            85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, /* Level 81-90 */
            85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195, 85195 /* Level 91-99 */
        };

        public void UpdateExp(int addedExp)
        {
            this.Attributes[GameAttribute.Experience_Next] -= addedExp;

            // Levelup (maybe multiple levelups... remember Diablo2 Ancients)
            while (this.Attributes[GameAttribute.Experience_Next] <= 0)
            {
                // No more levelup at Level_Cap
                if (this.Attributes[GameAttribute.Level] >= this.Attributes[GameAttribute.Level_Cap])
                {
                    // Set maximun experience and exit.
                    this.Attributes[GameAttribute.Experience_Next] = 0;
                    break;
                }
                this.Attributes[GameAttribute.Level]++;
                this.Toon.LevelUp();

                this.InGameClient.SendMessage(new PlayerLevel()
                {
                    PlayerIndex = this.PlayerIndex,
                    Level = this.Toon.Level
                });

                this.Conversations.StartConversation(0x0002A777); //LevelUp Conversation

                this.Attributes[GameAttribute.Experience_Next] = this.Attributes[GameAttribute.Experience_Next] + LevelBorders[this.Attributes[GameAttribute.Level]];

                // 4 main attributes are incremented according to class
                this.Attributes[GameAttribute.Strength] = this.Strength;
                this.Attributes[GameAttribute.Intelligence] = this.Intelligence;
                this.Attributes[GameAttribute.Vitality] = this.Vitality;
                this.Attributes[GameAttribute.Dexterity] = this.Dexterity;
                //scripted //this.Attributes[GameAttribute.Strength_Total] = this.StrengthTotal;
                //scripted //this.Attributes[GameAttribute.Intelligence_Total] = this.IntelligenceTotal;
                //scripted //this.Attributes[GameAttribute.Dexterity_Total] = this.DexterityTotal;
                //scripted //this.Attributes[GameAttribute.Vitality_Total] = this.VitalityTotal;

                //scripted //this.Attributes[GameAttribute.Resistance_From_Intelligence] = this.Attributes[GameAttribute.Intelligence] * 0.1f;
                //scripted //this.Attributes[GameAttribute.Resistance_Total, 0] = this.Attributes[GameAttribute.Resistance_From_Intelligence];
                //scripted //this.Attributes[GameAttribute.Resistance_Total, 1] = this.Attributes[GameAttribute.Resistance_From_Intelligence];
                //scripted //this.Attributes[GameAttribute.Resistance_Total, 2] = this.Attributes[GameAttribute.Resistance_From_Intelligence];
                //scripted //this.Attributes[GameAttribute.Resistance_Total, 3] = this.Attributes[GameAttribute.Resistance_From_Intelligence];
                //scripted //this.Attributes[GameAttribute.Resistance_Total, 4] = this.Attributes[GameAttribute.Resistance_From_Intelligence];
                //scripted //this.Attributes[GameAttribute.Resistance_Total, 5] = this.Attributes[GameAttribute.Resistance_From_Intelligence];
                //scripted //this.Attributes[GameAttribute.Resistance_Total, 6] = this.Attributes[GameAttribute.Resistance_From_Intelligence];

                // Hitpoints from level may actually change. This needs to be verified by someone with the beta.
                //scripted //this.Attributes[GameAttribute.Hitpoints_Total_From_Level] = this.Attributes[GameAttribute.Level] * this.Attributes[GameAttribute.Hitpoints_Factor_Level];

                // For now, hit points are based solely on vitality and initial hitpoints received.
                // This will have to change when hitpoint bonuses from items are implemented.
                //scripted //this.Attributes[GameAttribute.Hitpoints_Total_From_Vitality] = this.Attributes[GameAttribute.Vitality] * this.Attributes[GameAttribute.Hitpoints_Factor_Vitality];
                //this.Attributes[GameAttribute.Hitpoints_Max] = this.Attributes[GameAttribute.Hitpoints_Total_From_Level] + this.Attributes[GameAttribute.Hitpoints_Total_From_Vitality];
                //scripted //this.Attributes[GameAttribute.Hitpoints_Max_Total] = GetMaxTotalHitpoints();

                // On level up, health is set to max
                this.Attributes[GameAttribute.Hitpoints_Cur] = this.Attributes[GameAttribute.Hitpoints_Max_Total];

                // force GameAttributeMap to re-calc resources for the active resource types
                this.Attributes[GameAttribute.Resource_Max, this.Attributes[GameAttribute.Resource_Type_Primary]] = this.Attributes[GameAttribute.Resource_Max, this.Attributes[GameAttribute.Resource_Type_Primary]];
                this.Attributes[GameAttribute.Resource_Max, this.Attributes[GameAttribute.Resource_Type_Secondary]] = this.Attributes[GameAttribute.Resource_Max, this.Attributes[GameAttribute.Resource_Type_Secondary]];

                // set resources to max as well
                this.Attributes[GameAttribute.Resource_Cur, this.Attributes[GameAttribute.Resource_Type_Primary]] = this.Attributes[GameAttribute.Resource_Max_Total, this.Attributes[GameAttribute.Resource_Type_Primary]];
                this.Attributes[GameAttribute.Resource_Cur, this.Attributes[GameAttribute.Resource_Type_Secondary]] = this.Attributes[GameAttribute.Resource_Max_Total, this.Attributes[GameAttribute.Resource_Type_Secondary]];

                //scripted //this.Attributes[GameAttribute.Resource_Max_Total, this.Attributes[GameAttribute.Resource_Type_Primary]] = GetMaxResource(this.Attributes[GameAttribute.Resource_Type_Primary]);
                //scripted //this.Attributes[GameAttribute.Resource_Effective_Max, this.Attributes[GameAttribute.Resource_Type_Primary]] = GetMaxResource(this.Attributes[GameAttribute.Resource_Type_Primary]);
                //scripted //this.Attributes[GameAttribute.Resource_Cur, this.Attributes[GameAttribute.Resource_Type_Primary]] = GetMaxResource(this.Attributes[GameAttribute.Resource_Type_Primary]);

                //scripted //this.Attributes[GameAttribute.Resource_Max_Total, this.Attributes[GameAttribute.Resource_Type_Secondary]] = GetMaxResource(this.Attributes[GameAttribute.Resource_Type_Secondary]);
                //scripted //this.Attributes[GameAttribute.Resource_Effective_Max, this.Attributes[GameAttribute.Resource_Type_Secondary]] = GetMaxResource(this.Attributes[GameAttribute.Resource_Type_Secondary]);
                //scripted //this.Attributes[GameAttribute.Resource_Cur, this.Attributes[GameAttribute.Resource_Type_Secondary]] = GetMaxResource(this.Attributes[GameAttribute.Resource_Type_Secondary]);

                this.Attributes.BroadcastChangedIfRevealed();

                this.PlayEffect(Effect.LevelUp);
                this.World.PowerManager.RunPower(this, 85954); //g_LevelUp.pow 85954
            }

            this.Attributes.BroadcastChangedIfRevealed();
            this.Toon.GameAccount.NotifyUpdate();
            //this.Attributes.SendMessage(this.InGameClient, this.DynamicID); kills the player atm
        }

        #endregion

        #region gold, heath-glob collection

        private void CollectGold()
        {
            List<Item> itemList = this.GetItemsInRange(5f);
            foreach (Item item in itemList)
            {
                if (!Item.IsGold(item.ItemType)) continue;

                List<Player> playersAffected = this.GetPlayersInRange(26f);
                int amount = (int)Math.Max(1, Math.Round((double)item.Attributes[GameAttribute.Gold] / playersAffected.Count, 0));
                item.Attributes[GameAttribute.Gold] = amount;
                foreach (Player player in playersAffected)
                {
                    player.InGameClient.SendMessage(new FloatingAmountMessage()
                    {
                        Place = new WorldPlace()
                        {
                            Position = player.Position,
                            WorldID = player.World.DynamicID,
                        },

                        Amount = amount,
                        Type = FloatingAmountMessage.FloatType.Gold,
                    });

                    player.Inventory.PickUpGold(item.DynamicID);
                }
                item.Destroy();
            }
        }

        private void CollectHealthGlobe()
        {
            var itemList = this.GetItemsInRange(5f);
            foreach (Item item in itemList)
            {
                if (!Item.IsHealthGlobe(item.ItemType)) continue;

                var playersAffected = this.GetPlayersInRange(26f);
                foreach (Player player in playersAffected)
                {
                    foreach (Player targetAffected in playersAffected)
                    {
                        player.InGameClient.SendMessage(new PlayEffectMessage()
                        {
                            ActorId = targetAffected.DynamicID,
                            Effect = Effect.HealthOrbPickup
                        });
                    }

                    //every summon and mercenary owned by you must broadcast their green text to you /H_DANILO
                    player.AddPercentageHP((int)item.Attributes[GameAttribute.Health_Globe_Bonus_Health]);
                }
                item.Destroy();
            }
        }

        public void AddPercentageHP(int percentage)
        {
            float quantity = (percentage * this.Attributes[GameAttribute.Hitpoints_Max_Total]) / 100;

            if (PoundOfFleshPassive()) // Barbarian Pound of Flesh passive (+100% additional life from health globes) [Necrosummon]
                this.AddHP(quantity * 2);
            else
                this.AddHP(quantity);
        }

        public void AddHP(float quantity)
        {
            this.Attributes[GameAttribute.Hitpoints_Cur] = Math.Min(
                this.Attributes[GameAttribute.Hitpoints_Cur] + quantity,
                this.Attributes[GameAttribute.Hitpoints_Max_Total]);

            this.InGameClient.SendMessage(new FloatingNumberMessage()
            {
                ActorID = this.DynamicID,
                Number = quantity,
                Type = FloatingNumberMessage.FloatType.Green
            });

            this.Attributes.BroadcastChangedIfRevealed();
        }

        #endregion

        #region Resource Generate/Use

        public void GeneratePrimaryResource(float amount)
        {
            _ModifyResourceAttribute(this.PrimaryResourceID, amount);
        }

        public void UsePrimaryResource(float amount)
        {
            _ModifyResourceAttribute(this.PrimaryResourceID, -amount);
        }

        public void GenerateSecondaryResource(float amount)
        {
            _ModifyResourceAttribute(this.SecondaryResourceID, amount);
        }

        public void UseSecondaryResource(float amount)
        {
            _ModifyResourceAttribute(this.SecondaryResourceID, -amount);
        }

        private void _ModifyResourceAttribute(int resourceID, float amount)
        {
            if (amount > 0f)
            {
                this.Attributes[GameAttribute.Resource_Cur, resourceID] = Math.Min(
                    this.Attributes[GameAttribute.Resource_Cur, resourceID] + amount,
                    this.Attributes[GameAttribute.Resource_Max_Total, resourceID]);
            }
            else
            {
                this.Attributes[GameAttribute.Resource_Cur, resourceID] = Math.Max(
                    this.Attributes[GameAttribute.Resource_Cur, resourceID] + amount,
                    0f);
            }

            this.Attributes.BroadcastChangedIfRevealed();
        }


        private void _UpdateResources()
        {
            // will crash client when loading if you try to update resources too early
            if (!InGameClient.TickingEnabled) return;

            // 1 tick = 1/60s, so multiply ticks in seconds against resource regen per-second to get the amount to update
            float tickSeconds = 1f / 60f * (this.InGameClient.Game.TickCounter - _lastResourceUpdateTick);
            _lastResourceUpdateTick = this.InGameClient.Game.TickCounter;

            GeneratePrimaryResource(tickSeconds * this.Attributes[GameAttribute.Resource_Regen_Total,
                                                                  this.Attributes[GameAttribute.Resource_Type_Primary]]);
            GenerateSecondaryResource(tickSeconds * this.Attributes[GameAttribute.Resource_Regen_Total,
                                                                  this.Attributes[GameAttribute.Resource_Type_Secondary]]);
            AddHP(tickSeconds * this.Attributes[GameAttribute.Hitpoints_Regen_Per_Second]);

            // TODO: replace this with Trait_Barbarian_Fury.pow implementation
            if (this.Toon.Class == ToonClass.Barbarian)
            {
                UsePrimaryResource(tickSeconds * 0.9f);

                if (UnforgivingPassive()) // Barbarian Unforgiving Passive [Necrosummon]
                    GeneratePrimaryResource(tickSeconds * 1.5f);
            }

        }

        #endregion

        #region lore

        /// <summary>
        /// Checks if player has lore
        /// </summary>
        /// <param name="loreSNOId"></param>
        /// <returns></returns>
        public bool HasLore(int loreSNOId)
        {
            return LearnedLore.m_snoLoreLearned.Contains(loreSNOId);
        }

        /// <summary>
        /// Plays lore to player
        /// </summary>
        /// <param name="loreSNOId"></param>
        /// <param name="immediately">if false, lore will have new lore button</param>
        public void PlayLore(int loreSNOId, bool immediately)
        {
            // play lore to player
            InGameClient.SendMessage(new Mooege.Net.GS.Message.Definitions.Quest.LoreMessage
            {
                LoreSNOId = loreSNOId
            });
            if (!HasLore(loreSNOId))
            {
                AddLore(loreSNOId);
            }
        }

        /// <summary>
        /// Adds lore to player's state
        /// </summary>
        /// <param name="loreSNOId"></param>
        public void AddLore(int loreSNOId)
        {
            if (this.LearnedLore.Count < this.LearnedLore.m_snoLoreLearned.Length)
            {
                LearnedLore.m_snoLoreLearned[LearnedLore.Count] = loreSNOId;
                LearnedLore.Count++; // Count
                UpdateHeroState();
            }
        }

        #endregion

        #region StoneOfRecall

        public void EnableStoneOfRecall()
        {
            Attributes[GameAttribute.Skill, 0x0002EC66] = 1;
            Attributes[GameAttribute.Skill_Total, 0x0002EC66] = 1;

            Attributes.SendChangedMessage(this.InGameClient);
        }

        #endregion

        #region PassiveSkillEffects

        #region PassiveCheck
        public bool PassiveEffect(int PassiveID)
        {
            if (this.Toon.DBToon.DBActiveSkills.Passive0 == PassiveID || this.Toon.DBToon.DBActiveSkills.Passive1 == PassiveID || this.Toon.DBToon.DBActiveSkills.Passive2 == PassiveID)
                return true;
            else
                return false;
        }
        #endregion

        #region BarbarianPassives
        #region PoundOfFlesh
        public bool PoundOfFleshPassive()
        {
            if (PassiveEffect(205205))
                return true;
            else
                return false;
        }
        #endregion
        #region Unforgiving
        public bool UnforgivingPassive()
        {
            if (PassiveEffect(205300))
                return true;
            else
                return false;
        }
        #endregion
        #region Ruthless
        public bool RuthlessPassive()
        {
            if (PassiveEffect(205175))
                return true;
            else
                return false;
        }

        #endregion
        #region NervesOfSteel
        public bool NervesOfSteelPassive()
        {
            if (PassiveEffect(217819))
                return true;
            else
                return false;
        }
        #endregion
        #endregion // end BarbarianPassives region

        #endregion // end PassiveSkillEffects region

    }
}
