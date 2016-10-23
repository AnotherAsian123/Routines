using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Buddy.BehaviorTree;
using Buddy.Common;
using Buddy.CommonBot;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using Buddy.Navigation;
using Buddywing;
using Buddy.CommonBot.Settings;
using System.IO;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using PureSWTor.Classes;
using PureSWTor.Core;
using PureSWTor.Helpers;
using PureSWTor.Managers;
using PureSWTor.Settings.GUI;

using Action = Buddy.BehaviorTree.Action;

namespace PureSWTor
{
    public partial class Pure : CombatRoutine
    {
        public override sealed string Name { get { return "Pure Rotation"; } }
        private static TorPlayer Me { get { return BuddyTor.Me; } }
        public override CharacterClass Class { get { return Me.Class; } }
        internal static readonly Version Version = new Version(0, 0, 1);
        public static bool EnableAOE = true;
        private static Stopwatch rebuildTimer;
        private static int rebuldsetting = 30000;
        public static bool isHealer = false;

        private Form _newtempui;

        #region Overridden Methods
        
        public override void Dispose()
        {
        }
        public override Window ConfigWindow { get { return null; } }

        public override void Initialize()
        {
            LazyRaider.Initialize();
            rebuildTimer = new Stopwatch();
            LazyRaider.DisableAll();

            
            Hotkeys.RegisterHotkey("Toggle AOE (F7)", () => { ChangeAOE(); }, Keys.F7);
            Logging.Write("[Hot Key][F7] Toggle AOE");
            Hotkeys.RegisterHotkey("Pause Rotation (F8)", () => { LoadUI(); }, Keys.F8);
            Logging.Write("[Hot Key][F8] Load UI");
            Hotkeys.RegisterHotkey("Pause Rotation (F9)", () => { LazyRaider.ChangePause(); }, Keys.F9);

            Hotkeys.RegisterHotkey("Set Tank (F12)", () => { HealingManager.setTank(); }, Keys.F12);
            Logging.Write("[Hot Key][F12] Set Tank");

            
            Logging.Write("[Hot Key][F9] Toggle Pause");
            /*
            Hotkeys.RegisterHotkey("Disable/Enable Movement (F10)", () => { LazyRaider.ChangeMovement(); }, Keys.F10);
            Logging.Write("[Hot Key][F10] Toggle Movement");
            Hotkeys.RegisterHotkey("Disable/Enable Targeting (F11)", () => { LazyRaider.ChangeTargeting(); }, Keys.F11);
            Logging.Write("[Hot Key][F11] Toggle Targeting");
            Hotkeys.RegisterHotkey("Disable/Enable All (F12)", () => { LazyRaider.ChangeAll(); }, Keys.F12);
            Logging.Write("[Hot Key][F12] Toggle All");
             */


         
            Logging.Write("Initialize Behaviors");

            // Intialize Behaviors....
            if (_combatBehavior == null)
                _combatBehavior = new PrioritySelector();

            if (_preCombatBehavior == null)
                _preCombatBehavior = new PrioritySelector();

            // Behaviors
            if (!RebuildBehaviors())
                return;
        }

        public void LoadUI()
        {
            if (_newtempui == null || _newtempui.IsDisposed || _newtempui.Disposing) _newtempui = new ConfigurationForm();
            if (_newtempui != null || _newtempui.IsDisposed) _newtempui.ShowDialog();
        }

        public void ChangeAOE()
        {
            if (EnableAOE)
            {
                Logging.Write("AOE Disabled");
                EnableAOE = false;
            }
            else
            {
                Logging.Write("AOE Enabled");
                EnableAOE = true;
            }
        }

        public static bool CheckRebuildTimer
        {
            get
            {
                if (!rebuildTimer.IsRunning)
                {
                    rebuildTimer.Start();
                    return false;
                }

                if (rebuildTimer.ElapsedMilliseconds >= rebuldsetting)
                {
                    rebuildTimer.Restart();
                    return true;
                }

                return false;
            }
        }

        public Composite CheckRebuild
        {
            get
            {
                return new Decorator(ret => CheckRebuildTimer,
                    new PrioritySelector(new Action(delegate
                {
                    RebuildBehaviors();
                    return RunStatus.Success;
                })));
            }
        }


        public static Composite RaidBehavior
        {
            get
            {                
                return new LockSelector(
                        new Decorator(ret => !BuddyTor.Me.InCombat, RoutineManager.Current.OutOfCombat),
                        new Decorator(ret => BuddyTor.Me.InCombat && 
                            ((!isHealer && BuddyTor.Me.CurrentTarget != null) || (isHealer)), 
                            RoutineManager.Current.Combat)
                    );
            }
        }
        #endregion
    }

    [UsedImplicitly]
    public partial class Pure
    {
        private RotationBase _currentRotation; // the current Rotation
        private List<RotationBase> _rotations; // list of Rotations

        #region Behaviours

        private Composite _combatBehavior, _preCombatBehavior;

        public override Composite Combat { get { return _combatBehavior; } }
        public override Composite OutOfCombat { get { return _preCombatBehavior; } }
        public override Composite Pull { get { return _combatBehavior; } }

        private Composite PreCombat { get { return new Decorator( ret => !Me.IsDead && !BuddyTor.Me.IsMounted, new PrioritySelector(CheckRebuild, _currentRotation.PreCombat)); } }
        private Composite Rotation { get { return Me.PvpFlagged ? _currentRotation.PVPRotation : _currentRotation.PVERotation; } }
        
        public bool RebuildBehaviors()
        {
            Logging.Write("RebuildBehaviors called.");

            List<string> HookNames = new List<string>();
            foreach (KeyValuePair<string, List<Composite>> h in TreeHooks.Instance.Hooks.ToList())
                HookNames.Add(h.Key);

            foreach (string s in HookNames)
                TreeHooks.Instance.ReplaceHook(s, new PrioritySelector());

            
            CharacterSettings.Instance.AutomaticMount = false;
            CharacterSettings.Instance.MountName = string.Empty;
            CharacterSettings.Instance.PullDistance = 0.1f;
            CombatTargeting.Instance.Provider = new PureITargetingProvider();
            LootTargeting.Instance.Provider = new PureITargetingProvider();
            Navigator.MovementProvider = new PureMovementProvider();
            Navigator.NavigationProvider = new PureNavigationProvider();
            TreeHooks.Instance.ReplaceHook("RepopCorpse", new PrioritySelector());            

            _currentRotation = null; // clear current rotation

            GetRotations();

            SetRotation(); // set the new rotation

            if (_combatBehavior != null)
                _combatBehavior = new PrioritySelector(LazyRaider.RotationPaused, Rotation);
            if (_preCombatBehavior != null)
                _preCombatBehavior = new PrioritySelector(LazyRaider.RotationPaused, PreCombat);

            switch (_currentRotation.KeySpec)
            {
                case CharacterDiscipline.CombatMedic:
                    isHealer = true;
                    break;
                case CharacterDiscipline.Bodyguard:
                    isHealer = true;
                    break;
                case CharacterDiscipline.Medicine:
                    isHealer = true;
                    break;
                case CharacterDiscipline.Seer:
                    isHealer = true;
                    break;
                case CharacterDiscipline.Sawbones:
                    isHealer = true;
                    break;
                case CharacterDiscipline.Corruption:
                    isHealer = true;
                    break;
                default:
                    isHealer = false;
                    break;
            }


            TreeHooks.Instance.ReplaceHook("TreeStart", RaidBehavior);
            Logging.Write("Rebuild Complete.");

            return true;
        }
        #endregion

        #region Set & Get the current rotation - Also builds the rotations list if it hasnt already done so

        /// <summary>Set the Current Rotation</summary>
        private void SetRotation()
        {
            try
            {
                if (_rotations != null && _rotations.Count > 0)
                {
                    //Logging.Write(" We have rotations so lets use the best one...");
                    foreach (var rotation in _rotations)
                    {
                        if (rotation != null && rotation.KeySpec == Me.Discipline)
                        {
                            Logging.Write(" Using " + rotation.Name + " rotation based on Character Spec ");
                            _currentRotation = rotation;
                        }
                        else
                        {
                            if (rotation != null)
                            {
                                //Logging.Write(" Skipping " + rotation.Name + " rotation. Character is not in " + rotation.KeySpec);
                            }
                        }
                    }
                }
                else
                {
                    Logging.Write(" We have no rotations -  calling GetRotations");
                    GetRotations();
                }
            }
            catch (Exception ex)
            {
                Logging.Write(" Failed to Set Rotation " + ex);
            }
        }

        /// <summary>Get & Set the Current Rotation</summary>
        private void GetRotations()
        {
            try
            {
                _rotations = new List<RotationBase>();
                _rotations.AddRange(new TypeLoader<RotationBase>());

                foreach (var rotation in _rotations)
                {
                    if (rotation != null && rotation.KeySpec == Me.Discipline)
                    {
                        //Logging.Write(" Using " + rotation.Name + " rotation based on Character Spec ");
                        _currentRotation = rotation;
                    }
                    else
                    {
                        if (rotation != null)
                        {
                            //Logging.Write(" Skipping " + rotation.Name + " rotation. Character is not in " + rotation.KeySpec);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (var exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException)
                    {
                        var exFileNotFound = exSub as FileNotFoundException;
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("CLU Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Logging.Write(" Woops, we could not set the rotation.");
                Logging.Write(errorMessage);
            }
        }
        #endregion
    }
    public class LockSelector : PrioritySelector
    {
        public LockSelector(params Composite[] children)
            : base(children)
        {
        }

        public override RunStatus Tick(object context)
        {
            using (BuddyTor.Memory.AcquireFrame())
            {
                return base.Tick(context);
            }
        }
    }
}
