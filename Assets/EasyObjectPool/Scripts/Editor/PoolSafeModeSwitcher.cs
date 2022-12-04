using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace AillieoUtils
{
    public static class PoolSafeModeSwitcher
    {
        private static readonly string symbolEasyObjectPoolSafeMode = "EASY_OBJECT_POOL_SAFE_MODE";

        private static readonly bool safeModeEnabled
#if EASY_OBJECT_POOL_SAFE_MODE
            = true;
#else
            = false;
#endif

        [MenuItem("AillieoUtils/EasyObjectPool/SafeMode/On", true)]
        [MenuItem("AillieoUtils/EasyObjectPool/SafeMode/Off", true)]
        private static bool SafeModeSwitcherValidate()
        {
            Menu.SetChecked("AillieoUtils/EasyObjectPool/SafeMode/Off", !safeModeEnabled);
            Menu.SetChecked("AillieoUtils/EasyObjectPool/SafeMode/On", safeModeEnabled);

            return !Application.isPlaying;
        }

        [MenuItem("AillieoUtils/EasyObjectPool/SafeMode/On")]
        private static void EnableSafeMode()
        {
            var str = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            str += $";{symbolEasyObjectPoolSafeMode}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, str);
            ReimportAllScripts();
        }

        [MenuItem("AillieoUtils/EasyObjectPool/SafeMode/Off")]
        private static void DisableSafeMode()
        {
            var str = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            var symbols = str.Split(';');
            for (int i = 0; i < symbols.Length; ++i)
            {
                if (symbols[i].Trim() == symbolEasyObjectPoolSafeMode)
                {
                    symbols[i] = string.Empty;
                }
            }
            str = string.Concat(symbols);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, str);
            ReimportAllScripts();
        }

        private static void ReimportAllScripts()
        {
            CompilationPipeline.RequestScriptCompilation();
            //AssetDatabase.FindAssets("t:script").ToList().ForEach(s => AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(s)));
        }
        
    }
}
