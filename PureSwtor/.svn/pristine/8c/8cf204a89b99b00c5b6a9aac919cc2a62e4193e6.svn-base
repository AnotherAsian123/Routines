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

namespace PureSWTor.Classes.Guardian
{
    class Defense : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Defense; }
        }

        public override string Name
        {
            get { return "Guardian Defense by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Soresu Form"),
                    Spell.Buff("Force Might"),
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
                    Spell.Buff("Resolute"),
                    Spell.Buff("Saber Reflect", ret => Me.HealthPercent <= 90),
                    Spell.Buff("Enure", ret => Me.HealthPercent <= 80),
                    Spell.Buff("Focused Defense", ret => Me.HealthPercent < 70),
                    Spell.Buff("Warding Call", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 30),
                    MedPack.UseItem(ret => Me.HealthPercent <= 10),
                    Spell.Buff("Combat Focus", ret => Me.ActionPoints <= 6)
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Saber Throw", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 0.5f && Me.CurrentTarget.Distance <= 3f),
                    Spell.Cast("Force Leap", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),

                    //Movement
                    CloseDistance(Distance.Melee),

                    //Interrupts
                    Spell.Cast("Force Kick", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Stasis", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                    //Rotation
                    Spell.Cast("Riposte"),
                    Spell.Cast("Guardian Slash"),
                    Spell.Cast("Blade Storm"),
                    Spell.Cast("Warding Strike", ret => Me.ActionPoints <= 7 || !Me.HasBuff("Warding Strike")),
                    Spell.Cast("Force Sweep", ret => !Me.CurrentTarget.HasDebuff("Unsteady (Force)") && ShouldPBAOE(1, 0.5f)),
                    Spell.Cast("Hilt Bash", ret => !Me.CurrentTarget.IsStunned),
                    Spell.Cast("Master Strike"),
                    Spell.Cast("Dispatch", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Slash", ret => Me.ActionPoints >= 9),
                    Spell.Cast("Strike"),
                    Spell.Cast("Saber Throw", ret => Me.CurrentTarget.Distance >= 0.5f && Me.InCombat)
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(2, 0.5f),
                    new LockSelector(
                        CloseDistance(Distance.Melee),
                        Spell.Cast("Force Sweep"),
                        Spell.Cast("Guardian Slash", ret => Me.HasBuff("Warding Strike")),
                        Spell.Cast("Warding Strike", ret => !Me.HasBuff("Warding Strike")),
                        Spell.Cast("Riposte"),
                        Spell.Cast("Blade Storm"),
                        Spell.Cast("Cyclone Slash")
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
                    HandleSingleTarget
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
