using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AillieoUtils
{
    public class GameObjectPool
    {
#if UNITY_EDITOR
        [MenuItem("AillieoUtils/EasyObjectPool/GameObjectPool/RemoveInvalid")]
#endif
        public static void RemoveInvalid()
        {
            GameObjectPoolImpl.Instance.InternalRemoveInvalid();
        }

        public static GameObject Get(GameObject prefab, Transform parent = null)
        {
#if UNITY_EDITOR
            if (!PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                Debug.LogWarning($"Create a pool with a non-prefab GameObject '{prefab.name}' may lead to unexpected behaviour");
            }
#endif
            return GameObjectPoolImpl.Instance.InternalGet(prefab, parent);
        }

        public static void Recycle(GameObject instance)
        {
            GameObjectPoolImpl.Instance.InternalRecycle(instance);
        }

#if UNITY_EDITOR
        [MenuItem("AillieoUtils/EasyObjectPool/GameObjectPool/OnLowMemory")]
#endif
        public static void OnLowMemory()
        {
            GameObjectPoolImpl.Instance.OnLowMemory();
        }

        public static void Prepare(GameObject prefab, int count)
        {
            GameObjectPoolImpl.Instance.Prepare(prefab, count);
        }

        public static void Shrink(GameObject prefab)
        {
            GameObjectPoolImpl.Instance.Shrink(prefab);
        }
    }
}
