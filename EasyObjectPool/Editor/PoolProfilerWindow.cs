using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AillieoUtils
{
    public class PoolProfilerWindow : EditorWindow
    {

        [MenuItem("AillieoUtils/EasyObjectPool/Profiler")]
        public static void Open()
        {
            GetWindow<PoolProfilerWindow>("EasyObjectPool Profiler");
        }

        private readonly List<PoolProfiler.ProfilerInfo> records = new List<PoolProfiler.ProfilerInfo>();


        private GUILayoutOption[] widthControl_150;
        private GUILayoutOption[] widthControl_80;
        private Vector2 scrollPos;

        private void OnEnable()
        {
            widthControl_150 = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(150) };
            widthControl_80 = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(80) };
        }

        private void OnGUI()
        {
            PoolProfiler.RetrieveRecords(records);

            if(records.Count == 0)
            {
                GUILayout.Label("No active pool found.");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", widthControl_150);
            GUILayout.Label("Type", widthControl_150);
            GUILayout.Label("Policy", widthControl_150);
            GUILayout.Label($"Create", widthControl_80);
            GUILayout.Label($"Get", widthControl_80);
            GUILayout.Label($"Recycle", widthControl_80);
            GUILayout.Label($"Destroy", widthControl_80);
            GUILayout.EndHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach(var i in records)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(i.name, widthControl_150);
                GUILayout.Label(i.type, widthControl_150);
                GUILayout.Label($"{i.policy}", widthControl_150);
                GUILayout.Label($"{i.timesCreate}", widthControl_80);
                GUILayout.Label($"{i.timesGet}", widthControl_80);
                GUILayout.Label($"{i.timesRecycle}", widthControl_80);
                GUILayout.Label($"{i.timesDestroy}", widthControl_80);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }


    }

}
