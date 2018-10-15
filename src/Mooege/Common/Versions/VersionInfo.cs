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

using System.Collections.Generic;
using Mooege.Common.Extensions;
using Mooege.Net.MooNet;

namespace Mooege.Common.Versions
{
    /// <summary>
    /// Supported Versions Info.
    /// </summary>
    /// <remarks>Put anything related to versions here.</remarks>
    public static class VersionInfo
    {
        /// <summary>
        /// Main assembly versions info.
        /// </summary>
        public static class Assembly
        {
            /// <summary>
            /// Main assemblies version.
            /// </summary>
            public const string Version = "1.10485.*"; // also 1.11327.*
            //public const string Version = "1.11327.*"; 
        }

        /// <summary>
        /// MooNet versions info.
        /// </summary>
        public static class MooNet
        {
            /// <summary>
            /// Required client version.
            /// </summary>
            public const int RequiredClientVersion = 10485; // also 11327
            //public const int RequiredClientVersion = 11327;

            public static Dictionary<string, int> ClientVersionMaps = new Dictionary<string, int>
            {
                {"Aurora 127cc0376a_public", 13300},
                {"Aurora d2b2e2dbd0_public", 11327},
                {"Aurora ab0ebd5e2c_public", 10485}, // also 10057, 10235
                {"Aurora 24e2d13e54_public", 9991},
                {"Aurora 79fef7ae8e_public", 9950},
                {"Aurora 8018401a9c_public", 9749},
                {"Aurora 31c8df955a_public", 9558},
                {"Aurora 8eac7d44dc_public", 9359},
                {"Aurora _public", 9183},
                {"Aurora bcd3e50524_public", 8896},
                {"Aurora 4a39a60e1b_public", 8815},
                {"Aurora 7f06f1aabd_public", 8610},
                {"Aurora 9e9ccb8fdf_public", 8392},
                {"Aurora f506438e8d_public", 8101},
                {"Aurora fbb3e7d1b4_public", 8059},
                {"Aurora 04768e5dce_public", 7931},
                {"Aurora 0ee3b2e0e2_public", 7841}, 
                {"Aurora b4367eba86_public", 7728}
            };

            /// <summary>
            /// Auth modules' hash maps for client platforms.
            /// </summary>
            //TODO: Get Hashes for Mac client.
            public static Dictionary<MooNetClient.ClientPlatform, byte[]> PasswordHashMap = new Dictionary<MooNetClient.ClientPlatform, byte[]>()
            {
                { MooNetClient.ClientPlatform.Win,"8F52906A2C85B416A595702251570F96D3522F39237603115F2F1AB24962043C".ToByteArray() },
                { MooNetClient.ClientPlatform.Mac,"63BC118937E6EA2FAA7B7192676DAEB1B7CA87A9C24ED9F5ACD60E630B4DD7A4".ToByteArray() }
            };

            public static Dictionary<MooNetClient.ClientPlatform, byte[]> ThumbprintHashMap = new Dictionary<MooNetClient.ClientPlatform, byte[]>()
            {
                { MooNetClient.ClientPlatform.Win,"36b27cd911b33c61730a8b82c8b2495fd16e8024fc3b2dde08861c77a852941c".ToByteArray() },
                { MooNetClient.ClientPlatform.Mac,"36b27cd911b33c61730a8b82c8b2495fd16e8024fc3b2dde08861c77a852941c".ToByteArray() },
            };

            public static Dictionary<MooNetClient.ClientPlatform, byte[]> TokenHashMap = new Dictionary<MooNetClient.ClientPlatform, byte[]>()
            {
                { MooNetClient.ClientPlatform.Win,"bfa574bcff509b3c92f7c4b25b2dc2d1decb962209f8c9c8582ddf4f26aac176".ToByteArray() },
                { MooNetClient.ClientPlatform.Mac,"bfa574bcff509b3c92f7c4b25b2dc2d1decb962209f8c9c8582ddf4f26aac176".ToByteArray() },
            };

            public static Dictionary<MooNetClient.ClientPlatform, byte[]> RiskFingerprintHashMap = new Dictionary<MooNetClient.ClientPlatform, byte[]>()
            {
                { MooNetClient.ClientPlatform.Win,"bcfa324ab555fc66614976011d018d2be2b9dc23d0b54d94a3bd7d12472aa107".ToByteArray() },
                { MooNetClient.ClientPlatform.Mac,"bcfa324ab555fc66614976011d018d2be2b9dc23d0b54d94a3bd7d12472aa107".ToByteArray() },
            };

            public static Dictionary<MooNetClient.ClientPlatform, byte[]> AgreementHashMap = new Dictionary<MooNetClient.ClientPlatform, byte[]>()
            {
                { MooNetClient.ClientPlatform.Win,"41686a009b345b9cbe622ded9c669373950a2969411012a12f7eaac7ea9826ed".ToByteArray() },
                { MooNetClient.ClientPlatform.Mac,"41686a009b345b9cbe622ded9c669373950a2969411012a12f7eaac7ea9826ed".ToByteArray() },
            };

            public static byte[] TOS = "00736F74006167726500005553014970E37CCD158A64A2844D6D4C05FC1697988A617E049BB2E0407D71B6C6F2".ToByteArray();
            public static byte[] EULA = "00616C75656167726500005553DDD1D77970291A4E8A64BB4FE25B2EA2D69D8915D35D53679AE9FDE5EAE47ECC".ToByteArray();
            public static byte[] RMAH = "0068616D72616772650000555398A3FC047004D6D4A0A1519A874AC9B1FC5FBD62C3EAA23188E095D6793537D7".ToByteArray();

            public static Dictionary<string, uint> Regions = new Dictionary<string, uint>()
            {
                { "US", 0x5553 },
                { "XX", 0x5858 },
            };

            public static string Region = "US";

            public static class Resources
            {
                public static string ProfanityFilterHash = "de1862793fdbabb6eb1edec6ad1c95dd99e2fd3fc6ca730ab95091d694318a24"; //9558-10485
                public static string AvailableActs = "bd9e8fc323fe1dbc1ef2e0e95e46355953040488621933d0685feba5e1163a25"; //10057
                public static string AvailableQuests = "9303df8f917e2db14ec20724c04ea5d2af4e4cb6c72606b67a262178b7e18104"; //10057
            }

            public static class Achievements
            {
                /// <summary>
                /// AchievementFile hash.
                /// </summary>
                public static string AchievementFileHash = "f0a945924510ece166812b241bd0724af5d0f1569e72430a67b46518fee37fb3"; //10057

                /// <summary>
                /// AchievementFile filename.
                /// </summary>
                public static string AchievementFilename = AchievementFileHash + ".achu";

                /// <summary>
                /// AchievementFile download URL.
                /// </summary>
                public static string AchievementURL = "http://" + MooNet.Region + ".depot.battle.net:1119/" + AchievementFilename;

            }
        }

        /// <summary>
        /// MPQ storage versions info.
        /// </summary>
        public static class MPQ
        {
            /// <summary>
            /// Required MPQ patch version.
            /// </summary>
            public const int RequiredPatchVersion = 10485; // also 11327
            //public const int RequiredPatchVersion = 11327;
        }

        /// <summary>
        /// Ingame connection & client versions info.
        /// </summary>
        public static class Ingame
        {
            /// <summary>
            /// Ingame protocol hash.
            /// </summary>
            public const int ProtocolHash = unchecked((int)0xFDD6012B);
			// public const int ProtocolHash = unchecked((int)0xA3B7C936); // also 11327

			/// <summary>
            /// Server version sent in VersionsMessage.
            /// </summary>
            public const string MajorVersion = "1.0.3"; // also 1.0.4
            public const string ServerBuild = "10485"; // also 11327
            public const string VersionString = MajorVersion + "." + ServerBuild;

        }
    }
}