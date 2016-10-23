using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using Buddy.Common;
using Buddy.Navigation;
using Buddy.BehaviorTree;
using Buddy.CommonBot;
using Buddy.Swtor.Objects;
using Buddy.CommonBot.Settings;
using Buddy.Swtor;

using Action = Buddy.BehaviorTree.Action;

namespace PureSWTor.Managers
{
    public class LazyRaider
    {
        public static bool TargetingDisabled = false,
                            MovementDisabled = false,
                            AllDisabled = false,
                            PauseRotation = false;

        public static IMovementProvider DefaultMovementProvider;
        public static INavigationProvider DefaultNavigationProvider;

        public static ITargetingProvider DefaultTargetProvider,
                                         DefaultLootTargetProvider;
        public static bool DefaultAutoMount = true;
        public static string DefaultMountName = CharacterSettings.Instance.MountName;

        public static PrioritySelector DefaultRepop;

        public static void Initialize()
        {
            TargetingDisabled = false;
            MovementDisabled = false;
            AllDisabled = false;

            // Capture the Buddy-provided Movement- & Navigation-providers, and mount preferences...
            if (DefaultMovementProvider == null)
                DefaultMovementProvider = Navigator.MovementProvider;

            if (DefaultNavigationProvider == null)
                DefaultNavigationProvider = Navigator.NavigationProvider;

            //DefaultAutoMount = CharacterSettings.Instance.AutomaticMount;

            if (DefaultMountName == null)
                DefaultMountName = CharacterSettings.Instance.MountName;

            if (DefaultTargetProvider == null)
                DefaultTargetProvider = CombatTargeting.Instance.Provider;

            if (DefaultLootTargetProvider == null)
                DefaultLootTargetProvider = LootTargeting.Instance.Provider;

            if (DefaultRepop == null)
                DefaultRepop = GetHookValue("Repop Corpse");
        }

        public static Composite RotationPaused
        {
            get
            {
                return new Decorator(ret => PauseRotation, new Action(ret => RunStatus.Success));
            }
        }

        public static void ChangePause()
        {
            if (PauseRotation)
            {
                Logging.Write("Rotation Resumed");
                PauseRotation = false;
            }
            else
            {
                Logging.Write("Rotation Paused");
                PauseRotation = true;
            }
        }

        public static void ChangeAll()
        {
            if (AllDisabled)
            {
                AllDisabled = false;
                TargetingDisabled = false;
                MovementDisabled = false;
                EnableAll();
            }
            else
            {
                AllDisabled = true;
                TargetingDisabled = true;
                MovementDisabled = true;
                DisableAll();
            }
        }

        public static void ChangeMovement()
        {
            if (MovementDisabled)
            {
                MovementDisabled = false;
                EnableMovement();
            }
            else
            {
                MovementDisabled = true;
                DisableMovement();
            }
        }

        public static void ChangeTargeting()
        {
            if (TargetingDisabled)
            {
                TargetingDisabled = false;
                EnableTargeting();
            }
            else
            {
                TargetingDisabled = true;
                DisableTargeting();
            }
        }

        public static void DisableAll()
        {
            AllDisabled = true;
            TargetingDisabled = true;
            MovementDisabled = true;
            DisableLooting();
            DisableMounting();
            DisableMovement();
            DisableTargeting();
            DisableRepop();
        }

        public static void EnableAll()
        {
            AllDisabled = false;
            TargetingDisabled = false;
            MovementDisabled = false;
            EnableLooting();
            EnableMounting();
            EnableMovement();
            EnableTargeting();
            EnableRepop();
        }

        public static void DisableRepop()
        {
            Logging.Write("LazyRaider Disabling Repop");
            TreeHooks.Instance.ReplaceHook("RepopCorpse", new PrioritySelector());
                        
        }

        private static PrioritySelector GetHookValue(string name)
        {
            PrioritySelector p = new PrioritySelector();

            var val = TreeHooks.Instance.Hooks.Where(pair => pair.Key == name).FirstOrDefault().Value;

            if (val == null)
                return p;

            foreach (Composite c in val)
                p.AddChild(c);

            return p;
        }


        public static void EnableMovement()
        {
            Logging.Write("LazyRaider Enabling Movement");
            Navigator.MovementProvider = DefaultMovementProvider;
            Navigator.NavigationProvider = DefaultNavigationProvider;
        }

        public static void EnableMounting()
        {
            Logging.Write("LazyRaider Enabling Mounting");
            CharacterSettings.Instance.AutomaticMount = DefaultAutoMount;
            CharacterSettings.Instance.MountName = DefaultMountName;
        }

        public static void EnableLooting()
        {
            Logging.Write("LazyRaider Enabling Loot Targeting");
            LootTargeting.Instance.Provider = DefaultLootTargetProvider;
        }

        public static void EnableRepop()
        {
            Logging.Write("LazyRaider Enabling Repop");
            TreeHooks.Instance.ReplaceHook("RepopCorpse", DefaultRepop);
        }

        public static void EnableTargeting()
        {
            Logging.Write("LazyRaider Enabling Combat Targeting");
            CharacterSettings.Instance.PullDistance = 3.0f;
            CombatTargeting.Instance.Provider = DefaultTargetProvider;
        }

        public static void DisableMovement()
        {
            Logging.Write("LazyRaider Disabling Movement");
            Navigator.MovementProvider = new PureMovementProvider();
            Navigator.NavigationProvider = new PureNavigationProvider();
        }

        public static void DisableMounting()
        {
            Logging.Write("LazyRaider Disabling Mounting");
            CharacterSettings.Instance.AutomaticMount =  false;
            CharacterSettings.Instance.MountName = string.Empty;
        }

        public static void DisableLooting()
        {
            Logging.Write("LazyRaider Disabling Loot Targeting");
            LootTargeting.Instance.Provider = new PureITargetingProvider();
        }

        public static void DisableTargeting()
        {
            Logging.Write("LazyRaider Disabling Targeting");
            CharacterSettings.Instance.PullDistance = 0.1f;
            CombatTargeting.Instance.Provider = new PureITargetingProvider();
        }

    }

    public class PureITargetingProvider : ITargetingProvider
    {
        public List<TorObject> GetObjectsByWeight()
        {
            return new List<TorObject>();
        }
    }
       

    class PureMovementProvider : IMovementProvider
    {
        public void MoveTowards(Buddy.Common.Math.Vector3 location) { /*empty*/ }
        public void StopMovement() { /*empty*/ }
    }

    class PureNavigationProvider : INavigationProvider
    {
        public void Clear() { /*empty*/ }
        public MoveResult MoveTo(Buddy.Common.Math.Vector3 destination, string destName, bool useRaycast, bool useStraightLine)
        {
            return (MoveResult.Moved);
        }
    }
}
