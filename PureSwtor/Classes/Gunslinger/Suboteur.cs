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
    class Saboteur : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Saboteur; }
        }

        public override string Name
        {
            get { return "Gunslinger Saboteur by Ama"; }
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
                    Spell.Cast("Incendiary Grenade", ret => Me.HasBuff("Dealer's Discount")),
                    Spell.Cast("Sabotage Charge", ret => Me.IsInCover()),
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

                    Spell.Cast("Crouch", ret => !Me.IsInCover()),
                    Spell.Cast("Flourish Shot", ret => !Me.CurrentTarget.HasDebuff("Armor Reduced")),
                    Spell.Cast("Speed Shot", ret => Me.IsInCover()),
                    Spell.Cast("Incendiary Grenade"),
                    Spell.DoT("Shock Charge", "", 15000),
                    Spell.DoT("Vital Shot", "", 12000),
                    Spell.Cast("Sabotage Charge", ret => Me.IsInCover()),
                    Spell.Cast("Quickdraw", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Aimed Shot", ret => Me.IsInCover()),
                    Spell.Cast("Charged Burst", ret => Me.IsInCover()),
                    Spell.Cast("Quick Shot", ret => !Me.IsInCover())
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
                        Spell.Cast("Incendiary Grenade"),
                        Spell.Cast("Thermal Grenade")
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