using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AillieoUtils
{
    internal class GameObjectPoolImpl : SingletonMonoBehaviour<GameObjectPoolImpl>
    {
        internal class GameObjectPoolInfo
        {
            public Pool<GameObject> pool;
            public GameObjectPoolPolicy poolPolicy;

            public float lastAccess;
            public float shrinkTimer;
        }

        private readonly Dictionary<GameObject, GameObjectPoolInfo> prefabLookUp = new Dictionary<GameObject, GameObjectPoolInfo>();
        private readonly Dictionary<GameObject, GameObjectPoolInfo> instanceLookUp = new Dictionary<GameObject, GameObjectPoolInfo>();

        private const float autoCleanupInterval = 60f;
        private const int subRootCount = 13;

        private float autoCleanupTimer = 0;
        private int rootIndex;

        private Func<GameObject, GameObject> delCreate;
        private Action<GameObject> delDestroy;
        private Action<GameObject> delRecycle;
        private Action<GameObject> delGet;

        protected override void Awake()
        {
            base.Awake();

            Application.lowMemory += OnLowMemory;

            foreach (var i in Enumerable.Range(0, subRootCount))
            {
                GameObject subRoot = new GameObject($"{i}");
                subRoot.SetActive(false);
                subRoot.transform.SetParent(this.transform, false);
            }

            delCreate = this.PoolOnCreate;
            delDestroy = this.PoolOnDestroy;
            delGet = this.PoolOnGet;
            delRecycle = this.PoolOnRecycle;
        }

        protected override void OnDestroy()
        {
            Application.lowMemory -= OnLowMemory;

            base.OnDestroy();
        }

        private void Update()
        {
            autoCleanupTimer += Time.deltaTime;

            if (autoCleanupTimer > autoCleanupInterval)
            {
                autoCleanupTimer -= autoCleanupInterval;
                InternalRemoveInvalid();
            }

            foreach (var pair in Instance.prefabLookUp)
            {
                // pair.Value.Shrink();
            }
        }

        private GameObject PoolOnCreate(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        private void PoolOnGet(GameObject instance)
        {
            instance.transform.SetParent(null, false);
        }

        private void PoolOnRecycle(GameObject instance)
        {
            rootIndex++;
            rootIndex %= subRootCount;
            Transform root = this.transform.GetChild(rootIndex);

            instance.transform.SetParent(root, false);
        }

        private void PoolOnDestroy(GameObject instance)
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(instance);
#else
            GameObject.Destroy(instance);
#endif
        }

        private GameObjectPoolInfo CreatePoolInfoForGameObject(GameObject prefab)
        {
            var builder = Pool<GameObject>.Create();
            GameObjectPoolPolicyBehaviour policyBehaviour = prefab.GetComponent<GameObjectPoolPolicyBehaviour>();
            if (policyBehaviour != null)
            {
                builder.SetCapacity(policyBehaviour.poolPolicy.capacity);
                builder.SetSizeMax(policyBehaviour.poolPolicy.sizeMax);
            }

            var pool = builder.SetCreateFunc(() => delCreate(prefab))
                .SetDestroyFunc(delDestroy)
                .SetOnRecycle(delRecycle)
                .SetOnGet(delGet)
                .SetNameForProfiler($"GO|{prefab.name}")
                .AsPool();

            return new GameObjectPoolInfo
            {
                pool = pool,
            };
        }

        internal GameObject InternalGet(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException($"invalid argument: {nameof(prefab)}");
            }

            if (!prefabLookUp.TryGetValue(prefab, out GameObjectPoolInfo poolInfo))
            {
                poolInfo = CreatePoolInfoForGameObject(prefab);
                prefabLookUp.Add(prefab, poolInfo);
            }

            GameObject instance = poolInfo.pool.Get();
            instanceLookUp[instance] = poolInfo;
            return instance;
        }

        internal void InternalRecycle(GameObject instance)
        {
            if (instanceLookUp.TryGetValue(instance, out GameObjectPoolInfo poolInfo))
            {
                poolInfo.pool.Recycle(instance);
                instanceLookUp.Remove(instance);
            }
            else
            {
                Debug.LogWarning($"attempts to recycle an invalid gameObject {instance}: has been recycled or does not belong to this pool");
            }
        }

        private List<KeyValuePair<GameObject, GameObjectPoolInfo>> toRemove = new List<KeyValuePair<GameObject, GameObjectPoolInfo>>();

        internal void InternalRemoveInvalid()
        {
            toRemove.Clear();
            foreach (var pair in instanceLookUp)
            {
                if (pair.Key == null)
                {
                    // destroyed external
                    toRemove.Add(pair);
                }
            }

            foreach (var item in toRemove)
            {
                instanceLookUp.Remove(item.Key);
                item.Value.pool.Recycle(item.Key);
            }

            toRemove.Clear();
        }

        internal void OnLowMemory()
        {
            foreach (var pair in Instance.prefabLookUp)
            {
                pair.Value.pool.Shrink();
            }
        }

        internal void Prepare(GameObject prefab, int count)
        {
            if (Instance.prefabLookUp.TryGetValue(prefab, out GameObjectPoolInfo poolInfo))
            {
                poolInfo.pool.Prepare(count);
            }
        }

        internal void Shrink(GameObject prefab)
        {
            if (Instance.prefabLookUp.TryGetValue(prefab, out GameObjectPoolInfo poolInfo))
            {
                poolInfo.pool.Shrink();
            }
        }
    }
}
