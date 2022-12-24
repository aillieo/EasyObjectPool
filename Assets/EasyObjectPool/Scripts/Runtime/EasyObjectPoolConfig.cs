using System;
using UnityEngine;

namespace AillieoUtils
{
    [SettingsMenuPath(settingsPath)]
    internal class EasyObjectPoolConfig : SingletonScriptableObject<EasyObjectPoolConfig>
    {
        private const string settingsPath = "AillieoUtils/EasyObjectPoolConfig";

        [SerializeField]
        internal bool enableProfiler;
        [SerializeField]
        internal bool enableSafeMode;

        [SerializeField]
        internal PoolPolicy defaultPoolPolicy;

        [SerializeField]
        internal GameObjectPoolPolicy[] defaultGameObjectPoolPolicies;

        protected override void Awake()
        {
            base.Awake();

            if (defaultGameObjectPoolPolicies == null || defaultGameObjectPoolPolicies.Length == 0)
            {
                int qualityLevels = QualitySettings.names.Length;
                defaultGameObjectPoolPolicies = new GameObjectPoolPolicy[qualityLevels];
                for (int i = 0; i < qualityLevels; i++)
                {
                    defaultGameObjectPoolPolicies[i] = new GameObjectPoolPolicy();
                }
            }
        }
    }
}
