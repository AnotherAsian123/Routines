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

namespace PureSWTor.Classes.Mercenary
{
    class Arsenal : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Arsenal; }
        }

        public override string Name
        {
            get { return "Arsenal by Ama"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("High Velocity Gas Cylinder"),
                    Spell.Buff("Hunter's Boon"),
                    Rest.HandleRest                    
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    MedPack.UseItem(ret => Me.HealthPercent <= 50),
                    Spell.Buff("Determination", ret => Me.IsStunned),
                    Spell.Buff("Vent Heat", ret => Me.ResourcePercent() >= 50),
                    Spell.Buff("Supercharged Gas", ret => Me.BuffCount("Supercharge") == 10),
                    Spell.Buff("Energy Shield", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Kolto Overload", ret => Me.HealthPercent <= 30)
                    );
            }
        }

        private Composite HandleHighHeat
        {
            get
            {
                return new LockSelector(
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
                    CloseDistance(Distance.Ranged),

                    Spell.Cast("Disabling Shot", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= Distance.Melee && !LazyRaider.MovementDisabled),
                    Spell.Cast("Blazing Bolts", ret => Me.HasBuff("Barrage") && Me.Level >= 57),
                    Spell.Cast("Unload", ret => Me.Level < 57),
                    Spell.Cast("Heatseeker Missiles", ret => Me.CurrentTarget.HasDebuff("Heat Signature")),
                    Spell.Cast("Rail Shot", ret => Me.BuffCount("Tracer Lock") == 5),
                    Spell.Cast("Electro Net"),
                    Spell.Cast("Priming Shot"),
                    Spell.Cast("Tracer Missile")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                            new LockSelector(
                                Spell.Buff("Thermal Sensor Override"),
                                Spell.Buff("Power Surge"),
                                Spell.CastOnGround("Death from Above", ret => Me.CurrentTarget.Distance > Distance.MeleeAoE),
                                Spell.Cast("Fusion Missile", ret => Me.ResourcePercent() <= 10 && Me.HasBuff("Power Surge")),
                                Spell.Cast("Flame Thrower", ret => Me.CurrentTarget.Distance <= 1f),
                                Spell.CastOnGround("Sweeping Blasters", ret => Me.ResourcePercent() <= 10))
                                );
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