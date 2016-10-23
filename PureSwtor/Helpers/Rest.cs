using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buddy.BehaviorTree;
using Buddy.Common;
using Buddy.CommonBot;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using Buddy.Navigation;
using Buddywing;
using PureSWTor.Core;
using System.Diagnostics;
using System.Threading;
using PureSWTor.Managers;

using Action = Buddy.BehaviorTree.Action;

namespace PureSWTor.Helpers
{
    public static class Rest
    {
        private static TorPlayer Me { get { return BuddyTor.Me; } }

        public static int NormalizedResource()
        {
            if (Me.AdvancedClass == AdvancedClass.None)
            {
                switch (Me.Class)
                {
                    //Add cases needed for reverse basic classes
                    case CharacterClass.BountyHunter:
                        return 100 - (int)Me.ResourcePercent();
                    case CharacterClass.Warrior:
                        return 100;
                    case CharacterClass.Knight:
                        return 100;
                    default:
                        return (int)Me.ResourcePercent();
                }
            }
            else
            {
                switch (Me.AdvancedClass)
                {
                    //Add cases needed for reverse Advance classes
                    case AdvancedClass.Mercenary:
                        return 100 - (int)Me.ResourcePercent();
                    case AdvancedClass.Powertech:
                        return 100 - (int)Me.ResourcePercent();
                    case AdvancedClass.Juggernaut:
                        return 100;
                    case AdvancedClass.Marauder:
                        return 100;
                    case AdvancedClass.Guardian:
                        return 100;
                    case AdvancedClass.Sentinel:
                        return 100;
                    default:
                        return (int)Me.ResourcePercent();
                }
            }

        }

        private static string RestSpell()
        {
            if (Me.AdvancedClass == AdvancedClass.None)
            {
                switch (Me.Class)
                {
                    //Add cases needed for reverse basic classes
                    case CharacterClass.Agent:
                        return "Recuperate";
                    case CharacterClass.Smuggler:
                        return "Recuperate";
                    case CharacterClass.BountyHunter:
                        return "Recharge and Reload";
                    case CharacterClass.Trooper:
                        return "Recharge and Reload";
                    case CharacterClass.Warrior:
                        return "Channel Hatred";
                    case CharacterClass.Knight:
                        return "Introspection";
                    case CharacterClass.Inquisitor:
                        return "Seethe";
                    case CharacterClass.Consular:
                        return "Meditation";
                    default:
                        return "None";
                }
            }
            else
            {
                switch (Me.AdvancedClass)
                {
                    //Add cases needed for reverse Advance classes
                    case AdvancedClass.Operative:
                        return "Recuperate";
                    case AdvancedClass.Sniper:
                        return "Recuperate";
                    case AdvancedClass.Gunslinger:
                        return "Recuperate";
                    case AdvancedClass.Scoundrel:
                        return "Recuperate";
                    case AdvancedClass.Mercenary:
                        return "Recharge and Reload";
                    case AdvancedClass.Powertech:
                        return "Recharge and Reload";
                    case AdvancedClass.Commando:
                        return "Recharge and Reload";
                    case AdvancedClass.Vanguard:
                        return "Recharge and Reload";
                    case AdvancedClass.Juggernaut:
                        return "Channel Hatred";
                    case AdvancedClass.Marauder:
                        return "Channel Hatred";
                    case AdvancedClass.Guardian:
                        return "Introspection";
                    case AdvancedClass.Sentinel:
                        return "Introspection";
                    case AdvancedClass.Assassin:
                        return "Seethe";
                    case AdvancedClass.Sorcerer:
                        return "Seethe";
                    case AdvancedClass.Sage:
                        return "Meditation";
                    case AdvancedClass.Shadow:
                        return "Meditation";
                    default:
                        return "None";
                }
            }
        }

        public static bool NeedRest()
        {
            int resource = NormalizedResource();
            return  !LazyRaider.MovementDisabled && !Me.InCombat && ((resource < 50 || Me.HealthPercent < 90)
                || (Me.Companion != null && !Me.Companion.IsDead && Me.Companion.HealthPercent < 90));
        }

        public static bool KeepResting()
        {
            int resource = NormalizedResource();
            return  !LazyRaider.MovementDisabled && !Me.InCombat && ((resource < 100 || Me.HealthPercent < 100)
                || (Me.Companion != null && !Me.Companion.IsDead && Me.Companion.HealthPercent < 100));
        }

        public static Composite HandleRest
        {
            get
            {
                return new Action(delegate
                {

                    if (NeedRest())
                    {
                        while (KeepResting())
                        {
                            if(!Me.IsCasting)
                                AbilityManager.Cast(RestSpell(), Me);

                            Thread.Sleep(100);
                        }

                        Movement.Move(Buddy.Swtor.MovementDirection.Forward, System.TimeSpan.FromMilliseconds(1));
                        return RunStatus.Success;
                    }

                    return RunStatus.Failure;
                });
            }
        }

    }
}
