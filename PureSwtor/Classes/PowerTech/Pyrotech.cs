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
    class Pyrotech : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Firebug; }
        }

        public override string Name
        {
            get { return "Powertech Pyrotech by Ama"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Combustible Gas Cylinder"),
                    Spell.Buff("Hunter's Boon"),
                    Rest.HandleRest
                    //Scavenge.ScavengeCorpse
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Determination"),
                    Spell.Buff("Vent Heat", ret => Me.ResourcePercent() >= 65),
                    Spell.Buff("Energy Shield", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Kolto Overload", ret => Me.HealthPercent <= 30)
                    );
            }
        }

        private Composite HandleHighHeat
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Flame Burst", ret => Me.HasBuff("Flame Barrage")),
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
                    CloseDistance(Distance.Melee),
                    //Rotation
                    Spell.Cast("Flame Thrower", ret => Me.BuffCount("Superheated Flame Thrower") == 3),
                    Spell.DoT("Incendiary Missile", "Burning (Incendiary Missle)"),
                    Spell.DoT("Scorch", "Burning (Scorch)"),
                    Spell.Cast("Rail Shot", ret => Me.HasBuff("Charged Gauntlets")),
                    Spell.Cast("Immolate"),
                    Spell.Cast("Thermal Detonator"),
                    Spell.Cast("Flaming Fist"),
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
                                Spell.DoT("Scorch", "Burning (Scorch)"),
                                Spell.CastOnGround("Death from Above", ret => Me.CurrentTarget.Distance > Distance.MeleeAoE),
                                Spell.Cast("Explosive Dart")));
            }
        }

        private Composite HandlePBAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                            new LockSelector(
                                Spell.DoT("Scorch", "Burning (Scorch)"),
                                Spell.Cast("Flame Thrower"),
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