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

namespace PureSWTor.Classes.Sorcerer
{
    class Lightning : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {   
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Lightning; }
        }

        public override string Name
        {
            get { return "Sorcerer Lightning by aquintus"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
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
                    Spell.Buff("Recklessness"),
                    Spell.Buff("Polarity Shift"),
                    Spell.Buff("Static Barrier", ret => !Me.HasBuff("Deionized")),
                    Spell.Buff("Unnatural Preservation", ret => Me.HealthPercent < 50),
                    Spell.Buff("Consumption", ret => Me.HealthPercent > 15 && Me.ForcePercent < 10));
            }
        }

        private Composite Healing
        {
            get
            {
                return new LockSelector(
                    Spell.HoT("Static Barrier", on => Tank, 100, ret => !Tank.HasDebuff("Deionized")),
                    Spell.HoT("Static Barrier", 95, ret => !HealTarget.HasDebuff("Deionized")));
            }
        }

        private Composite DPS
        {
            get
            {
                return new Decorator(ret => Me.CurrentTarget != null, 
                    new LockSelector(
                        //Movement
                        CloseDistance(Distance.Ranged),

                        //Rotation
                        Spell.Cast("Affliction", ret => !Me.CurrentTarget.HasDebuff("Affliction")),
                        Spell.Cast("Thundering Blast", ret => Me.CurrentTarget.HasDebuff("Affliction")),
                        Spell.Cast("Lightning Flash"),	
                        Spell.DoT("Crushing Darkness", "", 6000),
                        Spell.Cast("Force Lightning", ret => Me.HasBuff("Lightning Barrage")),
                        Spell.Cast("Chain Lightning", ret => Me.HasBuff("Lightning Storm")),
                        Spell.Cast("Lightning Bolt"),
                        Spell.Cast("Shock")));
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.Cast("Chain Lightning", ret => Me.HasBuff("Lightning Storm")),
                        Spell.CastOnGround("Force Storm")
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
                    PreCombat,
                    new Decorator(ret => !Me.IsInParty(), 
                        new PrioritySelector(
                            HealingManager.AcquireHealTargets,
                            Healing)),
                    HandleCoolDowns,
                    HandleAOE,
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
