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
using Mooege.Common.Extensions;
using Mooege.Common.Logging;
using Mooege.Core.GS.Players;
using Mooege.Core.MooNet.Toons;
using Mooege.Net.GS.Message;
using Mooege.Net.MooNet;

namespace Mooege.Core.GS.Games
{
    public static class GameManager
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private static readonly Dictionary<int, Game> Games = new Dictionary<int, Game>();

        public static Game CreateGame(int gameId, List<MooNetClient> clients)
        {
            if (Games.ContainsKey(gameId))
                return Games[gameId];

            var game = new Game(gameId, clients);
            Games.Add(gameId, game);
            return game;
        }

        public static Game GetGameById(int gameId)
        {
            return !Games.ContainsKey(gameId) ? null : Games[gameId];
        }

        public static void RemovePlayerFromGame(Net.GS.GameClient gameClient)
        {
            if (gameClient == null || gameClient.Game == null) return;

            var gameId = gameClient.Game.GameId;
            if (!Games.ContainsKey(gameId)) return;

            var game = Games[gameId];
            if (!game.Players.ContainsKey(gameClient)) return;

            Player p = null;
            if (!game.Players.TryRemove(gameClient, out p))
            {
                Logger.Error("Can't remove player ({0}) from game with id: {1}", gameClient.Player.Toon.Name, gameId);
            }

            if (p != null)
            {
                //TODO: Move this inside player OnLeave event
                var toon = p.Toon;
                var gameAccount = toon.DBToon.DBGameAccount; // For update account profile [Necrosummon]
                toon.TimePlayed += DateTimeExtensions.ToUnixTime(DateTime.UtcNow) - toon.LoginTime;
                toon.ExperienceNext = p.Attributes[GameAttribute.Experience_Next];

                // Updating time played/highest level by classes [Necrosummon]
                if (toon.Class == ToonClass.Barbarian)
                {
                    gameAccount.BarbarianPlayedTime += DateTimeExtensions.ToUnixTime(DateTime.UtcNow) - toon.LoginTime;

                    if (gameAccount.BarbarianHighestLevel < toon.Level) // Updates the Highest Level of this class if is more high when you logout [Necrosummon]
                        gameAccount.BarbarianHighestLevel = toon.Level;
                }
                else if (toon.Class == ToonClass.DemonHunter)
                {
                    gameAccount.DemonHunterPlayedTime += DateTimeExtensions.ToUnixTime(DateTime.UtcNow) - toon.LoginTime;

                    if (gameAccount.DemonHunterHighestLevel < toon.Level)
                        gameAccount.DemonHunterHighestLevel = toon.Level;
                }
                else if (toon.Class == ToonClass.Monk)
                {
                    gameAccount.MonkPlayedTime += DateTimeExtensions.ToUnixTime(DateTime.UtcNow) - toon.LoginTime;

                    if (gameAccount.MonkHighestLevel < toon.Level)
                        gameAccount.MonkHighestLevel = toon.Level;
                }
                else if (toon.Class == ToonClass.WitchDoctor)
                {
                    gameAccount.WitchDoctorPlayedTime += DateTimeExtensions.ToUnixTime(DateTime.UtcNow) - toon.LoginTime;

                    if (gameAccount.WitchDoctorHighestLevel < toon.Level)
                        gameAccount.WitchDoctorHighestLevel = toon.Level;
                }
                else if (toon.Class == ToonClass.Wizard)
                {
                    gameAccount.WizardPlayedTime += DateTimeExtensions.ToUnixTime(DateTime.UtcNow) - toon.LoginTime;

                    if (gameAccount.WizardHighestLevel < toon.Level)
                        gameAccount.WizardHighestLevel = toon.Level;
                }

                // Hardcore Highest Level Account Profile Info
                if (toon.Hardcore && gameAccount.HighestHardcoreLevel < toon.Level)
                    gameAccount.HighestHardcoreLevel = toon.Level;

                // Remove Player From World
                if (p.InGameClient != null)
                    p.World.Leave(p);

                // Generate Update for Client
                gameClient.BnetClient.Account.CurrentGameAccount.NotifyUpdate();
                //save hero to db after player data was updated in toon
                ToonManager.SaveToDB(toon);
            }

            if (game.Players.Count == 0)
            {
                Games.Remove(gameId); // we should be also disposing it /raist.
            }
        }
    }
}
