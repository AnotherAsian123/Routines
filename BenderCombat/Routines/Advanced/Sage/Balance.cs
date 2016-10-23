// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	internal class Balance : RotationBase
	{
		public override string Name
		{
			get { return "Sage Balance"; }
		}

		public override Composite Buffs
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Force Valor")
					);
			}
		}

		public override Composite Cooldowns
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Force Barrier", ret => Me.HealthPercent <= 20),
					Spell.Buff("Force Armor", ret => !Me.HasBuff("Force Armor") && !Me.HasDebuff("Force-imbalance")),
					Spell.Buff("Force Mend", ret => Me.HealthPercent <= 50),
					Spell.Buff("Mental Alacrity", ret => Me.CurrentTarget.BossOrGreater()),
					Spell.Buff("Force Potency")
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
					Spell.Cast("Mind Snap", ret => Me.CurrentTarget.IsCasting && !BenderCombat.MovementDisabled),
					Spell.Cast("Force Stun", ret => Me.CurrentTarget.IsCasting && !BenderCombat.MovementDisabled),
					Spell.Cast("Weaken Mind", ret => !Me.CurrentTarget.HasDebuff("Weaken Mind")),
					Spell.Cast("Vanquish", ret => Me.BuffCount("Presence of Mind") == 4 && Me.Level >= 57),
					Spell.Cast("Mind Crush", ret => Me.BuffCount("Presence of Mind") == 4 && Me.Level < 57),
					Spell.Cast("Sever Force", ret => !Me.CurrentTarget.HasDebuff("Sever Force")),					
					Spell.CastOnGround("Force in Balance",
						ret => Me.CurrentTarget.HasDebuff("Weaken Mind") && Me.CurrentTarget.HasDebuff("Sever Force")),
					Spell.Cast("Force Serenity", ret => Me.CurrentTarget.HasDebuff("Weaken Mind")),
					Spell.Cast("Disturbance", ret => Me.BuffCount("Presence of Mind") == 4),
					Spell.Cast("Telekinetic Throw", ret => Me.BuffCount("Presence of Mind") < 4),
					Spell.Cast("Disturbance")
					);
			}
		}

		public override Composite AreaOfEffect
		{
			get
			{
				return new Decorator(ret => Targeting.ShouldAoe,
					new PrioritySelector(
						Spell.DoT("Weaken Mind", "Weaken Mind"),
						Spell.DoT("Sever Force", "Sever Force"),
						Spell.CastOnGround("Force in Balance",
							ret => Me.CurrentTarget.HasDebuff("Weaken Mind") && Me.CurrentTarget.HasDebuff("Sever Force")),
						Spell.CastOnGround("Forcequake", ret => Me.CurrentTarget.HasDebuff("Overwhelmed (Mental)"))
						));
			}
		}
	}
}