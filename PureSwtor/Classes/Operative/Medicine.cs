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

namespace PureSWTor.Classes.Operative
{
    class Medic : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Medicine; }
        }

        public override string Name
        {
            get { return "Operative Medic by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Coordination"),
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
                    Spell.Buff("Adrenaline Probe", ret => Me.EnergyPercent <= 20),
                    Spell.Buff("Stim Boost", ret => Me.EnergyPercent <= 70 && !Me.HasBuff("Tactical Advantage")),
                    Spell.Buff("Shield Probe", ret => Me.HealthPercent <= 75),
                    Spell.Buff("Evasion", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Escape")
                    );
            }
        }

        private Composite Healing
        {
            get
            {
                return new LockSelector(
                    Spell.Heal("Surgical Probe", 30),
                    Spell.Heal("Recuperative Nanotech", on => Tank, 80, ret => HealingManager.ShouldAOE),
                    Spell.Heal("Kolto Probe", on => Tank, 100, ret => Tank != null && Tank.BuffCount("Kolto Probe") < 2 || Tank.BuffTimeLeft("Kolto Probe") < 6),
                    Spell.Heal("Kolto Infusion", 80, ret => Me.BuffCount("Tactical Advantage") >= 2 && Me.EnergyPercent >= 60 && !HealTarget.HasBuff("Kolto Infusion")),
                    Spell.Heal("Surgical Probe", 80, ret => Me.BuffCount("Tactical Advantage") >= 2),
                    Spell.Heal("Kolto Injection", 80),
                    Spell.Cleanse("Toxin Scan"),
                    Spell.Heal("Kolto Probe", 90, ret => HealTarget.BuffCount("Kolto Probe") <2 || HealTarget.BuffTimeLeft("Kolto Probe") < 6),
                    Spell.Heal("Diagnostic Scan", 90)
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
                    Spell.Cast("Distraction", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= 1f),
                    Spell.Buff("Crouch", ret => !Me.IsInCover() && !Me.IsMoving),
                    Spell.CastOnGround("Orbital Strike", ret => ShouldAOE(3, 1f)),
                    Spell.Cast("Explosive Probe", ret => Me.IsInCover()),
                    Spell.Cast("Hidden Strike", ret => Me.HasBuff("Stealth")),
                    Spell.Cast("Backstab"),
                    Spell.DoT("Corrosive Dart", "", 16000),
                    Spell.Cast("Snipe", ret => Me.IsInCover() && Me.EnergyPercent >= 70),
                    Spell.Cast("Overload Shot", ret => Me.EnergyPercent >= 70),
                    Spell.Cast("Rifle Shot")
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