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

namespace PureSWTor.Classes.Sniper
{
    class Engineering : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Engineering; }
        }

        public override string Name
        {
            get { return "Sniper Engineering by Ama"; }
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
                    Spell.Buff("Escape"),
                    Spell.Buff("Shield Probe", ret => Me.HealthPercent <= 50),
                    Spell.Buff("Evasion", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Adrenaline Probe", ret => Me.EnergyPercent <= 50),
                    Spell.Buff("Laze Target"),
                    Spell.Cast("Target Acquired")
                    );
            }
        }

        private Composite HandleLowEnergy
        {
            get
            {
                return new LockSelector(
                    Spell.CastOnGround("Plasma Probe", ret => Me.HasBuff("Energy Overrides")),
                    Spell.Cast("Explosive Probe", ret => Me.IsInCover() && Me.HasBuff("Energy Overrides")),
                    Spell.Cast("Overload Shot", ret => Me.HasBuff("Calculated Pursuit")),
                    Spell.Cast("Rifle Shot")
                    );
            }
        }

        private Composite HandleRailgun
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Crouch", ret => !Me.IsInCover() && !Me.IsMoving),
                    Spell.Cast("Series of Shots", ret => Me.IsInCover()),
                    Spell.Cast("Snipe", ret => Me.IsInCover()),
                    Spell.Cast("Overload Shot", ret => !Me.IsInCover())
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

                    //Rotation
                    Spell.Cast("Distraction", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Buff("Crouch", ret => !Me.IsInCover() && !Me.IsMoving),
                    Spell.Cast("Shatter Shot", ret => !Me.CurrentTarget.HasDebuff("Armor Reduced")),
                    Spell.Cast("Series of Shots", ret => Me.IsInCover()),
                    Spell.CastOnGround("Plasma Probe"),
                    Spell.DoT("Interrogation Probe", "", 15000),
                    Spell.DoT("Corrosive Dart", "", 12000),
                    Spell.Cast("Explosive Probe", ret => Me.IsInCover()),
                    Spell.Cast("Takedown", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Snipe", ret => Me.IsInCover()),
                    Spell.Cast("Overload Shot", ret => !Me.IsInCover())
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.CastOnGround("Orbital Strike"),
                        Spell.CastOnGround("Plasma Probe"),
                        Spell.Cast("Fragmentation Grenade")
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
