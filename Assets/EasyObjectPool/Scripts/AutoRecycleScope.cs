using System;
using System.Collections.Generic;

namespace AillieoUtils
{
    public struct AutoRecycleScope<T> : IDisposable where T : class
    {
        internal AutoRecycleScope(Pool<T> pool)
        {
            recorder = HashSetPool<T>.Get();
            this.pool = pool;
        }

        private readonly Pool<T> pool;
        private HashSet<T> recorder;

        public T Get()
        {
            T obj = pool.Get();
            recorder.Add(obj);
            return obj;
        }

        public void Recycle(T obj)
        {
            recorder.Remove(obj);
            pool.Recycle(obj);
        }

        public void Dispose()
        {
            foreach(T item in recorder)
            {
                pool.Recycle(item);
            }
            recorder.Clear();

            HashSetPool<T>.Recycle(recorder);
        }
    }
}
