using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using Buddy.Common;
using Buddy.CommonBot;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using Buddy.Common.Math;
using Buddy.Navigation;
using Buddy.BehaviorTree;
using Buddy.CommonBot;
using Buddy.Common.Plugins;
using DefaultCombat.Helpers;

namespace Buddywing.Plugins
{
    class HarvestCorpses : IPlugin
    {
        
        #region Implementation of IEquatable<IPlugin>

        /// <summary>

        /// Indicates whether the current object is equal to another object of the same type.

        /// </summary>

        /// <returns>

        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.

        /// </returns>

        /// <param name="other">An object to compare with this object.</param>

        public bool Equals(IPlugin other)
        {

            return other.Name == Name;

        }

        #endregion

        #region Implementation of IPlugin


        public DateTime LastInteractCheck;
		public ulong LastInteract;

        public string Author { get { return "Wired203"; } }

        public Version Version { get { return new Version(0, 2); } }

        public string Name { get { return "Harvest Corpses"; } }

        public string Description { get { return "Use Companion on salvaging nodes or similar"; } }

        public Window DisplayWindow { get { return null; } }

        /// <summary> Executes the pulse action. This is called every "tick" of the bot. </summary>
        private static DateTime datLCL;                                                     // Last Check Local (used locally in various routines)
        private static Dictionary<ulong, DateTime> MyBlacklist = new Dictionary<ulong, DateTime>();
        private static TorCharacter LastComp = null;
        private static DateTime datLC;                                                      // Last check for harvestables
        private static bool HasScav = false;
        private static bool HasBio = false;
        public static TorPlayer Me { get { return (TorPlayer)BuddyTor.Me; } }
        private static float MaxNodeDist = 2.0f;
        private static TorCharacter CompTarg = null;
        private static float CHP = 0;
        public static string CompName = "";
        private static DateTime datLCC = DateTime.Now;
        private static float THP = 0;
        private static DateTime datLOMU;

        public void OnPulse()
        {
            HarvestIfNec();

        }

        private static void SynchObjCacheAll()
        {
            SynchObjCache(true);
        }

        public static DateTime Now { get { return DateTime.Now; } }

        private static void SynchObjCache(bool Npcs = false)
        {
            // 'Force' the load of objects from memory into the cache (to make sure they're 'Current').
            // A generic query will load objects into the cache from memory... Seems to kill questing errors as well as 'missing buffs'.
            DateTime datStart = Now;
            using (BuddyTor.Memory.AcquireFrame())
            {
                try
                {
                    if (Npcs) foreach (TorCharacter tx in ObjectManager.GetObjects<TorCharacter>().AsParallel().ToList()) { var ty = tx; };
                    try { if (Npcs) foreach (TorNpc tx in ObjectManager.GetObjects<TorNpc>().AsParallel().ToList()) { var ty = tx; }; }
                    catch { }
                    //ObjectManager.Update();
                }
                catch { }
            }
            //Logger.Write("Object Cache Synch: " + Now.Subtract(datStart).TotalMilliseconds.ToString("0"));
        }

        private static long BotMemory()
        {
            return Process.GetCurrentProcess().PrivateMemorySize64;
        }

        private static void CheckObjCount()
        {
            bool BadMojo = false;
            using (BuddyTor.Memory.AcquireFrame())
            {
                try
                {
                    BadMojo = ObjectManager.Cache.Count() > (Int32)1000 || ObjectManager.Cache.Count() < (Int32)0 || BotMemory() > (long)200000000;
                }
                catch { BadMojo = true; }

                if (BadMojo)
                {
                    while (true)
                    {
                        try
                        {
                            ObjectManager.Cache.Clear();
                            GC.Collect();
                            GC.GetTotalMemory(true);
                            SynchObjCacheAll();
                            var objAll = ObjectManager.GetObjects<TorObject>();
                            var objChar = ObjectManager.GetObjects<TorCharacter>();
                            Thread.Sleep(300);
                            break;
                        }
                        catch { }
                    }
                }
            }
            if (BadMojo)
            {
                System.GC.Collect();
                System.GC.GetTotalMemory(true);
            }
        }
        public static bool CanHarvestObj(TorPlaceable obj)
        {
            try
            {
                using (BuddyTor.Memory.AcquireFrame())
                {
                    string SkillName = obj.Harvestable.ProfessionRequired.ToString();
                    ulong MySkill = PISkill(SkillName);
                    //Logger.Write("Skill Level Required for " + obj.Name + ": " + LR.ToString() + " Skill: " + SkillName + " MySkill: " + MySkill.ToString());
                    //if (ObjIsArch(obj) && HasArch && LR <= PISkill("Archaeology")) return true;                         // Arch placeables don't (currently) have the Harvestable or other properties from what I can see, so free-ball it

                    if (obj != null && !obj.IsDeleted && obj.Harvestable != null && obj.Name != null)
                    {
                            // (
                            // (obj.Harvestable.ProfessionRequired == ProfessionType.Archaeology || obj.Name.Contains("Crystal Formation")) && HasArch && LR <= PISkill("Archaeology")) || (obj.Harvestable.ProfessionRequired == ProfessionType.Bioanalysis && HasBio && LR <= PISkill("Bioanalysis")) ||
                            // (obj.Harvestable.ProfessionRequired == ProfessionType.Scavenging && HasScav && LR <= PISkill("Scavenging")) || (obj.Harvestable.ProfessionRequired == ProfessionType.Slicing && HasSlicing && LR <= PISkill("Slicing"))
                            //)
                            return true;
                    }
                    //if (obj != null) if (!obj.IsDeleted) if (ObjIsArch(obj) && HasArch && LR <= PISkill("Archaology")) return true;
                }
            }
            catch { }
            return false;
        }

        public static void WaitForComp()
        {
            var CompX = Comp;
            datLCL = DateTime.Now;
            if (CompX != null) { while (CompX.IsCasting && DateTime.Now.Subtract(datLCL).TotalSeconds <= 5) Thread.Sleep(500); }
        }

        public static TorCharacter Comp
        {
            get
            {
                float HealthTest = 0;
                float DistTest = 0;

                DateTime CompCheckBeg = DateTime.Now;

                //UpdateGameEvents();

                for (int i = 0; i < 3; i++)
                {
                    //SynchObjCache(true, true);
                    UpdateObjects();
                    using (BuddyTor.Memory.AcquireFrame())
                    {
                        if (Me.Companion != null && UpdateCompInfo(Me.Companion)) { LastComp = Me.Companion; return LastComp; }
                        bool bErr = false;
                        if (LastComp != null)
                        {
                            if (LastComp.IsDeleted || LastComp == null) bErr = true;
                            else
                            {
                                try { HealthTest = LastComp.HealthPercent; DistTest = LastComp.Distance; if (DistTest > 4.0f) bErr = true; }
                                catch { bErr = true; }
                            }
                            if (!bErr && LastComp != null)
                            {
                                if (UpdateCompInfo(LastComp)) return LastComp;
                            }
                        }

                        LastComp = null;
                        try
                        {
                            var theList = ObjectManager.GetObjects<TorObject>().OrderBy(t => t.Distance)
                                .AsParallel().ToArray().Where(t => (t.IsKindOf(Buddy.Swtor.Enums.DomDefIds.npc_class) || t.IsKindOf(Buddy.Swtor.Enums.DomDefIds.character_class)) && ((TorCharacter)t).Toughness == CombatToughness.Companion);
                            if (theList != null && theList.Count() > 0) foreach (TorCharacter mob in theList.Where(d => d.Distance < 4.5f))
                                {
                                    //Logger.Write("TorCharacter Name: " + mob.Name + " Toughness: " + mob.Toughness.ToString() + " Distance: " + mob.Distance.ToString());
                                    //TorNpc compTest = null;
                                    //if (mob != null) try
                                    //    {
                                    //        Logger.Write("Testing the goddamn mob...");
                                    //        compTest = (TorNpc)mob;
                                    //    }
                                    //    catch { }

                                }
                        }
                        catch
                        {
                            // fails on Comp death, CheckLootability errors, just query TorNpc and TorCharacter objs at that point
                        }
                        try
                        {
                            foreach (TorCharacter mob in ObjectManager.GetObjects<TorCharacter>().AsParallel().ToList().OrderBy(t => (float)t.Distance).Where(t => t.Toughness == CombatToughness.Companion))
                            {
                                //Logger.Write("TorCharacter Name: " + mob.Name + " Toughness: " + mob.Toughness.ToString() + " Distance: " + mob.Distance.ToString());
                                LastComp = mob; UpdateCompInfo(LastComp); break;
                            }
                        }
                        catch { }
                        if (LastComp != null) return LastComp;

                        try
                        {
                            foreach (TorNpc mob in ObjectManager.GetObjects<TorNpc>().AsParallel().ToList().OrderBy(t => (float)t.Distance).Where(t => t.Toughness == CombatToughness.Companion && t.Distance <= 3.0f))
                            {
                                LastComp = (TorCharacter)mob; UpdateCompInfo(LastComp); break;
                            }
                        }
                        catch { }

                        if (LastComp != null) return LastComp;
                    }
                    //Thread.Sleep(50);
                }
                return LastComp;
            }
        }


        public static ulong PISkill(string Skill)
        {
            foreach (Buddy.Swtor.Objects.Components.ProfessionInfo pi in BuddyTor.Me.ProfessionInfos) if (pi.Name.Contains(Skill)) return (ulong)pi.CurrentLevel;
            return 0;
        }

        public static void GetProfessions()
        {
            try
            {
                using (BuddyTor.Memory.AcquireFrame())
                {
                    foreach (Buddy.Swtor.Objects.Components.ProfessionInfo pi in BuddyTor.Me.ProfessionInfos)
                    {
                        if (pi.Name.Contains("Bioanalysis")) HasBio = true;
                        if (pi.Name.Contains("Scavenging")) HasScav = true;
                    }
                }
            }
            catch {}
        }

        public static bool UpdateCompInfo(TorCharacter theComp)
        {
            if (theComp == null) return false;
            try
            {
                LastComp = theComp; CompTarg = LastComp.CurrentTarget; CHP = LastComp.HealthPercent; CompName = theComp.Name; datLCC = DateTime.Now;
            }
            catch { LastComp = null; THP = 100f; return false; }
            return true;
        }

        public static bool CanHarvestCorpse(TorNpc unit)
        {
            //Logger.Write("Harvest: " + unit.Name + " UnitType: " + unit.CreatureType.ToString());
            try
            {
                using (BuddyTor.Memory.AcquireFrame())
                {
                    ulong SkillReq = Math.Max(unit.Level - 11, 1) * 8;
                    if (unit != null && !unit.IsDeleted && unit.IsDead && !unit.IsFriendly && unit.Distance <= 4.0f && StrongOrGreater(unit) && !MyBlacklist.ContainsKey(unit.Guid))
                        if
                        (
                            (HasBio && unit.CreatureType == Buddy.Swtor.CreatureType.Creature && PISkill("Bioanalysis") >= SkillReq) ||
                            (HasScav && (unit.CreatureType == Buddy.Swtor.CreatureType.Droid || unit.Name.Contains("Droid")) && PISkill("Scavenging") >= SkillReq)
                        )
                        {
                            //Logger.Write("Can Harvest the corpse.");
                            return true;
                        }
                }
            }
            catch { }
            //Logger.Write("Can't Harvest the corpse.");
            return false;
        }

        public static bool StrongOrGreater(TorCharacter unit)
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                return (int)unit.Toughness >= 4;
                //if (unit != null) if (unit.Toughness == CombatToughness.Strong || unit.Toughness == CombatToughness.Boss1 || unit.Toughness == CombatToughness.Boss2 ||
                //    unit.Toughness == CombatToughness.Boss3 || unit.Toughness == CombatToughness.Boss4 || unit.Toughness == CombatToughness.RaidBoss || unit.Toughness == CombatToughness.Player) return true;
                //return false;
            }
        }

        public static MoveResult MoveToPlaceable(TorPlaceable thePlc)
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                return Buddy.Navigation.Navigator.MoveTo(thePlc.Position);
            }
        }

        public static void MoveToAndInteract(TorObject obj, double ToWithin = .99f, int WaitAfterInteract = 4000)
        {

            MoveResult MR = MoveResult.Moved;
            datLCL = DateTime.Now;
            while (obj.Distance > ToWithin && MR != MoveResult.Failed && MR != MoveResult.PathGenerationFailed && DateTime.Now.Subtract(datLCL).TotalSeconds <= 20)
            {
                if (IAmInCombat()) { ClearTarget(); return; } // If it's a chest, grab that shit FIRST
                //MR = Buddy.Navigation.Navigator.MoveTo(obj.Position);
                MR = MoveToPlaceable((TorPlaceable)obj);
                Thread.Sleep(200);
                //Buddy.Swtor.Input.MoveTo(obj.Position, MeleeDist); Thread.Sleep(2000); 
            }
            StopMoving();
            Thread.Sleep(1000);
            if (IAmInCombat()) { ClearTarget(); return; }
            if (obj.Distance <= ToWithin)
            {
                try
                {
                    using (BuddyTor.Memory.AcquireFrame()) { obj.Interact(); Thread.Sleep(300); ((TorPlaceable)obj).Interact(); Thread.Sleep(300); ((TorItem)obj).Use(); }
                    if (!IAmInCombat() && obj != null) MyBlacklist.Add(obj.Guid, DateTime.Now);
                }
                catch { }
                Thread.Sleep(4000);
                if (IAmInCombat()) { ClearTarget(); return; }
            }
            if (IAmInCombat()) return;
        }

        public static bool HarvestIfNec()
        {
            if (!(HasBio || HasScav)) return false;

            datLCL = DateTime.Now;

            if (DateTime.Now.Subtract(datLC).TotalSeconds < 10) return false;
            //Logger.Write("Entering Harvest...");
            if (IAmInCombat()) return false;
            if (BuddyTor.Me.Companion != null) if (BuddyTor.Me.Companion.InCombat) return false;

            int H = 0;

            if (IAmInCombat()) return false;
            //if (DateTime.Now.Subtract(datLCL).TotalSeconds >= 2f) return false;
            float theDist = MaxNodeDist;
            while (true)
            {
                TorNpc unit = GetNextHarvestable();
                if (unit == null) break;
                try
                {
                    {
                        if (IAmInCombat()) { ClearTarget(); return true; }
                        MoveToAndInteract(unit);
                        if (IAmInCombat()) { ClearTarget(); return true; }
                        H += 1;
                    }
                }
                catch { }
            }
            datLC = DateTime.Now;
            return (H > 0);
        }

        public static TorNpc GetNextHarvestable()
        {
            SynchObjCache(true);
            IEnumerable<TorNpc> theHarvestables = new List<TorNpc>();
            using (BuddyTor.Memory.AcquireFrame())
            {
                theHarvestables = ObjectManager.GetObjects<TorNpc>().AsParallel().ToList().OrderBy(t => t.Distance).Where(t => CanHarvestCorpse(t));
            }
            foreach (TorNpc unit in theHarvestables) if (!MyBlacklist.ContainsKey(unit.Guid)) return unit;
            return null;
        }

        public static bool MoveToNoCombat()
        {
            Vector3 theLoc = new Vector3((float)-130.6159, (float)32.53959, (float)163.3082);
            while (Me.Position.Distance(theLoc) > .40f) { Navigator.MoveTo(theLoc); Thread.Sleep(200); }
            return true;
        }

        public static void InteractWithUnit(TorCharacter theUnit)
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                theUnit.Interact();
                Thread.Sleep(150);
                //theUnit.Interact(); 
            }
        }


        private static Vector3 CloneUnitPos(TorCharacter theUnit)
        {
            try
            {
                using (BuddyTor.Memory.AcquireFrame())
                {
                    if (theUnit == null) return new Vector3(0f, 0f, 0f);
                    return new Vector3(theUnit.Position.X, theUnit.Position.Y, theUnit.Position.Z);
                    //return theUnit.Position;
                }
            }
            catch { return new Vector3(0f, 0f, 0f); }
        }

        public static void MoveToAndInteract(TorNpc unit, double ToWithin = .99f)
        {
            if (MyBlacklist.ContainsKey(unit.Guid)) return;//|| !CanHarvestCorpse(unit)) return;
            datLCL = DateTime.Now;

            MoveResult MR = MoveResult.Moved;
            Vector3 Loc = CloneUnitPos(unit);
            while (PosDist(Loc) > ToWithin && MR != MoveResult.Failed && MR != MoveResult.PathGenerationFailed && DateTime.Now.Subtract(datLCL).TotalSeconds <= 15)
            {
                if (IAmInCombat()) { ClearTarget(); return; }
                MR = MoveToUnit(unit);
                //MR = Buddy.Navigation.Navigator.MoveTo(Loc);
                if (IAmInCombat()) { ClearTarget(); return; }
                Thread.Sleep(200);
            }
            StopMoving();
            StopMoving();
            Thread.Sleep(1500);
            if (IAmInCombat()) { ClearTarget(); return; }
            if (PosDist(Loc) <= ToWithin)
            {
                //if (Comp != null) while (Comp.IsCasting) Thread.Sleep(1000);
                if (unit != null)
                {
                    if (IAmInCombat()) { ClearTarget(); return; }
                    InteractWithUnit(unit);
                    Thread.Sleep(2000);
                    if (IAmInCombat()) { ClearTarget(); return; }
                    if (CanHarvestCorpse(unit)) { InteractWithUnit(unit); Thread.Sleep(3500); while (IAmCasting()) Thread.Sleep(1000); }
                    MyBlacklist.Add(unit.Guid, Now);
                    //return; 
                }
            }
            MyBlacklist.Add(unit.Guid, DateTime.Now);
            return;

        }

        public static MoveResult MoveToUnit(TorCharacter theUnit)
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                return Buddy.Navigation.Navigator.MoveTo(theUnit.Position);
            }
        }

        public static bool IAmCasting()
        {
            bool ac = false;
            using (BuddyTor.Memory.AcquireFrame())
            {
                try { ac = Me.IsCasting; }
                catch { return false; }
            }
            return ac;
        }

        private static float PosDist(Vector3 thePos)
        {
            if (thePos == null) return 0f;

            return thePos.Distance(Me.Position);
        }

        public static void UpdateObjects(bool FlushOld = false)
        {
            if (DateTime.Now.Subtract(datLOMU).TotalMilliseconds < 250) return;
            DateTime datStart = DateTime.Now;

            using (BuddyTor.Memory.AcquireFrame())
                try
                {

                    //Buddy.CommonBot.BotMain.CurrentBot.Pulse();

                    if (ObjectManager.Cache.Count() > (int)1000 || ObjectManager.Cache.Count() < (int)0) ObjectManager.Cache.Clear();

                    if (FlushOld) ObjectManager.FlushOldEntries();
                    ObjectManager.Update(); ;

                    datLOMU = DateTime.Now;

                }
                catch {}

            //Logger.Write("UpdateObjects Duration: " + DateTime.Now.Subtract(datStart).TotalMilliseconds);
        }

        public static bool IAmMounted()
        {
            UpdateObjects();
            using (BuddyTor.Memory.AcquireFrame())
            {
                try { return Me.IsMounted || Me.HasBuff("Rocket Boost"); }
                catch { return false; }
            }
        }

        public static bool IAmInCombat()
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                try { UpdateObjects(); return Me.InCombat; }
                catch { return false; }
            }
        }

        public static void ClearTarget()
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                if (MyTarget() != null) { BuddyTor.Me.ClearTarget(); }
            }
            Thread.Sleep(50);
        }

        public static TorCharacter MyTarget()
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                return Me.CurrentTarget;
            }
        }

        public static void StopMoving()
        {

            //Thread.Sleep(50);
            using (BuddyTor.Memory.AcquireFrame())
            {
                //Buddy.CommonBot.CommonBehaviors.MoveStop();
                //Buddy.CommonBot.CommonBehaviors.MoveStop();
                //Buddy.Swtor.Movement.Stop(MovementDirection.Forward);
                //Buddy.Swtor.Input.MoveStopAll();
                Buddy.CommonBot.CommonBehaviors.MoveStop();
                Thread.Sleep(50);
                Buddy.Swtor.Input.MoveStopAll();
                Thread.Sleep(50);
                Buddy.Swtor.Input.MoveStopAll();
                //Buddy.Swtor.Input.MoveStopAll();
            }
            //Thread.Sleep(50);
            //Buddy.CommonBot.CommonBehaviors.MoveStop();
        }
      
        
        /// <summary> Executes the initialize action. This is called at initial bot startup. (When the bot itself is started, not when Start() is called) </summary>
        public void OnInitialize()
        {            
           
        }


        public void OnStart()
        {
      
        }

        public void OnStop()
        {
            
        }

        /// <summary> Executes the shutdown action. This is called when the bot is shutting down. (Not when Stop() is called) </summary>

        public void OnShutdown()
        {

        }

        /// <summary> Executes the enabled action. This is called when the user has enabled this specific plugin via the GUI. </summary>

        public void OnEnabled()
        {
             
        }


        /// <summary> Executes the disabled action. This is called whent he user has disabled this specific plugin via the GUI. </summary>

        public void OnDisabled()
        {
            
        }     

        #endregion


    }
}
