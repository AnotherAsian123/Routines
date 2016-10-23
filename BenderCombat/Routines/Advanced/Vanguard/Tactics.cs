// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	public class Tactics : RotationBase
	{
		public override string Name
		{
			get { return "Vanguard Tactics"; }
		}

		public override Composite Buffs
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("High Energy Cell"),
					Spell.Buff("Fortification")
					);
			}
		}

		public override Composite Cooldowns
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Tenacity"),
					Spell.Buff("Recharge Cells", ret => Me.ResourcePercent() <= 50),
					Spell.Buff("Reactive Shield", ret => Me.HealthPercent <= 60),
					Spell.Buff("Adrenaline Rush", ret => Me.HealthPercent <= 30),
					Spell.Buff("Reserve Powercell", ret => Me.ResourceStat <= 80)					
					);
			}
		}

		public override Composite SingleTarget
		{
			get
			{
				return new PrioritySelector(
					//Movement
					Spell.Cast("Storm", ret => Me.CurrentTarget.Distance >= 1f && !BenderCombat.MovementDisabled),
					CombatMovement.CloseDistance(Distance.Melee),
					new Decorator(ret => Me.HasBuff("Tactical Accelerator"),
						new PrioritySelector(
							Spell.Cast("High Impact Bolt")
							)),					
					new Decorator(ret => Buddy.CommonBot.AbilityManager.CanCast("Shoulder Cannon", Me.CurrentTarget),
						new PrioritySelector(
							Spell.Cast("Battle Focus"),
							Spell.Cast("Shoulder Cannon")
							)),							
					new Decorator(ret => Me.ResourcePercent() < 60,
						new PrioritySelector(
							Spell.Cast("Hammer Shot")
							)),												
					new Decorator(ret => Me.BuffCount("Energy Lode") == 4,
						new PrioritySelector(
							Spell.Cast("Cell Burst")
							)),													
					Spell.Cast("Riot Strike", ret => Me.CurrentTarget.IsCasting && !BenderCombat.MovementDisabled),
					Spell.Cast("Gut",ret => Me.CurrentTarget.Distance <= 0.4f && !Me.CurrentTarget.HasDebuff("Bleeding")),
					Spell.Cast("Assault Plastique"),					
					Spell.Cast("Stockstrike"),
					Spell.Cast("Tactical Surge", ret => Me.ResourcePercent() > 60 && Me.Level >= 26),
					Spell.Cast("Ion Pulse", ret => Me.Level < 26)
					//Spell.Cast("Pulse Cannon", ret => Me.CurrentTarget.Distance <= 1f),
					//Spell.Cast("Hammer Shot")
					);
			}
		}

		public override Composite AreaOfEffect
		{
			get
			{
				return new PrioritySelector(
					new Decorator(ret => Targeting.ShouldAoe,
						new PrioritySelector(
							Spell.CastOnGround("Morter Volley"),
							Spell.Cast("Sticky Grenade", ret => Me.CurrentTarget.HealthPercent > 70 && Me.ResourcePercent() > 30),
							Spell.Cast("Explosive Round", ret => Me.ResourcePercent() > 30)
							)),
					new Decorator(ret => Targeting.ShouldPbaoe,
						new PrioritySelector(
							Spell.CastOnGround("Morter Volley"),
							Spell.Cast("Pulse Cannon"),
							Spell.Cast("Explosive Surge"))
						));
			}
		}
	}
}