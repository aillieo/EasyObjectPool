using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace AillieoUtils
{
    public class PoolStatisticInfo
    {
        public string name { get; internal set; }

        public string type { get; internal set; }

        public float lifetime { get; internal set; }

        public int timesCreate { get; internal set; }

        public int timesGet { get; internal set; }

        public int timesRecycle { get; internal set; }

        public int timesDestroy { get; internal set; }

        public int shrink1stDestroy { get; internal set; }

        public int shrink2ndDestroy { get; internal set; }
    }
}
