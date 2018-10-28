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

using System.Collections.Generic;
using Mooege.Common.MPQ.FileFormats.Types;
using Mooege.Core.GS.Common.Types.SNO;
using Mooege.Common.Logging;
using Mooege.Core.GS.Items;
using Mooege.Common.Helpers.Math;
using Mooege.Core.GS.Players;
using Mooege.Common.Storage;
using Mooege.Core.MooNet.Toons;

namespace Mooege.Common.MPQ.FileFormats
{
    [FileFormat(SNOGroup.TreasureClass)]
    public class TreasureClass : FileFormat
    {
        Logger Logger = new Logger("TreasureClass");

        public static TreasureClass GenericTreasure
        {
            get
            {
                return new StandardTreasureClass();
            }
        }

        public class StandardTreasureClass : TreasureClass
        {
            public override Item CreateDrop(Player player)
            {
                return ItemGenerator.CreateGold(player, RandomHelper.Next(1*player.Toon.Level, 10 * player.Toon.Level));
            }
            public override Item CreateWeaponDrop(Player player)
            {
                if (player.Toon.Class == ToonClass.Barbarian)
                {
                    string Weapon = "axe_1h_";
                    if (player.Toon.Level <= 3)
                        Weapon += "001";
                    else if (player.Toon.Level <= 5)
                        Weapon += "002";
                    else if (player.Toon.Level <= 8)
                        Weapon += "003";
                    else if (player.Toon.Level <= 13)
                        Weapon += "004";
                    else if (player.Toon.Level <= 17)
                        Weapon += "005";
                    else if (player.Toon.Level <= 21)
                        Weapon += "006";
                    else if (player.Toon.Level <= 25)
                        Weapon += "007";
                    else if (player.Toon.Level <= 30)
                        Weapon += "101";
                    else if (player.Toon.Level <= 34)
                        Weapon += "102";
                    else if (player.Toon.Level <= 38)
                        Weapon += "103";
                    else if (player.Toon.Level <= 42)
                        Weapon += "104";
                    else if (player.Toon.Level <= 46)
                        Weapon += "105";
                    else if (player.Toon.Level <= 48)
                        Weapon += "106";
                    else if (player.Toon.Level <= 51)
                        Weapon += "107";
                    else if (player.Toon.Level <= 53)
                        Weapon += "201";
                    else if (player.Toon.Level <= 55)
                        Weapon += "202";
                    else if (player.Toon.Level <= 57)
                        Weapon += "203";
                    else if (player.Toon.Level <= 59)
                        Weapon += "204";
                    else if (player.Toon.Level <= 60)
                        Weapon += "205";
                    else if (player.Toon.Level < 61)
                        Weapon += "206";
                    else if (player.Toon.Level < 63)
                        Weapon += "207";
                    return ItemGenerator.Cook(player, Weapon);
                }

                if (player.Toon.Class == ToonClass.DemonHunter)
                {
                    string Weapon = "crossbow_";
                    if (player.Toon.Level <= 3)
                        Weapon += "001";
                    else if (player.Toon.Level <= 5)
                        Weapon += "002";
                    else if (player.Toon.Level <= 8)
                        Weapon += "003";
                    else if (player.Toon.Level <= 13)
                        Weapon += "004";
                    else if (player.Toon.Level <= 17)
                        Weapon += "005";
                    else if (player.Toon.Level <= 21)
                        Weapon += "006";
                    else if (player.Toon.Level <= 25)
                        Weapon += "007";
                    else if (player.Toon.Level <= 30)
                        Weapon += "101";
                    else if (player.Toon.Level <= 34)
                        Weapon += "102";
                    else if (player.Toon.Level <= 38)
                        Weapon += "103";
                    else if (player.Toon.Level <= 42)
                        Weapon += "104";
                    else if (player.Toon.Level <= 46)
                        Weapon += "105";
                    else if (player.Toon.Level <= 48)
                        Weapon += "106";
                    else if (player.Toon.Level <= 51)
                        Weapon += "107";
                    else if (player.Toon.Level <= 53)
                        Weapon += "201";
                    else if (player.Toon.Level <= 55)
                        Weapon += "202";
                    else if (player.Toon.Level <= 57)
                        Weapon += "203";
                    else if (player.Toon.Level <= 59)
                        Weapon += "204";
                    else if (player.Toon.Level <= 60)
                        Weapon += "205";
                    else if (player.Toon.Level < 61)
                        Weapon += "206";
                    else if (player.Toon.Level < 63)
                        Weapon += "207";
                    return ItemGenerator.Cook(player, Weapon);
                }

                if (player.Toon.Class == ToonClass.Monk)
                {
                    string Weapon = "fistweapon_1h_";
                    if (player.Toon.Level <= 3)
                        Weapon += "001";
                    else if (player.Toon.Level <= 5)
                        Weapon += "002";
                    else if (player.Toon.Level <= 8)
                        Weapon += "003";
                    else if (player.Toon.Level <= 13)
                        Weapon += "004";
                   else if (player.Toon.Level <= 30)
                        Weapon += "101";
                    else if (player.Toon.Level <= 34)
                        Weapon += "102";
                    else if (player.Toon.Level <= 38)
                        Weapon += "103";
                    else if (player.Toon.Level <= 42)
                        Weapon += "104";
                    else if (player.Toon.Level <= 53)
                        Weapon += "201";
                    else if (player.Toon.Level <= 55)
                        Weapon += "202";
                    else if (player.Toon.Level <= 57)
                        Weapon += "203";
                    else if (player.Toon.Level <= 60)
                        Weapon += "204";
                    
                    return ItemGenerator.Cook(player, Weapon);
                }

                if (player.Toon.Class == ToonClass.WitchDoctor)
                {
                    string Weapon = "CeremonialDagger_1H_";
                    if (player.Toon.Level <= 3)
                        Weapon += "001";
                    else if (player.Toon.Level <= 7)
                        Weapon += "002";
                    else if (player.Toon.Level <= 13)
                        Weapon += "003";
                    else if (player.Toon.Level <= 17)
                        Weapon += "004";
                    else if (player.Toon.Level <= 25)
                        Weapon += "101";
                    else if (player.Toon.Level <= 28)
                        Weapon += "102";
                    else if (player.Toon.Level <= 33)
                        Weapon += "103";
                    else if (player.Toon.Level <= 40)
                        Weapon += "104";
                    else if (player.Toon.Level <= 44)
                        Weapon += "201";
                    else if (player.Toon.Level <= 48)
                        Weapon += "202";
                    else if (player.Toon.Level <= 54)
                        Weapon += "203";
                    else if (player.Toon.Level <= 60)
                        Weapon += "204";
                    return ItemGenerator.Cook(player, Weapon);
                }

                if (player.Toon.Class == ToonClass.Wizard)
                {
                    string Weapon = "wand_";
                    if (player.Toon.Level <= 3)
                        Weapon += "001";
                    else if (player.Toon.Level <= 5)
                        Weapon += "002";
                    else if (player.Toon.Level <= 8)
                        Weapon += "003";
                    else if (player.Toon.Level <= 13)
                        Weapon += "004";
                    else if (player.Toon.Level <= 17)
                        Weapon += "005";
                    else if (player.Toon.Level <= 21)
                        Weapon += "006";
                    else if (player.Toon.Level <= 25)
                        Weapon += "007";
                    else if (player.Toon.Level <= 30)
                        Weapon += "101";
                    else if (player.Toon.Level <= 34)
                        Weapon += "102";
                    else if (player.Toon.Level <= 38)
                        Weapon += "103";
                    else if (player.Toon.Level <= 42)
                        Weapon += "104";
                    else if (player.Toon.Level <= 46)
                        Weapon += "105";
                    else if (player.Toon.Level <= 48)
                        Weapon += "106";
                    else if (player.Toon.Level <= 51)
                        Weapon += "107";
                    else if (player.Toon.Level <= 53)
                        Weapon += "201";
                    else if (player.Toon.Level <= 55)
                        Weapon += "202";
                    else if (player.Toon.Level <= 57)
                        Weapon += "203";
                    else if (player.Toon.Level <= 59)
                        Weapon += "204";
                    else if (player.Toon.Level <= 60)
                        Weapon += "205";
                    else if (player.Toon.Level < 61)
                        Weapon += "206";
                    else if (player.Toon.Level < 63)
                        Weapon += "207";
                    return ItemGenerator.Cook(player, Weapon);
                }
                else
                {
                    return ItemGenerator.CreateGold(player, RandomHelper.Next(1, 10));
                }
            }
        }


        [PersistentProperty("Percentage")]
        public float Percentage { get; private set; }

        [PersistentProperty("I0")]
        public int I0 { get; private set; }

        [PersistentProperty("LootDropModifiersCount")]
        public int LootDropModifiersCount { get; private set; }

        [PersistentProperty("LootDropModifiers")]
        public List<LootDropModifier> LootDropModifiers { get; private set; }

        public TreasureClass() { }

        public virtual Item CreateDrop(Player player)
        {
            Logger.Warn("Treasure classes not implemented, using generic treasure class");
            return TreasureClass.GenericTreasure.CreateDrop(player);
        }
        public virtual Item CreateWeaponDrop(Player player)
        {
            Logger.Warn("Тестовый дроп оружия) v0.1");
            return TreasureClass.GenericTreasure.CreateWeaponDrop(player);
        }
    }

    public class LootDropModifier
    {
        [PersistentProperty("I0")]
        public int I0 { get; private set; }

        [PersistentProperty("SNOSubTreasureClass")]
        public int SNOSubTreasureClass { get; private set; }

        [PersistentProperty("Percentage")]
        public float Percentage { get; private set; }

        [PersistentProperty("I1")]
        public int I1 { get; private set; }

        [PersistentProperty("GBIdQualityClass")]
        public int GBIdQualityClass { get; private set; }

        [PersistentProperty("I2")]
        public int I2 { get; private set; }

        [PersistentProperty("I3")]
        public int I3 { get; private set; }

        [PersistentProperty("SNOCondition")]
        public int SNOCondition { get; private set; }

        [PersistentProperty("ItemSpecifier")]
        public ItemSpecifierData ItemSpecifier { get; private set; }

        [PersistentProperty("I5")]
        public int I5 { get; private set; }

        [PersistentProperty("I4", 4)]
        public int[] I4 { get; private set; }

        [PersistentProperty("I6")]
        public int I6 { get; private set; }

        public LootDropModifier() { }
    }
}
