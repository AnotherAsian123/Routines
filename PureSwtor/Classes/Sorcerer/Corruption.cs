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
    class Corruption : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Corruption; }
        }

        public override string Name
        {
            get { return "Sorcerer Corruption"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    PVERotation,
                    Spell.Buff("Mark of Power"),
                    Rest.HandleRest,
                    Scavenge.ScavengeCorpse
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
                    
                    Spell.Buff("Recklessness", ret => HealingManager.ShouldAOE),
                    Spell.Buff("Unnatural Preservation", ret => Me.HealthPercent < 50)
                    );
            }
        }

        private Composite Healing
        {
            get
            {
                return new LockSelector(
                    //BuffLog.Instance.LogTargetBuffs,

                    //Cleanse if needed
                    Spell.Cleanse("Purge"),

                    //Emergency Heal (Insta-cast)
                    Spell.Heal("Dark Heal", 40, ret => Me.HasBuff("Dark Concentration")),

                    //Single Target Healing
                    Spell.Heal("Roaming Mend", 85),
                    Spell.Heal("Innervate", 80),
                    Spell.HoT("Static Barrier", 75, ret => HealTarget != null && !HealTarget.HasDebuff("Deionized")),     

                    //Buff Tank
                    Spell.HoT("Static Barrier", on => Tank, 100, ret => Tank != null && Tank.InCombat && !Tank.HasDebuff("Deionized")),
                    
                    //Use Force Bending
                    new Decorator(ret => Me.HasBuff("Force Bending"),
                        new LockSelector(
                            Spell.Heal("Innervate", 90),
                            Spell.Heal("Roaming Mend", 50)
                        )),
                        
                    //Build Force Bending
                    Spell.HoT("Resurgence", 80),
                    Spell.HoT("Resurgence", on => Tank, 100, ret => Tank != null && Tank.InCombat),
                    
                    //Force Regen
                    Spell.Cast("Consumption", on => Me, ret => Me.HasBuff("Force Surge") && Me.HealthPercent > 15 && Me.ForcePercent < 55),

                    //Aoe Heal
                    Spell.HealGround("Revivification"),

                    //Single Target Healing                  
                    Spell.Heal("Dark Heal", 35),
                    Spell.Heal("Dark Infusion", 80));
            }
        }

        private Composite DPS
        {
            get
            {
                    return new LockSelector(
                        //Movement
                        CloseDistance(Distance.Ranged),

                        //Rotation
                        Spell.Cast("Jolt", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                        Spell.Cast("Crushing Darkness"),
                        Spell.Cast("Affliction", ret => !Me.CurrentTarget.HasDebuff("Affliction")),
                        Spell.Cast("Lightning Strike"),
                        Spell.Cast("Shock"));
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
