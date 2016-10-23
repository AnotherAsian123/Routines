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
    public partial class Unpure : CombatRoutine
    {
        public override sealed string Name { get { return "Unpure Rotation"; } }
        private static TorPlayer Me { get { return BuddyTor.Me; } }
        public override CharacterClass Class { get { return Me.Class; } }
        internal static readonly Version Version = new Version(0, 0, 1);

        private Form _newtempui;

        #region Overridden Methods

        public override void Dispose()
        {
        }
        public override Window ConfigWindow { get { return null; } }

        public override void Initialize()
        {
            LazyRaider.Initialize();
            Hotkeys.RegisterHotkey("Pause Rotation (F8)", () => { LoadUI(); }, Keys.F8);
            Logging.Write("[Hot Key][F8] Load UI");
            Hotkeys.RegisterHotkey("Pause Rotation (F9)", () => { LazyRaider.ChangePause(); }, Keys.F9);
            Logging.Write("[Hot Key][F9] Toggle Pause");
            Hotkeys.RegisterHotkey("Disable/Enable Movement (F10)", () => { LazyRaider.ChangeMovement(); }, Keys.F10);
            Logging.Write("[Hot Key][F10] Toggle Movement");
            Hotkeys.RegisterHotkey("Disable/Enable Targeting (F11)", () => { LazyRaider.ChangeTargeting(); }, Keys.F11);
            Logging.Write("[Hot Key][F11] Toggle Targeting");
            Hotkeys.RegisterHotkey("Disable/Enable All (F12)", () => { LazyRaider.ChangeAll(); }, Keys.F12);
            Logging.Write("[Hot Key][F12] Toggle All");

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
        #endregion
    }

    [UsedImplicitly]
    public partial class Unpure
    {
        private RotationBase _currentRotation; // the current Rotation
        private List<RotationBase> _rotations; // list of Rotations

        #region Behaviours

        private Composite _combatBehavior, _preCombatBehavior;

        public override Composite Combat { get { return _combatBehavior; } }
        public override Composite OutOfCombat { get { return _preCombatBehavior; } }
        public override Composite Pull { get { return _combatBehavior; } }

        private Composite PreCombat { get { return new Decorator(ret => !Me.IsDead, _currentRotation.PreCombat); } }
        private Composite Rotation { get { return Me.PvpFlagged ? _currentRotation.PVPRotation : _currentRotation.PVERotation; } }


        public bool RebuildBehaviors()
        {
            Logging.Write("RebuildBehaviors called.");

            _currentRotation = null; // clear current rotation

            GetRotations();

            SetRotation(); // set the new rotation

            if (_combatBehavior != null)
                _combatBehavior = new PrioritySelector(LazyRaider.RotationPaused, Rotation);
            if (_preCombatBehavior != null)
                _preCombatBehavior = new PrioritySelector(LazyRaider.RotationPaused, CompanionHandler, PreCombat);

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
                    }
                }
                else
                    GetRotations();
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
                        Logging.Write(" Using " + rotation.Name + " rotation based on Character Spec ");
                        _currentRotation = rotation;
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

        private Composite CompanionHandler
        {
            get
            {
                return new PrioritySelector(
                   new Decorator(ret => Me.Companion != null && Me.Companion.IsDead && Me.CompanionUnlocked > 0,
                        new PrioritySelector(
                            Spell.WaitForCast(),
                            CommonBehaviors.MoveAndStop(location => Me.Companion.Position, 0.2f, true),
                            Spell.Cast("Revive Companion", on => Me.Companion, when => Me.Companion.Distance <= 0.2f)
                            ))
                   );
            }
        }

        #endregion
    }
}
        