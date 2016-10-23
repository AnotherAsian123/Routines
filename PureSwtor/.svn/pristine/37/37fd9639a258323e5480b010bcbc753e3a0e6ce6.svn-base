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

namespace PureSWTor.Classes.Scoundrel
{
    class Sawbones : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Sawbones; }
        }

        public override string Name
        {
            get { return "Scoundrel Sawbones by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Lucky Shots"),
                    Rest.HandleRest,
                    //Scavenge.ScavengeCorpse,
                    Spell.Buff("Stealth", ret => !Rest.KeepResting() && !Rest.NeedRest() && !LazyRaider.MovementDisabled)
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Cool Head", ret => Me.EnergyPercent <= 20),
                    Spell.Buff("Pugnacity", ret => Me.EnergyPercent <= 70 && Me.BuffCount("Upper Hand") < 3),
                    Spell.Buff("Defense Screen", ret => Me.HealthPercent <= 75),
                    Spell.Buff("Dodge", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Escape")
                    );
            }
        }

        private Composite Healing
        {
            get
            {
                return new LockSelector(
                    Spell.Heal("Kolto Cloud", on => Tank, 80, ret => HealingManager.ShouldAOE),
                    Spell.Heal("Slow-release Medpac", on => Tank, 100, ret => Tank != null && Tank.BuffCount("Slow-release Medpac") < 2),
                    Spell.Heal("Kolto Pack", 80, ret => Me.BuffCount("Upper Hand") >= 2 && Me.EnergyPercent >= 60 && !HealTarget.HasBuff("Kolto Pack")),
                    Spell.Heal("Emergency Medpac", 90, ret => Me.BuffCount("Upper Hand") >= 2 && HealTarget.BuffCount("Slow-release Medpac") == 2),
                    Spell.Heal("Underworld Medicine", 80),
                    Spell.Cleanse("Triage"),
                    Spell.Heal("Slow-release Medpac", 90, ret => HealTarget.BuffCount("Slow-release Medpac") < 2),
                    Spell.Heal("Diagnostic Scan", on => Tank, 100, ret => Me.IsInParty())
                    );
            }
        }

        private Composite DPS
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("Distraction", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= 1f),
                    Spell.Cast("Fragmentation Grenade", ret => ShouldAOE(3, Distance.Melee)),
                    Spell.Cast("Blaster Volley", ret => ShouldAOE(3, Distance.MeleeAoE) && Me.CurrentTarget.Distance <= 1f && Me.HasBuff("Upper Hand")),
                    Spell.Cast("Back Blast", ret => Me.IsBehind(Me.CurrentTarget)),
                    Spell.Cast("Blaster Whip"),
                    Spell.DoT("Vital Shot", "Bleeding (Vital Shot)"),
                    Spell.Cast("Quick Shot", ret => Me.EnergyPercent >= 70),
                    Spell.Cast("Flurry of Bolts")
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