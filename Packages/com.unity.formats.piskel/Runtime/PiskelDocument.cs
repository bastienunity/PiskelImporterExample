using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Unity.Formats.Piskel
{
    [Serializable]
    public struct PiskelDocument
    {
        public string name;
        public string description;
        public int fps;
        public int height;
        public int width;
        public PiskelLayer[] layers;

        public void GenerateTexturesAndSpritesForLayer(int layer, out Texture2D[] textures, out Sprite[] sprites, PiskelSpriteSettings spriteSettings)
        {
            layers[layer].GenerateTexturesAndSprites(new Vector2(width, height), out textures, out sprites, spriteSettings);
        }

        public static PiskelDocument FromAnimation(AnimationClip animation)
        {
            var document = new PiskelDocument();
            document.name = animation.name;
            document.fps = (int)animation.frameRate;

            var curves = AnimationUtility.GetObjectReferenceCurveBindings(animation);
            document.layers = new PiskelLayer[curves.Length];
            for (int i = 0; i < curves.Length; i++)
            {
                document.layers[i] = new PiskelLayer();
                var keyFramesList = AnimationUtility.GetObjectReferenceCurve(animation, curves[i]);
                var firstSprite = (Sprite)keyFramesList[0].value;
                document.width = (int)firstSprite.rect.width;
                document.height = (int)firstSprite.rect.height;
                document.layers[i].FillFromAnimationCurve(keyFramesList, curves[i]);
            }

            return document;
        }

        public static PiskelDocument FromPiskelJson(string json)
        {
            var jsonParse = JsonUtility.FromJson<PiskelJsonFile>(json);
            var layoutFixer = new Regex(@"(""layout"":\[)(?:(\[[0-9,]+\])(,?))+(\],)");

            var piskelDocument = new PiskelDocument()
            {
                name = jsonParse.piskel.name,
                description = jsonParse.piskel.description,
                fps = jsonParse.piskel.fps,
                height = jsonParse.piskel.height,
                width = jsonParse.piskel.width,
            };
            piskelDocument.layers = new PiskelLayer[jsonParse.piskel.layers.Length];
            for (int i = 0; i < jsonParse.piskel.layers.Length; i++)
            {
                var jsonFixed = layoutFixer.Replace(jsonParse.piskel.layers[i], match =>
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(match.Groups[1]);
                    for (int j = 0; j < match.Groups[2].Captures.Count; j++)
                    {
                        builder.Append("{\"array\":");
                        builder.Append(match.Groups[2].Captures[j]);
                        builder.Append("}");
                        builder.Append(match.Groups[3].Captures[j]);
                    }
                    builder.Append(match.Groups[4]);
                    return builder.ToString();
                });
                piskelDocument.layers[i] = JsonUtility.FromJson<PiskelLayer>(jsonFixed);
            }

            return piskelDocument;
        }

        public string ToPiskelJson()
        {
            var layersString = "";
            for (var index = 0; index < layers.Length; index++)
            {
                var layer = layers[index];
                layersString += layer.ToPiskelJson();
                if (index != layers.Length - 1)
                {
                    layersString += ",";
                }
            }

            return "{\"modelVersion\": 2," +
                "\"piskel\": {" +
                "\"name\": \"" + name + "\"," +
                "\"description\": \"" + description + "\"," +
                "\"fps\": " + fps + "," +
                "\"height\": " + height + "," +
                "\"width\": " + width + "," +
                "\"layers\": [ \"" + layersString.Replace("\"", "\\\"") + "\"]}}";
        }

        // Disabling not set values warning because it is set internally by the Unity Json deserializer.
#pragma warning disable 0649
        // Because the Piskel document is using a json string inside a Json file,
        // we are using this temporary struct to get the Unity serializer to deserialize it correctly.
        [Serializable]
        private struct PiskelJsonFile
        {
            public int modelVersion;
            public PiskelJsonDocument piskel;
        }

        [Serializable]
        private struct PiskelJsonDocument
        {
            public string name;
            public string description;
            public int fps;
            public int height;
            public int width;
            public string[] layers;
        }
#pragma warning restore 0649
    }

    [Serializable]
    public struct PiskelLayer
    {
        public string name;
        public float opacity;
        public int frameCount;
        public PiskelChunk[] chunks;

        public void GenerateTexturesAndSprites(Vector2 spriteSize, out Texture2D[] textures, out Sprite[] sprites, PiskelSpriteSettings spriteSettings)
        {
            textures = new Texture2D[chunks.Length];
            sprites = new Sprite[frameCount];
            int spriteId = 0;
            for (var i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                textures[i] = chunk.GetTexture();
                textures[i].name = $"{name}_Tex{i}";
                foreach (var sprite in chunk.GetSprites(textures[i], spriteSize, spriteSettings))
                {
                    sprite.name = $"{name}_{spriteId}";
                    sprites[spriteId++] = sprite;
                }
            }
        }

        public void FillFromAnimationCurve(ObjectReferenceKeyframe[] frames, EditorCurveBinding curve)
        {
            var subIndex = curve.path.LastIndexOf('.');
            name = subIndex > 0 ? curve.path.Substring(subIndex + 1) : curve.path;
            frameCount = frames.Length;
            opacity = 1f;
            chunks = new[] {new PiskelChunk()};
            chunks[0].SetFrames(frames);
        }

        public string ToPiskelJson()
        {
            string chunksString = "";
            for (var index = 0; index < chunks.Length; index++)
            {
                var chunk = chunks[index];
                chunksString += chunk.ToPiskelJson();
                if (index != chunks.Length - 1)
                {
                    chunksString += ",";
                }
            }
            return "{\"name\":\"" + name + "\"," +
                "\"opacity\":" + opacity + "," +
                "\"frameCount\":" + frameCount + "," +
                "\"chunks\":[" + chunksString + "]}";
        }
    }

    [Serializable]
    public struct PiskelChunk
    {
        [Serializable]
        public struct IntArray
        {
            public int[] array;
        }

        public IntArray[] layout;
        public string base64PNG;

        public Texture2D GetTexture()
        {
            var tex = new Texture2D(1, 1);
            tex.LoadImage(System.Convert.FromBase64String(base64PNG.Remove(0, "data:image/png;base64,".Length)));
            tex.Apply();
            return tex;
        }

        public void SetTexture(Texture2D texture)
        {
            var bytes = texture.EncodeToPNG();
            base64PNG = $"data:image/png;base64,{System.Convert.ToBase64String(bytes)}";
        }

        public Sprite[] GetSprites(Texture2D texture, Vector2 spriteSize, PiskelSpriteSettings spriteSettings)
        {
            Vector2 pivot;
            if (spriteSettings.pivot == PiskelSpriteSettings.PivotType.Custom)
                pivot = spriteSettings.customPivotValue;
            else
                pivot = PivotTypeToValue(spriteSettings.pivot);
            var size = layout.Select(a => a.array.Length).Sum();
            var sprites = new Sprite[size];
            for (int a = 0; a < layout.Length; a++)
            {
                for (int b = 0; b < layout[a].array.Length; b++)
                {
                    var index = layout[a].array[b];
                    sprites[index] = Sprite.Create(texture, new Rect(
                        new Vector2(a * spriteSize.x, b * spriteSize.y),
                        spriteSize),
                        pivot,
                        spriteSettings.pixelsPerUnits);
                }
            }

            return sprites;
        }

        private Vector2 PivotTypeToValue(PiskelSpriteSettings.PivotType spriteSettingsPivot)
        {
            Vector2 pivot = Vector2.zero;
            switch ((int)spriteSettingsPivot >> 2)
            {
                case 1:
                    pivot.y = .5f;
                    break;
                case 2:
                    pivot.y = 1f;
                    break;
            }
            switch ((int)spriteSettingsPivot & 3)
            {
                case 1:
                    pivot.x = .5f;
                    break;
                case 2:
                    pivot.x = 1f;
                    break;
            }

            return pivot;
        }

        public void SetFrames(ObjectReferenceKeyframe[] keyFramesList)
        {
            layout = new IntArray[keyFramesList.Length];
            Texture2D texture = null;

            for (int i = 0; i < keyFramesList.Length; i++)
            {
                layout[i] = new IntArray() {array = new[] {i}};
                var sprite = (Sprite)keyFramesList[i].value;
                if (texture == null)
                {
                    texture = new Texture2D((int)(sprite.rect.width * keyFramesList.Length), (int)sprite.rect.height);
                }
                texture.SetPixels((int)(i * sprite.rect.width), 0, (int)sprite.rect.width, (int)sprite.rect.height, sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height));
            }
            SetTexture(texture);
        }

        public string ToPiskelJson()
        {
            var layoutArrayStr = new List<string>();
            foreach (var intArray in layout)
            {
                layoutArrayStr.Add(string.Join(",", intArray.array));
            }
            return "{\"layout\":[[" + string.Join("],[", layoutArrayStr) + "]]," +
                "\"base64PNG\":\"" + base64PNG + "\"}";
        }
    }
}
