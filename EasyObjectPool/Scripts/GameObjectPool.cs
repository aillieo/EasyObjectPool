using UnityEngine;

namespace AillieoUtils
{
    public class GameObjectPool
    {
        public static GameObject Instantiate(GameObject prefab)
        {
            return Object.Instantiate(prefab);
        }

        public static void Recycle(GameObject instance)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(instance);
#else
            Object.Destroy(instance);
#endif
        }
    }
}
