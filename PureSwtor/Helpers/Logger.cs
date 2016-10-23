using System.Windows.Media;
using Buddy.Common;
using Buddy.BehaviorTree;

namespace PureSWTor.Helpers
{
    static class Logger
    {

        private static string _lastCombatmsg;
        private static string _lastDebugmsg;
        private static string _lastItemmsg;

        public static Composite Write(string s)
        {
            return new Action(delegate
                                {
                                    Logging.Write(s);
                                    return RunStatus.Failure;
                                });
        }

        public static void InitLog(string message, params object[] args)
        {
            if (message == null) return;
            Logging.Write(Colors.Gainsboro, "  {0}", string.Format(message, args));
        }

        public static void ItemLog(string message, params object[] args)
        {
            if (message == _lastItemmsg) return;
            Logging.Write(LogLevel.Diagnostic, Colors.LawnGreen, "[PureRotation {0}]: {1}", Pure.Version, string.Format(message, args));
            _lastItemmsg = message;
        }

        public static void FailLog(string message, params object[] args)
        {
            if (message == null) return;
            Logging.Write(Colors.GreenYellow, "[PureRotation {0}]: {1}", Pure.Version, string.Format(message, args));
        }

        public static void InfoLog(string message, params object[] args)
        {
            if (message == null) return;
            Logging.Write(Colors.CornflowerBlue, "[PureRotation {0}]: {1}", Pure.Version, string.Format(message, args));
        }

        public static void CombatLog(string message, params object[] args)
        {
            if (message == _lastCombatmsg) return;
            Logging.Write(LogLevel.Diagnostic, Colors.Pink, "[PureRotation {0}]: {1}", Pure.Version, string.Format(message, args));
            _lastCombatmsg = message;
        }

        public static void DebugLog(string message, params object[] args)
        {
            if (message == _lastDebugmsg) return;
            Logging.Write(LogLevel.Diagnostic, Colors.DarkGoldenrod, "[PureRotation {0}]: {1}", Pure.Version, string.Format(message, args));
            _lastDebugmsg = message;
        }
    }
}
