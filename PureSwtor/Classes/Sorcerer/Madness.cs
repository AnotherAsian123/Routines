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

namespace PureSWTor.Classes.Sorcerer
{
    public class Madness : RotationBase
    {

        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Madness; }
        }

        public override string Name
        {
            get { return "Sorcerer Madness by Distiny"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Mark of Power"),
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
                    Spell.Buff("Force Barrier", ret => Me.HealthPercent <= 20),
                    Spell.Buff("Static Barrier", ret => !Me.HasBuff("Deionized")),
                    Spell.Buff("Unnatural Preservation", ret => Me.HealthPercent <= 70),
                    MedPack.UseItem(ret => Me.HealthPercent <= 25),
                    Spell.Buff("Cloud Mind"),
                    Spell.Buff("Polarity Shift"),
                    Spell.Buff("Recklessness")
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
                    Spell.Cast("Jolt", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.Cast("Electrocute", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),
                    Spell.CastOnGround("Death Field"),
                    Spell.Cast("Affliction", ret => !Me.CurrentTarget.HasDebuff("Affliction")),
                    Spell.Cast("Creeping Terror", ret => !Me.CurrentTarget.HasDebuff("Creeping Terror")),
                    Spell.Cast("Demolish"),
                    Spell.Cast("Force Leech"),
                    Spell.Cast("Force Lightning"),
                    Spell.Cast("Shock")
                     );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                        Spell.CastOnGround("Death Field"),
                        Spell.MultiDot("Affliction", "Affliction (Force)"),
                        Spell.CastOnGround("Force Storm")

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
