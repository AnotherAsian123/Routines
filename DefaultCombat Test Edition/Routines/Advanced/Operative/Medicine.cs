// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using DefaultCombat.Core;
using DefaultCombat.Helpers;

namespace DefaultCombat.Routines
{
    public class Medicine : RotationBase
    {
        public override string Name
        {
            get { return "Operative Medicine"; }
        }

        public override Composite Buffs
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Coordination"),
                    Spell.Cast("Stealth", ret => !Me.InCombat && !Me.HasBuff("Coordination"))
                    );
            }
        }

        public override Composite Cooldowns
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Adrenaline Probe", ret => Me.EnergyPercent <= 20),
                    Spell.Buff("Stim Boost", ret => Me.EnergyPercent <= 70 && !Me.HasBuff("Tactical Advantage")),
                    Spell.Buff("Shield Probe", ret => Me.HealthPercent <= 75),
                    Spell.Buff("Evasion", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Escape")
                    );
            }
        }

        //DPS 
        public override Composite SingleTarget
        {
            get
            {
                return new PrioritySelector(
                    Spell.Cast("Distraction", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= 1f),
                    Spell.Cast("Snipe", ret => Me.IsInCover() && Me.EnergyPercent >= 70),
                    Spell.Cast("Overload Shot", ret => Me.EnergyPercent >= 70),
                    Spell.Cast("Rifle Shot")
                    );
            }
        }

        //Healing 
        public override Composite AreaOfEffect
        {
            get
            {
                return new PrioritySelector(
                    Spell.Heal("Surgical Probe", 30),
                    Spell.Heal("Recuperative Nanotech", on => Tank, 80, ret => Targeting.ShouldAoeHeal),
                    Spell.Heal("Kolto Probe", on => Tank, 100, ret => Tank != null && Tank.BuffCount("Kolto Probe") < 2 || Tank.BuffTimeLeft("Kolto Probe") < 6),
                    Spell.Heal("Kolto Infusion", 80, ret => Me.BuffCount("Tactical Advantage") >= 2 && Me.EnergyPercent >= 60 && !HealTarget.HasBuff("Kolto Infusion")),
                    Spell.Heal("Surgical Probe", 80, ret => Me.BuffCount("Tactical Advantage") >= 2),
                    Spell.Heal("Kolto Injection", 80),
                    Spell.Cleanse("Toxin Scan"),
                    Spell.Heal("Kolto Probe", 90, ret => HealTarget.BuffCount("Kolto Probe") < 2 || HealTarget.BuffTimeLeft("Kolto Probe") < 6),
                    Spell.Heal("Diagnostic Scan", 90)
                    );
            }
        }

        public static bool tankSlowMedpac()
        {
            if (Targeting.Tank.BuffCount("Kolto Probe") < 2)
                return true;
            if (Targeting.Tank.BuffTimeLeft("Kolto Probe") < 3)
                return true;
            return false;
        }

        public static bool targetSlowMedpac()
        {
            if (Targeting.HealTarget.BuffCount("Kolto Probe") < 2)
                return true;
            if (Targeting.HealTarget.BuffTimeLeft("Kolto Probe") < 3)
                return true;
            return false;
        }

        public static bool emergencyMedpac()
        {
            if (Targeting.HealTarget.BuffTimeLeft("Kolto Probe") > 6)
                return false;
            if (Targeting.HealTarget.BuffCount("Kolto Probe") < 2)
                return false;
            if (Me.BuffCount("Coordination") < 2)
                return false;
            return true;
        }
    }
}