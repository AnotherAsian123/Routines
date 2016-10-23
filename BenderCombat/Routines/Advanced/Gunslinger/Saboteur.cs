// Copyright (C) 2011-2015 Bossland GmbH// See the file LICENSE for the source code's detailed license


using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	internal class Saboteur : RotationBase
	{
		public override string Name
		{
			get { return "Gunslinger Saboteur"; }
		}


		public override Composite Buffs
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Lucky Shots")
					);
			}
		}


		public override Composite Cooldowns
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Escape"),
					//Spell.Buff("Defense Screen", ret => Me.HealthPercent <= 50),
					Spell.Buff("Dodge", ret => Me.HealthPercent <= 30),
					Spell.Buff("Cool Head", ret => Me.EnergyPercent <= 50),
					Spell.Buff("Smuggler's Luck", ret => Me.CurrentTarget.BossOrGreater()),
					Spell.Buff("Illegal Mods", ret => Me.CurrentTarget.BossOrGreater())
					);
			}
		}


		public override Composite SingleTarget
		{
			get
			{
				return new PrioritySelector(
					//Movement
					CombatMovement.CloseDistance(Distance.Ranged),				
				
					//Low Energy
					new Decorator(ret => Me.EnergyPercent < 10,
						new PrioritySelector(
							Spell.Cast("Thermal Grenade", ret => Me.HasBuff("Seize the Moment")),
							Spell.Cast("Speed Shot"),
							Spell.Cast("Flurry of Bolts")
							)),

					//Rotation
					Spell.Cast("Crouch", ret => !Me.IsInCover() && !BenderCombat.MovementDisabled),
					Spell.Cast("Distraction", ret => Me.CurrentTarget.IsCasting && !BenderCombat.MovementDisabled),
					Spell.Cast("Quickdraw", ret => Me.CurrentTarget.HealthPercent <= 30),
					Spell.CastOnGround("Incendiary Grenade", ret => Me.IsInCover() && !Me.CurrentTarget.HasDebuff("Overwhelmed (Mental)") && Me.EnergyPercent >= 60 && Me.CurrentTarget.IsHostile),
					Spell.CastOnGround("XS Freighter Flyby", ret => Me.IsInCover() && Me.CurrentTarget.IsHostile),					
					Spell.Cast("Sabotage Charge"),
					//Spell.DoTGround("Incendiary Grenade", 9000),
					Spell.DoT("Shock Charge", "Shock Charge"),
					Spell.Cast("Sabotage", ret => Me.CurrentTarget.HasDebuff("Shock Charge")),
					Spell.Cast("Thermal Grenade", ret => Me.HasBuff("Seize the Moment")),
					Spell.Cast("Speed Shot"),					
					Spell.Cast("Vital Shot", ret => Me.EnergyPercent >= 60),
					Spell.Cast("Flurry of Bolts")
					);
			}
		}


		public override Composite AreaOfEffect
		{
			get
			{
				return new Decorator(ret => Targeting.ShouldAoe && Me.CurrentTarget.IsHostile,
					new PrioritySelector(
						Spell.CastOnGround("XS Freighter Flyby"),
						Spell.CastOnGround("Incendiary Grenade", ret => !Me.CurrentTarget.HasDebuff("Overwhelmed (Mental)") && Me.EnergyPercent >= 60),
						Spell.CastOnGround("Sweeping Gunfire", ret => Me.IsInCover())
						));
			}
		}
	}
}