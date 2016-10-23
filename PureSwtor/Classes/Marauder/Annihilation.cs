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

namespace PureSWTor.Classes.Marauder
{
    class Annihilation : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Annihilation; }
        }

        public override string Name
        {
            get { return "Marauder Annihilation by aquintus"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Juyo Form"),
                    Spell.Buff("Unnatural Might"),
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
                    Spell.Buff("Cloak of Pain", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Undying Rage", ret => Me.HealthPercent <= 10),
                    Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Deadly Saber", ret => !Me.HasBuff("Deadly Saber")),
                    Spell.Buff("Frenzy", ret => Me.BuffCount("Fury") < 5),
                    Spell.Buff("Berserk")
                    );
            }
        }

        /*private Composite HandlePull
        {
            get
            {
                return new Decorator(
                    ret => !Me.InCombat,
                    new LockSelector(
                        Spell.Cast("Force Charge"),
                        Spell.Buff("Deadly Saber"),
                        Spell.Cast("Battering Assault"),
                        Spell.Cast("Rupture"),
                        Spell.Cast("Annihilate"),
                        Spell.Cast("Ravage")
                    )
                );
            }
        }*/

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    Spell.Cast("Dual Saber Throw", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),
                    CloseDistance(Distance.Melee),
                    
                    //Rotation
                    Spell.Cast("Disruption", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.DoT("Rupture", "", 6000),
                    Spell.Cast("Force Rend" , ret => !Me.CurrentTarget.HasDebuff("Force Rend")),
                    Spell.Cast("Annihilate"),
                    Spell.Cast("Vicious Throw", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Dual Saber Throw" , ret => Me.HasBuff("Pulverize")),
                    Spell.Cast("Force Choke"),
                    Spell.Cast("Ravage"),
                    Spell.Cast("Vicious Slash", ret => Me.ActionPoints >= 9),
                    Spell.Cast("Battering Assault", ret => Me.ActionPoints <= 6),
                    Spell.Cast("Force Charge", ret => Me.ActionPoints <= 8),
                    Spell.Cast("Assault", ret => Me.ActionPoints < 9)
                    );
            }
        }
        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                   new LockSelector(
                   Spell.Cast("Dual Saber Throw", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),
                   Spell.Cast("Smash"),
                   Spell.Cast("Sweeping Slash")
                   )
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
                    HandleAOE,
                    //HandlePull,
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
