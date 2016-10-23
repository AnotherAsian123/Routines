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
    class Concealment : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Concealment; }
        }

        public override string Name
        {
            get { return "Operative Concealment by aquintus"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Coordination"),
                    //Scavenge.ScavengeCorpse,
                    Rest.HandleRest,
                    Spell.Cast("Stealth", ret => !Me.InCombat && !Me.HasBuff("Coordination"))
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
                    Spell.Cast("Backstab", ret => Me.IsBehind(Me.CurrentTarget))
                    );
            }
        }

        private Composite HandleLowEnergy
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Backstab", ret => (Me.IsBehind(Me.CurrentTarget) || Me.HasBuff("Jarring Strike"))),
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
                    Spell.Cast("Backstab", ret => Me.IsBehind(Me.CurrentTarget)),
                    Spell.Cast("Crippling Slice"),
                    Spell.DoT("Corrosive Dart", "", 12000),
                    Spell.Cast("Veiled Strike", ret => Me.Level >= 41),
                    Spell.Cast("Shiv", ret => Me.Level < 41),
                    Spell.Cast("Laceration", ret => Me.HasBuff("Tactical Advantage")),
                    Spell.Cast("Overload Shot", ret => Me.EnergyPercent >= 87)
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
