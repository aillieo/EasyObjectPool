using System;

namespace AillieoUtils
{
    internal static class GameObjectPoolProfilerEvents
    {
        internal static Action OnPoolCreated;

        internal static Action OnPoolDestroyed;

        internal static Action OnGet;

        internal static Action OnRecycle;

        internal static Action OnCreate;

        internal static Action OnDestroy;
    }

    internal static class GameObjectPoolProfiler
    {
    }
}
