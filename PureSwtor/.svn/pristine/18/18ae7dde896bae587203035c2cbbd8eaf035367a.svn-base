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

namespace PureSWTor.Classes.Gunslinger
{
    class DirtyFighting : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.DirtyFighting; }
        }

        public override string Name
        {
            get { return "Gunslinger Dirty Fighting by Ama"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Lucky Shots"),
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
                    Spell.Buff("Escape"),
                    Spell.Buff("Defense Screen", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Dodge", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Cool Head", ret => Me.EnergyPercent <= 50),
                    Spell.Buff("Smuggler's Luck"),
                    Spell.Buff("Illegal Mods")
                    );
            }
        }

        private Composite HandleLowEnergy
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Flurry of Bolts")
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //Movement
                    CloseDistance(Distance.Ranged),

                    Spell.Cast("Charged Burst", ret => Me.HasBuff("Smuggler's Luck")),
                    Spell.Cast("Flurry of Bolts", ret => Me.EnergyPercent < 40),
                    Spell.DoT("Flourish Shot", "Armor Reduced"),
                    Spell.Cast("Sabotage Charge", ret => Me.IsInCover()),
                    Spell.DoT("Shrap Bomb", "", 18000),
                    Spell.DoT("Vital Shot", "", 12000),
                    Spell.Cast("Hemorrhaging Blast"),
                    Spell.Cast("Quickdraw", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Wounding Shots"),
                    Spell.Cast("Crouch", ret => !Me.IsInCover()),
                    Spell.Cast("Speed Shot", ret => Me.IsInCover()),
                    Spell.Cast("Aimed Shot", ret => Me.IsInCover()),
                    Spell.Cast("Quick Shot", ret => !Me.IsInCover()),
                    Spell.Cast("Flurry of Bolts")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.CastOnGround("XS Freighter Flyby"),
                        Spell.Cast("Thermal Grenade"),
                        Spell.DoT("Shrap Bomb", "", 18000),
                        Spell.CastOnGround("Sweeping Gunfire")
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
                    new Decorator(ret => Me.EnergyPercent < 60, HandleLowEnergy),
                    new Decorator(ret => Me.EnergyPercent >= 60, HandleSingleTarget)
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
