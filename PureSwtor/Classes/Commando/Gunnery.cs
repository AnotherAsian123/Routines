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

namespace PureSWTor.Classes.Commando
{
    class Gunnery : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Gunnery; }
        }

        public override string Name
        {
            get { return "Commando Gunnery by alltrueist"; }
        }
        

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Armor-piercing Cell"),
                    Spell.Buff("Fortification"),
                    Rest.HandleRest
                    );
            }
        }

        private Composite HandleCoolDowns
        {
            get
            {
                return new LockSelector(
                    Spell.Buff("Tenacity"),
                    MedPack.UseItem(ret => Me.HealthPercent <= 40),
                    Spell.Buff("Reactive Shield", ret => Me.HealthPercent <= 70),
                    Spell.Buff("Adrenaline Rush", ret => Me.HealthPercent <= 30),
                    Spell.Buff("Recharge Cells", ret => Me.ResourceStat <= 40),
                    Spell.Buff("Supercharged Cell", ret => Me.BuffCount("Supercharge") == 10),
                    Spell.Buff("Reserve Powercell", ret => Me.ResourceStat <= 60)
                    );
            }
        }

        private Composite HandleLowCells
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Hammer Shot")
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

                    Spell.Cast("Disabling Shot", ret => Me.CurrentTarget.IsCasting && !LazyRaider.MovementDisabled),

                    Spell.Cast("Boltstorm", ret => Me.HasBuff("Curtain of Fire") && Me.Level >= 57),
                    Spell.Cast("Full Auto", ret => Me.HasBuff("Curtain of Fire") && Me.Level < 57),
                    Spell.Cast("Demolition Round", ret => Me.CurrentTarget.HasDebuff("Gravity Vortex")),
                    Spell.Cast("Electro Net", ret => Me.CurrentTarget.StrongOrGreater()),
                    Spell.Cast("Vortex Bolt"),
                    Spell.Cast("High Impact Bolt", ret => Me.BuffCount("Charged Barrel") == 5),
                    Spell.Cast("Grav Round")
                    );
            }
        }

        private Composite HandleAOE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(2, Distance.MeleeAoE),
                            new LockSelector(
                                Spell.Cast("Tech Override"),
                                Spell.CastOnGround("Mortar Volley", ret => Me.CurrentTarget.Distance > 0.5f),
                                Spell.Cast("Plasma Grenade", ret => Me.ResourceStat >= 90 && Me.HasBuff("Tech Override")),
                                Spell.Cast("Pulse Cannon", ret => Me.CurrentTarget.Distance <= 1f),
                                Spell.CastOnGround("Hail of Bolts", ret => Me.ResourceStat >= 90)
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
                    new Decorator(ret => Me.ResourcePercent() < 60, HandleLowCells),
                    new Decorator(ret => Me.ResourcePercent() >= 60, HandleSingleTarget)
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
