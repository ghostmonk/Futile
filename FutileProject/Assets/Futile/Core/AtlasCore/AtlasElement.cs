using UnityEngine;

namespace Futile.Core.AtlasCore
{
    public class AtlasElement
    {
        public Atlas Atlas;
        public int AtlasIndex;
        public int IndexInAtlas;
        public bool IsTrimmed;
        public string Name;
        public Vector2 SourcePixelSize;
        public Rect SourceRect;
        public Vector2 SourceSize;
        public Vector2 UvBottomLeft;
        public Vector2 UvBottomRight;
        public Rect UvRect;
        public Vector2 UvTopLeft;
        public Vector2 UvTopRight;

        public AtlasElement Clone()
        {
            return new AtlasElement
            {
                Name = Name,
                IndexInAtlas = IndexInAtlas,
                Atlas = Atlas,
                AtlasIndex = AtlasIndex,
                UvRect = UvRect,
                UvTopLeft = UvTopLeft,
                UvTopRight = UvTopRight,
                UvBottomRight = UvBottomRight,
                UvBottomLeft = UvBottomLeft,
                SourceRect = SourceRect,
                SourceSize = SourceSize,
                IsTrimmed = IsTrimmed
            };
        }

        /// <summary>
        /// This will alter the element so that it uses the trimmed area as its bounds
        /// </summary>
        public void UseTrimmedSizeAsBounds()
        {
            SourceSize.x = SourceRect.width;
            SourceSize.y = SourceRect.height;
        }
    }
}