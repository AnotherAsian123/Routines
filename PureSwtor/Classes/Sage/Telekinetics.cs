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
    class Telekinetics : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Telekinetics; }
        }

        public override string Name
        {
            get { return "Sage Telekinetics by aquintus"; }
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
                    Spell.Buff("Force Potency", ret => Me.CurrentTarget.StrongOrGreater()),
                    Spell.Buff("Mental Alacrity", ret => Me.CurrentTarget.StrongOrGreater()),
                    Spell.Buff("Force Mend", ret => Me.HealthPercent <= 80),
                    Spell.HoT("Force Armor", on => Me, 99, ret => !Me.HasDebuff("Force-imbalance") && !Me.HasBuff("Force Armor")),
                    Spell.Buff("Noble Sacrifice", ret => Me.HealthPercent > 25 && Me.ForcePercent < 50));
            }
        }

        private Composite HandleAoE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                    //Move To Range
                    CloseDistance(Distance.Ranged),

                    //Rotation
                    Spell.Cast("Telekinetic Wave", ret => Me.HasBuff("Tidal Force")),
                    Spell.CastOnGround("Forcequake")
                    ));
            }
        }

        private Composite DPS
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    CloseDistance(Distance.Ranged),

                    //Rotation
                    Spell.Cast("Weaken Mind", ret => !Me.CurrentTarget.HasDebuff("Weaken Mind")),
                    Spell.Cast("Turbulence", ret => Me.CurrentTarget.HasDebuff("Weaken Mind")),
                    Spell.Cast("Mind Crush"),
                    Spell.Cast("Telekinetic Gust"),
                    Spell.Cast("Telekinetic Wave", ret => Me.HasBuff("Tidal Force")),
                    Spell.Cast("Telekinetic Burst", ret => Me.Level >= 57),
                    Spell.Cast("Disturbance", ret => Me.Level < 57),
                    Spell.Cast("Project")
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
                    HandleAoE,
                    DPS
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