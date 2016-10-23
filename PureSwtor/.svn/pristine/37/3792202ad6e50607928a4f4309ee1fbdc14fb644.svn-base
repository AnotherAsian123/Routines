using System;
using System.Collections.Generic;
using System.Linq;
using Buddy.BehaviorTree;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using Buddy.CommonBot;
using PureSWTor.Helpers;
using PureSWTor.Core;
using PureSWTor.Managers;

using Action = Buddy.BehaviorTree.Action;
using Distance = PureSWTor.Helpers.Global.Distance;

namespace PureSWTor.Classes.Sniper
{
    class Marksmanship : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Marksmanship; }
        }

        public override string Name
        {
            get { return "Sniper Marksmanship by alltrueist - improved by JNP"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Coordination"),
                    Rest.HandleRest
                    //Scavenge.ScavengeCorpse
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Escape", ret => Me.IsStunned),
                    Spell.Buff("Shield Probe", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Evasion", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Adrenaline Probe", ret => Me.EnergyPercent <= 40),
                    Spell.Buff("Sniper Volley", ret => Me.EnergyPercent <= 60),
                    Spell.Buff("Entrench", ret => Me.CurrentTarget.StrongOrGreater() && Me.IsInCover()),
                    Spell.Buff("Laze Target"),
                    Spell.Buff("Target Acquired")
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
                    Spell.Buff("Crouch", ret => !Me.IsInCover() && !Me.IsMoving),
                    Spell.Cast("Followthrough"),
                    Spell.Cast("Penetrating Blasts", ret => Me.IsInCover() && Me.Level >= 26),
                    Spell.Cast("Series of Shots", ret => Me.IsInCover() && Me.Level < 26),
                    Spell.DoT("Corrosive Dart", "", 15000, ret => Me.CurrentTarget.StrongOrGreater()),
                    Spell.Cast("Ambush", ret => Me.IsInCover() && Me.BuffCount("Zeroing Shots") == 2),
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
                        Spell.Cast("Fragmentation Grenade"),
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
