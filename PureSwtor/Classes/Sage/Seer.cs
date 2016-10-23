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

namespace PureSWTor.Classes.Sage
{
    class Seer : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Seer; }
        }

        public override string Name
        {
            get { return "Jedi Sage Seer by Ama"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Force Valor"),
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
                    Spell.HoT("Force Potency", on => Me, 100, ret => HealingManager.ShouldAOE),
                    Spell.Buff("Noble Sacrifice", ret => NeedSacrifice()),
                    Spell.Buff("Force Mend", ret => Me.HealthPercent <= 75)
                    );
            }
        }

        private Composite Healing
        {
            get
            {
                return new LockSelector(
                    //Cleanse if needed
                    Spell.Cleanse("Restoration"),

                    //Emergency Heal (Insta-cast)
                    Spell.Heal("Benevolence", 40, ret => Me.HasBuff("Altruism")),

                    //Aoe Heal
                    Spell.HealGround("Salvation", ret => HealingManager.ShouldAOE),

                    //Single Target Healing
                    Spell.Heal("Healing Trance", 80),
                    Spell.HoT("Force Armor", 90, ret => HealTarget != null && !HealTarget.HasDebuff("Force-imbalance") && !HealTarget.HasBuff("Force Armor")),     

                    //Buff Tank
                    Spell.HoT("Force Armor", on => Tank, 100, ret => Tank != null && Tank.InCombat && !Tank.HasDebuff("Force-imbalance") && !Tank.HasBuff("Force Armor")),
                    
                    //Use Force Bending
                    new Decorator(ret => Me.HasBuff("Conveyance"),
                        new LockSelector(
                            Spell.Heal("Healing Trance", 90),
                            Spell.Heal("Deliverance", 50)
                        )),
                        
                    //Build Force Bending
                    Spell.HoT("Rejuvenate", 80),
                    Spell.HoT("Rejuvenate", on => Tank, 100, ret => Tank != null && Tank.InCombat),

                    //Single Target Healing                  
                    Spell.Heal("Benevolence", 35),
                    Spell.Heal("Deliverance", 80));
            }
        }

        private Composite DPS
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    CloseDistance(Distance.Ranged),
                    
                    //Interrupts
                    Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Stun", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Force Wave", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= 1f && !LazyRaider.MovementDisabled),

                    //Rotation
                    new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE), Spell.CastOnGround("Forcequake")),
                    Spell.Cast("Mind Crush", ret => Me.CurrentTarget.StrongOrGreater()),
                    Spell.Cast("Weaken Mind", ret => Me.CurrentTarget.StrongOrGreater() && !Me.CurrentTarget.HasDebuff("Weaken Mind (Force)")),
                    Spell.Cast("Telekinetic Throw"),
                    Spell.Cast("Project"),
                    Spell.Cast("Disturbance"));
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

        public bool NeedSacrifice()
        {
            if (Me.ForcePercent <= 20 && Me.HealthPercent >= 16)
                return true;
            if (Me.HasBuff("Resplendence") && Me.HealthPercent >= 16 && Me.ForcePercent < 90)
                return true;
            return false;
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