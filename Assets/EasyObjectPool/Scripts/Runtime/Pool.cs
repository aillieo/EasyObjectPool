using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AillieoUtils
{
    public class Pool<T>
        where T : class
    {
        private readonly Stack<T> stack;

        private readonly Func<T> createFunc;
        private readonly Action<T> destroyFunc;

        private readonly Action<T> onGet;
        private readonly Action<T> onRecycle;

        internal readonly PoolPolicy policy;

        internal readonly string nameForProfiler;

        internal readonly PoolStatisticInfo statisticInfo;

        private HashSet<T> validationSet = new HashSet<T>();

        internal Pool(PoolPolicy policy, Func<T> createFunc, Action<T> onGet, Action<T> onRecycle, Action<T> destroyFunc, string nameForProfiler = null)
        {
            this.stack = new Stack<T>(policy.capacity);
            this.policy = policy;
            this.createFunc = createFunc;
            this.onGet = onGet;
            this.onRecycle = onRecycle;
            this.destroyFunc = destroyFunc;
            if (string.IsNullOrWhiteSpace(nameForProfiler))
            {
                nameForProfiler = $"Pool<{typeof(T).Name}>";
            }

            this.nameForProfiler = nameForProfiler;
        }

        public static Pool<T> CreateDefault()
        {
            PoolBuilder<T> builder = Create();
            if (typeof(IPoolable).IsAssignableFrom(typeof(T)))
            {
                builder.SetOnGet(o => (o as IPoolable)?.OnGet());
                builder.SetOnRecycle(o => (o as IPoolable)?.OnRecycle());
            }

            var attr = typeof(T).GetCustomAttribute<PoolPolicyAttribute>(true);
            if (attr != null)
            {
                builder.SetPolicy(new PoolPolicy()
                {
                    sizeMax = attr.sizeMax,
                    capacity = attr.capacity,
                });
            }

            return builder.AsPool();
        }

        public static PoolBuilder<T> Create()
        {
            return new PoolBuilder<T>();
        }

        public void Shrink()
        {
            Shrink(policy.capacity);
        }

        public T Get()
        {
            T item;
            if (stack.Count == 0)
            {
                item = CreateNewItem();
            }
            else
            {
                item = stack.Pop();
            }

            onGet?.Invoke(item);
            PoolProfiler.Report(this, PoolAction.Get, stack.Count);

            if (EasyObjectPoolConfig.Instance.enableSafeMode)
            {
                validationSet.Add(item);
            }

            return item;
        }

        public void Recycle(T item)
        {
            if (EasyObjectPoolConfig.Instance.enableSafeMode)
            {
                if (validationSet.Contains(item))
                {
                    validationSet.Remove(item);
                }
                else
                {
                    throw new Exception("attempts to recycle an invalid object: has been recycled or does not belong to this pool");
                }
            }

            onRecycle?.Invoke(item);
            if (stack.Count < policy.sizeMax)
            {
                stack.Push(item);
                PoolProfiler.Report(this, PoolAction.Recycle, stack.Count);
            }
            else
            {
                PoolProfiler.Report(this, PoolAction.Recycle, stack.Count);
                destroyFunc?.Invoke(item);
                PoolProfiler.Report(this, PoolAction.Destroy, stack.Count);
            }
        }

        public void Prepare(int count)
        {
            while (stack.Count < count)
            {
                stack.Push(CreateNewItem());
            }
        }

        public void Shrink(int keepCount)
        {
            keepCount = Mathf.Max(keepCount, 0);
            while (stack.Count > keepCount)
            {
                T item = stack.Pop();
                destroyFunc?.Invoke(item);
                PoolProfiler.Report(this, PoolAction.Destroy, stack.Count);
            }
        }

        public void Purge()
        {
            Shrink(0);
        }

        public int CountInPool()
        {
            return stack.Count;
        }

        private T CreateNewItem()
        {
            T newInstance = default;
            if (createFunc != null)
            {
                newInstance = createFunc();
            }
            else
            {
                newInstance = Activator.CreateInstance<T>();
            }

            PoolProfiler.Report(this, PoolAction.Create, stack.Count);
            return newInstance;
        }

        public PoolScope<T> GetScope()
        {
            return new PoolScope<T>(this);
        }
    }
}
