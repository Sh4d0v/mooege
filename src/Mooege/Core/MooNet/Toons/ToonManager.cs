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

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Mooege.Common.Logging;
using Mooege.Common.Storage;
using Mooege.Common.Storage.AccountDataBase.Entities;
using Mooege.Core.MooNet.Accounts;
using NHibernate.Linq;
using Mooege.Core.GS.Items;
using Mooege.Core.GS.Players;

namespace Mooege.Core.MooNet.Toons
{
    // Just a quick hack - not to be meant final
    public static class ToonManager
    {
        private static readonly HashSet<Toon> LoadedToons = new HashSet<Toon>();
        private static readonly Logger Logger = LogManager.CreateLogger();


        public static Toon GetToonByDBToon(DBToon dbToon)
        {
            if (!LoadedToons.Any(dbt => dbt.DBToon.Id == dbToon.Id))
                LoadedToons.Add(new Toon(dbToon));
            return LoadedToons.Single(dbt => dbt.DBToon.Id == dbToon.Id);
        }


        public static Account GetOwnerAccountByToonLowId(ulong id)
        {
            return GetToonByLowID(id).GameAccount.Owner;
        }

        public static GameAccount GetOwnerGameAccountByToonLowId(ulong id)
        {
            return GetToonByLowID(id).GameAccount;
        }



        public static Toon GetToonByLowID(ulong id)
        {
            var dbToon = DBSessions.AccountSession.Get<DBToon>(id);
            return GetToonByDBToon(dbToon);
        }

        public static Toon GetDeletedToon(GameAccount account)
        {
            var query = DBSessions.AccountSession.Query<DBToon>().Where(dbt => dbt.DBGameAccount.Id == account.PersistentID && dbt.Deleted);
            return query.Any() ? GetToonByLowID(query.First().Id) : null;
        }

        public static List<Toon> GetToonsForGameAccount(GameAccount account)
        {
            var toons = account.DBGameAccount.DBToons.Select(dbt => GetToonByLowID(dbt.Id));
            return toons.ToList();
        }


        public static int TotalToons
        {
            get { return DBSessions.AccountSession.Query<DBToon>().Count(); }
        }


        public static Toon CreateNewToon(string name, int classId, bool isHardcore, ToonFlags flags, byte level, GameAccount gameAccount)
        {

            #region LevelConfs Server.conf
            if (Mooege.Net.GS.Config.Instance.LevelStarter < 1)
                level = 1;
            else if (Mooege.Net.GS.Config.Instance.LevelStarter > Mooege.Net.GS.Config.Instance.MaxLevel)
                level = (byte)Mooege.Net.GS.Config.Instance.MaxLevel;
            else
                level = (byte)Mooege.Net.GS.Config.Instance.LevelStarter;
            #endregion

            // Create Character
            var dbGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID);
            var newDBToon = new DBToon
            {
                Class = @Toon.GetClassByID(classId),
                Name = name,
                /*HashCode = GetUnusedHashCodeForToonName(name),*/
                Hardcore = isHardcore,
                Flags = flags,
                Level = level,

                DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID)
            };



            dbGameAccount.DBToons.Add(newDBToon);



            //Стандартные Предметы
            #region Стандартные предметы Barbarian
            if (newDBToon.Class == ToonClass.Barbarian)
            {
                #region Топор
                //Структура сущности предмета
                DBItemInstance BARAxeInstance = new DBItemInstance
                {
                    GbId = 1661412389, // 
                    Affixes = "",
                    Attributes = "295,:2|2,802597E-45;91,:0|0;308,:1|1,401298E-45;312,:281445791|7,830311E-29;157,:1067030938|1,2;159,:1067030938|1,2;161,:1067030938|1,2;412,:1067030938|1,2;414,:1067030938|1,2;420,:1067030938|1,2;164,:1067030938|1,2;326,:0|0;327,:0|0;328,:0|0;329,:0|0;413,:0|0;415,:0|0;183,0:1073741824|2;177,0:1077936128|3;184,0:1073741824|2;178,0:1077936128|3;179,:1077936128|3;185,:1073741824|2;416,0:1073741824|2;421,0:1073741824|2;173,0:1073741824|2;172,0:1073741824|2;172,:0|0;417,0:0|0;175,0:1065353216|1;176,0:1065353216|1;180,0:1065353216|1;181,:1065353216|1;418,0:1065353216|1;422,0:1065353216|1;169,0:1065353216|1;419,0:0|0;89,30592:1|1,401298E-45;90,30592:1|1,401298E-45;297,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(BARAxeInstance);
                // Структура предмета
                DBInventory BARAxeFirstWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = BARAxeInstance, // Использовать свеже созданную сущность предмета
                    Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARAxeFirstWeapon);
                #endregion

                #region Щиток
                //Структура сущности предмета
                DBItemInstance BARBuckInstance = new DBItemInstance
                {
                    GbId = 1815806857, // 
                    Affixes = "",
                    Attributes = "295,:2|2,802597E-45;91,:0|0;308,:1|1,401298E-45;312,:978984494|0,0008320537;48,:1090519040|8;51,:1090519040|8;52,:1090519040|8;53,:1090519040|8;207,:1040522936|0,13;208,:1040522936|0,13;205,:1040522936|0,13;213,:1088421888|7;211,:1088421888|7;212,:1094713344|12;214,:1084227584|5;297,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(BARBuckInstance);
                // Структура предмета
                DBInventory BARBuckler = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = BARBuckInstance, // Использовать свеже созданную сущность предмета
                    Hardcore = isHardcore, // Хардкорный или нет персонаж
                    DBToon = newDBToon, // Выдать созданному персонажу
                    EquipmentSlot = 3, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(BARBuckler);
                #endregion

            }
            #endregion 

            #region Стандартные предметы Demon Hunter
            if (newDBToon.Class == ToonClass.DemonHunter)
            {
                #region Арбалет
                //Структура сущности предмета
                DBItemInstance DHFWInstance = new DBItemInstance
                {
                    GbId = -363391670, // 
                    Affixes = "",
                    Attributes = "295,:2|2,802597E-45;91,:0|0;308,:1|1,401298E-45;312,:889997518|5,226001E-07;157,:1067030938|1,2;159,:1067030938|1,2;161,:1067030938|1,2;412,:1067030938|1,2;414,:1067030938|1,2;420,:1067030938|1,2;164,:1067030938|1,2;326,:0|0;327,:0|0;328,:0|0;329,:0|0;413,:0|0;415,:0|0;183,0:1073741824|2;177,0:1077936128|3;184,0:1073741824|2;178,0:1077936128|3;179,:1077936128|3;185,:1073741824|2;416,0:1073741824|2;421,0:1073741824|2;173,0:1073741824|2;172,0:1073741824|2;172,:0|0;417,0:0|0;175,0:1065353216|1;176,0:1065353216|1;180,0:1065353216|1;181,:1065353216|1;418,0:1065353216|1;422,0:1065353216|1;169,0:1065353216|1;419,0:0|0;89,30599:1|1,401298E-45;90,30599:1|1,401298E-45;377,:1065353216|1;378,:1065353216|1;398,:1065353216|1;400,:1065353216|1;297,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(DHFWInstance);
                // Структура предмета
                DBInventory DHFirstWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = DHFWInstance, // Использовать свеже созданную сущность предмета
                    DBToon = newDBToon, // Выдать созданному персонажу
                    Hardcore = isHardcore, // Хардкорный или нет персонаж
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(DHFirstWeapon);
                #endregion

                
            }
            #endregion

            #region Стандартные предметы Monk
            if (newDBToon.Class == ToonClass.Monk)
            {
                #region Перчатки
                //Структура сущности предмета
                DBItemInstance MKKnuckInstance = new DBItemInstance
                {
                    GbId = 1236604967, // 
                    Affixes = "",
                    Attributes = "295,:2|2,802597E-45;91,:0|0;308,:1|1,401298E-45;312,:277358000|5,370061E-29;157,:1067030938|1,2;159,:1067030938|1,2;161,:1067030938|1,2;412,:1067030938|1,2;414,:1067030938|1,2;420,:1067030938|1,2;164,:1067030938|1,2;326,:0|0;327,:0|0;328,:0|0;329,:0|0;413,:0|0;415,:0|0;183,0:1073741824|2;177,0:1077936128|3;184,0:1073741824|2;178,0:1077936128|3;179,:1077936128|3;185,:1073741824|2;416,0:1073741824|2;421,0:1073741824|2;173,0:1073741824|2;172,0:1073741824|2;172,:0|0;417,0:0|0;175,0:1065353216|1;176,0:1065353216|1;180,0:1065353216|1;181,:1065353216|1;418,0:1065353216|1;422,0:1065353216|1;169,0:1065353216|1;419,0:0|0;89,30592:1|1,401298E-45;90,30592:1|1,401298E-45;297,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(MKKnuckInstance);
                // Структура предмета
                DBInventory MKKnucktWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = MKKnuckInstance, // Использовать свеже созданную сущность предмета
                    DBToon = newDBToon, // Выдать созданному персонажу
                    Hardcore = isHardcore, // Хардкорный или нет персонаж
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(MKKnucktWeapon);
                #endregion


            }
            #endregion

            #region Стандартные предметы Witch Doctor
            if (newDBToon.Class == ToonClass.WitchDoctor)
            {
                #region Нож
                //Структура сущности предмета
                DBItemInstance WDKnifeInstance = new DBItemInstance
                {
                    GbId = -635269584, // 
                    Affixes = "",
                    Attributes = "295,:1|1,401298E-45;91,:0|0;308,:1|1,401298E-45;312,:233363062|1,434973E-30;157,:1067030938|1,2;159,:1067030938|1,2;161,:1067030938|1,2;412,:1067030938|1,2;414,:1067030938|1,2;420,:1067030938|1,2;164,:1067030938|1,2;326,:0|0;327,:0|0;328,:0|0;329,:0|0;413,:0|0;415,:0|0;183,0:1073741824|2;177,0:1077936128|3;184,0:1073741824|2;178,0:1077936128|3;179,:1077936128|3;185,:1073741824|2;416,0:1073741824|2;421,0:1073741824|2;173,0:1073741824|2;172,0:1073741824|2;172,:0|0;417,0:0|0;175,0:1065353216|1;176,0:1065353216|1;180,0:1065353216|1;181,:1065353216|1;418,0:1065353216|1;422,0:1065353216|1;169,0:1065353216|1;419,0:0|0;89,30592:1|1,401298E-45;90,30592:1|1,401298E-45;297,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(WDKnifeInstance);
                // Структура предмета
                DBInventory WDKnifeWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = WDKnifeInstance, // Использовать свеже созданную сущность предмета
                    DBToon = newDBToon, // Выдать созданному персонажу
                    Hardcore = isHardcore, // Хардкорный или нет персонаж
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(WDKnifeWeapon);
                #endregion


            }
            #endregion

            #region Стандартные предметы Wizzard
            if (newDBToon.Class == ToonClass.Wizard)
            {
                #region Палочка
                //Структура сущности предмета
                DBItemInstance WWandInstance = new DBItemInstance
                {
                    GbId = 88665049, // 
                    Affixes = "",
                    Attributes = "295,:1|1,401298E-45;91,:0|0;308,:1|1,401298E-45;312,:1184309910|19345,29;157,:1067030938|1,2;159,:1067030938|1,2;161,:1067030938|1,2;412,:1067030938|1,2;414,:1067030938|1,2;420,:1067030938|1,2;164,:1067030938|1,2;326,:0|0;327,:0|0;328,:0|0;329,:0|0;413,:0|0;415,:0|0;183,0:1073741824|2;177,0:1077936128|3;184,0:1073741824|2;178,0:1077936128|3;179,:1077936128|3;185,:1073741824|2;416,0:1073741824|2;421,0:1073741824|2;173,0:1073741824|2;172,0:1073741824|2;172,:0|0;417,0:0|0;175,0:1065353216|1;176,0:1065353216|1;180,0:1065353216|1;181,:1065353216|1;418,0:1065353216|1;422,0:1065353216|1;169,0:1065353216|1;419,0:0|0;89,30601:1|1,401298E-45;90,30601:1|1,401298E-45;297,:1|1,401298E-45"
                };
                // Добавляем сущность в базу
                DBSessions.AccountSession.SaveOrUpdate(WWandInstance);
                // Структура предмета
                DBInventory WWandWeapon = new DBInventory
                {
                    DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                    DBItemInstance = WWandInstance, // Использовать свеже созданную сущность предмета
                    DBToon = newDBToon, // Выдать созданному персонажу
                    Hardcore = isHardcore, // Хардкорный или нет персонаж
                    EquipmentSlot = 4, // Вооружить в первую руку
                    LocationX = 0,
                    LocationY = 0
                };
                // Добавляем предмет в базу
                DBSessions.AccountSession.SaveOrUpdate(WWandWeapon);
                #endregion


            }
            #endregion


            #region Начало прогресса. 1 Акт, Пока не взятый квест.
            DBProgressToon StartProgress = new DBProgressToon
            {
                DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID), // Привязка к используемому аккаунту
                DBToon = newDBToon, // Выдать созданному персонажу
                LastQuest = 0, // Последний выполненный квест
                StepOfQuest = 0, //Текущий шаг квеста
                ActiveAct = 0, // Активный акт
                ActiveQuest = -1 // Активный квест
            };
            DBSessions.AccountSession.SaveOrUpdate(StartProgress);
            DBPortalOfToon NullPortal = new DBPortalOfToon
            {
                DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID),
                DBToon = newDBToon,
                X = 0,
                Y = 0,
                Z = 0,
                WorldDest = 0
            };
            DBSessions.AccountSession.SaveOrUpdate(NullPortal);
            DBArtisansOfToon BaseArtisans = new DBArtisansOfToon
            {
                DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(gameAccount.PersistentID),
                DBToon = newDBToon,
                Blacksmith = 1,
                Jeweler = 1,
                Mystic = 1,
            };
            DBSessions.AccountSession.SaveOrUpdate(BaseArtisans);
            #endregion

            DBSessions.AccountSession.Flush();
            DBSessions.AccountSession.Refresh(dbGameAccount);
            return GetToonByLowID(newDBToon.Id);
        }


        public static void DeleteToon(Toon toon)
        {
            if (toon == null)
                return;

            //remove toonActiveSkills
            if (toon.DBToon.DBActiveSkills != null)
            {
                DBSessions.AccountSession.Delete(toon.DBToon.DBActiveSkills);
                toon.DBToon.DBActiveSkills = null;
            }

            //remove toon inventory
            var inventoryToDelete = DBSessions.AccountSession.Query<DBInventory>().Where(inv => inv.DBToon.Id == toon.DBToon.Id);
            foreach (var inv in inventoryToDelete)
            {
                //toon.DBToon.DBGameAccount.DBInventories.Remove(inv);
                DBSessions.AccountSession.Delete(inv);
            }

            //remove lastplayed hero if it was toon
            if (toon.DBToon.DBGameAccount.LastPlayedHero != null && toon.DBToon.DBGameAccount.LastPlayedHero.Id == toon.DBToon.Id)
                toon.DBToon.DBGameAccount.LastPlayedHero = null;


            //remove toon from dbgameaccount
            while (toon.DBToon.DBGameAccount.DBToons.Contains(toon.DBToon))
                toon.DBToon.DBGameAccount.DBToons.Remove(toon.DBToon);

            //save all this thinks
            DBSessions.AccountSession.SaveOrUpdate(toon.DBToon.DBGameAccount);
            DBSessions.AccountSession.Delete(toon.DBToon);
            DBSessions.AccountSession.Flush();

            DBSessions.AccountSession.Refresh(toon.DBToon.DBGameAccount);

            //remove toon from loadedToon list
            if (LoadedToons.Contains(toon))
                LoadedToons.Remove(toon);

            Logger.Debug("Deleting toon {0}", toon.PersistentID);
        }


        public static void Sync()
        {
            foreach (var toon in LoadedToons)
            {
                SaveToDB(toon);
            }
        }

        public static void SaveToDB(Toon toon)
        {
            try
            {
                // save character base data
                var dbToon = DBSessions.AccountSession.Get<DBToon>(toon.PersistentID);
                dbToon.Name = toon.Name;
                /*dbToon.HashCode = toon.HashCode;*/
                dbToon.Class = toon.Class;
                dbToon.Flags = toon.Flags;
                dbToon.Level = toon.Level;
                dbToon.Hardcore = toon.Hardcore;
                dbToon.Experience = toon.ExperienceNext;
                dbToon.DBGameAccount = DBSessions.AccountSession.Get<DBGameAccount>(toon.GameAccount.PersistentID);
                dbToon.TimePlayed = toon.TimePlayed;
                dbToon.Deleted = toon.Deleted;

                DBSessions.AccountSession.SaveOrUpdate(dbToon);
                DBSessions.AccountSession.Flush();
            }
            catch (Exception e)
            {
                Logger.ErrorException(e, "Toon.SaveToDB()");
            }
        }
    }
}
