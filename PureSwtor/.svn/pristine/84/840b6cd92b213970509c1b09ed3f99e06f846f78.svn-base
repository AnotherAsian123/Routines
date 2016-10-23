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

namespace PureSWTor.Classes.Sentinel
{
    class Watchman : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Watchman; }
        }

        public override string Name
        {
            get { return "Sentinel Watchman by aquintus"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                        Spell.Buff("Juyo Form"),
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
                    Spell.Buff("Rebuke", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Guarded by the Force", ret => Me.HealthPercent <= 10),
                    Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 30)
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    Spell.Cast("Force Leap", ret => Me.CurrentTarget.Distance <= 30 && Me.CurrentTarget.Distance >= 10 && !LazyRaider.MovementDisabled),
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("Force Kick", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Buff("Zen", ret => Me.HasBuff("Centering") && Me.BuffCount("Centering") > 29),
                    Spell.Buff("Valorous Call", ret => Me.HasBuff("Centering") && Me.BuffCount("Centering") < 15),
                    Spell.Cast("Cauterize", ret => !Me.CurrentTarget.HasDebuff("Burning (Cauterize)")),
                    Spell.Cast("Merciless Slash"),
                    Spell.Cast("Force Melt"),
                    Spell.Cast("Dispatch", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Buff("Overload Saber"),
                    Spell.Cast("Zealous Strike", ret => Me.ActionPoints <= 5),
                    Spell.Cast("Master Strike"),
                    Spell.Cast("Slash"),
                    Spell.Cast("Strike")
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
                   Spell.Cast("Twin Saber Throw"),
                   Spell.Cast("Cyclone Slash")
                   )
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