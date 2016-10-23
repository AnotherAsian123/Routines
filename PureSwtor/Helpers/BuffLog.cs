using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Buddy.Common;
using Buddy.CommonBot;
using Buddy.Common.Math;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using Buddy.Swtor.Objects.Containers;
using Buddy.BehaviorTree;

using Action = Buddy.BehaviorTree.Action;
namespace PureSWTor.Helpers
{
    public class BuffLog
    {
        public Stopwatch MeWatch, TargetWatch, UserInputWatch;
        protected static Buddy.Swtor.Objects.TorPlayer Me { get { return BuddyTor.Me; } }

        public static Composite Log()
        {
            return new Action(delegate
            {
                string name = Me.CurrentTarget.Name;
                TorContainer<TorEffect> d = Me.CurrentTarget.Debuffs;
                Logging.Write("***********Start*************");
                
                foreach (TorEffect t in d)
                {
                    try
                    {                       
                        Logging.Write("[" + name + "][Debuff]  "
                            + t.Guid + "\t"
                            + t.GetStacks() + "\t"
                            + t.Name);
                    }
                    catch
                    {
                        Logging.Write("Error logging debuff");
                    }
                }

                foreach (TorEffect t in Me.Buffs)
                {
                    try
                    {
                        Logging.Write("[" + "Me" + "][Buff]  "
                            + t.Guid + "\t"
                            + t.GetStacks() + "\t"
                            + t.Name);
                    }
                    catch
                    {
                        Logging.Write("Error logging Buff");
                    }
                }
                Logging.Write("***********Stop**************");
                return RunStatus.Failure;
            });
        }


        public static Composite LogTargetBuffs()
        {
            return new Decorator(ret => Me.InCombat && Me.CurrentTarget != null, Log());
        }
    }
}
