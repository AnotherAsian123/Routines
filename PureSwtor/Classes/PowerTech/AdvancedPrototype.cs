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
    class Advanced : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.AdvancedPrototype; }
        }

        public override string Name
        {
            get { return "Powertech Advanced Prototype by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("High Energy Gas Cylinder"),
                    Spell.Buff("Hunter's Boon"),
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
                    Spell.Buff("Determination"),
                    Spell.Buff("Vent Heat", ret => Me.ResourcePercent() >= 50),
                    Spell.Buff("Energy Shield", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Kolto Overload", ret => Me.HealthPercent <= 30),
                    MedPack.UseItem(ret => Me.HealthPercent <= 20)
                    );
            }
        }

        private Composite HandleHighHeat
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Rail Shot", ret => Me.HasBuff("Prototype Particle Accelerator")),
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
                    Spell.Cast("Quell", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                    Spell.Cast("Energy Burst", ret => Me.BuffCount("Energy Lode") == 4),
                    Spell.Cast("Rail Shot", ret => Me.CurrentTarget.HasDebuff("Bleeding (Retractable Blade)") && Me.HasBuff("Prototype Particle Accelerator")),
                    Spell.DoT("Retractable Blade", "Bleeding (Retractable Blade)"),
                    Spell.Cast("Thermal Detonator"),
                    Spell.Cast("Rocket Punch"),
                    Spell.Cast("Magnetic Blast", ret => Me.Level >= 26),
                    Spell.Cast("Flame Burst", ret => Me.Level < 26)
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
                                Spell.Cast("Explosive Dart", ret => Me.CurrentTarget.HasDebuff("Bleeding (Retractable Blade)")),
                                Spell.MultiDot("Retractable Blade", "Bleeding (Retractable Blade)")
                                ));
            }
        }

        private Composite HandlePBAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                            new LockSelector(
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
                    PreCombat,
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