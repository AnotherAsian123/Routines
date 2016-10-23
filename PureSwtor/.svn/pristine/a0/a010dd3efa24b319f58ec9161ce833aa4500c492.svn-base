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
    class Madness : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Madness; }
        }

        public override string Name
        {
            get { return "Assassin Madness by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Lightning Charge", ret => !Me.HasBuff("Lightning Charge")),
                    Spell.Buff("Mark of Power"),
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
                    Spell.Buff("Unbreakable Will"),
                    Spell.Buff("Overcharge Saber", ret => Me.HealthPercent <= 85),
                    Spell.Buff("Deflection", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Force Shroud", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Recklessness"),
                    Spell.Buff("Blackout", ret => Me.ForcePercent <= 40)
                    );
            }
        }

        private Composite HandleLowEnergy
        {
            get
            {
                return new LockSelector(
                    CloseDistance(Distance.Melee),
                    Spell.Cast("Crushing Darkness", ret => Me.HasBuff("Raze")),
                    Spell.Cast("Saber Strike")
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //*Steath and Force Speed Handling. Remove the // to enable the automatic usage of these abilities
                    Spell.Buff("Force Speed", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),

                    //Movement
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("Jolt", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.CastOnGround("Death Field"),
                    Spell.DoT("Discharge", "", 15000),
                    Spell.DoT("Creeping Terror", "", 15000),
                    Spell.Cast("Crushing Darkness", ret => Me.HasBuff("Raze")),
                    Spell.Cast("Assassinate", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Thrash"),
                    Spell.Buff("Force Speed", ret => Me.CurrentTarget.Distance >= 1.1f && Me.IsMoving && Me.InCombat)
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.CastOnGround("Death Field"),
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