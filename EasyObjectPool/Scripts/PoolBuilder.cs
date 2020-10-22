using System;

namespace AillieoUtils
{
    public class PoolBuilder<T>
    {
        private Func<T> createFunc;
        private Action<T> onGet;
        private Action<T> onRecycle;
        private Action<T> destroyFunc;
        private PoolPolicy policy;

        private void EnsurePoolPolicy()
        {
            if (this.policy == null)
            {
                this.policy = new PoolPolicy();
            }
        }

        internal PoolBuilder()
        {
        }

        public Pool<T> AsPool()
        {
            EnsurePoolPolicy();
            return new Pool<T>(policy, createFunc, onGet, onRecycle, destroyFunc);
        }

        public PoolBuilder<T> SetPolicy(PoolPolicy policy)
        {
            this.policy = policy;
            return this;
        }

        public PoolBuilder<T> SetReserveOnTrim(int reserveOnTrim)
        {
            EnsurePoolPolicy();
            this.policy.reserveOnTrim = reserveOnTrim;
            return this;
        }

        public PoolBuilder<T> SetSizeMax(int sizeMax)
        {
            EnsurePoolPolicy();
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
    }
}
