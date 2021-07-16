using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AillieoUtils
{
    public static class ListPool<T>
    {
        private const int subPoolCount = 3;
        private static readonly Stack<List<T>>[] stacks = new Stack<List<T>>[] {
            new Stack<List<T>>(),
            new Stack<List<T>>(),
            new Stack<List<T>>()
        };
        private static readonly int[] thresholds = new int[] {
            512,
            64,
            0
        };
        private static readonly int[] poolSizes = new int[] {
            16,
            32,
            32
        };

        private static readonly PoolPolicy policy = new PoolPolicy() { sizeMax = poolSizes.Sum() };

#if EASY_OBJECT_POOL_SAFE_MODE
        private static readonly HashSet<List<T>> validationSet = new HashSet<List<T>>();
#endif

        public static List<T> Get(int expectedCapacity = 0)
        {
            for (int i = 0; i < subPoolCount; ++ i)
            {
                if (expectedCapacity > thresholds[i])
                {
                    Stack<List<T>> stack = stacks[i];
                    if (stack.Count > 0)
                    {
                        List<T> instance = stack.Pop();
                        OnGet(instance);
                        return instance;
                    }

                    if (i == subPoolCount - 1)
                    {
                        // 最后一层了
                        List<T> instance = new List<T>();
                        OnCreate(instance);
                        OnGet(instance);
                        return instance;
                    }
                }
            }
            throw new ArgumentException($"invalid params: {nameof(expectedCapacity)}={expectedCapacity}");
        }

        public static void Recycle(List<T> list)
        {
            list.Clear();

            int capacity = list.Capacity;
            for (int i = 0; i < subPoolCount; ++i)
            {
                if (capacity > thresholds[i])
                {
                    Stack<List<T>> stack = stacks[i];
                    if (stack.Count < poolSizes[i])
                    {
                        OnRecycle(list);
                        stack.Push(list);
                        return;
                    }

                    if (i == subPoolCount - 1)
                    {
                        // 最后一层了
                        OnRecycle(list);
                        OnDestroy(list);
                        return;
                    }
                }
            }
        }

        public static ListAutoRecycleScope<List<T>> GetScope()
        {
            ListAutoRecycleScope<List<T>> scope = new ListAutoRecycleScope<List<T>>();
            return scope;
        }

        public static int CountInPool()
        {
            int count = 0;
            for (int i = 0; i < subPoolCount; ++ i)
            {
                count += stacks[i].Count;
            }
            return count;
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("EASY_OBJECT_POOL_SAFE_MODE")]
        private static void OnGet(List<T> item)
        {
#if EASY_OBJECT_POOL_SAFE_MODE
            validationSet.Add(item);
#endif
            PoolProfiler.Report<List<T>>($"ListPool<{typeof(T).Name}>", policy, PoolProfiler.PoolAction.Get, CountInPool());
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("EASY_OBJECT_POOL_SAFE_MODE")]
        private static void OnCreate(List<T> item)
        {
            PoolProfiler.Report<List<T>>($"ListPool<{typeof(T).Name}>", policy, PoolProfiler.PoolAction.Create, CountInPool());
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("EASY_OBJECT_POOL_SAFE_MODE")]
        private static void OnRecycle(List<T> item)
        {
#if EASY_OBJECT_POOL_SAFE_MODE
            if (validationSet.Contains(item))
            {
                validationSet.Remove(item);
            }
            else
            {
                throw new Exception("attempts to recycle an invalid object: has been recycled or does not belong to this pool");
            }
#endif
            PoolProfiler.Report<List<T>>($"ListPool<{typeof(T).Name}>", policy, PoolProfiler.PoolAction.Recycle, CountInPool());
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("EASY_OBJECT_POOL_SAFE_MODE")]
        private static void OnDestroy(List<T> item)
        {
            PoolProfiler.Report<List<T>>($"ListPool<{typeof(T).Name}>", policy, PoolProfiler.PoolAction.Destroy, CountInPool());
        }
    }
}
