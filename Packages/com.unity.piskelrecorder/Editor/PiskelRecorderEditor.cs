using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;

namespace Unity.PiskelRecorder
{
    [CustomEditor(typeof(PiskelRecorderSettings))]
    class PiskelRecorderEditor : RecorderEditor
    {
        SerializedProperty m_CaptureAlpha;
        SerializedProperty m_OutputFrameRate;

        static class Styles
        {
            internal static readonly GUIContent CaptureAlphaLabel = new GUIContent("Capture Alpha");
            internal static readonly GUIContent OutputFrameRate = new GUIContent("Piskel file frame rate");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            m_CaptureAlpha = serializedObject.FindProperty("captureAlpha");
            m_OutputFrameRate = serializedObject.FindProperty("outputFrameRate");
        }

        protected override void FileTypeAndFormatGUI()
        {
            var imageSettings = (PiskelRecorderSettings)target;
            var supportsAlpha = imageSettings.imageInputSettings.supportsTransparent;
            if (!supportsAlpha)
                m_CaptureAlpha.boolValue = false;

            using (new EditorGUI.DisabledScope(!supportsAlpha))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(m_CaptureAlpha, Styles.CaptureAlphaLabel);
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.PropertyField(m_OutputFrameRate, Styles.OutputFrameRate);
        }
    }
}
