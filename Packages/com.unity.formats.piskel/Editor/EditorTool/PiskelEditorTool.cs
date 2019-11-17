using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.Formats.Piskel.Editor
{
    [EditorTool("Piskel Editor", typeof(PiskelPlayer))]
    public class PiskelEditorTool : EditorTool
    {
        // Disabling not set values warning because it is set in the MonoBehaviour Importer as default value.
#pragma warning disable 0649
        [SerializeField] private Texture2D toolIcon;
#pragma warning restore 0649
        GUIContent _toolbarIcon;

        // Layer selection
        private AnimationClip _animation;

        // Playing animation
        private float _animationTimer = 0f;

        // Changing animation
        EditorCurveBinding[] _curveBindings;


        private PiskelPlayer piskelTarget => target as PiskelPlayer;

        public override GUIContent toolbarIcon
        {
            get
            {
                if (_toolbarIcon == null)
                    _toolbarIcon = new GUIContent(
                        toolIcon,
                        "Piskel Editor Tool");
                return _toolbarIcon;
            }
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGuiDelegate;
            EditorTools.activeToolChanged += EditorToolsOnActiveToolChanged;
        }

        private void EditorToolsOnActiveToolChanged()
        {
            if (EditorTools.IsActiveTool(this))
            {
                AnimationMode.StartAnimationMode();
                _animationTimer = Time.realtimeSinceStartup;
                InitializeEditorTool(piskelTarget.clip);
            }
            else
            {
                AnimationMode.SampleAnimationClip(piskelTarget.gameObject, piskelTarget.clip, 0);
                AnimationMode.StopAnimationMode();
            }
        }

        private void InitializeEditorTool(AnimationClip animation)
        {
            // Instantiate the clip, we always want to work on a copy.
            _animation = Instantiate(animation);
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGuiDelegate;
            EditorTools.activeToolChanged -= EditorToolsOnActiveToolChanged;
        }

        private void OnSceneGuiDelegate(SceneView sceneview)
        {
            if (!EditorTools.IsActiveTool(this))
                return;

            AnimationMode.SampleAnimationClip(piskelTarget.gameObject, _animation, Time.realtimeSinceStartup - _animationTimer);
            if (Time.realtimeSinceStartup - _animationTimer > _animation.length)
                _animationTimer += _animation.length;

            var renderer = piskelTarget.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = piskelTarget.GetComponentInChildren<SpriteRenderer>();
            }
            var piskelBound = renderer.bounds;

            var oldColor = Handles.color;
            Handles.color = AnimationMode.animatedPropertyColor;
            Handles.DrawWireCube(piskelBound.center, piskelBound.size);
            Handles.color = oldColor;
            // Force sceneview.Repaint() to make sure the animation is always playing
            sceneview.Repaint();
        }
    }
}
