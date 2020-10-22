using System.Collections.Generic;

namespace AillieoUtils
{
    public static class ListPool<T>
    {
        private static Pool<List<T>> pool = Pool<List<T>>.Create().SetSizeMax(10).SetCreateFunc(() => { return new List<T>(); }).SetOnRecycle(list => list.Clear()).AsPool();

        public static List<T> Get()
        {
            return pool.Get();
        }

        public static void Recycle(List<T> list)
        {
            pool.Recycle(list);
        }
    }

    public static class DictPool<T, U>
    {
        private static Pool<Dictionary<T, U>> pool = Pool<Dictionary<T, U>>.Create().SetSizeMax(10).SetCreateFunc(() => { return new Dictionary<T, U>(); }).SetOnRecycle(dict => dict.Clear()).AsPool();

        public static Dictionary<T, U> Get()
        {
            return pool.Get();
        }

        public static void Recycle(Dictionary<T, U> dict)
        {
            pool.Recycle(dict);
        }
    }

    public static class QueuePool<T>
    {
        private static Pool<Queue<T>> pool = Pool<Queue<T>>.Create().SetSizeMax(10).SetCreateFunc(() => { return new Queue<T>(); }).SetOnRecycle(queue => queue.Clear()).AsPool();

        public static Queue<T> Get()
        {
            return pool.Get();
        }

        public static void Recycle(Queue<T> queue)
        {
            pool.Recycle(queue);
        }
    }

    public static class StackPool<T>
    {
        private static Pool<Stack<T>> pool = Pool<Stack<T>>.Create().SetSizeMax(10).SetCreateFunc(() => { return new Stack<T>(); }).SetOnRecycle(stack => stack.Clear()).AsPool();

        public static Stack<T> Get()
        {
            return pool.Get();
        }

        public static void Recycle(Stack<T> stack)
        {
            pool.Recycle(stack);
        }
    }

    public static class HashSetPool<T>
    {
        private static Pool<HashSet<T>> pool = Pool<HashSet<T>>.Create().SetSizeMax(10).SetCreateFunc(() => { return new HashSet<T>(); }).SetOnRecycle(set => set.Clear()).AsPool();

        public static HashSet<T> Get()
        {
            return pool.Get();
        }

        public static void Recycle(HashSet<T> set)
        {
            pool.Recycle(set);
        }
    }
}
