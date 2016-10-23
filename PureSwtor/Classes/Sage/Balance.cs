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

namespace PureSWTor.Classes.Sage
{
    public class Balance : RotationBase
    {

        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Balance; }
        }

        public override string Name
        {
            get { return "Jedi Sage Balance by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
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
                    Spell.Buff("Force Barrier", ret => Me.HealthPercent <= 20),
                    Spell.Buff("Force Mend", ret => Me.HealthPercent <= 50),
                    MedPack.UseItem(ret => Me.HealthPercent <= 25),
                    Spell.Buff("Mental Alacrity", ret => Me.CurrentTarget.BossOrGreater()),
                    Spell.Buff("Force Potency")
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    CloseDistance(Distance.Ranged),

                    //Rotation
                    Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Stun", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                    Spell.Cast("Vanquish", ret => Me.BuffCount("Presence of Mind") == 4 && Me.Level >= 57),
                    Spell.Cast("Mind Crush", ret => Me.BuffCount("Presence of Mind") == 4 && Me.Level < 57),
                    Spell.DoT("Weaken Mind", "Weaken Mind"),
                    Spell.DoT("Sever Force", "Sever Force"),
                    Spell.CastOnGround("Force in Balance", ret => Me.CurrentTarget.HasDebuff("Weaken Mind") && Me.CurrentTarget.HasDebuff("Sever Force")),
                    Spell.Cast("Force Serenity", ret => Me.CurrentTarget.HasDebuff("Weaken Mind")),
                    Spell.Cast("Disturbance", ret => Me.BuffCount("Presence of Mind") == 4),         
                    Spell.Cast("Telekinetic Throw", ret => Me.BuffCount("Presence of Mind") < 4)
                );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(4, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.DoT("Weaken Mind", "Weaken Mind"),
                        Spell.DoT("Sever Force", "Sever Force"),
                        Spell.CastOnGround("Force in Balance", ret => Me.CurrentTarget.HasDebuff("Weaken Mind") && Me.CurrentTarget.HasDebuff("Sever Force")),
                        Spell.CastOnGround("Forcequake", ret => Me.CurrentTarget.HasDebuff("Overwhelmed (Mental)"))
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