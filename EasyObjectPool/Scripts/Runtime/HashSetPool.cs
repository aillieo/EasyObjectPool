using System.Collections.Generic;

namespace AillieoUtils
{
    public static class HashSetPool<T>
    {
        private static Pool<HashSet<T>> pool = Pool<HashSet<T>>.Create()
            .SetSizeMax(32)
            .SetCreateFunc(() => new HashSet<T>())
            .SetOnRecycle(set => set.Clear())
            .AsPool();

        public static HashSet<T> Get()
        {
            return pool.Get();
        }

        public static void Recycle(HashSet<T> set)
        {
            pool.Recycle(set);
        }

        public static PoolScope<HashSet<T>> GetScope()
        {
            return pool.GetScope();
        }

        public static void Shrink(int keepCount = 0)
        {
            pool.Shrink(keepCount);
        }
    }
}
