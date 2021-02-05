using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace AillieoUtils
{
    public class LocalGameObjectPool
    {
        private readonly Pool<GameObject> pool;

        public LocalGameObjectPool(GameObject prefab, Action<GameObject> onGet, Action<GameObject> onRecycle)
        {
            Assert.IsNotNull(prefab, $"invalid {nameof(prefab)}");
            Assert.IsNotNull(onGet, $"invalid {nameof(onGet)}");
            Assert.IsNotNull(onRecycle, $"invalid {nameof(onRecycle)}");
            var builder = Pool<GameObject>.Create();
            GameObjectPoolPolicy policy = prefab.GetComponent<GameObjectPoolPolicy>();
            if (policy != null)
            {
                builder.SetPolicy(policy.poolPolicy);
            }

            pool = builder.SetCreateFunc(() => GameObject.Instantiate(prefab))
                .SetOnGet(onGet)
                .SetOnRecycle(onRecycle)
                .SetDestroyFunc(obj => {
#if UNITY_EDITOR
                    GameObject.DestroyImmediate(obj);
#else
                    GameObject.Destroy(obj);
#endif
                })
                .SetNameForProfiler($"LGO|{prefab.name}")
                .AsPool();
        }

        public LocalGameObjectPool(GameObject prefab, Transform poolRootNode)
        {
            Assert.IsNotNull(prefab, $"invalid {nameof(prefab)}");
            Assert.IsNotNull(poolRootNode, $"invalid {nameof(poolRootNode)}");
            var builder = Pool<GameObject>.Create();
            GameObjectPoolPolicy policy = prefab.GetComponent<GameObjectPoolPolicy>();
            if (policy != null)
            {
                builder.SetPolicy(policy.poolPolicy);
            }

            pool = builder.SetCreateFunc(() => GameObject.Instantiate(prefab))
                .SetOnGet(obj => obj.transform.SetParent(null, false))
                .SetOnRecycle(obj => obj.transform.SetParent(poolRootNode, false))
                .SetDestroyFunc(obj => {
#if UNITY_EDITOR
                    GameObject.DestroyImmediate(obj);
#else
                    GameObject.Destroy(obj);
#endif
                })
                .SetNameForProfiler($"LGO|{prefab.name}({poolRootNode.name})")
                .AsPool();
        }

        public GameObject Get()
        {
            return pool.Get();
        }

        public void Recycle(GameObject instance)
        {
            pool.Recycle(instance);
        }

        public AutoRecycleScope<GameObject> GetAutoRecycleScope()
        {
            return pool.GetAutoRecycleScope();
        }

        public void Purge()
        {
            pool.Purge();
        }

        public void Prepare(int count)
        {
            pool.Prepare(count);
        }

        public void Trim()
        {
            pool.Trim();
        }
    }
}
