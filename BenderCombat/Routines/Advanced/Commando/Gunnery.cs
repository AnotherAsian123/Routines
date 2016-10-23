// Copyright (C) 2011-2015 Bossland GmbH
// See the file LICENSE for the source code's detailed license

using Buddy.BehaviorTree;
using BenderCombat.Core;
using BenderCombat.Helpers;

namespace BenderCombat.Routines
{
	internal class Gunnery : RotationBase
	{
		public override string Name
		{
			get { return "Commando Gunnery"; }
		}

		public override Composite Buffs
		{
			get
			{
				return new PrioritySelector(
					Spell.Buff("Armor-piercing Cell"),
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
					Spell.Buff("Reactive Shield", ret => Me.HealthPercent <= 70),
					Spell.Buff("Adrenaline Rush", ret => Me.HealthPercent <= 30),
					Spell.Buff("Recharge Cells", ret => Me.ResourceStat <= 40),
					Spell.Buff("Supercharged Cell", ret => Me.BuffCount("Supercharge") == 10),
					Spell.Buff("Reserve Powercell", ret => Me.ResourceStat <= 60)
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
					new Decorator(ret => Me.ResourcePercent() < 30,
						new PrioritySelector(
							Spell.Cast("Hammer Shot")
							)),
					new Decorator(ret => Me.HasBuff("Grav Primer") || Me.HasBuff("Tech Override"),
						new PrioritySelector(
							Spell.Cast("Grav Round")
							)),
					new Decorator(ret => Me.HasBuff("Curtain of Fire"),
						new PrioritySelector(
							Spell.Cast("Boltstorm", ret => Me.Level >= 57),
							Spell.Cast("Full Auto", ret => Me.Level < 57)
							)),
					new Decorator(ret => Me.BuffCount("Charged Barrel") == 5 && Me.CurrentTarget.HasDebuff("Gravity Vortex"),
						new PrioritySelector(
							Spell.Cast("High Impact Bolt")
							)),													
					//Rotation
					Spell.Cast("Disabling Shot", ret => Me.CurrentTarget.IsCasting && !BenderCombat.MovementDisabled),
					Spell.Cast("Electro Net"),
					Spell.Cast("Demolition Round",ret => Me.CurrentTarget.HasDebuff("Gravity Vortex")),
					Spell.Cast("Vortex Bolt", ret => !Me.HasBuff("Curtain of Fire")),
					Spell.Cast("Tech Override", ret => !Me.HasBuff("Curtain of Fire")),
					Spell.Cast("Grav Round", ret => !Me.HasBuff("Curtain of Fire"))
					);
			}
		}

		public override Composite AreaOfEffect
		{
			get
			{
				return new Decorator(ret => Targeting.ShouldAoe,
					new PrioritySelector(
						//Spell.Cast("Tech Override"),
						Spell.CastOnGround("Mortar Volley"),
						//Spell.Cast("Plasma Grenade", ret => Me.ResourceStat >= 90 && Me.HasBuff("Tech Override")),
						Spell.Cast("Pulse Cannon", ret => Me.CurrentTarget.Distance <= 1f),
						Spell.CastOnGround("Hail of Bolts", ret => Me.ResourceStat >= 10)
						));
			}
		}
	}
}
