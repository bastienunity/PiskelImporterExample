using System;
using UnityEngine;

namespace Unity.Formats.Piskel
{
    [Serializable]
    public class PiskelSpriteSettings
    {
        public enum PivotType
        {
            BottomLeft = 0,
            BottomCenter = 1,
            BottomRight = 2,
            MiddleLeft = 1 << 2,
            MiddleCenter = 1 << 2 | 1,
            MiddleRight = 1 << 2 | 2,
            TopLeft = 2 << 2,
            TopCenter = 2 << 2 | 1,
            TopRight = 2 << 2 | 2,
            Custom = -1
        }

        public PivotType pivot;
        public Vector2 customPivotValue;
        public float pixelsPerUnits;

        public PiskelSpriteSettings()
        {
            pivot = PivotType.BottomCenter;
            customPivotValue = Vector2.zero;
            pixelsPerUnits = 100f;
        }
    }
}
