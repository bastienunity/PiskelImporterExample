using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Unity.Formats.Piskel.Editor
{
    [ScriptedImporter(2, "piskel")]
    public class PiskelImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var piskel = PiskelDocument.FromPiskelJson(File.ReadAllText(ctx.assetPath));

            // Creates the root first and save it as main asset.
            var go = new GameObject("root");
            ctx.AddObjectToAsset("root", go);
            ctx.SetMainObject(go);

            // Create a Clip
            var clip = new AnimationClip();
            clip.name = piskel.name;
            clip.frameRate = piskel.fps;
            // Create clip settings to make it loop
            var settings = new AnimationClipSettings
            {
                loopTime = true
            };
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            ctx.AddObjectToAsset("clip", clip);

            var animator = go.AddComponent<PiskelPlayer>();
            animator.clip = clip;

            for (var i = 0; i < piskel.layers.Length; i++)
            {
                GameObject layerObj = new GameObject(piskel.layers[i].name);
                layerObj.transform.parent = go.transform;

                var spriteRenderer = layerObj.AddComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(1, 1, 1, piskel.layers[i].opacity);

                piskel.GenerateTexturesAndSpritesForLayer(i, out var textures, out var sprites, new PiskelSpriteSettings());
                foreach (var texture2D in textures)
                {
                    texture2D.hideFlags = HideFlags.HideInHierarchy;
                    ctx.AddObjectToAsset(texture2D.name, texture2D);
                }
                spriteRenderer.sprite = sprites.Length > 0 ? sprites[0] : null;
                var keys = new ObjectReferenceKeyframe[sprites.Length];
                for (var index = 0; index < sprites.Length; index++)
                {
                    var sprite = sprites[index];
                    ctx.AddObjectToAsset(sprite.name, sprite);
                    keys[index] = new ObjectReferenceKeyframe()
                    {
                        time = (1 / clip.frameRate) * index, value = sprite
                    };
                }

                var binding = new EditorCurveBinding
                {
                    path = layerObj.name,
                    type = typeof(SpriteRenderer),
                    propertyName = "m_Sprite"
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            }
        }
    }
}
