using System;
using UnityEngine;

namespace AillieoUtils
{
    [Serializable]
    public class GameObjectPoolPolicy
    {
        public int capacity = 4;
        public int sizeMax = 16;

        public float shrink1stInterval = 30;
        public float shrink1stRatio = 0.2f;

        public float shrink2ndInterval = 60;
        public float shrink2ndRatio = 0.1f;

        private static GameObjectPoolPolicy defaultInstance;

        public static GameObjectPoolPolicy defaultGameObjectPolicy
        {
            get
            {
                if (defaultInstance == null)
                {
                    defaultInstance = new GameObjectPoolPolicy();
                    try
                    {
                        int level = QualitySettings.GetQualityLevel();
                        GameObjectPoolPolicy preset = EasyObjectPoolConfig.Instance.defaultGameObjectPoolPolicies[level];
                        defaultInstance.capacity = preset.capacity;
                        defaultInstance.sizeMax = preset.sizeMax;
                        defaultInstance.shrink1stInterval = preset.shrink1stInterval;
                        defaultInstance.shrink1stRatio = preset.shrink1stRatio;
                        defaultInstance.shrink2ndInterval = preset.shrink2ndInterval;
                        defaultInstance.shrink2ndRatio = preset.shrink2ndRatio;
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
