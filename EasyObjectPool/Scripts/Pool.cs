using System;
using System.Collections.Generic;
using System.Reflection;

namespace AillieoUtils
{
    public class Pool<T> where T : class
    {

#if EASY_OBJECT_POOL_SAFE_MODE
        public static readonly bool SAFE_MODE = true;
#else
        public static readonly bool SAFE_MODE = false;
#endif

        private readonly Stack<T> stack;

        private readonly Func<T> createFunc;
        private readonly Action<T> destroyFunc;

        private readonly Action<T> onGet;
        private readonly Action<T> onRecycle;

        internal readonly PoolPolicy policy;

        internal readonly string nameForProfiler;

#if EASY_OBJECT_POOL_SAFE_MODE
        private HashSet<T> validationSet = new HashSet<T>();
#endif

        internal Pool(PoolPolicy policy, Func<T> createFunc, Action<T> onGet, Action<T> onRecycle, Action<T> destroyFunc, string nameForProfiler = null)
        {
            this.stack = new Stack<T>(policy.reserveOnTrim);
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

        public static PoolBuilder<T> Create()
        {
            return new PoolBuilder<T>();
        }

        public static Pool<R> CreateDefault<R>() where R : class, T, IPoolable, new()
        {
            PoolBuilder<R> builder = Pool<R>.Create();
            builder.SetOnGet(o => o.OnGet());
            builder.SetOnRecycle(o => o.OnRecycle());
            var attr = typeof(R).GetCustomAttribute<PoolStrategyAttribute>(true);
            if (attr != null)
            {
                builder.SetPolicy(new PoolPolicy()
                {
                    sizeMax = attr.sizeMax,
                    reserveOnTrim = attr.reserveOnTrim,
                });
            }

            return builder.AsPool();
        }

        public void Trim()
        {
            Trim(policy.reserveOnTrim);
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
            PoolProfiler.Report(this, PoolProfiler.PoolAction.Get, stack.Count);

#if EASY_OBJECT_POOL_SAFE_MODE
            validationSet.Add(item);
#endif
            return item;
        }

        public void Recycle(T item)
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

            onRecycle?.Invoke(item);
            if (stack.Count < policy.sizeMax)
            {
                stack.Push(item);
                PoolProfiler.Report(this, PoolProfiler.PoolAction.Recycle, stack.Count);
            }
            else
            {
                PoolProfiler.Report(this, PoolProfiler.PoolAction.Recycle, stack.Count);
                destroyFunc?.Invoke(item);
                PoolProfiler.Report(this, PoolProfiler.PoolAction.Destroy, stack.Count);
            }
        }

        public void Prepare(int count)
        {
            while(stack.Count < count)
            {
                stack.Push(CreateNewItem());
            }
        }

        public void Trim(int keepCount)
        {
            while (stack.Count > keepCount)
            {
                T item = stack.Pop();
                onRecycle?.Invoke(item);
                PoolProfiler.Report(this, PoolProfiler.PoolAction.Recycle, stack.Count);
                destroyFunc?.Invoke(item);
                PoolProfiler.Report(this, PoolProfiler.PoolAction.Destroy, stack.Count);
            }
        }

        public void Purge()
        {
            Trim(0);
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
            PoolProfiler.Report(this, PoolProfiler.PoolAction.Create, stack.Count);
            return newInstance;
        }

        public AutoRecycleScope<T> GetScope()
        {
            return new AutoRecycleScope<T>(this);
        }
    }
}
