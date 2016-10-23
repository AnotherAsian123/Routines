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
    class Balance : RotationBase
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
            get { return "Shadow Balance by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Force Technique"),
                    Spell.Buff("Force Valor"),
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
                    Spell.Buff("Force of Will"),
                    Spell.Buff("Battle Readiness", ret => Me.HealthPercent <= 85),
                    Spell.Buff("Deflection", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Resilience", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Force Potency"),
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
                    Spell.Cast("Mind Crush", ret => Me.HasBuff("Force Strike")),
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
                    Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.CastOnGround("Force in Balance"),
                    Spell.Cast("Sever Force", ret => !Me.CurrentTarget.HasDebuff("Sever Force")),
                    Spell.DoT("Force Breach", "Crushed (Force Breach)"),
                    Spell.Cast("Vanquish", ret => Me.HasBuff("Force Strike") && Me.Level >= 57),
                    Spell.Cast("Mind Crush", ret => Me.HasBuff("Force Strike") && Me.Level < 57),
                    Spell.Cast("Spinning Strike", ret => Me.CurrentTarget.HealthPercent <= 30 || Me.HasBuff("Crush Spirit")),
                    Spell.Cast("Serenity Strike", ret => Me.HealthPercent <= 70),
                    Spell.Cast("Double Strike"),
                    Spell.Buff("Force Speed", ret => Me.CurrentTarget.Distance >= 1.1f && Me.IsMoving && Me.InCombat)
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.DoT("Force Breach", "Crushed (Force Breach)"),
                        Spell.Cast("Sever Force", ret => !Me.CurrentTarget.HasDebuff("Sever Force")),
                        Spell.CastOnGround("Force in Balance"),
                        Spell.Cast("Whirling Blow", ret => Me.ForcePercent > 70)
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
