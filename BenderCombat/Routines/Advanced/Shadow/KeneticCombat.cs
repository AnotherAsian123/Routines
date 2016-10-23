// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	internal class KeneticCombat : RotationBase
	{
		public override string Name
		{
			get { return "Shadow Kenetic Combat"; }
		}

		public override Composite Buffs
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Combat Technique"),
					Spell.Buff("Force Valor"),
					Spell.Cast("Guard", on => Me.Companion,
						ret => Me.Companion != null && !Me.Companion.IsDead && !Me.Companion.HasBuff("Guard")),
					Spell.Buff("Stealth", ret => !Rest.KeepResting() && !BenderCombat.MovementDisabled)
					);
			}
		}

		public override Composite Cooldowns
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Kinetic Ward", ret => Me.BuffCount("Kinetic Ward") <= 1 || Me.BuffTimeLeft("Kinetic Ward") < 3),
					Spell.Buff("Force of Will"),
					Spell.Buff("Battle Readiness", ret => Me.HealthPercent <= 85),
					//Spell.Buff("Deflection", ret => Me.HealthPercent <= 60),
					//Spell.Buff("Resilience", ret => Me.HealthPercent <= 50),
					Spell.Buff("Force Potency")
					);
			}
		}

		public override Composite SingleTarget
		{
			get
			{
				return new PrioritySelector(
				    Spell.Cast("Force Pull", ret => Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f && !Me.CurrentTarget.BossOrGreater()), // !BenderCombat.MovementDisabled
					//Spell.Cast("Shadow Stride", ret => Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f), // !BenderCombat.MovementDisabled

					Spell.Buff("Force Speed", ret => !BenderCombat.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),

					//Movement
					CombatMovement.CloseDistance(Distance.Melee),

					//Rotation
					Spell.Cast("Saber Strike", ret => Me.ForcePercent < 25),
					Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && !BenderCombat.MovementDisabled),
					Spell.Cast("Force Stun", ret => Me.CurrentTarget.IsCasting && !BenderCombat.MovementDisabled),
					Spell.Cast("Slow Time"),
					Spell.Cast("Cascading Debris", ret => Me.BuffCount("Harnessed Shadows") == 3),
					Spell.Cast("Project", ret => Me.HasBuff("Particle Acceleration")),
					Spell.Cast("Shadow Strike", ret => Me.HasBuff("Shadow Wrap")),
					Spell.Cast("Spinning Strike", ret => Me.CurrentTarget.HealthPercent <= 30),
					Spell.Cast("Force Breach"),
					Spell.Cast("Double Strike"),
					Spell.Cast("Saber Strike")
					);
			}
		}

		public override Composite AreaOfEffect
		{
			get
			{
				return new Decorator(ret => Targeting.ShouldPbaoe,
					new PrioritySelector(
						Spell.Cast("Slow Time"),
						Spell.Cast("Force Breach"),
						Spell.Cast("Whirling Blow", ret => Me.ForcePercent >= 20 && Me.CurrentTarget.Distance <= 0.5f)
						));
			}
		}
	}
}
