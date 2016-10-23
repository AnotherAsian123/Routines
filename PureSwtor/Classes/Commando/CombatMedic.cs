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
    class CombatMedic : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.CombatMedic; }
        }

        public override string Name
        {
            get { return "Commando Combat Medic by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Fortification"),
                    Spell.Buff("Combat Support Cell"),
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
                    Spell.Buff("Supercharged Cell", ret => Me.ResourceStat >= 20 && HealTarget != null && HealTarget.HealthPercent <= 80 && Me.BuffCount("Supercharge") == 10),
                    Spell.Buff("Adrenaline Rush", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Reactive Shield", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Reserve Powercell", ret => Me.ResourceStat <= 60),
                    Spell.Buff("Recharge Cells", ret => Me.ResourceStat <= 50),
                    Spell.Cast("Tech Override", ret => Tank != null && Tank.HealthPercent <= 50)
                    );
            }
        }

        private Composite Healing
        {
            get
            {
                return new LockSelector(
                    //Dispel
                    Spell.Cleanse("Field Aid"),

                    //Keep Trauma Probe on Tank
                    Spell.Heal("Trauma Probe", on => HealTarget, 100, ret => HealTarget != null && HealTarget.BuffCount("Trauma Probe") <= 1),

                    //Kolto Bomb to keep up buff
                    new Decorator(ctx => Tank != null,
                        Spell.CastOnGround("Kolto Bomb", on => Tank.Position, ret => !Tank.HasBuff("Kolto Residue"))),

                    //Single Target Healing
                    Spell.Heal("Bacta Infusion", 80),
                    Spell.Heal("Advanced Medical Probe", 80, ret => Me.BuffCount("Field Triage") == 3),
                    Spell.Heal("Medical Probe", 75),

                    //To keep Supercharge buff up; filler heal
                    Spell.Heal("Med Shot", on => Tank, 100, ret => Tank != null && Me.InCombat)
                    );
            }
        }

        private Composite HandleSupercharge
        {
            get
            {
                return new Decorator(ret => Me.HasBuff("Supercharged Cell"),
                    new LockSelector(
                        new Decorator(ctx => Tank != null,
                        Spell.CastOnGround("Kolto Bomb", on => Tank.Position, ret => !Tank.HasBuff("Kolto Residue"))),
                        Spell.Heal("Bacta Infusion", 60),
                        Spell.Heal("Advanced Medical Probe", 85)
                    ));
            }
        }

        private Composite DPS
        {
            get
            {
                return new LockSelector(
                    //Movement
                    CloseDistance(Distance.Ranged),

                    Spell.Cast("Disabling Shot", ret => Me.CurrentTarget.IsCasting),
                    Spell.Cast("High Impact Bolt"),
                    Spell.Cast("Full Auto"),
                    Spell.Cast("Charged Bolts", ret => Me.ResourceStat >= 70),
                    Spell.Cast("Hammer Shot")
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
                    HandleSupercharge,
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