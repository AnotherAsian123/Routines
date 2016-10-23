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
    class Vigilance : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Vigilance; }
        }

        public override string Name
        {
            get { return "Guardian Vigilance by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Shien Form"),
                    Spell.Buff("Force Might"),
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
                    Spell.Buff("Resolute", ret => Me.IsStunned),
                    Spell.Buff("Saber Reflect", ret => Me.HealthPercent <= 90),
                    Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Focused Defense", ret => Me.HealthPercent < 70),
                    Spell.Buff("Enure", ret => Me.HealthPercent <= 30),
                    MedPack.UseItem(ret => Me.HealthPercent <= 20),
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

                    //Rotation
                    Spell.Cast("Plasma Brand"),
                    Spell.Cast("Overhead Slash"),
                    Spell.Cast("Blade Storm", ret => Me.BuffCount("Force Rush") == 2),
                    Spell.Cast("Master Strike"),
                    Spell.Cast("Dispatch", ret => Me.HasBuff("Keening") || Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Sundering Strike", ret => Me.ActionPoints <= 7),
                    Spell.Cast("Slash", ret => Me.ActionPoints >= 9),
                    Spell.Cast("Strike"),
                    Spell.Cast("Riposte"),
                    Spell.Cast("Saber Throw", ret => Me.CurrentTarget.Distance >= 0.5f && Me.InCombat)
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.Cast("Vigilant Thrust", ret => Me.Level >= 57 && Me.CurrentTarget.HasDebuff("Burning (Plasma Brand)") && Me.CurrentTarget.HasDebuff("Burning (Burning Purpose)") && Me.CurrentTarget.HasDebuff("Burning (Burning Blade)")),
                        Spell.Cast("Force Sweep", ret => Me.Level < 57 && Me.CurrentTarget.HasDebuff("Burning (Plasma Brand)") && Me.CurrentTarget.HasDebuff("Burning (Burning Purpose)") && Me.CurrentTarget.HasDebuff("Burning (Burning Blade)")),
                        Spell.Cast("Cyclone Slash", ret => Me.CurrentTarget.HasDebuff("Burning (Plasma Brand)") || Me.CurrentTarget.HasDebuff("Burning (Burning Purpose)") || Me.CurrentTarget.HasDebuff("Burning (Burning Blade)"))
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
