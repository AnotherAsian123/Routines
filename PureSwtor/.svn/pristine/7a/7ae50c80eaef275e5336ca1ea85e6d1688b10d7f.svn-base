using System.Linq;
using Buddy.Common;
using Buddy.Navigation;
using Buddy.BehaviorTree;
using Buddy.CommonBot;
using Buddy.Swtor.Objects;
using Buddy.CommonBot.Settings;
using Buddy.Swtor;

using Action = Buddy.BehaviorTree.Action;

namespace PureSWTor.Helpers
{
    public class RandomGrind
    {
        public static TorCharacter Target;

        public static void EnableRandomGrind()
        {
            CharacterSettings.Instance.PullDistance = 3.0f;
            TreeHooks.Instance.ReplaceHook("Combat_OOC", new PrioritySelector(SetPoi(), RoutineManager.Current.OutOfCombat));
            Logging.Write("Enabling Random Grind");
        }

        public static Composite SetPoi()
        {
            return new Action(delegate
            {
                if (Poi.Current.Type == PoiType.None && !NeedToLoot)
                {
                    var target = ObjectManager.GetObjects<TorCharacter>()
                        .Where(n => n.IsHostile && !n.IsDead)
                        .OrderBy(n => n.DistanceSqr).FirstOrDefault();

                    if (target == null)
                        return RunStatus.Failure;
                    
                    Poi.Current = new Poi(target as TorObject, PoiType.Kill);

                    return RunStatus.Success;
                }
                else
                {
                    return RunStatus.Failure;
                }
            });
        }

        private static bool NeedToLoot
        {
            get
            {
                return ObjectManager.GetObjects<TorNpc>().Any(n => n.IsLootable); 
            }
        }
    }
}
