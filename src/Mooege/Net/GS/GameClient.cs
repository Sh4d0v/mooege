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
using System.Linq;
using Mooege.Common.Logging;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Games;
using Mooege.Core.GS.Players;
using Mooege.Net.GS.Message;
using Mooege.Net.GS.Message.Definitions.Tick;
using Mooege.Net.MooNet;

namespace Mooege.Net.GS
{
    public sealed class GameClient : IClient
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public IConnection Connection { get; set; }
        public MooNetClient BnetClient { get; set; }

        private readonly GameBitBuffer _incomingBuffer = new GameBitBuffer(512);
        private readonly GameBitBuffer _outgoingBuffer = new GameBitBuffer(ushort.MaxValue);

        public Game Game { get; set; }
        public Player Player { get; set; }

        public bool TickingEnabled { get; set; }

        private object _bufferLock = new object(); // we should be locking on this private object, locking on gameclient (this) may cause deadlocks. detailed information: http://msdn.microsoft.com/fr-fr/magazine/cc188793%28en-us%29.aspx /raist.

        public bool IsLoggingOut;

        public GameClient(IConnection connection)
        {
            this.TickingEnabled = false;
            this.Connection = connection;
            _outgoingBuffer.WriteInt(32, 0);
        }

        public void Parse(ConnectionDataEventArgs e)
        {
            //Console.WriteLine(e.Data.Dump());

            _incomingBuffer.AppendData(e.Data.ToArray());

            while (_incomingBuffer.IsPacketAvailable())
            {
                int end = _incomingBuffer.Position;
                end += _incomingBuffer.ReadInt(32) * 8;

                while ((end - _incomingBuffer.Position) >= 9)
                {
                    var message = _incomingBuffer.ParseMessage();

                    if (message == null) continue;
                    try
                    {
                        Logger.LogIncomingPacket(message); // change ConsoleTarget's level to Level.Dump in program.cs if u want to see messages on console.
                        if(message.Id == 280)
                        { }
                        if (message.Consumer != Consumers.None)
                        {
                            if (message.Consumer == Consumers.ClientManager) ClientManager.Instance.Consume(this, message); // Client should be greeted by ClientManager and sent initial game-setup messages.
                            else this.Game.Route(this, message);
                        }

                        else if (message is ISelfHandler) (message as ISelfHandler).Handle(this); // if message is able to handle itself, let it do so.
                        // Кустарный перехват портала
                        else if (message.Id == 87)
                        {
                            MooNetClient mooNetClient = BnetClient;
                            Logger.Warn("Portal to New Tristram. Version 2.2.", message.GetType(), message.Id);
                            //Vector3D ToPortal = new Vector3D(2985.6241f, 2795.627f, 24.04532f);
                            Vector3D ToPortal = new Vector3D(2985.6241f, 2795.627f, 24.04532f);
                            try
                            {
                                //Search Old Portals
                                var OldOTG = Player.World.GetActorsBySNO(5648);
                                foreach (var OldP in OldOTG)
                                {
                                    OldP.Destroy();
                                }
                            }
                            catch { }

                            var ToHome = new Portal(Player.World, 5648, Player.World.Game.GetWorld(71150).StartingPoints[0].Tags);
                            
                            ToHome.NameSNOId = 71150;
                            ToHome.Scale = 0.9f;
                            Vector3D PositionToPortal = new Vector3D(Player.Position.X, Player.Position.Y + 3, Player.Position.Z);
                            ToHome.EnterWorld(PositionToPortal);
                        }
                        // Первая версия крафта
                        else if (message.Id == 277)
                        {
                            var dbArtisans = Common.Storage.DBSessions.AccountSession.Get<Common.Storage.AccountDataBase.Entities.DBArtisansOfToon>(this.Player.Toon.PersistentID);
                            Logger.Debug("Внимание! Тестовая функция!");
                            Logger.Debug("Апгрейд ремесленников v0.3");
                            
                            #region Кузнец
                            if (this.Player.SelectedNPC.ActorSNO.Id == 56947)
                            {
                                int now_level = dbArtisans.Blacksmith;
                                
                                if (now_level <= 4)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-1000);
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                }
                                else if (now_level == 5)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-1000);
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    //this.Player.SelectedNPC.PlayEffect(Message.Definitions.Effect.Effect.LevelUp);
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                    //101353
                                }
                                else if (now_level >= 6 && now_level <= 9)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-1000);
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                }
                                else if (now_level == 10)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-2000);
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level >= 11 && now_level <= 19 && now_level != 15)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-2000);
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                }
                                else if (now_level == 15)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-2000);
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 20)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-4000);
                                    //Добавить в требование 2 страницы кузнечного дела
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 226723)
                                        {
                                            //item.
                                            //       CharInv.DestroyInventoryItem(item);
                                            int needed = 2;
                                            if(item.Attributes[GameAttribute.ItemStackQuantityLo]> needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);

                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level >= 21 && now_level <= 24)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-4000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 226723)
                                        {
                                            int needed = 1;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 25)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-4000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 226723)
                                        {
                                            int needed = 2;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level >= 26 && now_level <= 29)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-4000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 226723)
                                        {
                                            int needed = 1;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 30)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-6000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 230696)
                                        {
                                            int needed = 2;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level >= 31 && now_level <= 34)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-6000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 230696)
                                        {
                                            int needed = 1;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 35)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-6000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 230696)
                                        {
                                            int needed = 2;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level >= 36 && now_level <= 39)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-6000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 230696)
                                        {
                                            int needed = 1;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 40)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-8000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 189523)
                                        {
                                            int needed = 2;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level >= 41 && now_level <= 44)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-8000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 189523)
                                        {
                                            int needed = 1;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 45)
                                {
                                    dbArtisans.Blacksmith++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-8000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 189523)
                                        {
                                            int needed = 5;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetBlacksmithData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                            }
                            #endregion

                            #region Каменьщик
                            if (this.Player.SelectedNPC.ActorSNO.Id == 56949)
                            {
                                int now_level = dbArtisans.Jeweler;

                                if (now_level <= 2)
                                {
                                    dbArtisans.Jeweler++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-5000);
                                    this.Player.InGameClient.SendMessage(this.Player.GetJewelerData(dbArtisans));
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 3 || now_level == 4)
                                {
                                    dbArtisans.Jeweler++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-10000);
                                    this.Player.InGameClient.SendMessage(this.Player.GetJewelerData(dbArtisans));
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 226724)
                                        {
                                            int needed = 10;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);

                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.SelectedNPC.PlayEffect(Message.Definitions.Effect.Effect.LevelUp);
                                    this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                    //101353
                                }
                                else if (now_level == 5 && now_level == 6)
                                {
                                    dbArtisans.Jeweler++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-10000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 230697)
                                        {
                                            int needed = 10;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetJewelerData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 7)
                                {
                                    dbArtisans.Jeweler++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-10000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 189523)
                                        {
                                            int needed = 10;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetJewelerData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 8)
                                {
                                    dbArtisans.Jeweler++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-30000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 189523)
                                        {
                                            int needed = 10;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetJewelerData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                                else if (now_level == 9)
                                {
                                    dbArtisans.Jeweler++;
                                    Inventory CharInv = this.Player.Inventory;
                                    CharInv.AddGoldAmount(-40000);
                                    foreach (var item in CharInv.GetBackPackItems())
                                    {
                                        if (item.ActorSNO.Id == 189523)
                                        {
                                            int needed = 20;
                                            if (item.Attributes[GameAttribute.ItemStackQuantityLo] > needed)
                                                item.Attributes[GameAttribute.ItemStackQuantityLo] -= needed;
                                            else
                                                CharInv.DestroyInventoryItem(item);
                                            item.Attributes.SendChangedMessage(this.Player.InGameClient);
                                            CharInv.Reveal(this.Player);

                                        }
                                    }
                                    this.Player.InGameClient.SendMessage(this.Player.GetJewelerData(dbArtisans));
                                    //this.Player.World.PowerManager.RunPower(this.Player.SelectedNPC, 85954);
                                }
                            }
                            #endregion

                            Common.Storage.DBSessions.AccountSession.SaveOrUpdate(dbArtisans);
                            Common.Storage.DBSessions.AccountSession.Flush();
                        }
                        else Logger.Warn("{0} - ID:{1} has no consumer or self-handler.", message.GetType(), message.Id);

                    }
                    catch (NotImplementedException)
                    {
                        Logger.Warn("Unhandled game message: 0x{0:X4} {1}", message.Id, message.GetType().Name);
                    }
                }

                _incomingBuffer.Position = end;
            }
            _incomingBuffer.ConsumeData();
        }

        public void SendMessage(GameMessage message, bool flushImmediatly = false)
        {
            lock (this._bufferLock)
            {
                Logger.LogOutgoingPacket(message);
                _outgoingBuffer.EncodeMessage(message); // change ConsoleTarget's level to Level.Dump in program.cs if u want to see messages on console.
                if (flushImmediatly) this.SendTick();
            }
        }

        public void SendTick()
        {
            lock (this._bufferLock)
            {
                if (_outgoingBuffer.Length <= 32) return;

                if (this.TickingEnabled) this.SendMessage(new GameTickMessage(this.Game.TickCounter)); // send the tick.
                this.FlushOutgoingBuffer();
            }
        }

        private void FlushOutgoingBuffer()
        {
            lock (this._bufferLock)
            {
                if (_outgoingBuffer.Length <= 32) return;

                var data = _outgoingBuffer.GetPacketAndReset();
                Connection.Send(data);
            }
        }
    }
}
