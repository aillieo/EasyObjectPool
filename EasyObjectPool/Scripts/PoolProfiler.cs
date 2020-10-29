using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace AillieoUtils
{
    public static class PoolProfiler
    {
        public enum PoolAction
        {
            Create,
            Get,
            Recycle,
            Destroy,
        }

        public class ProfilerInfo
        {
            internal ProfilerInfo(string name, string type, string policy)
            {
                this.name = name;
                this.type = type;
                this.policy = policy;
            }

            public string name { get; private set; }
            public string type { get; private set; }
            public string policy { get; private set; }
            public int timesCreate { get; internal set; }
            public int timesGet { get; internal set; }
            public int timesRecycle { get; internal set; }
            public int timesDestroy { get; internal set; }
        }

        [Conditional("UNITY_EDITOR")]
        public static void Report<T>(Pool<T> pool, PoolAction action) where T : class
        {
            ProfilerInfo info;
            if (!records.TryGetValue(pool, out info))
            {
                info = new ProfilerInfo(pool.nameForProfiler, typeof(T).Name, pool.policy.ToString());
                records.Add(pool, info);
            }
            switch(action)
            {
                case PoolAction.Create:
                    info.timesCreate++;
                    break;
                case PoolAction.Get:
                    info.timesGet++;
                    break;
                case PoolAction.Recycle:
                    info.timesRecycle++;
                    break;
                case PoolAction.Destroy:
                    info.timesDestroy++;
                    break;
            }
        }

        private static readonly Dictionary<object, ProfilerInfo> records = new Dictionary<object, ProfilerInfo>();

        public static void RetrieveRecords(List<ProfilerInfo> infoList)
        {
            infoList.Clear();
            foreach(var pair in records)
            {
                infoList.Add(pair.Value);
            }
        }
    }

}
