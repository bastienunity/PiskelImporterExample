using UnityEngine;

namespace Unity.Formats.Piskel
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PiskelRuntimePlayer : MonoBehaviour
    {
        private Sprite[] sprites;
        private Texture2D[] textures;
        private int frameRate;
        private SpriteRenderer spriteRenderer;
        private float currentTime = 0f;
        private int currentSprite = 0;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (sprites.Length > 0 && frameRate > 0)
            {
                currentTime += Time.deltaTime;
                if (currentTime > 1f / frameRate)
                {
                    currentTime -= 1f / frameRate;
                    currentSprite = (currentSprite + 1) % sprites.Length;
                    spriteRenderer.sprite = sprites[currentSprite];
                }
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < textures.Length; i++)
            {
                Destroy(textures[i]);
            }
        }

        public static GameObject CreateFromPiskelFile(string piskelJson, PiskelSpriteSettings spriteSettings)
        {
            var piskel = PiskelDocument.FromPiskelJson(piskelJson);
            var root = new GameObject(piskel.name);
            for (int i = 0; i < piskel.layers.Length; i++)
            {
                var piskelLayer = piskel.layers[i];
                var layer = new GameObject(piskelLayer.name);
                layer.transform.parent = root.transform;

                var player = layer.AddComponent<PiskelRuntimePlayer>();
                player.frameRate = piskel.fps;
                piskel.GenerateTexturesAndSpritesForLayer(i, out player.textures, out player.sprites, spriteSettings);
            }
            return root;
        }
    }
}
