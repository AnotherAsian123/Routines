using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using DefaultCombat;
using DefaultCombat.Helpers;

using Buddy.BehaviorTree;
using Buddy.Common;
using Buddy.CommonBot;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using Buddy.Swtor.Objects.Containers;
using Buddy.Navigation;
using Buddywing;
using Buddy.CommonBot.Settings;
using Buddy.Common.Math;

using Action = Buddy.BehaviorTree.Action;
using Distance = DefaultCombat.Helpers.Distance;
using Health = DefaultCombat.Helpers.Health;

namespace DefaultCombat.Core
{
    [UsedImplicitly]
    public class Targeting
    {
        //Collections
        public static List<TorCharacter> HealCandidates;
        public static List<TorCharacter> Tanks;
        public static List<Vector3> HealCandidatePoints;

        //Static Points and People
        public static string TankName = "";
        public static TorCharacter Tank;
        public static TorCharacter HealTarget;
        public static TorCharacter AOEHealTarget;
        public static Vector3 AOEHealPoint = Vector3.Zero;
        public static TorCharacter SpecialTarget;
        public static TorCharacter DispelTarget;
        private static TorPlayer Me { get { return BuddyTor.Me; } }
        //public static DispelManager DM = new DispelManager();

        //Unused bool to keep code compilable
        public static bool ShouldAoe = false;
        public static bool ShouldPbaoe = false;

        //Counts
        public static int DispelCount = 0;
        public static int AOEInitialCount = 0;

        //Settings for making target queries
        private const int MaxHealth = Health.Max;
        private const float HealingDistance = Distance.Ranged;
        private const float AOEHealDist = Distance.MeleeAoE;
        private static int AOECount = 2;            //{ get { return PRSettings.Instance.AoEHealCount; } }
        private static int AOEHealHP = Health.High;           //{ get { return PRSettings.Instance.AoEHealHP; } }

        //Determine if we should use the tank's target.
    //    private static bool UseTankTarget
   //     {
    //        get
   //         {
   //             return Me.CurrentTarget == null && Tank != null && Tank.Guid != Me.Guid && Tank.InCombat && Tank.CurrentTarget != null;
    //        }
    //    }

        //Other shit I need!
        //public static DispelManager DM = new DispelManager();
        public static bool ShouldAoeHeal;

        //Caching shit
        public static int cacheCount = 75;
        public static int maxCacheCount = 2;
        public static List<TorCharacter> Objects;

        public static Composite ScanTargets
        {
            get
            {
                return new Action(delegate
                {
                    //increment shit!
                    cacheCount++;

                    //Reset counts
                    AOEInitialCount = 0;
                    DispelCount = 0;

                    //Reset Targets
                    Tank = null;
                    HealTarget = null;
                    AOEHealTarget = null;
                    AOEHealPoint = Vector3.Zero;
                    SpecialTarget = null;
                    DispelTarget = null;

                    //Reset Lists and shit
                    HealCandidates = new List<TorCharacter>();
                    HealCandidatePoints = new List<Vector3>();
                    Tanks = new List<TorCharacter>();
                    ShouldAoeHeal = false;

                    //update the cache when we feel like it
                    if (cacheCount >= maxCacheCount)
                        updateObjects();

                    foreach (TorCharacter p in Objects)
                    {
                        //Doing this shit early
                        // Got a Focus Tank?
                        //  if (Me.FocusTargetIsActive && p.Guid == Me.FocusTargetID && !p.IsDead)
                        //     Tank = p;

                        //   if (p.IsPartyRoleTank())
                        //       Tanks.Add(p);

                        //     if (Tank == null)
                        //        Tank = Me.PartyMembers(false).ToList().Where(c => c.Name.Contains(TankName)).FirstOrDefault();


                        //Check for HealTarget
                        if (p.HealthPercent <= MaxHealth && !p.IsDead && p.HealthPercent > 0)
                        {
                            if (HealTarget == null || p.HealthPercent < HealTarget.HealthPercent)
                                HealTarget = p;

                            //Add to candidtates list
                            HealCandidates.Add(p);
                            HealCandidatePoints.Add(p.Position);

                            //increment our AOEHealCount
                            if (p.HealthPercent <= AOEHealHP)
                                AOEInitialCount++;
                        }

                        if (p.NeedsCleanse())
                        {

                            if (DispelTarget != null && p.HealthPercent < DispelTarget.HealthPercent)
                                DispelTarget = p;

                            if (DispelTarget == null)
                                DispelTarget = p;
                        }

                        //SpecialTarget
                        SpecialTarget = null;
                    }

                    // Damn couldnt find a tank ima be the boss!
              //      if (Tank == null && Me.Companion != null)
              //          Tank = Me.Companion;

              //      if (Tank == null && Me.Companion == null)
             //           Tank = Me;

                    //We have checked everyone out, lets set AOE stuff
                    if (AOEInitialCount >= AOECount)
                    {
                        ShouldAoeHeal = true;
                        //AOEHealTarget
                        AOEHealTarget = AOEHealLocation(AOEHealDist);

                        //AOEHealPoint
                        if (AOEHealTarget != null)
                            AOEHealPoint = AOEHealLocation(AOEHealTarget);
                    }

                    return RunStatus.Failure;
                });
            }
        }

        public static void SetTank()
        {
            if (Me.CurrentTarget != null && Me.CurrentTarget.IsFriendly)
            {
                Tank = Me.CurrentTarget;
                Logging.Write("Tank set to : " + Tank.Name);
                TankName = Tank.ToString();
            }
        }

        private static void updateObjects()
        {
            Objects = Me.PartyMembers(false).ToList().FindAll(p =>
                !p.IsDead
                && p.DistanceSqr < HealingDistance * HealingDistance
                && p.InLineOfSight);

            if (!Objects.Contains(Me))
                Objects.Add(Me);

            if (Me.Companion != null && !Objects.Contains(Me.Companion))
                Objects.Add(Me.Companion);

            //Reset dat count
            cacheCount = 0;
        }

        private static Vector3 AOEHealLocation(TorCharacter p)
        {
            return p != null ? p.Position : Vector3.Zero;
        }

        private static TorCharacter AOEHealLocation(float dist)
        {
            TorCharacter pt = Me;

            if (Tank != null)
                pt = Tank;

            var currentPtCount = PeopleAroundPoint(pt.Position, dist);
            var tempCount = 0;
            foreach (TorCharacter p in HealCandidates)
            {
                tempCount = PeopleAroundPoint(p.Position, dist);
                if (p.Guid != Me.Guid && tempCount > currentPtCount)
                {
                    pt = p;
                    currentPtCount = tempCount;
                }
            }

            return tempCount >= AOECount ? pt : null;
        }

        private static int PeopleAroundPoint(Vector3 pt, float dist)
        {
            var maxDistance = dist * dist;
            return HealCandidates.Count(p => pt.DistanceSqr(p.Position) <= maxDistance);
        }
    }
}
