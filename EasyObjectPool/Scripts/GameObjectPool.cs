using System;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils
{
    public class GameObjectPool : SingletonMonoBehaviour<GameObjectPool>
    {
        private readonly Dictionary<GameObject, Pool<GameObject>> prefabLookUp = new Dictionary<GameObject, Pool<GameObject>>();
        private readonly Dictionary<GameObject, Pool<GameObject>> instanceLookUp = new Dictionary<GameObject, Pool<GameObject>>();
        private Task task;

        private void Awake()
        {
            gameObject.SetActive(false);
            task = Scheduler.Schedule(RemoveInvalid, 60f);

            Application.lowMemory += OnLowMemory;
        }

        private new void OnDestroy()
        {
            Application.lowMemory -= OnLowMemory;

            Scheduler.Unschedule(task);
            task = null;
            base.OnDestroy();
        }

        private void InternalDestroy(GameObject instance)
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

            return builder.SetCreateFunc(() => GameObject.Instantiate(prefab))
                .SetDestroyFunc(InternalDestroy)
                .SetOnRecycle(obj => obj.transform.SetParent(transform, false))
                .SetOnGet(obj => obj.transform.SetParent(null, false))
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
        
        private void RemoveInvalid()
        {
            foreach (var pair in instanceLookUp)
            {
                if (pair.Key == null)
                {
                    // destroyed external
                    // todo
                }
            }
        }

        public static GameObject Get(GameObject prefab)
        {
            return Instance.InternalGet(prefab);
        }

        public static void Recycle(GameObject instance)
        {
            Instance.InternalRecycle(instance);
        }
        
        public static void OnLowMemory()
        {
            foreach (var pair in Instance.prefabLookUp)
            {
                pair.Value.Trim();
            }
        }
    }
}
