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

namespace PureSWTor.Classes.Juggernaut
{
    class Rage : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Rage; }
        }

        public override string Name
        {
            get { return "Juggernaut Rage"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Shii-Cho Form"),
                    Spell.Buff("Unnatural Might"),
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
                    Spell.Buff("Unleash", ret => Me.IsStunned),
                    MedPack.UseItem(ret => Me.HealthPercent <= 30),
                    Spell.Buff("Saber Reflect", ret => Me.HealthPercent <= 90),
                    Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Endure Pain", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Enrage", ret => Me.ActionPoints <= 6)
                    );
            }
        }

        private Composite HandleSingleTarget
        {
            get
            {
                return new LockSelector(
                    //Move To Range
                    CloseDistance(Distance.Melee),

                    Spell.Cast("Vicious Throw", ret => Me.CurrentTarget.HealthPercent <= 30),
                    Spell.Cast("Smash", ret => Me.BuffCount("Shockwave") == 3 && Me.HasBuff("Dominate") && Me.CurrentTarget.Distance <= 0.5f),
                    Spell.Cast("Force Choke", ret => Me.BuffCount("Shockwave") < 4),
                    Spell.Cast("Force Crush", ret => Me.BuffCount("Shockwave") < 4),
                    Spell.Cast("Obliterate", ret => Me.HasBuff("Shockwave")),
                    Spell.Cast("Force Scream", ret => Me.HasBuff("Battle Cry") || Me.ActionPoints >= 5),
                    Spell.Cast("Ravage"),
                    Spell.Cast("Vicious Slash"),
                    Spell.Cast("Sundering Assault", ret => Me.CurrentTarget.DebuffCount("Armor Reduced") <= 5),
                    Spell.Cast("Assault")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.Cast("Smash"),
                        Spell.Cast("Sweeping Slash")
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
                    HandleSingleTarget
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
