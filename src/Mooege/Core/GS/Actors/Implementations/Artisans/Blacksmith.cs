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

using Mooege.Core.GS.Map;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Net.GS.Message;

namespace Mooege.Core.GS.Actors.Implementations.Artisans
{
    [HandledSNO(56947 /* PT_Blacksmith.acr */)]
    public class Blacksmith : Artisan
    {
        public Blacksmith(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            // TODO add all blacksmith functionality? /fasbat
            /* Все ресурсы
            [189847] Crafting_Tier_01B - Летучая эссенция
            [189848] Crafting_Tier_01C - Зуб падшего
            [189853] Crafting_Tier_02B - Сияющая эссенция
            [189854] Crafting_Tier_02C - Глаз ящерицы
            [189857] Crafting_Tier_03B - Желанная эссенция
            [189858] Crafting_Tier_03C - Окаменевшее копыто
            [189861] Crafting_Tier_04B - Изысканная эссенция
            [189862] Crafting_Tier_04C - Переливающаяся слеза
            [189863] Crafting_Tier_04D - Горящая сера
            */

        }
    }
}
