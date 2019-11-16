using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Unity.Formats.Piskel.Editor
{
    [ScriptedImporter(1, "piskel")]
    public class PiskelImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var piskel = PiskelDocument.FromPiskelJson(File.ReadAllText(ctx.assetPath));

            // Creates the root first and save it as main asset.
            var go = new GameObject("root");
            ctx.AddObjectToAsset("root", go);
            ctx.SetMainObject(go);

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
                for (var index = 0; index < sprites.Length; index++)
                {
                    var sprite = sprites[index];
                    ctx.AddObjectToAsset(sprite.name, sprite);
                }
            }
        }
    }
}
