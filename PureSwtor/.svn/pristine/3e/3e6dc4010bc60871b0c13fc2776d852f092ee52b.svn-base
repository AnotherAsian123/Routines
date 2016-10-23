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
    class Lethality : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Virulence; }
        }

        public override string Name
        {
            get { return "Sniper Lethality by Ama"; }
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
                    Spell.Cast("Rifle Shot")
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
                    Spell.Cast("Shatter Shot", ret => !Me.CurrentTarget.HasDebuff("Armor Reduced")),
                    Spell.DoT("Corrosive Grenade", "", 18000),
                    Spell.DoT("Corrosive Dart", "", 12000),
                    Spell.Cast("Weakening Blast"),
                    Spell.Cast("Takedown", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Cull"),
                    Spell.Buff("Crouch", ret => !Me.IsInCover() && !Me.IsMoving),
                    Spell.Cast("Series of Shots", ret => Me.IsInCover()),
                    Spell.Cast("Snipe", ret => Me.IsInCover()),
                    Spell.Cast("Overload Shot")
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
                        Spell.Cast("Fragmentation Grenade"),
                        Spell.DoT("Corrosive Grenade", "", 18000),
                        Spell.CastOnGround("Suppressive Fire")
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
