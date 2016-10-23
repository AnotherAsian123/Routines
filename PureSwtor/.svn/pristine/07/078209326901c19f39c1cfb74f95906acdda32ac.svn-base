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

namespace PureSWTor.Classes.Juggernaut
{
    class Vengeance : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Vengeance; }
        }

        public override string Name
        {
            get { return "Juggernaut Vengeance by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Shien Form"),
                    Spell.Buff("Unnatural Might"),
                    Rest.HandleRest
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Unleash", ret => Me.IsStunned),
                    Spell.Buff("Enraged Defense", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Saber Reflect", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Endure Pain", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Enrage", ret => Me.ActionPoints <= 6)
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
                    Spell.Cast("Force Charge", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),

                    //Move To Range
                    CloseDistance(Distance.Melee),

                    Spell.Cast("Disruption", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Scream", ret => Me.BuffCount("Savagery") == 2),
                    Spell.Cast("Vicious Throw", ret => Me.HasBuff("Destroyer") || Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Shatter"),
                    Spell.Cast("Impale"),
                    Spell.Cast("Sundering Assault", ret => Me.ActionPoints <= 7),
                    Spell.Cast("Ravage"),
                    Spell.Cast("Vicious Slash", ret => Me.ActionPoints >= 11),
                    Spell.Cast("Assault"),
                    Spell.Cast("Retaliation")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.Cast("Vengeful Slam"),
                        Spell.Cast("Smash"),
                        Spell.Cast("Vicious Slash")
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
