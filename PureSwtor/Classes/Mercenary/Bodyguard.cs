using System;
using System.Collections.Generic;
using System.Linq;
using Buddy.BehaviorTree;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using Buddy.CommonBot;
using PureSWTor.Helpers;
using PureSWTor.Core;
using PureSWTor.Managers;

using Action = Buddy.BehaviorTree.Action;
using Distance = PureSWTor.Helpers.Global.Distance;

namespace PureSWTor.Classes.Mercenary
{
    class Bodyguard : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Bodyguard; }
        }

        public override string Name
        {
            get { return "Mercenary Bodyguard by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    PVERotation,
                    Spell.Buff("Combat Support Cylinder"),
                    Spell.Buff("Hunter's Boon"),
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
                    MedPack.UseItem(ret => Me.HealthPercent <= 30),
                    Spell.Buff("Determination", ret => Me.IsStunned),
                    Spell.Buff("Supercharged Gas", ret => Me.BuffCount("Supercharge") == 30 
                        && Me.ResourcePercent() <= 60
                        && HealTarget != null && HealTarget.HealthPercent <= 80),
                    Spell.Buff("Vent Heat", ret => Me.ResourcePercent() >= 70),
                    Spell.Buff("Energy Shield", ret => Me.HealthPercent <= 40),
                    Spell.Buff("Kolto Overload", ret => Me.HealthPercent <= 30)
                    );
            }
        }

        private Composite Healing
        {
            get
            {
                return new LockSelector(
                    //Urgent Dispelling!
                    //BuffLog.Instance.LogTargetBuffs,
                    Spell.Cleanse("Cure"),

                    //Buff Kolto Residue
                    new Decorator(ctx => HealTarget != null,
                        Spell.CastOnGround("Kolto Missile", on => HealTarget.Position, ret => HealTarget.HealthPercent < 60 && !HealTarget.HasBuff("Kolto Residue"))),
                    
                    //Free, so use it!
                    Spell.Heal("Emergency Scan", 80),

                    //Important Buffs to take advantage of
                    new Decorator(ctx => Tank != null,
                    Spell.CastOnGround("Kolto Missile", on => Tank.Position, ret => Me.HasBuff("Supercharged Gas") && Me.InCombat && !Tank.HasBuff("Charge Screen"))),
                    Spell.Heal("Rapid Scan", 80, ret => Me.HasBuff("Critical Efficiency")),
                    Spell.HealGround("Kolto Missile", ret => Me.HasBuff("Supercharged Gas")),                    
                    Spell.Heal("Healing Scan", 80, ret => Me.HasBuff("Supercharged Gas")),                    

                    //Buff Tank
                    Spell.Heal("Kolto Shell", on => HealTarget, 100, ret => HealTarget != null && Me.InCombat && !HealTarget.HasBuff("Kolto Shell")),

                    //Single Target Priority
                    Spell.Heal("Healing Scan", 75),
                    Spell.Heal("Rapid Scan", 75),

                    //Filler
                    Spell.Heal("Rapid Shots", 95, ret => HealTarget != null && HealTarget.Name != Me.Name),
                    Spell.Heal("Rapid Shots", on => Tank, 100, ret => Tank != null && Me.InCombat)
                    );
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
                    Spell.Cast("Disabling Shot", ret => Me.CurrentTarget.IsCasting),
                    Spell.Cast("Unload"),
                    Spell.Cast("Power Shot", ret => Me.ResourceStat >= 70),
                    Spell.Cast("Rapid Shots")
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
                    HealingManager.AcquireHealTargets,
                    Spell.WaitForCast(),
                    HandleCoolDowns,
                    Healing,
                    new Decorator(ret => !Me.IsInParty(), DPS)
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
