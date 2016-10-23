using System;
using System.Collections.Generic;
using System.Linq;
using Buddy.BehaviorTree;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using PureSWTor.Helpers;
using PureSWTor.Core;
using PureSWTor.Managers;

using Action = Buddy.BehaviorTree.Action;
using Distance = PureSWTor.Helpers.Global.Distance;

namespace PureSWTor.Classes.PowerTech
{
    class ShieldTech : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.ShieldTech; }
        }

        public override string Name
        {
            get { return "Shield Tech by Ama"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Ion Gas Cylinder"),
                    Spell.Buff("Hunter's Boon"),
                    Spell.Cast("Guard", on => Me.Companion, ret => Me.Companion != null && !Me.Companion.IsDead && !Me.Companion.HasBuff("Guard")),
                    //Scavenge.ScavengeCorpse,
                    Rest.HandleRest  
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Determination", ret => Me.IsStunned),
                    Spell.Buff("Vent Heat", ret => Me.ResourcePercent() >= 50),
                    Spell.Buff("Energy Shield", ret => Me.HealthPercent <= 40),
                    Spell.Buff("Kolto Overload", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Shoulder Cannon", ret => !Me.HasBuff("Shoulder Cannon") && Me.CurrentTarget.BossOrGreater()),
                    MedPack.UseItem(ret => Me.HealthPercent <= 20)
                    );
            }
        }

        private Composite HandleHighHeat
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Heat Blast", ret => Me.BuffCount("Heat Screen") == 3),
                    Spell.Cast("Firestorm", ret => Me.HasBuff("Flame Engine") && Me.CurrentTarget.Distance <= 1f && Me.Level >= 57),
                    Spell.Cast("Flame Thrower", ret => Me.HasBuff("Flame Engine") && Me.CurrentTarget.Distance <= 1f && Me.Level < 57),
                    Spell.Cast("Flame Burst", ret => Me.HasBuff("Flame Surge")),
                    Spell.Cast("Rapid Shots")
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    Spell.Cast("Jet Charge", ret => Me.CurrentTarget.Distance >= 1f && !LazyRaider.MovementDisabled),
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("Quell", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= Distance.Melee && !LazyRaider.MovementDisabled),
                    Spell.CastOnGround("Oil Slick", ret => Me.CurrentTarget.BossOrGreater() && Me.CurrentTarget.Distance <= 0.8f),
                    Spell.Cast("Shoulder Cannon", ret => Me.HasBuff("Shoulder Cannon") && Me.CurrentTarget.BossOrGreater()),
                    Spell.Cast("Heat Blast", ret => Me.BuffCount("Heat Screen") == 3),
                    Spell.Cast("Rocket Punch"),
                    Spell.Cast("Rail Shot"),
                    Spell.Cast("Firestorm", ret => Me.HasBuff("Flame Engine") && Me.CurrentTarget.Distance <= 1f && Me.Level >= 57),
                    Spell.Cast("Flame Thrower", ret => Me.HasBuff("Flame Engine") && Me.CurrentTarget.Distance <= 1f && Me.Level < 57),
                    Spell.Cast("Flame Burst")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                            new LockSelector(
                                Spell.CastOnGround("Death from Above"),
                                Spell.Cast("Explosive Dart")));
            }
        }

        private Composite HandlePBAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                            new LockSelector(
                                Spell.Cast("Firestorm", ret => Me.Level >= 57),
                                Spell.Cast("Flame Thrower", ret => Me.Level < 57),
                                Spell.Cast("Flame Sweep")));
            }
        }

        private class LockSelector : PrioritySelector
        {
            public LockSelector(params Composite[] children)
                : base(children)
            {
            }

            public override RunStatus Tick(object context)
            {
                using (BuddyTor.Memory.AcquireFrame())
                {
                    return base.Tick(context);
                }
            }
        }

        public override Composite PVERotation
        {
            get
            {
                return new PrioritySelector(
                    Spell.WaitForCast(),
                    HandleCoolDowns,
                    HandleAOE,
                    HandlePBAOE,
                    new Decorator(ret => Me.ResourcePercent() > 40, HandleHighHeat),
                    new Decorator(ret => Me.ResourcePercent() <= 40, HandleSingleTarget)
                );
            }
        }

        public override Composite PVPRotation
        {
            get { return PVERotation; }
        }

        #endregion
    }
}