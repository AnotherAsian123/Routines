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

namespace PureSWTor.Classes.Shadow
{
    class Infiltration : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Infiltration; }
        }

        public override string Name
        {
            get { return "Shadow Infiltration by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Shadow Technique"),
                    Spell.Buff("Force Valor"),
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
                    Spell.Buff("Force of Will"),
                    Spell.Buff("Battle Readiness", ret => Me.HealthPercent <= 85),
                    Spell.Buff("Deflection", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Resilience", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Force Potency"),
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
                    Spell.Cast("Spinning Kick")
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
                    Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Stun", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Low Slash", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                    //Rotation
                    Spell.Cast("Force Breach", ret => Me.BuffCount("Breaching Shadows") == 3),
                    Spell.Cast("Shadow Strike", ret => Me.HasBuff("Stealth") || Me.HasBuff("Infiltration Tactics")),
                    Spell.Cast("Project", ret => Me.BuffCount("Circling Shadows") == 2),
                    Spell.Cast("Spinning Strike", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Clairvoyant Strike"),
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
                        Spell.Cast("Whirling Blow", ret => Me.ForcePercent >= 60)
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
