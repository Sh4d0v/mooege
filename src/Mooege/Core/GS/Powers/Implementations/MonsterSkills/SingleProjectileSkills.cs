﻿/*
* Copyright (C) 2011 mooege project
*
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mooege.Core.GS.Ticker;
using Mooege.Core.GS.Actors;
using Mooege.Core.GS.Common.Types.Math;
using Mooege.Core.GS.Powers.Payloads;

namespace Mooege.Core.GS.Powers.Implementations
{
    public abstract class SingleProjectileSkill : ActionTimedSkill
    {
        protected Projectile projectile;
        protected float speed;

        protected void SetProjectile(PowerContext context, int actorSNO, Common.Types.Math.Vector3D position, float speed = 1f, Action<Actor> OnCollision = null)
        {
            projectile = new Projectile(context, actorSNO, position);
            projectile.OnCollision = OnCollision;
            this.speed = speed;
        }

        protected IEnumerable<TickTimer> Launch()
        {
            projectile.Launch(Target.Position, speed);
            yield break;
        }
    }

    [ImplementsPowerSNO(30334)] // Monster_Ranged_Projectile.pow
    public class MonsterRangedProjectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 3901, User.Position, 1f, (hit) =>
            {
                WeaponDamage(hit, 1.00f, DamageType.Physical);
                projectile.Destroy();
            });
            return Launch();
        }
    }
    [ImplementsPowerSNO(30570)] // TriuneSummoner_Projectile.pow
    public class TriuneSummonerProjectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 6040, User.Position, 0.7f, (hit) =>
            {
                WeaponDamage(hit, 1.00f, DamageType.Fire);
                projectile.Position.Z += 5f;
                projectile.Destroy();
            });
            return Launch();
        }
    }
    [ImplementsPowerSNO(83008)] // Butcher_Hook.pow
    public class ButcherHookProjectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            bool hitAnything = false;
            projectile = new Projectile(this, 3528, User.Position);
            speed = 2f;
            /*SetProjectile(this, 3528, User.Position, 2f, (hit) =>
            {
                WeaponDamage(hit, 1.00f, DamageType.Arcane);
                projectile.Destroy();
            });*/
            User.World.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
            {
                ActorID = User.DynamicID,
                Field1 = 5,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                                        {
                                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                                        {
                                            Duration = 50,
                                            AnimationSNO = User.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.SpecialAttack],
                                            PermutationIndex = 0,
                                            Speed = 1f
                                        }
                                        }
            }, User);
            projectile.Position.Z += 5f;
            projectile.Position.X += 5f;
            projectile.Scale = 2f;
            projectile.Timeout = WaitSeconds(0.5f);
            projectile.OnCollision = (hit) =>
            {
                hitAnything = true;

                _setupReturnProjectile(hit.Position);

                AttackPayload attack = new AttackPayload(this);
                attack.SetSingleTarget(hit);
                
                attack.AddWeaponDamage(ScriptFormula(0), DamageType.Physical);
                attack.OnHit = (hitPayload) =>
                {
                    for (int i = 0; i < ScriptFormula(17); ++i)
                    {
                        hitPayload.Target.PlayEffectGroup(18748);//18748
                        Knockback(hitPayload.Target, -25f, ScriptFormula(3), ScriptFormula(4));
                    }
                };
                attack.Apply();

            };
            projectile.OnTimeout = () =>
            {
                _setupReturnProjectile(projectile.Position);
                
            };

            projectile.Launch(TargetPosition, ScriptFormula(8));
            User.AddRopeEffect(30877, projectile);

            return Launch();
        }
        private void _setupReturnProjectile(Vector3D spawnPosition)
        {
            Vector3D inFrontOfUser = PowerMath.TranslateDirection2D(User.Position, spawnPosition, User.Position, 5f);

            var return_proj = new Projectile(this, 3528, new Vector3D(spawnPosition.X, spawnPosition.Y, User.Position.Z));
            return_proj.Scale = 2f;
            return_proj.DestroyOnArrival = true;
            return_proj.LaunchArc(inFrontOfUser, 1f, -0.03f);
            
            Target.Teleport(inFrontOfUser);
            Target.Move(inFrontOfUser, Target.RotationW);
            TickTimer StunTime = new SecondsTickTimer(User.World.Game, 2f);
            AddBuff(Target, new DebuffStunned(StunTime));
            User.AddRopeEffect(30877, return_proj);
            User.World.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
            {
                ActorID = User.DynamicID,
                Field1 = 5,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                                        {
                                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                                        {
                                            Duration = 50,
                                            AnimationSNO = User.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Attack2],
                                            PermutationIndex = 0,
                                            Speed = 1f
                                        }
                                        }
            }, User);
        }
    }
    //[030877] [Rope] Butcher_Hookshot_Chain


    [ImplementsPowerSNO(30474)] // Shield_Skeleton_Melee_Instant.pow
    public class ShieldSkeletonMeleeInstant : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            WeaponDamage(GetBestMeleeEnemy(), 1.50f, DamageType.Physical);
            yield break;
        }
    }

    [ImplementsPowerSNO(30160)] // Butcher_Smash.pow
    public class ButcherSmashInstant : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            User.World.BroadcastIfRevealed(new Mooege.Net.GS.Message.Definitions.Animation.PlayAnimationMessage
            {
                ActorID = User.DynamicID,
                Field1 = 5,
                Field2 = 0,
                tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                                        {
                                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                                        {
                                            Duration = 50,
                                            AnimationSNO = User.AnimationSet.TagMapAnimDefault[Core.GS.Common.Types.TagMap.AnimationSetKeys.Attack2],
                                            PermutationIndex = 0,
                                            Speed = 1f
                                        }
                                        }
            }, User);
            WeaponDamage(GetBestMeleeEnemy(), 1.50f, DamageType.Physical);
            yield break;
        }
    }

    [ImplementsPowerSNO(30258)] // graveRobber_Projectile.pow
    public class graveRobberProjectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 4365, User.Position, 1f, (hit) =>
            {
                WeaponDamage(hit, 1.00f, DamageType.Physical);
                projectile.Destroy();
            });
            
            return Launch();
        }
    }

    [ImplementsPowerSNO(30503)] // SkeletonSummoner_Projectile.pow
    public class SkeletonSummonerProjectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 5392, User.Position, 0.9f, (hit) =>
            {
                hit.PlayEffectGroup(19052);
                WeaponDamage(hit, 2.00f, DamageType.Arcane);
                projectile.Destroy();
            });
            projectile.Position.Z += 5f; //adjust height
            return Launch();
        }
    }

    [ImplementsPowerSNO(99077)] // Goatman_Shaman_Iceball.pow
    public class ShamanIceBallProjectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 80143, User.Position, 0.5f, (hit) =>
            {
                hit.PlayEffectGroup(99355);
                WeaponDamage(hit, 2.00f, DamageType.Cold);
                projectile.Destroy();
            });
            projectile.Position.Z += 5f; //adjust height
            return Launch();
        }
    }

    [ImplementsPowerSNO(129661)] // DemonHunter_Sentry_TurretAttack.pow
    public class TurretAttackProjectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 141734, User.Position, 0.5f, (hit) =>
            {
                //hit.PlayEffectGroup(150040);
                WeaponDamage(hit, ScriptFormula(1), DamageType.Physical);
                projectile.Destroy();
            });
            projectile.Position.Z += 5f; //adjust height
            return Launch();
        }
    }

    [ImplementsPowerSNO(152540)] // Unique_Monster_Generic_Projectile.pow
    public class UniqueMonsterGenericProjectile : SingleProjectileSkill
    {
        //I believe this is correct, because uniques come in different elements
        //it needs different projectiles and effects. just unsure how to use them different /wet
        //Arcane 159150 / 159158
        //Poison 159160 / 159139
        //phys 159161 / 159109
        //holy 158977 / 159075
        //fire 158577 / 159100
        //lightning 158976 / 159092
        //cold 158575 / 159143
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 159150, User.Position, 0.5f, (hit) =>
            {
                hit.PlayEffectGroup(159158);
                WeaponDamage(hit, 2.00f, DamageType.Arcane);
                projectile.Destroy();
            });
            projectile.Position.Z += 5f; //adjust height
            return Launch();
        }
    }

    [ImplementsPowerSNO(107729)] // QuillDemon_Projectile.pow
    public class QuillDemonProjectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 4981, User.Position, 1f, (hit) =>
            {
                // Looking at the tagmaps for 107729, the damage should probably be more accurately calculated, but this will have to do for now.
                WeaponDamage(hit, 1.00f, DamageType.Physical);
                projectile.Destroy();
            });
            projectile.Position.Z += 2f + (float)Rand.NextDouble() * 4;
            return Launch();
        }
    }

    [ImplementsPowerSNO(110518)] // ZombieFemale_Projectile.pow
    public class WretchedMother_Projectile : SingleProjectileSkill
    {
        public override IEnumerable<TickTimer> Main()
        {
            SetProjectile(this, 120957, User.Position, 1.00f, (hit) =>
            {
                hit.PlayEffectGroup(142812);
                WeaponDamage(hit, 1.00f, DamageType.Poison);
                projectile.Destroy();
            });
            projectile.Position.Z += 5f;  // fix height
            return Launch();
        }
    }
}
