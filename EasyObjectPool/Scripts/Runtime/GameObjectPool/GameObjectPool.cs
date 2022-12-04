using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AillieoUtils
{
    public class GameObjectPool : SingletonMonoBehaviour<GameObjectPool>
    {
        private readonly Dictionary<GameObject, Pool<GameObject>> prefabLookUp = new Dictionary<GameObject, Pool<GameObject>>();
        private readonly Dictionary<GameObject, Pool<GameObject>> instanceLookUp = new Dictionary<GameObject, Pool<GameObject>>();

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

        private Pool<GameObject> CreatePoolForGameObject(GameObject prefab)
        {
            var builder = Pool<GameObject>.Create();
            var policy = prefab.GetComponent<GameObjectPoolPolicy>();
            if (policy != null)
            {
                builder.SetPolicy(policy.poolPolicy);
            }

            return builder.SetCreateFunc(() => delCreate(prefab))
                .SetDestroyFunc(delDestroy)
                .SetOnRecycle(delRecycle)
                .SetOnGet(delGet)
                .SetNameForProfiler($"GO|{prefab.name}")
                .AsPool();
        }

        private GameObject InternalGet(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new Exception($"invalid argument: {nameof(prefab)}");
            }

            if (!prefabLookUp.TryGetValue(prefab, out Pool<GameObject> pool))
            {
                pool = CreatePoolForGameObject(prefab);
                prefabLookUp.Add(prefab, pool);
            }

            GameObject instance = pool.Get();
            instanceLookUp[instance] = pool;
            return instance;
        }

        private void InternalRecycle(GameObject instance)
        {
            if (instanceLookUp.TryGetValue(instance, out Pool<GameObject> pool))
            {
                pool.Recycle(instance);
                instanceLookUp.Remove(instance);
            }
            else
            {
                Debug.LogWarning($"attempts to recycle an invalid gameObject {instance}: has been recycled or does not belong to this pool");
            }
        }

#if UNITY_EDITOR
        [MenuItem("AillieoUtils/EasyObjectPool/GameObjectPool/RemoveInvalid")]
#endif
        public static void RemoveInvalid()
        {
            Instance.InternalRemoveInvalid();
        }

        private List<KeyValuePair<GameObject, Pool<GameObject>>> toRemove = new List<KeyValuePair<GameObject, Pool<GameObject>>>();

        private void InternalRemoveInvalid()
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
                item.Value.Recycle(item.Key);
            }

            toRemove.Clear();
        }

        public static GameObject Get(GameObject prefab)
        {
#if UNITY_EDITOR
            if (!PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                Debug.LogWarning($"Create a pool with a non-prefab GameObject '{prefab.name}' may lead to unexpected behaviour");
            }
#endif
            return Instance.InternalGet(prefab);
        }

        public static void Recycle(GameObject instance)
        {
            Instance.InternalRecycle(instance);
        }

#if UNITY_EDITOR
        [MenuItem("AillieoUtils/EasyObjectPool/GameObjectPool/OnLowMemory")]
#endif
        public static void OnLowMemory()
        {
            foreach (var pair in Instance.prefabLookUp)
            {
                pair.Value.Trim();
            }
        }

        public static void Prepare(GameObject prefab, int count)
        {
            if (Instance.prefabLookUp.TryGetValue(prefab, out Pool<GameObject> pool))
            {
                pool.Prepare(count);
            }
        }

        public static void Trim(GameObject prefab)
        {
            if (Instance.prefabLookUp.TryGetValue(prefab, out Pool<GameObject> pool))
            {
                pool.Trim();
            }
        }
    }
}
