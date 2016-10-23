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
    class Focus : RotationBase
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
            get { return "Sentinel Focus by distiny"; }
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
                    Spell.Cast("Force Leap"),
                    Spell.Cast("Dispatch", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Force Sweep", ret => Me.HasBuff("Singularity") && Me.HasBuff("Felling Blow")),
                    Spell.Cast("Force Exhaustion"),
                    Spell.Cast("Zealous Leap", ret => Me.HasBuff("Singularity")),
                    Spell.Cast("Blade Storm", ret => Me.HasBuff("Battle Cry") || Me.Energy >= 5),
                    Spell.Cast("Dual Saber Throw"),
                    Spell.Cast("Master Strike"),
                    Spell.Cast("Force Stasis"),
                    Spell.Cast("Slash", ret => Me.HasBuff("Zen")),
                    Spell.Cast("Zealous Strike"),
                    Spell.Cast("Strike")
                    );
            }
        }

        private Composite HandleAOE
         {
             get
             {
                 return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                     new LockSelector());
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
                    //HandleAOE,
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
