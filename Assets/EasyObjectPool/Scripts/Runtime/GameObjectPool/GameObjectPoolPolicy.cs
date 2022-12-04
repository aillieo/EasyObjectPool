using UnityEngine;

namespace AillieoUtils
{
    [DisallowMultipleComponent]
    public class GameObjectPoolPolicy : MonoBehaviour
    {
        public readonly PoolPolicy poolPolicy = new PoolPolicy();
    }
}
