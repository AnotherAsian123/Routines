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
    class Assault : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Plasmatech; }
        }

        public override string Name
        {
            get { return "Vanguard Assault by Ama"; }
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
                    Spell.Buff("Tenacity"),
                    Spell.Buff("Battle Focus"),
                    Spell.Buff("Recharge Cells", ret => Me.ResourcePercent() >= 50),
                    Spell.Buff("Reserve Powercell"),
                    Spell.Buff("Reactive Shield", ret => Me.HealthPercent <= 60),
                    Spell.Buff("Adrenaline Rush", ret => Me.HealthPercent <= 30)
                    );
            }
        }

        private Composite HandleLowCells
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("High Impact Bolt", ret => Me.HasBuff("Ionic Accelerator")),
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
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("High Impact Bolt"),
                    Spell.Cast("Assault Plastique"),
                    Spell.DoT("Incendiary Round", "", 12000),
                    Spell.Cast("Stockstrike"),
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
                                Spell.Cast("Pulse Cannon", ret => Me.CurrentTarget.Distance <= 1f),
                                Spell.Cast("Explosive Surge", ret => Me.CurrentTarget.Distance <= .5f)
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