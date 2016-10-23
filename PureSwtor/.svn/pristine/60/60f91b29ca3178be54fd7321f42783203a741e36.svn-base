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
    class Combat : RotationBase
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
            get { return "Shadow Kinetic Combat by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Combat Technique"),
                    Spell.Buff("Force Valor"),
                    Spell.Cast("Guard", ret => Me.Companion, ret => Me.Companion != null && !Me.Companion.IsDead && !Me.Companion.HasBuff("Guard")),
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
                    Spell.Buff("Kinetic Ward", ret => Me.BuffCount("Kinetic Ward") <= 1 || Me.BuffTimeLeft("Kinetic Ward") < 3),
                    Spell.Buff("Force of Will"),
                    Spell.Buff("Battle Readiness", ret => Me.HealthPercent <= 85),
                    Spell.Buff("Deflection", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Resilience", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Force Potency")
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
                    Spell.Buff("Force Speed", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),

                    //Movement
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Stun", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Telekinetic Throw", ret => Me.BuffCount("Harnessed Shadows") == 3),
                    Spell.Cast("Slow Time"),
                    Spell.Cast("Project", ret => Me.HasBuff("Particle Acceleration")),
                    Spell.Cast("Shadow Strike", ret => Me.HasBuff("Shadow Wrap")),
                    Spell.Cast("Spinning Strike", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Force Breach"),
                    Spell.Cast("Double Strike"),
                    Spell.Cast("Force Speed", ret => Me.CurrentTarget.Distance >= 1.1f && Me.IsMoving && Me.InCombat)
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.Cast("Slow Time"),
                        Spell.Cast("Force Breach"),
                        Spell.Cast("Whirling Blow", ret => Me.ForcePercent >= 60 && Me.CurrentTarget.Distance <= 0.5f)
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
                    new Decorator(ret => Me.ForcePercent < 25, HandleLowEnergy),
                    new Decorator(ret => Me.ForcePercent >= 25, HandleSingleTarget)
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
