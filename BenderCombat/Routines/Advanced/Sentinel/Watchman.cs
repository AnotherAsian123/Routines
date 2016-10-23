// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	internal class Watchman : RotationBase
	{
		public override string Name
		{
			get { return "Sentinel Watchman"; }
		}

		public override Composite Buffs
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Juyo Form"),
					Spell.Buff("Force Might")
					);
			}
		}

		public override Composite Cooldowns
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Rebuke", ret => Me.HealthPercent <= 50),
					Spell.Buff("Guarded by the Force", ret => Me.HealthPercent <= 10),
					Spell.Buff("Saber Ward", ret => Me.HealthPercent <= 30)
					);
			}
		}

		public override Composite SingleTarget
		{
			get
			{
				return new PrioritySelector(
				
					Spell.Cast("Force Leap", ret => Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),
					Spell.Cast("Twin Saber Throw", ret => Me.CurrentTarget.Distance <= 3f),				
				
					//Movement
					CombatMovement.CloseDistance(Distance.Melee),

					//Rotation
					Spell.Cast("Force Kick", ret => Me.CurrentTarget.IsCasting && !BenderCombat.MovementDisabled),
					Spell.Buff("Zen", ret => Me.BuffCount("Centering") > 29),
					//Spell.Buff("Valorous Call", ret => Me.BuffCount("Centering") < 5),
					Spell.Buff("Overload Saber", ret => !Me.HasBuff("Overload Saber")),
					Spell.DoT("Cauterize", "Burning (Cauterize)"),
					Spell.DoT("Force Melt", "Force Melt"),
					Spell.Cast("Dispatch", ret => Me.CurrentTarget.HealthPercent <= 30),
					Spell.Cast("Merciless Slash"),
					Spell.Cast("Zealous Strike", ret => Me.ActionPoints <= 5),
					Spell.Cast("Blade Dance"),
					Spell.Cast("Strike")
					);
			}
		}

		public override Composite AreaOfEffect
		{
			get
			{
				return new Decorator(ret => Targeting.ShouldPbaoe,
					new PrioritySelector(
						Spell.Cast("Force Sweep"),
						Spell.Cast("Twin Saber Throw"),
						Spell.Cast("Cyclone Slash")
						));
			}
		}
	}
}