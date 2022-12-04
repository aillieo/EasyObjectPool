using System;

namespace AillieoUtils
{
    public class PoolBuilder<T> where T : class
    {
        private Func<T> createFunc;
        private Action<T> onGet;
        private Action<T> onRecycle;
        private Action<T> destroyFunc;
        private PoolPolicy policy;
        private string nameForProfiler;

        private void EnsurePoolPolicy(bool createNewInstance)
        {
            if (this.policy == null)
            {
                if (createNewInstance)
                {
                    this.policy = new PoolPolicy();
                }
                else
                {
                    this.policy = PoolPolicy.defaultInstance;
                }
            }
        }

        internal PoolBuilder()
        {
        }

        public Pool<T> AsPool()
        {
            EnsurePoolPolicy(false);
            return new Pool<T>(policy, createFunc, onGet, onRecycle, destroyFunc, nameForProfiler);
        }

        public PoolBuilder<T> SetPolicy(PoolPolicy policy)
        {
            this.policy = policy;
            return this;
        }

        public PoolBuilder<T> SetReserveOnTrim(int reserveOnTrim)
        {
            EnsurePoolPolicy(true);
            this.policy.reserveOnTrim = reserveOnTrim;
            return this;
        }

        public PoolBuilder<T> SetSizeMax(int sizeMax)
        {
            EnsurePoolPolicy(true);
            this.policy.sizeMax = sizeMax;
            return this;
        }

        public PoolBuilder<T> SetCreateFunc(Func<T> createFunc)
        {
            this.createFunc = createFunc;
            return this;
        }

        public PoolBuilder<T> SetOnGet(Action<T> onGet)
        {
            this.onGet = onGet;
            return this;
        }

        public PoolBuilder<T> SetOnRecycle(Action<T> onRecycle)
        {
            this.onRecycle = onRecycle;
            return this;
        }

        public PoolBuilder<T> SetDestroyFunc(Action<T> destroyFunc)
        {
            this.destroyFunc = destroyFunc;
            return this;
        }

        public PoolBuilder<T> SetNameForProfiler(string nameForProfiler)
        {
            this.nameForProfiler = nameForProfiler;
            return this;
        }
    }
}
