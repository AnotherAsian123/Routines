// Copyright (C) 2011-2015 Bossland GmbH// See the file LICENSE for the source code's detailed license


using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	internal class Sharpshooter : RotationBase
	{
		public override string Name
		{
			get { return "Gunslinger Sharpshooter"; }
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
					Spell.Buff("Burst Volley", ret => !Buddy.CommonBot.AbilityManager.CanCast("Penetrating Rounds", Me.CurrentTarget)),
					//Spell.Buff("Defense Screen", ret => Me.HealthPercent <= 70),
					Spell.Buff("Dodge", ret => Me.HealthPercent <= 30),
					Spell.Buff("Cool Head", ret => Me.EnergyPercent <= 50),
					Spell.Buff("Smuggler's Luck"), //, ret => Me.CurrentTarget.BossOrGreater()),
					Spell.Buff("Illegal Mods") //, ret => Me.CurrentTarget.BossOrGreater())
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
					new Decorator(ret => Me.EnergyPercent < 60 && !Buddy.CommonBot.AbilityManager.CanCast("Cool Head", Me),
						new PrioritySelector(
							Spell.Cast("Flurry of Bolts")
							)),
					//Rotation							
					Spell.Cast("Trickshot"),					
					Spell.Cast("Quickdraw", ret => Me.CurrentTarget.HealthPercent <= 30),							
					Spell.Cast("Penetrating Rounds", ret => Me.Level >= 26),
					Spell.Cast("Speed Shot", ret => Me.Level < 26),											
					new Decorator(ret => Me.BuffCount("Charged Aim") == 2,
						new PrioritySelector(						
							Spell.Cast("Aimed Shot")
							)),
					Spell.Cast("Vital Shot", ret => !Me.CurrentTarget.HasMyDebuff("Vital Shot") || Me.CurrentTarget.DebuffTimeLeft("Vital Shot") <= 2),
					Spell.Cast("Charged Burst")
					);
			}
		}


		public override Composite AreaOfEffect
		{
			get
			{
				return new Decorator(ret => Targeting.ShouldAoe,
					new PrioritySelector(
						Spell.CastOnGround("XS Freighter Flyby"),
						Spell.Cast("Thermal Grenade"),
						Spell.CastOnGround("Sweeping Gunfire")
						));
			}
		}
	}
}