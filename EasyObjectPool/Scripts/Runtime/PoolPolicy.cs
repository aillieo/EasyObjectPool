using System;
using UnityEngine;

namespace AillieoUtils
{
    [Serializable]
    public class PoolPolicy
    {
        public int capacity = 4;
        public int sizeMax = 16;

        private static PoolPolicy defaultInstance;

        public static PoolPolicy defaultPolicy
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new PoolPolicy();
                    try
                    {
                        PoolPolicy preset = EasyObjectPoolConfig.Instance.defaultPoolPolicy;
                        defaultInstance.capacity = preset.capacity;
                        defaultInstance.sizeMax = preset.sizeMax;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                return defaultInstance;
            }

            set
            {
                defaultInstance = value;
            }
        }
    }
}
