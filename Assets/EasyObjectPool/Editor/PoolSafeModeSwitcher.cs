using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Compilation;

namespace AillieoUtils
{
    public static class PoolSafeModeSwitcher
    {
        private static readonly string symbolEasyObjectPoolSafeMode = "EASY_OBJECT_POOL_SAFE_MODE";
        
#if EASY_OBJECT_POOL_SAFE_MODE
        [MenuItem("AillieoUtils/EasyObjectPool/DisableSafeMode")]
        static void DisableSafeMode()
        {
            var str = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            var symbols = str.Split(';');
            for(int i = 0; i < symbols.Length; ++i)
            {
                if(symbols[i].Trim() == symbolEasyObjectPoolSafeMode)
                {
                    symbols[i] = string.Empty;
                }
            }
            str = string.Concat(symbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, str);
            ReimportAllScripts();
        }
#else
        [MenuItem("AillieoUtils/EasyObjectPool/EnableSafeMode")]
        static void EnableSafeMode()
        {
            var str = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            str += $";{symbolEasyObjectPoolSafeMode}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, str);
            ReimportAllScripts();
        }
#endif

        [MenuItem("AillieoUtils/EasyObjectPool/EnableSafeMode", true)]
        [MenuItem("AillieoUtils/EasyObjectPool/DisableDevMode", true)]
        static bool SafeModeSwitcher()
        {
            return !Application.isPlaying;
        }

        private static void ReimportAllScripts()
        {
            CompilationPipeline.RequestScriptCompilation();
            //AssetDatabase.FindAssets("t:script").ToList().ForEach(s => AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(s)));
        }
        
    }
}