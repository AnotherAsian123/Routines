﻿// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	public class Smuggler : RotationBase
	{
		public override string Name
		{
			get { return "Basic Smuggler"; }
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
					Spell.Cast("Saber Ward", ret => Me.HealthPercent <= 70)
					);
			}
		}

		public override Composite SingleTarget
		{
			get
			{
				return new PrioritySelector(
					CombatMovement.CloseDistance(Distance.Ranged),
					Spell.DoT("Vital Shot", "Bleeding (Vital Shot)"),
					Spell.Cast("Blaster Whip", ret => Me.CurrentTarget.Distance <= Distance.Melee),
					Spell.Cast("Sabotage Charge"),
					Spell.Cast("Charged Burst"),
					Spell.Cast("Flurry of Bolts")
					);
			}
		}

		public override Composite AreaOfEffect
		{
			get
			{
				return new Decorator(ret => Targeting.ShouldAoe,
					new PrioritySelector(
						Spell.Cast("Thermal Grenade", ret => Me.CurrentTarget.Distance <= Distance.Ranged)
						));
			}
		}
	}
}