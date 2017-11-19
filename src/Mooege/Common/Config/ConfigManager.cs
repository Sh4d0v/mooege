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
using Mooege.Common.Helpers.IO;
using Mooege.Common.Logging;
using Nini.Config;

namespace Mooege.Common.Config
{
    public sealed class ConfigurationManager
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private static readonly IniConfigSource Parser;
        private static readonly string ConfigFile;
        private static bool _fileExists = false;

        static ConfigurationManager()
        {
            try
            {
                /// <summary>
                /// Path to config file.
                /// </summary>
                ConfigFile = string.Format("{0}/{1}", FileHelpers.AssemblyRoot, "conf/Server.conf");
                Parser = new IniConfigSource(ConfigFile);
                _fileExists = true;
            }
            catch (Exception)
            {
                Parser = new IniConfigSource();
                _fileExists = false;
                Logger.Warn("Error loading settings Server.ini, will be using default settings.");
            }
            finally
            {
                Parser.Alias.AddAlias("On", true);
                Parser.Alias.AddAlias("Off", false);

                Parser.Alias.AddAlias("MinimumLevel", Logger.Level.Trace);
                Parser.Alias.AddAlias("MaximumLevel", Logger.Level.Trace);
            }

            Parser.ExpandKeyValues();
        }

        static internal IConfig Section(string section)
        {
            return Parser.Configs[section];
        }

        static internal IConfig AddSection(string section)
        {
            return Parser.AddConfig(section);
        }

        static internal void Save()
        {
            if (_fileExists) Parser.Save();
            else
            {
                Parser.Save(ConfigFile);
                _fileExists = true;
            }
        }
    }
}
