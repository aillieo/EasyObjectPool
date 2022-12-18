using System;
using UnityEngine;

namespace AillieoUtils
{
    [Serializable]
    public class GameObjectPoolPolicy
    {
        public int capacity = 4;
        public int sizeMax = 16;

        public int shrink1stInterval;
        public int shrink1stRatio;

        public int shrink2ndInterval;
        public int shrink2ndRatio;

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
