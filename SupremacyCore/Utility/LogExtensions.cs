using System;
using System.Diagnostics;

using Supremacy.Annotations;

using log4net;

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode

namespace Supremacy.Utility
{
    public static class LogExtensions
    {
        public static void Catch([NotNull] this ILog log, [NotNull] Action action)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (action == null)
                throw new ArgumentNullException("action");

            try
            {
                action();
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        public static void Catch([NotNull] this GameLog log, [NotNull] Action action)
        {
            if (log == null)
                throw new ArgumentNullException("log");
            if (action == null)
                throw new ArgumentNullException("action");

            try
            {
                action();
            }
            catch (Exception e)
            {
                log.General.Error(e);
            }
        }

        [DebuggerStepThrough]
        public static void DropException([NotNull] this ILog log, [NotNull] Exception ex)
        {
            if (ex == null)
                log.Debug("DropException called with Null.");
        }
    }
}

// ReSharper restore HeuristicUnreachableCode
// ReSharper restore ConditionIsAlwaysTrueOrFalse