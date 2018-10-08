﻿/*
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

using Mooege.Common.Helpers.Hash;
using Mooege.Core.GS.Map;
using Mooege.Net.GS.Message;
using Mooege.Core.GS.Common.Types.TagMap;
using Mooege.Core.GS.Common.Types.Math;
using System.Threading;
using System;

namespace Mooege.Core.GS.Actors.Implementations.Hirelings
{
    [HandledSNO(4644 /* Scoundrel.acr */)]
    public class Scoundrel : Hireling, Objects.IUpdateable, Living
    {
        private static ThreadLocal<Random> _threadRand = new ThreadLocal<Random>(() => new Random());
        public static Random Rand { get { return _threadRand.Value; } }
        public Scoundrel(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            Brain = new AI.Brains.HirelingBrain(this);
           
            mainSNO = 4644;
            hirelingSNO = 52694;
            proxySNO = 192941;
            skillKit = 0x8AFE;
            hirelingGBID = StringHashHelper.HashItemName("Scoundrel");
            Attributes[GameAttribute.Hireling_Class] = 2;
            //Brain.State = Mooege.Core.GS.AI.BrainState.Follow;
            //this.Brain = new MonsterBrain(this);
            
            this.Attributes[GameAttribute.Attacks_Per_Second] = 1.0f;
            this.Attributes[GameAttribute.Damage_Weapon_Min, 0] = 5f;
            this.Attributes[GameAttribute.Damage_Weapon_Delta, 0] = 5f;
            //RandomDirection(player.Value.Position, 3f, 8f);
            //this.WalkSpeed = 0.3f * monsterData.Floats[129];  // TODO: this is probably multiplied by something erekose the 0.3 is because he is way too fast otherwise
            this.WalkSpeed = this.RunSpeed;
        }
        public Vector3D RandomDirection(Vector3D position, float minRadius, float maxRadius)
        {
            float angle = (float)(Rand.NextDouble() * Math.PI * 2);
            float radius = minRadius + (float)Rand.NextDouble() * (maxRadius - minRadius);
            return new Vector3D(position.X + (float)Math.Cos(angle) * radius,
                                position.Y + (float)Math.Sin(angle) * radius,
                                position.Z);
        }
        public void Update(int tickCounter)
        {
            if (this.Brain == null)
                return;
            //Brain.State = AI.BrainState.Follow;


            //var facingAngle = Actors.Movement.MovementHelpers.GetFacingAngle(this, OwnerPlayer.Position);
            try
            {
                if(CurrentScene.Players[0].ActiveHireling.ActorSNO.Id == 52694)
                {
                    //if(CurrentScene.Players[0].mo == true)
                    Vector3D Point = new Vector3D(CurrentScene.Players[0].Position.X -5, CurrentScene.Players[0].Position.Y, CurrentScene.Players[0].Position.Z);
                   // this.Move(Point, CurrentScene.Players[0].RotationW);
                }
            }
            catch { }
            
            
            this.Brain.Update(tickCounter);
            
            

            //Vector3D NearPlayer = new 
            //this.Move()
        }
        public override Hireling CreateHireling(World world, int snoId, TagMap tags)
        {
            return new Scoundrel(world, snoId, tags);
        }
    }
}