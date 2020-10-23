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
        private GUILayoutOption[] widthControl_100;

        private void OnEnable()
        {
            widthControl_150 = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(150) };
            widthControl_100 = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(100) };
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
            GUILayout.Label($"Create", widthControl_100);
            GUILayout.Label($"Get", widthControl_100);
            GUILayout.Label($"Recycle", widthControl_100);
            GUILayout.Label($"Destroy", widthControl_100);
            GUILayout.EndHorizontal();

            foreach(var i in records)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(i.name, widthControl_150);
                GUILayout.Label(i.type, widthControl_150);
                GUILayout.Label($"{i.timesCreate}", widthControl_100);
                GUILayout.Label($"{i.timesGet}", widthControl_100);
                GUILayout.Label($"{i.timesRecycle}", widthControl_100);
                GUILayout.Label($"{i.timesDestroy}", widthControl_100);
                GUILayout.EndHorizontal();
            }
        }


    }

}
