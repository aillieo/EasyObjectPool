using System;
using System.Collections.Generic;

namespace AillieoUtils
{
    public class Pool<T>
    {
        private readonly Stack<T> stack;

        private readonly Func<T> createFunc;
        private readonly Action<T> destroyFunc;

        private readonly Action<T> onGet;
        private readonly Action<T> onRecycle;

        private readonly PoolPolicy policy;

        internal Pool(PoolPolicy policy, Func<T> createFunc, Action<T> onGet, Action<T> onRecycle, Action<T> destroyFunc)
        {
            this.stack = new Stack<T>(policy.reserveOnTrim);
            this.policy = policy;
            this.createFunc = createFunc;
            this.onGet = onGet;
            this.onRecycle = onRecycle;
            this.destroyFunc = destroyFunc;
        }

        public static PoolBuilder<T> Create()
        {
            return new PoolBuilder<T>();
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
            return item;
        }

        public void Recycle(T item)
        {
            onRecycle?.Invoke(item);
            if (stack.Count < policy.sizeMax)
            {
                stack.Push(item);
            }
            else
            {
                destroyFunc?.Invoke(item);
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
                destroyFunc?.Invoke(item);
            }
        }

        public void Purge()
        {
            Trim(0);
        }

        private T CreateNewItem()
        {
            if (createFunc != null)
            {
                return createFunc();
            }
            else
            {
                return Activator.CreateInstance<T>();
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
