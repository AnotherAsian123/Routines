using System;
using System.Collections.Generic;
using System.Linq;
using Buddy.BehaviorTree;
using PureSWTor.Managers;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using PureSWTor.Helpers;
using Buddy.Common;
using Buddy.Common.Math;
using Buddy.CommonBot;
using Buddy.Navigation;


using Action = Buddy.BehaviorTree.Action;

namespace PureSWTor.Classes
{
    public abstract class RotationBase
    {
        protected static Buddy.Swtor.Objects.TorPlayer Me { get { return BuddyTor.Me; } }
        protected static TorCharacter Pet { get { return Me.Companion; } }
        protected static TorCharacter Tank { get { return HealingManager.Tank; } }
        protected static TorCharacter HealTarget { get { return HealingManager.HealTarget; } }
                
        protected static IManItem MedPack = new IManItem("Medpac", 90);

        public abstract string Revision { get; }
        public abstract CharacterDiscipline KeySpec { get; }
        public abstract string Name { get; }

        public abstract Composite PVPRotation { get; }
        public abstract Composite PVERotation { get; }
        public abstract Composite PreCombat { get; }
        
        public static bool ShouldAOE(int minMobs, float distance)
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                if (!Pure.EnableAOE || Me.CurrentTarget == null)
                    return false;

                Vector3 center = Me.CurrentTarget.Position;

                List<Vector3> points = new List<Vector3>();

                foreach (TorCharacter c in ObjectManager.GetObjects<TorCharacter>())
                {
                    try
                    {
                        if (c != null && c.InCombat && c.IsHostile && !c.IsDead && !c.IsStunned && !c.IsCrowdControlled())
                            points.Add(c.Position);
                    }
                    catch
                    {
                        Logging.Write("An error with aoe count unit");
                        continue;
                    }
                }


                return points.Count(p => p.DistanceSqr(center) <= (distance * distance)) >= minMobs;
                
            }

            /*
            return Pure.EnableAOE && ObjectManager.GetObjects<TorCharacter>().Count(mob => mob != null && mob.InCombat 
                                && mob.IsHostile
                                && !mob.IsDead
                                && !mob.IsStunned
                                && (Me.CurrentTarget != null && mob != null && mob.IsInRange(Me.CurrentTarget, distance))) >= minMobs;
             * */
        }

        public static bool ShouldPBAOE(int minMobs, float distance)
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                if (!Pure.EnableAOE || Me.CurrentTarget == null)
                    return false;

                Vector3 center = Me.Position;

                List<Vector3> points = new List<Vector3>();

                foreach (TorCharacter c in ObjectManager.GetObjects<TorCharacter>())
                {
                    try
                    {
                        if (c != null && c.InCombat && c.IsHostile && !c.IsDead && !c.IsStunned && !c.IsCrowdControlled())
                            points.Add(c.Position);
                    }
                    catch
                    {
                        Logging.Write("An error with aoe count unit");
                        continue;
                    }
                }

                return points.Count(p => p.DistanceSqr(center) <= (distance * distance)) >= minMobs;
                
            }
        }

        public static Composite CloseDistance(float range)
        {
            return new Decorator(ret => !LazyRaider.MovementDisabled && Me.CurrentTarget != null,
                new PrioritySelector(
                    new Decorator(ret => Me.CurrentTarget.Distance < range,
                        new Action( delegate{
                            Navigator.MovementProvider.StopMovement();
                            return RunStatus.Failure;
                        })),
                    new Decorator(ret => Me.CurrentTarget.Distance >= range,
                        CommonBehaviors.MoveAndStop(location => Me.CurrentTarget.Position, range, true)),
                    new Action(delegate { return RunStatus.Failure; })));
        }


        public Composite StopResting
        {
            get
            {
                return new Decorator(ret =>
                    !Me.InCombat
                    && Me.IsCasting
                    && Me.HealthPercent > 90
                    && (Me.Companion != null && Me.Companion.HealthPercent > 90),
                    //&& DateTime.Now.Subtract(Me.CastTimeEnd).TotalSeconds > 5,
                    new Action(ret => Buddy.Swtor.Movement.Move(Buddy.Swtor.MovementDirection.Forward, System.TimeSpan.FromMilliseconds(400))));
            }
        }
    }
}
