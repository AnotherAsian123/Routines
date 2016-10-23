// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using DefaultCombat.Core;
using DefaultCombat.Helpers;

namespace DefaultCombat.Routines
{
    internal class Carnage : RotationBase
    {
        public override string Name
        {
            get { return "Marauder Carnage"; }
        }

        public override Composite Buffs
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Ataru Form"),
                    Spell.Buff("Unnatural Might")
                    );
            }
        }

        public override Composite Cooldowns
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Unleash"),
                    Spell.Buff("Cloak of Pain", ret => Me.HealthPercent <= 95),
                    Spell.Buff("Force Camouflage", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Undying Rage", ret => Me.HealthPercent <= 10),
                    Spell.Buff("Frenzy", ret => Me.BuffCount("Fury") < 5)
                    );
            }
        }

        public override Composite SingleTarget
        {
            get
            {
                return new PrioritySelector(
                    Spell.Cast("Force Charge",
                        ret => !DefaultCombat.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),

                    //Movement
                    CombatMovement.CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Buff("Berserk"),
                    Spell.Cast("Battering Assault"),
                    Spell.Cast("Massacre", ret => !Me.HasBuff("Massacre")),
                    Spell.Cast("Gore"),
                    Spell.Cast("Ravage", ret => Me.HasBuff("Gore")),
                    Spell.Cast("Devastating Blast", ret => Me.HasBuff("Execute") && Me.Level > 57),
                    Spell.Cast("Assault"),
                    Spell.Cast("Massacre"),
                    Spell.Cast("Massacre"),
                    Spell.Cast("Battering Assault"),
                    Spell.Cast("Gore"),
                    Spell.Cast("Vicious Throw"),
                    Spell.Cast("Devastating Blast", ret => Me.HasBuff("Execute") && Me.Level > 57),
                    Spell.Cast("Massacre"),
                    Spell.Cast("Massacre"),
                    Spell.Cast("Massacre"),
                    Spell.Cast("Vicious Throw"),
                    Spell.Buff("Berserk"),
                    Spell.Cast("Dual Saber Throw"),
                    Spell.Cast("Battering Assault"),
                    Spell.Cast("Massacre"),
                    Spell.Cast("Gore"),
                    Spell.Cast("Ravage", ret => Me.HasBuff("Gore")),
                    Spell.Cast("Devastating Blast", ret => Me.HasBuff("Execute") && Me.Level > 57),
                    Spell.Cast("Assault")
                    );
            }
        }

        public override Composite AreaOfEffect
        {
            get
            {
                return new Decorator(ret => Targeting.ShouldPbaoe,
                    new PrioritySelector(
                        Spell.Cast("Smash"),
                        Spell.Cast("Sweeping Slash")
                        ));
            }
        }
    }
}