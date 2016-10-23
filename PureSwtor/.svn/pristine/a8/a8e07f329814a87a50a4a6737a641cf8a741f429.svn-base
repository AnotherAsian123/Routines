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

namespace PureSWTor.Classes.Vanguard
{
    class ShieldSpecialist : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.ShieldSpecialist; }
        }

        public override string Name
        {
            get { return "Shield Specialist by Ama"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Ion Cell"),
                    Spell.Buff("Fortification"),
                    Spell.Cast("Guard", on => Me.Companion, ret => Me.Companion != null && !Me.Companion.IsDead && !Me.Companion.HasBuff("Guard")),
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
                    Spell.Buff("Tenacity", ret => Me.IsStunned),
                    Spell.Buff("Recharge Cells", ret => Me.ResourcePercent() >= 50),
                    Spell.Buff("Reactive Shield", ret => Me.HealthPercent <= 40),
                    Spell.Buff("Adrenaline Rush", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Shoulder Cannon", ret => !Me.HasBuff("Shoulder Cannon") && Me.CurrentTarget.BossOrGreater()),
                    MedPack.UseItem(ret => Me.HealthPercent <= 20)
                    );
            }
        }

        private Composite HandleLowCells
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Energy Blast", ret => Me.BuffCount("Power Screen") == 3),
                    Spell.Cast("Pulse Cannon", ret => Me.HasBuff("Pulse Engine") && Me.CurrentTarget.Distance <= 1f),
                    Spell.Cast("Explosive Surge", ret => Me.HasBuff("Static Surge") && Me.CurrentTarget.Distance <= 0.5f),
                    Spell.Cast("Hammer Shot")
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //Movement
                    Spell.Cast("Storm", ret => Me.CurrentTarget.Distance >= 1f && !LazyRaider.MovementDisabled),
                    CloseDistance(Distance.Melee),

                    //Rotation
                    //Spell.Cast("Neural Jolt"),
                    Spell.CastOnGround("Smoke Grenade", ret => Me.CurrentTarget.BossOrGreater() && Me.CurrentTarget.Distance <= 0.8f),
                    Spell.Cast("Shoulder Cannon", ret => Me.HasBuff("Shoulder Cannon") && Me.CurrentTarget.BossOrGreater()),
                    Spell.Cast("Riot Strike", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= Distance.Melee && !LazyRaider.MovementDisabled),
                    Spell.Cast("Energy Blast", ret => Me.BuffCount("Power Screen") == 3),
                    Spell.Cast("Stockstrike"),
                    Spell.Cast("High Impact Bolt"),
                    Spell.Cast("Pulse Cannon", ret => Me.HasBuff("Pulse Engine") && Me.CurrentTarget.Distance <= 1f),
                    Spell.Cast("Explosive Surge", ret => Me.HasBuff("Static Surge") && Me.CurrentTarget.Distance <= 0.5f),
                    Spell.Cast("Ion Pulse")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                            new LockSelector(
                                Spell.CastOnGround("Mortar Volley", ret => Me.CurrentTarget.Distance > .5f),
                                Spell.Cast("Sticky Grenade"),
                                Spell.Cast("Pulse Cannon", ret => Me.CurrentTarget.Distance <= 1f),
                                Spell.Cast("Explosive Surge")));
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
                    new Decorator(ret => Me.ResourcePercent() < 60, HandleLowCells),
                    new Decorator(ret => Me.ResourcePercent() >= 60, HandleSingleTarget)
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