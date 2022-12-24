using System;
using System.Collections.Generic;

namespace AillieoUtils
{
    public struct ListPoolScope<T> : IDisposable
    {
        internal static ListPoolScope<T> Create()
        {
            ListPoolScope<T> scope = default;
            scope.recorder = HashSetPool<List<T>>.Get();
            scope.alive = true;
            return scope;
        }

        private bool alive;
        private HashSet<List<T>> recorder;

        public List<T> Get(int expectedCapacity = 0)
        {
            if (!alive)
            {
                throw new InvalidOperationException($"{typeof(ListPoolScope<T>)} is disposed");
            }

            List<T> list = ListPool<T>.Get(expectedCapacity);
            recorder.Add(list);
            return list;
        }

        public void Recycle(List<T> list)
        {
            if (!alive)
            {
                throw new InvalidOperationException($"{typeof(ListPoolScope<T>)} is disposed");
            }

            recorder.Remove(list);
            ListPool<T>.Recycle(list);
        }

        public void Dispose()
        {
            if (!alive)
            {
                return;
            }

            alive = false;

            foreach (List<T> list in recorder)
            {
                ListPool<T>.Recycle(list);
            }

            recorder.Clear();

            HashSetPool<List<T>>.Recycle(recorder);

            recorder = null;
        }
    }
}
