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
    class Lethality : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Lethality; }
        }

        public override string Name
        {
            get { return "Operative Lethality by aquintus"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Coordination"),
                    Rest.HandleRest,
                    //Scavenge.ScavengeCorpse,
                    Spell.Buff("Stealth", ret => !Rest.KeepResting() && !LazyRaider.MovementDisabled)
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Adrenaline Probe", ret => Me.EnergyPercent <= 45),
                    MedPack.UseItem(ret => Me.HealthPercent <= 20),
                    Spell.Buff("Stim Boost", ret => Me.BuffCount("Tactical Advantage") <= 2),
                    Spell.Buff("Shield Probe", ret => Me.HealthPercent <= 75),
                    Spell.Buff("Evasion", ret => Me.HealthPercent <= 50)
                    );
            }
        }

        private Composite HandleStealth
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Hidden Strike", ret => Me.IsBehind(Me.CurrentTarget))
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

        private Composite HandleAoE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                    Spell.CastOnGround("Orbital Strike"),
                    Spell.DoT("Corrosive Grenade", "", 18000),
                    Spell.Cast("Fragmentation Grenade"),
                    Spell.Cast("Carbine Burst", ret => Me.HasBuff("Tactical Advantage"))
                    ));
            }
        }
        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    CloseDistance(Distance.Melee),

                    //Rotation
                    Spell.Cast("Hidden Strike", ret => !Me.HasBuff("Tactical Advantage") && Me.IsBehind(Me.CurrentTarget) && Me.IsStealthed),
                    Spell.DoT("Corrosive Dart", "", 15000),
                    Spell.DoT("Corrosive Grenade", "", 18000),
                    Spell.Cast("Weakening Blast"),
                    Spell.Cast("Cull", ret => Me.HasBuff("Tactical Advantage")),
                    Spell.Cast("Shiv", ret => Me.BuffCount("Tactical Advantage") < 2 || Me.BuffTimeLeft("Tactical Advantage") < 6),
                    Spell.Cast("Backstab", ret => Me.IsBehind(Me.CurrentTarget)),
                    Spell.Cast("Overload Shot")
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
                    Spell.WaitForCast(),
                    HandleCoolDowns,
                    HandleAoE,
                    new Decorator(ret => Me.IsStealthed, HandleStealth),
                    new Decorator(ret => Me.EnergyPercent < 60, HandleLowEnergy),
                    new Decorator(ret => Me.EnergyPercent >= 60 && !Me.IsStealthed, HandleSingleTarget)
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