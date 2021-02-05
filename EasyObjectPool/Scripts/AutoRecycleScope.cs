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
            disposed = false;
        }

        private bool disposed;
        private readonly Pool<T> pool;
        private HashSet<T> recorder;

        public T Get()
        {
            if (disposed)
            {
                throw new Exception($"{typeof(AutoRecycleScope<T>)} is disposed");
            }

            T obj = pool.Get();
            recorder.Add(obj);
            return obj;
        }

        public void Recycle(T obj)
        {
            if (disposed)
            {
                throw new Exception($"{typeof(AutoRecycleScope<T>)} is disposed");
            }

            recorder.Remove(obj);
            pool.Recycle(obj);
        }

        public void Dispose()
        {
            if(disposed)
            {
                return;
            }
            disposed = true;

            foreach(T item in recorder)
            {
                pool.Recycle(item);
            }
            recorder.Clear();

            HashSetPool<T>.Recycle(recorder);
        }
    }
}
