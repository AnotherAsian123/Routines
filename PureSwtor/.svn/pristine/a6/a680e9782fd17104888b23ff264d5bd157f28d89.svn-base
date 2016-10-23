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
    class Combat : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Darkness; }
        }

        public override string Name
        {
            get { return "Assassin Darkness by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Dark Charge"),
                    Spell.Buff("Mark of Power"),
                    //Scavenge.ScavengeCorpse,
                    Rest.HandleRest,
                    Spell.Cast("Guard", on => Me.Companion, ret => Me.Companion != null && !Me.Companion.IsDead && !Me.Companion.HasBuff("Guard")),
                    Spell.Buff("Stealth", ret => !Rest.KeepResting() && !LazyRaider.MovementDisabled)
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Dark Ward", ret => Me.BuffCount("Dark Ward") <= 1 || Me.BuffTimeLeft("Dark Ward") < 2),
                    Spell.Buff("Unbreakable Will"),
                    MedPack.UseItem(ret => Me.HealthPercent <= 30),
                    Spell.Buff("Overcharge Saber", ret => Me.HealthPercent <= 85),
                    Spell.Buff("Deflection", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Force Shroud", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Unity", ret => Me.CurrentTarget.BossOrGreater()),
                    Spell.Buff("Recklessness", ret => Me.CurrentTarget.BossOrGreater())
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
                    //Check Debuffs
                    //BuffLog.LogTargetBuffs(),

                    Spell.Buff("Force Speed", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),

                    //Movement
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("Jolt", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Electrocute", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Lightning", ret => Me.BuffCount("Harnessed Darkness") == 3),
                    Spell.Cast("Wither"),
                    Spell.Cast("Shock"),
                    Spell.Cast("Maul", ret => Me.HasBuff("Conspirator's Cloak") || Me.IsBehind(Me.CurrentTarget)),
                    Spell.Cast("Assassinate", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Discharge"),
                    Spell.Cast("Thrash"),
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
                        Spell.Cast("Wither"),
                        Spell.Cast("Discharge"),
                        Spell.Cast("Lacerate", ret => Me.ForcePercent >= 60 && ShouldPBAOE(3, Distance.MeleeAoE))
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
                    new Decorator(ret => Me.ForcePercent < 30, HandleLowEnergy),
                    new Decorator(ret => Me.ForcePercent >= 30, HandleSingleTarget)
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