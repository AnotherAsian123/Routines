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
    class Focus : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Focus; }
        }

        public override string Name
        {
            get { return "Guardian Focus"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Shii-Cho Form"),
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
                    MedPack.UseItem(ret => Me.HealthPercent <= 10),
                    Spell.Buff("Combat Focus", ret => Me.ActionPoints <= 6 || Me.BuffCount("Singularity") < 3)
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
                    Spell.Cast("Awe", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Stasis", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                    //Rotation
                    Spell.Cast("Focused Burst", ret => Me.HasBuff("Felling Blow") && Me.BuffCount("Singularity") == 3),
                    Spell.Cast("Zealous Leap", ret => !Me.HasBuff("Felling Blow")),
                    Spell.Cast("Force Exhaustion", ret => Me.BuffCount("Singularity") < 3),
                    Spell.Cast("Blade Storm", ret => Me.HasBuff("Momentum")),
                    Spell.Cast("Concentrated Slice"),
                    Spell.Cast("Master Strike"),
                    Spell.Cast("Riposte"),
                    Spell.Cast("Sundering Strike", ret => Me.ActionPoints < 7),
                    Spell.Cast("Strike"),
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
                        Spell.Cast("Force Sweep", ret => Me.HasBuff("Felling Blow") && Me.BuffCount("Singularity") == 3),
                        Spell.Cast("Cyclone Smash")
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