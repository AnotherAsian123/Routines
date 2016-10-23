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

namespace PureSWTor.Classes.Commando
{
    class Assault : RotationBase
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
            get { return "Commando Assault by Ama"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Plasma Cell"),
                    Spell.Buff("Fortification"),
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
                    Spell.Buff("Recharge Cells", ret => Me.ResourceStat <= 40),
                    Spell.Buff("Reactive Shield", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Adrenaline Rush", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Supercharged Cell", ret => Me.BuffCount("Supercharge") == 10),
                    Spell.Buff("Reserve Powercell", ret => Me.ResourceStat <= 60)
                    );
            }
        }

        private Composite HandleLowCells
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Mag Bolt", ret => Me.HasBuff("Ionic Accelerator") && Me.Level >= 57),
                    Spell.Cast("High Impact Bolt", ret => Me.HasBuff("Ionic Accelerator") && Me.Level < 57),
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
                    CloseDistance(Distance.Ranged),

                    Spell.Cast("Disabling Shot", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                    Spell.Cast("Mag Bolt", ret => Me.HasBuff("Ionic Accelerator") && Me.Level >= 57),
                    Spell.Cast("High Impact Bolt", ret => Me.HasBuff("Ionic Accelerator") && Me.Level < 57),
                    Spell.Cast("Explosive Round", ret => Me.HasBuff("Hyper Assault Rounds")),
                    Spell.Cast("Assault Plastique"),
                    Spell.Cast("Serrated Bolt", ret => !Me.CurrentTarget.HasDebuff("Bleeding")),
                    Spell.Cast("Incendiary Round", ret => !Me.CurrentTarget.HasDebuff("Burning (Incendiary Round)")),
                    Spell.Cast("Electro Net", ret => Me.CurrentTarget.StrongOrGreater()),
                    Spell.Cast("Mag Bolt", ret => Me.Level >= 57),
                    Spell.Cast("High Impact Bolt", ret => Me.Level < 57),
                    Spell.Cast("Charged Bolts")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                            new LockSelector(
                               Spell.Cast("Incendiary Round", ret => !Me.CurrentTarget.HasDebuff("Burning (Incendiary Round)")),
                               Spell.Cast("Serrated Bolt", ret => !Me.CurrentTarget.HasDebuff("Bleeding")),
                               Spell.Cast("Plasma Grenade", ret => Me.CurrentTarget.HasDebuff("Burning (Incendiary Round)")),
                               Spell.Cast("Sticky Grenade", ret => Me.CurrentTarget.HasDebuff("Bleeding")),
                               Spell.Cast("Explosive Round", ret => Me.HasBuff("Hyper Assault Rounds")),
                               Spell.CastOnGround("Hail of Bolts", ret => Me.ResourcePercent() >= 90)
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