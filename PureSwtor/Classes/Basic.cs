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

namespace PureSWTor.Classes
{
    class Basic : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.None; }
        }

        public override string Name
        {
            get { return "Basic by Ama, pindleskin"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Sprint"),
                    new Switch<CharacterClass>(ret => Me.Class,
                       new PrioritySelector(),
                       new SwitchArgument<CharacterClass>(CharacterClass.Consular,
                           new PrioritySelector(
                               Spell.Buff("Force Valor")
                               )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Knight,
                           new PrioritySelector(
                               Spell.Buff("Force Might"),
                               Spell.Buff("Shii-Cho Form")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Smuggler,
                           new PrioritySelector(
                               Spell.Buff("Lucky Shots")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Trooper,
                           new PrioritySelector(
                               Spell.Buff("Fortification")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.BountyHunter,
                           new PrioritySelector(
                               Spell.Buff("Hunter's Boon")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Agent,
                           new PrioritySelector(
                               Spell.Buff("Coordination")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Inquisitor,
                           new PrioritySelector(
                               Spell.Buff("Mark of Power")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Warrior,
                           new PrioritySelector(
                               Spell.Buff("Unnatural Might"),
                               Spell.Buff("Shii-Cho Form")
                           ))),                
                       Rest.HandleRest);
            }
        }

        public override Composite PVERotation
        {
            get
            {
                return new PrioritySelector(
                    Spell.WaitForCast(),
                    PreCombat,
                    MedPack.UseItem(ret => Me.HealthPercent <= 40),
                    new Switch<CharacterClass>(ret => Me.Class,
                        new PrioritySelector(),
                        new SwitchArgument<CharacterClass>(CharacterClass.Consular,
                            new LockSelector(
                                CloseDistance(Distance.Melee),
                                Spell.Cast("Force Wave", ret => Me.CurrentTarget.Distance <= Distance.MeleeAoE && ShouldAOE(3, Distance.MeleeAoE)),
                                Spell.Cast("Telekinetic Throw"),
                                Spell.Cast("Project", ret => Me.Force > 75),
                                Spell.Cast("Double Strike", ret => Me.Force > 70),
                                Spell.Cast("Saber Strike")
                            )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Knight,
                            new LockSelector(
                                Spell.Cast("Force Leap", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance > 1f && Me.CurrentTarget.Distance <= 3f),
                                CloseDistance(Distance.Melee),
                                Spell.Cast("Saber Ward", ret => Me.HealthPercent <= 70),
                                Spell.Cast("Master Strike"),
                                Spell.Cast("Force Sweep", ret => Me.CurrentTarget.Distance <= Distance.MeleeAoE && ShouldAOE(3, Distance.MeleeAoE)),
                                Spell.Cast("Blade Storm"),
                                Spell.Cast("Riposte"),
                                Spell.Cast("Slash", ret => Me.ActionPoints >= 7),
                                Spell.Cast("Strike")
                                )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Smuggler,
                           new LockSelector(
                               CloseDistance(Distance.Ranged),
                               Spell.Cast("Thermal Grenade", ret => Me.CurrentTarget.Distance <= Distance.Ranged && ShouldAOE(3, Distance.MeleeAoE)),
                               Spell.DoT("Vital Shot", "", 15000),
                               Spell.Cast("Blaster Whip", ret => Me.CurrentTarget.Distance <= Distance.Melee),
                               Spell.Cast("Sabotage Charge"),
                               Spell.Cast("Charged Burst"),
                               Spell.Cast("Flurry of Bolts")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Trooper,
                           new LockSelector(
                               CloseDistance(Distance.Ranged),
                               Spell.Cast("Sticky Grenade"),
                               Spell.CastOnGround("Mortar Volley", ret => Me.CurrentTarget.Distance > .5f),
                               Spell.Cast("High Impact Bolt"),
                               Spell.Cast("Recharge Cells", ret => Me.ResourcePercent() <= 50),
                               Spell.Cast("Stockstrike", ret => Me.CurrentTarget.Distance <= .4f),
                               Spell.Cast("Pulse Cannon", ret => Me.CurrentTarget.Distance <= 1f),
                               Spell.Cast("Ion Pulse", ret => Me.ResourcePercent() >= 50),
                               Spell.Cast("Hammer Shot")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.BountyHunter,
                           new LockSelector(
                               CloseDistance(Distance.Ranged),
                               Spell.Cast("Explosive Dart"),
                               Spell.CastOnGround("Death from Above", ret => Me.CurrentTarget.Distance > .5f),
                               Spell.Cast("Rail Shot"),
                               Spell.Cast("Vent Heat", ret => Me.ResourcePercent() >= 50),
                               Spell.Cast("Rocket Punch", ret => Me.CurrentTarget.Distance <= .4f),
                               Spell.Cast("Flame Thrower", ret => Me.CurrentTarget.Distance <= 1f),
                               Spell.Cast("Flame Burst", ret => Me.ResourcePercent() <= 50),
                               Spell.Cast("Rapid Shots")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Agent,
                           new LockSelector(
                               CloseDistance(Distance.Ranged),
                               Spell.Cast("Fragmentation Grenade", ret => Me.CurrentTarget.Distance <= Distance.Ranged && ShouldAOE(3, Distance.MeleeAoE)),
                               Spell.DoT("Corrosive Dart", "", 15000),
                               Spell.Cast("Shiv", ret => Me.CurrentTarget.Distance <= Distance.Melee),
                               Spell.Cast("Explosive Probe"),
                               Spell.Cast("Snipe"),
                               Spell.Cast("Rifle Shot")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Inquisitor,
                           new LockSelector(
                                CloseDistance(Distance.Melee),
                                Spell.Cast("Overload", ret => Me.CurrentTarget.Distance <= Distance.MeleeAoE && ShouldAOE(3, Distance.MeleeAoE)),
                                Spell.Cast("Force Lightning"),
                                Spell.Cast("Shock", ret => Me.Force > 75),
                                Spell.Cast("Thrash", ret => Me.Force > 70),
                                Spell.Cast("Saber Strike")
                           )),
                       new SwitchArgument<CharacterClass>(CharacterClass.Warrior,
                           new LockSelector(
                                Spell.Cast("Force Charge", ret => !LazyRaider.MovementDisabled && Me.CurrentTarget.Distance > 1f && Me.CurrentTarget.Distance <= 3f),
                                CloseDistance(Distance.Melee),
                                Spell.Cast("Saber Ward", ret => Me.HealthPercent <= 70),
                                Spell.Cast("Ravage"),
                                Spell.Cast("Smash", ret => Me.CurrentTarget.Distance <= Distance.MeleeAoE && ShouldAOE(3, Distance.MeleeAoE)),
                                Spell.Cast("Force Scream"),
                                Spell.Cast("Retaliation"),
                                Spell.Cast("Vicious Slash", ret => Me.ActionPoints >= 7),
                                Spell.Cast("Assault")
                           )))
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

        public override Composite PVPRotation
        {
            get { return PVERotation; }
        }

        #endregion
    }
}