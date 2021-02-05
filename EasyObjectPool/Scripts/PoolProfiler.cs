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
            public struct PolicyInfo
            {
                public int reserveOnTrim;
                public int sizeMax;

                public PolicyInfo(int reserveOnTrim, int sizeMax)
                {
                    this.reserveOnTrim = reserveOnTrim;
                    this.sizeMax = sizeMax;
                }

                public override string ToString()
                {
                    return $"RES:{reserveOnTrim}/SIZE:{sizeMax}";
                }
            }

            internal ProfilerInfo(string name, string type, PolicyInfo policy)
            {
                this.name = name;
                this.type = type;
                this.policy = policy;
            }

            public string name { get; private set; }
            public string type { get; private set; }
            public PolicyInfo policy { get; private set; }
            public int timesCreate { get; internal set; }
            public int timesGet { get; internal set; }
            public int timesRecycle { get; internal set; }
            public int timesDestroy { get; internal set; }
            public int countInPool { get; internal set; }
        }

        [Conditional("UNITY_EDITOR")]
        public static void Report<T>(Pool<T> pool, PoolAction action, int countInPool) where T : class
        {
            ProfilerInfo info;
            if (!records.TryGetValue(pool, out info))
            {
                info = new ProfilerInfo(pool.nameForProfiler, typeof(T).Name, new ProfilerInfo.PolicyInfo(pool.policy.reserveOnTrim, pool.policy.sizeMax));
                records.Add(pool, info);
            }
            switch (action)
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
            info.countInPool = countInPool;
        }

        private static readonly Dictionary<object, ProfilerInfo> records = new Dictionary<object, ProfilerInfo>();

        public static void RetrieveRecords(List<ProfilerInfo> infoList)
        {
            infoList.Clear();
            foreach (var pair in records)
            {
                infoList.Add(pair.Value);
            }
        }

        public static bool IsBadPolicy(ProfilerInfo profilerInfo)
        {
            return profilerInfo.timesDestroy > 2 * profilerInfo.policy.sizeMax
                || profilerInfo.timesRecycle < profilerInfo.policy.reserveOnTrim * 0.1f;
        }

        public static bool IsLeakPotential(ProfilerInfo profilerInfo)
        {
            return profilerInfo.timesGet > profilerInfo.timesRecycle * 10;
        }

        public static bool IsErrorRecycling(ProfilerInfo profilerInfo)
        {
            return profilerInfo.timesRecycle > profilerInfo.timesGet;
        }
    }

}
