using System;
using System.Collections.Generic;

namespace AillieoUtils
{
    public struct ListAutoRecycleScope<T> : IDisposable 
    {
        internal static ListAutoRecycleScope<T> Create()
        {
            ListAutoRecycleScope<T> scope = default;
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
                throw new Exception($"{typeof(ListAutoRecycleScope<T>)} is disposed");
            }

            List<T> list = ListPool<T>.Get(expectedCapacity);
            recorder.Add(list);
            return list;
        }

        public void Recycle(List<T> list)
        {
            if (!alive)
            {
                throw new Exception($"{typeof(ListAutoRecycleScope<T>)} is disposed");
            }

            recorder.Remove(list);
            ListPool<T>.Recycle(list);
        }

        public void Dispose()
        {
            if(!alive)
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
        }
    }
}
