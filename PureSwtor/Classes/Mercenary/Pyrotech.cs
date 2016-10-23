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
    class Pyrotech : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.CanNotBeDetermined; }
        }

        public override string Name
        {
            get { return "Mercenary Pyrotech by Ama"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Combustible Gas Cylinder"),
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
                    Spell.Buff("Determination", ret => Me.IsStunned),
                    Spell.Buff("Vent Heat", ret => Me.ResourcePercent() >= 50),
                    Spell.Buff("Energy Shield", ret => Me.HealthPercent <= 40),
                    Spell.Buff("Kolto Overload", ret => Me.HealthPercent <= 30)
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
                    CloseDistance(Distance.Ranged),

                    //Rotation
                    Spell.Cast("Disabling Shot", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= Distance.Melee && !LazyRaider.MovementDisabled),
                    Spell.Cast("Rail Shot"),
                    Spell.DoT("Incendiary Missile", "", 12000),
                    Spell.Cast("Thermal Detonator"),
                    Spell.Cast("Electro Net"),
                    Spell.Cast("Power Shot")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                            new LockSelector(
                                Spell.CastOnGround("Death from Above", ret => Me.CurrentTarget.Distance > Distance.MeleeAoE),
                                Spell.Cast("Fusion Missle", ret => Me.HasBuff("Thermal Sensor Override")),
                                Spell.Cast("Explosive Dart")));
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