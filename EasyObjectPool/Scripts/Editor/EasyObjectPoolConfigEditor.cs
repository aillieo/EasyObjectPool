using System;
using UnityEditor;
using UnityEngine;

namespace AillieoUtils
{
    [CustomEditor(typeof(EasyObjectPoolConfig))]
    internal class EasyObjectPoolConfigEditor : Editor
    {
        private bool[] folders = Array.Empty<bool>();

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(EasyObjectPoolConfig.enableProfiler)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(EasyObjectPoolConfig.enableSafeMode)));

            EditorGUILayout.LabelField("Default pool policy:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(EasyObjectPoolConfig.defaultPoolPolicy)));

            EditorGUILayout.LabelField("Default GameObject pool policy for different quality settings:", EditorStyles.boldLabel);

            string[] qualityNames = QualitySettings.names;
            if (qualityNames.Length != folders.Length)
            {
                folders = new bool[qualityNames.Length];
            }

            SerializedProperty serializedProperty = serializedObject.FindProperty(nameof(EasyObjectPoolConfig.defaultGameObjectPoolPolicies));
            serializedProperty.arraySize = qualityNames.Length;

            for (int i = 0; i < folders.Length; ++i)
            {
                EditorGUILayout.BeginVertical("box");

                bool foldout = EditorGUILayout.Foldout(folders[i], qualityNames[i], true);
                folders[i] = foldout;
                if (foldout)
                {
                    EditorGUILayout.PropertyField(serializedProperty.GetArrayElementAtIndex(i));
                }

                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(PoolPolicy))]
    internal class PoolPolicyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel++;
            float height = EditorGUIUtility.singleLineHeight;
            position.height = height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(GameObjectPoolPolicy.capacity)));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(GameObjectPoolPolicy.sizeMax)));
            EditorGUI.indentLevel--;
        }
    }

    [CustomPropertyDrawer(typeof(GameObjectPoolPolicy))]
    internal class GameObjectPoolPolicyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 6f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel++;
            float height = EditorGUIUtility.singleLineHeight;
            position.height = height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(GameObjectPoolPolicy.capacity)));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(GameObjectPoolPolicy.sizeMax)));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(GameObjectPoolPolicy.shrink1stInterval)));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(GameObjectPoolPolicy.shrink1stRatio)));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(GameObjectPoolPolicy.shrink1stInterval)));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(GameObjectPoolPolicy.shrink2ndRatio)));
            EditorGUI.indentLevel--;
        }
    }
}
