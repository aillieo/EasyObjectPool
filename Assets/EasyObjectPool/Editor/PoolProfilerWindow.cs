using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static AillieoUtils.PoolProfiler;

namespace AillieoUtils
{
    public class PoolProfilerWindow : EditorWindow
    {

        [MenuItem("AillieoUtils/EasyObjectPool/Profiler")]
        public static void Open()
        {
            GetWindow<PoolProfilerWindow>("EasyObjectPool Profiler");
        }

        private readonly List<ProfilerInfo> records = new List<ProfilerInfo>();


        private GUILayoutOption[] widthControl_150;
        private GUILayoutOption[] widthControl_80;
        private GUIStyle colorRed;
        private Vector2 scrollPos;
        private static readonly float footHeight = EditorGUIUtility.singleLineHeight * 2f;

        private void OnEnable()
        {
            widthControl_150 = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(150) };
            widthControl_80 = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Width(80) };
        }

        private void OnGUI()
        {
            PoolProfiler.RetrieveRecords(records);

            if (records.Count == 0)
            {
                GUILayout.Label("No active pool found.");
                return;
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("OnLowMemory"))
            {
                GameObjectPool.OnLowMemory();
            }

            if (GUILayout.Button("RemoveInvalid"))
            {
                GameObjectPool.RemoveInvalid();
            }

            EditorGUILayout.EndHorizontal();

            DrawHeader();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (colorRed == null)
            {
                colorRed = new GUIStyle("label") { normal = new GUIStyleState() { textColor = Color.red } };
            }

            foreach (var i in records)
            {
                DrawEntry(i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", widthControl_150);
            GUILayout.Label("Type", widthControl_150);
            GUILayout.Label("Policy", widthControl_150);
            GUILayout.Label($"InPool", widthControl_80);
            GUILayout.Label($"Create", widthControl_80);
            GUILayout.Label($"Get", widthControl_80);
            GUILayout.Label($"Recycle", widthControl_80);
            GUILayout.Label($"Destroy", widthControl_80);
            GUILayout.EndHorizontal();
        }

        private void DrawEntry(ProfilerInfo profilerInfo)
        {
            GUILayout.BeginHorizontal("box");

            GUILayout.Label(profilerInfo.name, widthControl_150);

            GUILayout.Label(profilerInfo.type, widthControl_150);

            bool badPolicy = PoolProfiler.IsBadPolicy(profilerInfo);
            if (badPolicy)
            {
                GUILayout.Label($"{profilerInfo.policy}", colorRed, widthControl_150);
            }
            else
            {
                GUILayout.Label($"{profilerInfo.policy}", widthControl_150);
            }

            GUILayout.Label($"{profilerInfo.countInPool}", widthControl_80);

            GUILayout.Label($"{profilerInfo.timesCreate}", widthControl_80);

            GUILayout.Label($"{profilerInfo.timesGet}", widthControl_80);

            bool leakPotential = PoolProfiler.IsLeakPotential(profilerInfo);
            bool errorRecycling = PoolProfiler.IsErrorRecycling(profilerInfo);
            if (leakPotential || errorRecycling)
            {
                GUILayout.Label($"{profilerInfo.timesRecycle}", colorRed, widthControl_80);
            }
            else
            {
                GUILayout.Label($"{profilerInfo.timesRecycle}", widthControl_80);
            }

            GUILayout.Label($"{profilerInfo.timesDestroy}", widthControl_80);

            GUILayout.EndHorizontal();
        }
    }

}
