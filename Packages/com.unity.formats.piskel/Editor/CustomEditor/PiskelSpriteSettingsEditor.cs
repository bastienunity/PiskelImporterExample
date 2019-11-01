using UnityEditor;
using UnityEngine;

namespace Unity.Formats.Piskel.Editor
{
    [CustomPropertyDrawer(typeof(PiskelSpriteSettings))]
    public class PiskelSpriteSettingsEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
            if (property.FindPropertyRelative("pivot").intValue == (int)PiskelSpriteSettings.PivotType.Custom)
                return (EditorGUIUtility.singleLineHeight + 3f) * 4f;
            return (EditorGUIUtility.singleLineHeight + 3f) * 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var currentPosition = position;
            currentPosition.yMax = currentPosition.yMin + EditorGUIUtility.singleLineHeight;
            var expanded = EditorGUI.Foldout(currentPosition, property.isExpanded, label, true);
            if (property.isExpanded != expanded)
                property.isExpanded = expanded;
            if (expanded)
            {
                EditorGUI.indentLevel++;
                var pivot = property.FindPropertyRelative("pivot");
                currentPosition.y += EditorGUIUtility.singleLineHeight + 3f;
                EditorGUI.PropertyField(currentPosition, pivot);
                if (pivot.intValue == (int)PiskelSpriteSettings.PivotType.Custom)
                {
                    currentPosition.y += EditorGUIUtility.singleLineHeight + 3f;
                    EditorGUI.PropertyField(currentPosition, property.FindPropertyRelative("customPivotValue"));
                }
                currentPosition.y += EditorGUIUtility.singleLineHeight + 3f;
                EditorGUI.PropertyField(currentPosition, property.FindPropertyRelative("pixelsPerUnits"));
                EditorGUI.indentLevel--;
            }
        }
    }
}
