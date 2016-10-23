using System;
using System.Collections.Generic;
using System.Linq;
using Buddy.BehaviorTree;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using PureSWTor.Helpers;
using PureSWTor.Core;
using Buddy.CommonBot;
using PureSWTor.Managers;

using Action = Buddy.BehaviorTree.Action;
using Distance = PureSWTor.Helpers.Global.Distance;

namespace PureSWTor.Classes.Sentinel
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
            get { return CharacterDiscipline.Combat; }
        }

        public override string Name
        {
            get { return "Sentinel Combat by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                        Spell.Buff("Ataru Form"),
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
                    Spell.Buff("Resolute"),
                    Spell.Buff("Rebuke", ret => Me.HealthPercent <= 90),
                    Spell.Buff("Saber Reflect", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Valorous Call", ret => Me.BuffCount("Centering") < 5),
                    Spell.Buff("Zen")
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //*Ranged Attacks, remove the // in order to activate them. Otherwise, you use to start combat
                    Spell.Cast("Saber Throw", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 0.5f && Me.CurrentTarget.Distance <= 3f),
                    Spell.Cast("Force Leap", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),
                    Spell.Cast("Dual Saber Throw", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),

                    //Move To Range
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("Force Kick", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Dispatch", ret => Me.HasBuff("Hand of Justice")),
                    Spell.Cast("Dispatch", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Precision"),
                    Spell.Cast("Master Strike", ret => Me.HasBuff("Precision")),
                    Spell.Cast("Blade Storm", ret => Me.HasBuff("Opportune Attack")),
                    Spell.Cast("Blade Rush"),
                    Spell.Cast("Zealous Strike", ret => Me.ActionPoints <= 7),
                    Spell.Cast("Strike", ret => Me.ActionPoints <= 9),
                    Spell.Cast("Riposte")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.Cast("Force Sweep"),
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