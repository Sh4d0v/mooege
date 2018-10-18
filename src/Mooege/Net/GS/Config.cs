/*
 * Copyright (C) 2018 DiIiS project
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

namespace Mooege.Net.GS
{
    public sealed class Config : Common.Config.Config
    {
        public string BindIP { get { return this.GetString("BindIP", "0.0.0.0"); } set { this.Set("BindIP", value); } }
        public string BindIPv6 { get { return this.GetString("BindIPv6", "::1"); } set { this.Set("BindIPv6", value); } }
        public int Port { get { return this.GetInt("Port", 1345); } set { this.Set("Port", value); } }
        public bool TimeStamp { get { return this.GetBoolean("TimeStamp", false); } set { this.Set("TimeStamp", value); } }

        // Server Properties [Necrosummon]
        public int ExpRate { get { return this.GetInt("ExpRate", 1); } set { this.Set("ExpRate", value); } }
        public int MonsterHPRate { get { return this.GetInt("MonsterHPRate", 1); } set { this.Set("MonsterHPRate", value); } }
        public int MonsterDamageMultiplier { get { return this.GetInt("MonsterDamageMultiplier", 1); } set { this.Set("MonsterDamageMultiplier", value); } }
        public int LevelStarter { get { return this.GetInt("LevelStarter", 1); } set { this.Set("LevelStarter", value); } }
        //public int StarterGold { get { return this.GetInt("StarterGold", 0); } set { this.Set("StarterGold", value); } }
        //public int StarterStash { get { return this.GetInt("StarterStash", 14); } set { this.Set("StarterStash", value); } }
        public int MaxLevel { get { return this.GetInt("MaxLevel", 1); } set { this.Set("MaxLevel", value); } }
        public float ItemDropRate { get { return this.GetFloat("ItemDropRate", 1); } set { this.Set("ItemDropRate", value); } }
        public float GoldDropRate { get { return this.GetFloat("GoldDropRate", 1); } set { this.Set("GoldDropRate", value); } }
        public float GoldRate { get { return this.GetFloat("GoldRate", 1); } set { this.Set("GoldRate", value); } }
        public float HealthGlobeDropRate { get { return this.GetFloat("HealthGlobeDropRate", 1); } set { this.Set("HealthGlobeDropRate", value); } }
        
        private static readonly Config _instance = new Config();
        public static Config Instance { get { return _instance; } }
        private Config() : base("Game-Server") { }
    }
}

#region BarbarianSkills

namespace Mooege.Net.GS.BarbarianPrimarySkills
{
    public sealed class BarbarianPrimarySkillsConfig : Common.BarbarianSkillsConfig.Config
    {
        // Bash Base skill
        public float BashWeaponDamagePercent { get { return this.GetFloat("BashWeaponDamagePercent", 150); } set { this.Set("BashWeaponDamagePercent", value); } }
        public int BashFuryGeneration { get { return this.GetInt("BashFuryGeneration", 6); } set { this.Set("BashFuryGeneration", value); } }
        public float BashKnockbackChance { get { return this.GetFloat("BashKnockbackChance", 20); } set { this.Set("BashKnockbackChance", value); } }
        // Instigation (Rune D)
        public int BashExtraFuryGeneration { get { return this.GetInt("BashExtraFuryGeneration", 6); } set { this.Set("BashExtraFuryGeneration", value); } }
        // Punish (Rune B)
        public float BashIncreasedDamageSkills { get { return this.GetFloat("BashIncreasedDamageSkills", 6); } set { this.Set("BashIncreasedDamageSkills", value); } }
        public float BashPunishDuration { get { return this.GetFloat("BashPunishDuration", 5); } set { this.Set("BashPunishDuration", value); } }
        public int BashPunishMaxStacks { get { return this.GetInt("BashPunishMaxStacks", 3); } set { this.Set("BashPunishMaxStacks", value); } }
        // Onslaught (Rune A)
        public int BashReverberations { get { return this.GetInt("BashReverberations", 2); } set { this.Set("BashReverberations", value); } }
        public float BashReverberationWeaponDamage { get { return this.GetFloat("BashReverberationWeaponDamage", 22); } set { this.Set("BashReverberationWeaponDamage", value); } }
        // Clobber (Rune C)
        public float BashStunChance { get { return this.GetFloat("BashStunChance", 35); } set { this.Set("BashStunChance", value); } }
        public float BashStunDuration { get { return this.GetFloat("BashStunDuration", 1.5f); } set { this.Set("BashStunDuration", value); } }
        // Pulverize (Rune E)
        public float BashPulverizeWeaponDamage { get { return this.GetFloat("BashPulverizeWeaponDamage", 22); } set { this.Set("BashPulverizeWeaponDamage", value); } }
        public float BashPulverizeDistance { get { return this.GetFloat("BashPulverizeDistance", 26); } set { this.Set("BashPulverizeDistance", value); } }

        private static readonly BarbarianPrimarySkillsConfig _instance = new BarbarianPrimarySkillsConfig();
        public static BarbarianPrimarySkillsConfig Instance { get { return _instance; } }
        private BarbarianPrimarySkillsConfig() : base("Primary-Skills") { }
    }
}

namespace Mooege.Net.GS.BarbarianSecondarySkills
{
    public sealed class BarbarianSecondarySkillsConfig : Common.BarbarianSkillsConfig.Config
    {
        // Hammer of the Ancients Base skill
        public float HammerOfTheAncientsWeaponDamagePercent { get { return this.GetFloat("HammerOfTheAncientsWeaponDamagePercent", 100); } set { this.Set("HammerOfTheAncientsWeaponDamagePercent", value); } }
        public int HammerOfTheAncientsCost { get { return this.GetInt("HammerOfTheAncientsCost", 20); } set { this.Set("HammerOfTheAncientsCost", value); } }

        private static readonly BarbarianSecondarySkillsConfig _instance = new BarbarianSecondarySkillsConfig();
        public static BarbarianSecondarySkillsConfig Instance { get { return _instance; } }
        private BarbarianSecondarySkillsConfig() : base("Secondary-Skills") { }
    }
}

namespace Mooege.Net.GS.BarbarianRageSkills
{
    public sealed class BarbarianRageSkillsConfig : Common.BarbarianSkillsConfig.Config
    {
        // Hammer of the Ancients Base skill
        public float WrathOfTheBerserkerDuration { get { return this.GetFloat("WrathOfTheBerserkerDuration", 15); } set { this.Set("WrathOfTheBerserkerDuration", value); } }
        public float WrathOfTheBerserkerCooldown { get { return this.GetFloat("WrathOfTheBerserkerCooldown", 120); } set { this.Set("WrathOfTheBerserkerCooldown", value); } }
        public int WrathOfTheBerserkerFuryCost { get { return this.GetInt("WrathOfTheBerserkerFuryCost", 50); } set { this.Set("WrathOfTheBerserkerFuryCost", value); } }

        private static readonly BarbarianRageSkillsConfig _instance = new BarbarianRageSkillsConfig();
        public static BarbarianRageSkillsConfig Instance { get { return _instance; } }
        private BarbarianRageSkillsConfig() : base("Rage-Skills") { }
    }
}



#endregion


