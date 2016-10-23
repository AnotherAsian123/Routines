using System;
using System.Collections.Generic;
using System.Linq;
using Buddy.BehaviorTree;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using PureSWTor.Helpers;
using PureSWTor.Core;
using PureSWTor.Managers;
using Buddy.Common;

using Action = Buddy.BehaviorTree.Action;
using Distance = PureSWTor.Helpers.Global.Distance;

namespace PureSWTor.Classes.Juggernaut
{
    class Immortal : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Immortal; }
        }

        public override string Name
        {
            get { return "Juggernaut Immortal by alltrueist"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Soresu Form"),
                    Spell.Buff("Unnatural Might"),
                    Spell.Cast("Guard", ret => Me.Companion, ret => Me.Companion != null && !Me.Companion.IsDead && !Me.Companion.HasBuff("Guard")),
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
                    Spell.Buff("Invincible", ret => Me.HealthPercent <= 50),
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
                            //*Ranged Attacks, remove the // in order to activate them. Otherwise, you use to start combat
                            Spell.Cast("Saber Throw", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 0.5f && Me.CurrentTarget.Distance <= 3f),
                            Spell.Cast("Force Charge", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),
                            
                            //Move To Range
                            CloseDistance(Distance.Melee),
                            
                            //Interupts
                            Spell.Cast("Disruption", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                            Spell.Cast("Itimidating Roar", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                            Spell.Cast("Force Choke", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                            //Rotation
                            Spell.Cast("Sundering Assault", ret => !Me.CurrentTarget.HasDebuff("Armor Reduced")),
                            Spell.Cast("Force Scream"),
                            Spell.Cast("Crushing Blow"),
                            Spell.Cast("Backhand", ret => !Me.CurrentTarget.IsStunned),
                            Spell.Cast("Vicious Throw", ret => Me.CurrentTarget.HealthPercent <= 30),
                            Spell.Cast("Ravage"),
                            Spell.Cast("Vicious Slash", ret => Me.ActionPoints >= 11),
                            Spell.Cast("Assault"),
                            Spell.Cast("Retaliation")
                            );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldPBAOE(3, Global.Distance.MeleeAoE),
                    new LockSelector(
                        Spell.Cast("Crushing Blow", ret => Me.CurrentTarget.HasDebuff("Armor Reduced")),
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