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

namespace PureSWTor.Classes.Assassin
{
    class Deception : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Deception; }
        }

        public override string Name
        {
            get { return "Assassin Deception by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Surging Charge"),
                    Spell.Buff("Mark of Power"),
                    //Scavenge.ScavengeCorpse,
                    Rest.HandleRest,
                    Spell.Buff("Stealth", ret => !Rest.KeepResting() && !LazyRaider.MovementDisabled)
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Unbreakable Will", ret => Me.IsStunned),
                    Spell.Buff("Overcharge Saber", ret => Me.HealthPercent <= 85),
                    Spell.Buff("Deflection", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Force Shroud", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Recklessness", ret => Me.BuffCount("Static Charge") < 1),
                    Spell.Buff("Blackout", ret => Me.ForcePercent <= 40)
                    );
            }
        }

        private Composite HandleStealth
        {
            get
            {
                return new LockSelector(
                    CloseDistance(Distance.Melee),
                    Spell.Cast("Spike"),
                    Spell.Cast("Maul", ret => Me.IsBehind(Me.CurrentTarget))
                    );
            }
        }

        private Composite HandleLowEnergy
        {
            get
            {
                return new LockSelector(
                    CloseDistance(Distance.Melee),
                    Spell.Cast("Saber Strike")
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Force Speed", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f),

                    //Movement
                    CloseDistance(Distance.Melee),

                    //Interrupts
                    Spell.Cast("Jolt", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Electrocute", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Low Slash", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                    //Rotation
                    Spell.Cast("Discharge", ret => Me.BuffCount("Static Charge") == 3),
                    Spell.Cast("Maul", ret => (Me.HasBuff("Stealth") || Me.HasBuff("Duplicity")) && Me.IsBehind(Me.CurrentTarget)),
                    Spell.Cast("Shock", ret => Me.BuffCount("Induction") == 2),
                    Spell.Cast("Assassinate", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Voltaic Slash", ret => Me.Level >= 26),
                    Spell.Cast("Thrash", ret => Me.Level < 26),
                    Spell.Cast("Force Speed", ret => Me.CurrentTarget.Distance >= 1.1f && Me.IsMoving && Me.InCombat)
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.Cast("Lacerate", ret => Me.ForcePercent >= 60)
                    ));
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
                    new Decorator(ret => Me.IsStealthed, HandleStealth),
                    new Decorator(ret => Me.ForcePercent < 25, HandleLowEnergy),
                    new Decorator(ret => Me.ForcePercent >= 25 && !Me.IsStealthed, HandleSingleTarget)
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