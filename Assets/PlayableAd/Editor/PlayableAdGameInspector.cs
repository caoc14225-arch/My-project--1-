using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PlayableAd.Editor
{
    [CustomEditor(typeof(PlayableAdGame))]
    [CanEditMultipleObjects]
    public sealed class PlayableAdGameInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    DrawProperty(property);
                } while (property.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawProperty(SerializedProperty property)
        {
            bool isScript = property.propertyPath == "m_Script";
            bool drawFoldout = property.propertyType == SerializedPropertyType.Generic && property.hasVisibleChildren;
            if (!drawFoldout)
            {
                using (new EditorGUI.DisabledScope(isScript))
                {
                    EditorGUILayout.PropertyField(property, GetLabel(property), false);
                }
                return;
            }

            // PropertyField draws HeaderAttribute decorators for leaf fields. Foldouts are
            // rendered manually here, so only generic properties need a manual header.
            FieldInfo field = FindField(property.serializedObject.targetObject.GetType(), property.propertyPath);
            HeaderAttribute header = field != null ? field.GetCustomAttribute<HeaderAttribute>() : null;
            if (header != null)
                EditorGUILayout.LabelField(header.header, EditorStyles.boldLabel);

            Rect foldoutRect = EditorGUILayout.GetControlRect();
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GetLabel(property), true);
            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;
            SerializedProperty child = property.Copy();
            SerializedProperty end = child.GetEndProperty();
            bool enterChildren = true;
            while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
            {
                DrawProperty(child);
                enterChildren = false;
            }
            EditorGUI.indentLevel--;
        }

        private static GUIContent GetLabel(SerializedProperty property)
        {
            FieldInfo field = FindField(property.serializedObject.targetObject.GetType(), property.propertyPath);
            if (field != null)
            {
                InspectorNameAttribute inspectorName = field.GetCustomAttribute<InspectorNameAttribute>();
                if (inspectorName != null && !string.IsNullOrEmpty(inspectorName.displayName))
                    return new GUIContent(inspectorName.displayName);
            }

            return new GUIContent(property.displayName);
        }

        private static FieldInfo FindField(Type rootType, string propertyPath)
        {
            Type currentType = rootType;
            FieldInfo result = null;
            string[] parts = propertyPath.Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part == "Array" && i + 1 < parts.Length && parts[i + 1].StartsWith("data[", StringComparison.Ordinal))
                {
                    currentType = currentType.IsArray
                        ? currentType.GetElementType()
                        : GetCollectionElementType(currentType);
                    i++;
                    continue;
                }

                result = GetField(currentType, part);
                if (result == null)
                    return null;

                currentType = result.FieldType;
            }

            return result;
        }

        private static FieldInfo GetField(Type type, string name)
        {
            if (type == null)
                return null;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            while (type != null)
            {
                FieldInfo field = type.GetField(name, flags);
                if (field != null)
                    return field;
                type = type.BaseType;
            }

            return null;
        }

        private static Type GetCollectionElementType(Type type)
        {
            if (type == null)
                return null;
            if (type.IsArray)
                return type.GetElementType();
            if (type.IsGenericType && type.GetGenericArguments().Length == 1)
                return type.GetGenericArguments()[0];
            return null;
        }
    }
}
