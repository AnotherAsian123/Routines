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

namespace PureSWTor.Classes.Scoundrel
{
    class Scrapper : RotationBase
    {
        #region Overrides of RotationBase

        public override string Revision
        {
            get { return ""; }
        }

        public override CharacterDiscipline KeySpec
        {
            get { return CharacterDiscipline.Scrapper; }
        }

        public override string Name
        {
            get { return "Scoundrel Scrapper by aquintus"; }
        }

        public override Composite PreCombat
        {
            get
            {
                return new PrioritySelector(
                    Spell.Buff("Lucky Shots"),
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
                    Spell.Buff("Cool Head", ret => Me.EnergyPercent <= 45),
                    MedPack.UseItem(ret => Me.HealthPercent <= 20),
                    Spell.Buff("Pugnacity", ret => Me.HasBuff("Upper Hand")),
                    Spell.Buff("Defense Screen", ret => Me.HealthPercent <= 75),
                    Spell.Buff("Dodge", ret => Me.HealthPercent <= 50)
                    );
            }
        }

        private Composite HandleStealth
        {
            get
            {
                return new LockSelector(
                    CloseDistance(Distance.Melee),
                    Spell.Cast("Back Blast", ret => Me.IsBehind(Me.CurrentTarget))
                    );
            }
        }

        private Composite HandleLowEnergy
        {
            get
            {
                return new LockSelector(
                    Spell.Cast("Flurry of Bolts")
                    );
            }
        }

        private Composite HandleAoE
        {
            get
            {
                return new Decorator(ret => ShouldAOE(3, Distance.MeleeAoE),
                    new LockSelector(
                    Spell.Cast("Thermal Grenade"),
                    Spell.Cast("Blaster Volley", ret => Me.HasBuff("Upper Hand"))
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

                    //Interrupts
                    Spell.Cast("Distraction", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= 1f),
                    Spell.Cast("Dirty Kick", ret => Me.CurrentTarget.IsCasting && Me.CurrentTarget.Distance <= 4f),

                    //Rotation
                    Spell.Cast("Backblast", ret => Me.IsBehind(Me.CurrentTarget)),
                    Spell.DoT("Vital Shot", "Bleeding (Vital Shot)"),
                    Spell.Cast("Sucker Punch", ret => Me.HasBuff("Upper Hand") && Me.CurrentTarget.HasDebuff("Bleeding (Vital Shot)")),
                    Spell.Cast("Blood Boiler"),
                    Spell.Cast("Bludgeon", ret => Me.Level >= 41),
                    Spell.Cast("Blaster Whip", ret => Me.Level < 41),
                    Spell.Cast("Shank Shot"),      
                    Spell.Cast("Quick Shot")
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