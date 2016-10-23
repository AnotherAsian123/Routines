// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	internal class Serenity : RotationBase
	{
		public override string Name
		{
			get { return "Shadow Serenity New"; }
		}

		public override Composite Buffs
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Force Technique"),
					Spell.Buff("Force Valor"),
					Spell.Cast("Guard", on => Me.Companion,
						ret => Me.Companion != null && !Me.Companion.IsDead && !Me.Companion.HasBuff("Guard")),
					Spell.Buff("Force Speed", ret => Me.IsStealthed && !BenderCombat.MovementDisabled && Me.IsMoving && !Me.HasBuff("Rocket Boost")),
					new Decorator(ret => Me.IsStealthed && !BenderCombat.MovementDisabled && Me.IsMoving && !Me.HasBuff("Force Speed") && Buddy.CommonBot.AbilityManager.CanCast("Blackout", Me),
						new Action(delegate
						{
							Buddy.CommonBot.AbilityManager.Cast("Blackout", Me);
							Me.UseQuickslotAction(9);
							return RunStatus.Success;
						})),
					Spell.Buff("Stealth", ret => !Rest.KeepResting() && !BenderCombat.MovementDisabled)					
					);
			}
		}

		public override Composite Cooldowns
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Force of Will"),
					Spell.Buff("Battle Readiness", ret => Me.HealthPercent <= 85),
					Spell.Buff("Deflection", ret => Me.HealthPercent <= 60),
					Spell.Buff("Resilience", ret => Me.HealthPercent <= 50),
					Spell.Buff("Force Potency"),
					Spell.Buff("Blackout", ret => Me.ForcePercent <= 40)					
					);
			}
		}

		public override Composite SingleTarget
		{
			get
			{
				return new PrioritySelector(
					//Spell.Buff("Force Speed",
					//	ret => !BenderCombat.MovementDisabled && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),					
					Spell.Cast("Shadow Stride", ret => CombatHotkeys.EnableCharge && Me.CurrentTarget.Distance >= 1f && Me.CurrentTarget.Distance <= 3f),
					
					
					//Movement
					CombatMovement.CloseDistance(Distance.Melee),

					//Low Energy
					Spell.Cast("Mind Crush", ret => Me.ForcePercent < 25 && Me.HasBuff("Force Strike")),
					Spell.Cast("Saber Strike", ret => Me.ForcePercent < 25),

					//Rotation
					Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && CombatHotkeys.EnableInterrupts),
					Spell.CastOnGround("Force in Balance"),
					Spell.Cast("Sever Force", ret => !Me.CurrentTarget.HasDebuff("Sever Force")),
					Spell.DoT("Force Breach", "Crushed (Force Breach)"),
					Spell.Cast("Vanquish", ret => Me.HasBuff("Force Strike") && Me.Level >= 57),
					Spell.Cast("Mind Crush", ret => Me.HasBuff("Force Strike") && Me.Level < 57),
					Spell.Cast("Spinning Strike", ret => Me.CurrentTarget.HealthPercent <= 30 || Me.HasBuff("Crush Spirit")),
					Spell.Cast("Serenity Strike", ret => Me.HealthPercent <= 70),
					Spell.Cast("Shadow Strike", ret => Me.ForcePercent > 75 && Me.IsBehind(Me.CurrentTarget)),					
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
						Spell.DoT("Force Breach", "Crushed (Force Breach)"),
						Spell.Cast("Sever Force", ret => !Me.CurrentTarget.HasDebuff("Sever Force")),
						Spell.CastOnGround("Force in Balance"),
						Spell.Cast("Whirling Blow", ret => Me.ForcePercent > 20)
						));
			}
		}
	}
}