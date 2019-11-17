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

            _curveBindings = AnimationUtility.GetObjectReferenceCurveBindings(_animation);
            _selectedLayerIndex = _selectedLayerIndex >= _curveBindings.Length ? 0 : _selectedLayerIndex;
            InitializeFramesList();
        }

        private void InitializeFramesList()
        {
            _keyFramesList = AnimationUtility.GetObjectReferenceCurve(_animation, _curveBindings[_selectedLayerIndex]);
            _spritesListUi = new ReorderableList(_keyFramesList, typeof(ObjectReferenceKeyframe), true, false, false, false);
            _spritesListUi.headerHeight = 1f;
            _spritesListUi.footerHeight = 1f;
            _spritesListUi.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = _keyFramesList[index];
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                element.value = EditorGUI.ObjectField(rect, element.value, typeof(Sprite), false);
                if (EditorGUI.EndChangeCheck())
                {
                    _keyFramesList[index] = element;
                    AnimationUtility.SetObjectReferenceCurve(_animation, _curveBindings[_selectedLayerIndex], _keyFramesList);
                    _hasListChanged = true;
                }
            };
            _spritesListUi.onReorderCallback = list =>
            {
                for (int i = 0; i < _keyFramesList.Length; i++)
                {
                    _keyFramesList[i].time = (1 / _animation.frameRate) * i;
                }
                AnimationUtility.SetObjectReferenceCurve(_animation, _curveBindings[_selectedLayerIndex], _keyFramesList);
                _hasListChanged = true;
            };
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

            var windowCenter = piskelBound.center + piskelBound.size / 2f;
            windowCenter = sceneview.camera.WorldToScreenPoint(windowCenter);
            windowCenter.y = sceneview.position.height - windowCenter.y;
            windowCenter.x += 20f;
            Handles.BeginGUI();
            GUILayout.Window(GetInstanceID(), new Rect(windowCenter, new Vector2(200f, 180f)), DrawPiskelEditorToolWindow, "Piskel Editor Tool");
            Handles.EndGUI();

            // Force sceneview.Repaint() to make sure the animation is always playing
            sceneview.Repaint();
        }

        private ReorderableList _spritesListUi;
        private ObjectReferenceKeyframe[] _keyFramesList = new ObjectReferenceKeyframe[0];
        private Vector2 _scrollPosition;
        private bool _hasListChanged;
        private int _selectedLayerIndex = 0;

        private void DrawPiskelEditorToolWindow(int id)
        {
            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup(_selectedLayerIndex, _curveBindings.Select(i => i.propertyName).ToArray());
            if (EditorGUI.EndChangeCheck() && newIndex != _selectedLayerIndex)
            {
                InitializeFramesList();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            _spritesListUi.DoLayoutList();
            EditorGUILayout.EndScrollView();

            EditorGUI.BeginDisabledGroup(!_hasListChanged);
            if (GUILayout.Button("Save changes"))
            {
                string savePath =
                    EditorUtility.SaveFilePanelInProject("Save Piskel animation", piskelTarget.name, "piskel", "Select where to save the current animation.");
                if (!string.IsNullOrEmpty(savePath))
                {
                    PiskelDocument document = PiskelDocument.FromAnimation(_animation);
                    File.WriteAllText(savePath, document.ToPiskelJson());
                    AssetDatabase.ImportAsset(savePath);
                    _hasListChanged = false;
                }
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();
        }
    }
}
