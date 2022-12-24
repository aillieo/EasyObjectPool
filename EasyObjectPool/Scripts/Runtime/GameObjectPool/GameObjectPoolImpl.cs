using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;

namespace AillieoUtils
{
    internal class GameObjectPoolImpl : SingletonMonoBehaviour<GameObjectPoolImpl>
    {
        internal class GameObjectPoolInfo
        {
            public GameObject prefab;
            public Pool<GameObject> pool;
            public GameObjectPoolPolicy poolPolicy;

            public int instanceCount;
            public float lifecycleTimer;
            public float shrinkTimer;
        }

        private readonly Dictionary<GameObject, GameObjectPoolInfo> prefabLookUp = new Dictionary<GameObject, GameObjectPoolInfo>();
        private readonly Dictionary<GameObject, GameObjectPoolInfo> instanceLookUp = new Dictionary<GameObject, GameObjectPoolInfo>();
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private readonly Dictionary<GameObject, StackTrace> instanceRecords = new Dictionary<GameObject, StackTrace>();
#endif

        // remove invalid instances
        private const float autoCleanupInterval = 60f;

        // remove inactive pools
        private const float poolTimeout = 300f;
        private const float poolTimeoutCheckInterval = 1f;

        private const int subRootCount = 13;

        private float autoCleanupTimer = 0;
        private float poolTimeoutCheckTimer = 0;
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

            poolTimeoutCheckTimer += Time.deltaTime;

            if (poolTimeoutCheckTimer > poolTimeoutCheckInterval)
            {
                poolTimeoutCheckTimer -= poolTimeoutCheckInterval;
                InternalRemoveInactive();
            }

            foreach (var pair in Instance.prefabLookUp)
            {
                InternalCheckAndShrink(pair.Value);
            }
        }

        private GameObject PoolOnCreate(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        private void PoolOnGet(GameObject instance)
        {
            // instance.transform.SetParent(null, false);
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

            GameObjectPoolPolicy poolPolicy = GameObjectPoolPolicy.defaultGameObjectPolicy;

            return new GameObjectPoolInfo
            {
                prefab = prefab,
                pool = pool,
                poolPolicy = poolPolicy,
            };
        }

        internal GameObject InternalGet(GameObject prefab, Transform parent)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException($"{nameof(prefab)}");
            }

            if (!prefabLookUp.TryGetValue(prefab, out GameObjectPoolInfo poolInfo))
            {
                poolInfo = CreatePoolInfoForGameObject(prefab);
                prefabLookUp.Add(prefab, poolInfo);
            }

            GameObject instance = poolInfo.pool.Get();
            while (instance == null)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                UnityEngine.Debug.LogError($"Cached instance from pool<{poolInfo.prefab.name}> is externally destroyed.");
#endif
                instance = poolInfo.pool.Get();
            }

            instance.transform.SetParent(parent, false);
            poolInfo.lifecycleTimer = 0;
            poolInfo.instanceCount++;
            instanceLookUp[instance] = poolInfo;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            instanceRecords[instance] = new StackTrace(1);
#endif

            return instance;
        }

        internal void InternalRecycle(GameObject instance)
        {
            if (instanceLookUp.TryGetValue(instance, out GameObjectPoolInfo poolInfo))
            {
                if (instance != null)
                {
                    poolInfo.pool.Recycle(instance);
                    poolInfo.lifecycleTimer = 0;
                    poolInfo.instanceCount--;
                    instanceLookUp.Remove(instance);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    instanceRecords.Remove(instance);
#endif
                }
                else
                {
                    poolInfo.instanceCount--;
                    instanceLookUp.Remove(instance);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    if (instanceRecords.TryGetValue(instance, out StackTrace stackTrace))
                    {
                        UnityEngine.Debug.LogError($"Instance from pool<{poolInfo.prefab.name}> is destroyed, taken \n{stackTrace}");
                        instanceRecords.Remove(instance);
                    }
#endif
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Attempts to recycle an invalid gameObject {instance}: has been recycled or does not belong to this pool");
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
                item.Value.instanceCount--;
                instanceLookUp.Remove(item.Key);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (instanceRecords.TryGetValue(item.Key, out StackTrace stackTrace))
                {
                    UnityEngine.Debug.LogError($"Instance from pool <{item.Value.prefab.name}> is destroyed, taken \n{stackTrace}");
                    instanceRecords.Remove(item.Key);
                }
#endif
            }

            toRemove.Clear();
        }

        internal void InternalRemoveInactive()
        {
            toRemove.Clear();

            foreach (var pair in Instance.prefabLookUp)
            {
                pair.Value.lifecycleTimer += poolTimeoutCheckInterval;

                // 1.没有instance
                if (pair.Value.instanceCount > 0)
                {
                    continue;
                }

                // 2.lastAccess 超时
                if (pair.Value.lifecycleTimer < poolTimeout)
                {
                    continue;
                }

                // 3.删掉pool
                toRemove.Add(pair);
            }

            foreach (var item in toRemove)
            {
                item.Value.pool.Purge();
                prefabLookUp.Remove(item.Key);
            }

            toRemove.Clear();
        }

        internal void InternalCheckAndShrink(GameObjectPoolInfo poolInfo)
        {
            GameObjectPoolPolicy poolPolicy = poolInfo.poolPolicy;
            Pool<GameObject> pool = poolInfo.pool;

            int countInPool = pool.CountInPool();
            bool stage1 = countInPool > poolPolicy.capacity;
            float interval = stage1 ? poolPolicy.shrink1stInterval : poolPolicy.shrink2ndInterval;
            float ratio = stage1 ? poolPolicy.shrink1stRatio : poolPolicy.shrink2ndRatio;

            if (interval <= 0 || ratio <= 0)
            {
                return;
            }

            poolInfo.shrinkTimer += Time.deltaTime;

            if (poolInfo.shrinkTimer < interval)
            {
                return;
            }

            ratio = Mathf.Clamp(ratio, 0, 1);
            int removeCount = Mathf.CeilToInt(countInPool * ratio);
            int keep = countInPool - removeCount;
            keep = Mathf.Max(keep, 0);
            pool.Shrink(keep);
            poolInfo.shrinkTimer = 0;
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
                poolInfo.lifecycleTimer = 0;
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
