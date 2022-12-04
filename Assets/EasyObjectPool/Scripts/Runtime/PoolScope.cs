using System;
using System.Collections.Generic;

namespace AillieoUtils
{
    public struct PoolScope<T> : IDisposable
        where T : class
    {
        internal PoolScope(Pool<T> pool)
        {
            recorder = HashSetPool<T>.Get();
            this.pool = pool;
            alive = true;
        }

        private bool alive;
        private readonly Pool<T> pool;
        private HashSet<T> recorder;

        public T Get()
        {
            if (!alive)
            {
                throw new InvalidOperationException($"{typeof(PoolScope<T>)} is disposed");
            }

            T obj = pool.Get();
            recorder.Add(obj);
            return obj;
        }

        public void Recycle(T obj)
        {
            if (!alive)
            {
                throw new InvalidOperationException($"{typeof(PoolScope<T>)} is disposed");
            }

            recorder.Remove(obj);
            pool.Recycle(obj);
        }

        public void Dispose()
        {
            if (!alive)
            {
                return;
            }

            alive = false;

            foreach (T item in recorder)
            {
                pool.Recycle(item);
            }

            recorder.Clear();

            HashSetPool<T>.Recycle(recorder);
        }
    }
}
