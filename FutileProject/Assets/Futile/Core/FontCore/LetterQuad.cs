using UnityEngine;

namespace Futile.Core.FontCore
{
    public class LetterQuad
    {
        public Vector2 BottomLeft;
        public Vector2 BottomRight;
        public CharInfo CharInfo;
        public Rect Rect;
        public Vector2 TopLeft;
        public Vector2 TopRight;

        public void CalculateVectors()
        {
            TopLeft.Set( Rect.xMin, Rect.yMax );
            TopRight.Set( Rect.xMax, Rect.yMax );
            BottomRight.Set( Rect.xMax, Rect.yMin );
            BottomLeft.Set( Rect.xMin, Rect.yMin );
        }

        public void CalculateVectors( float offsetX, float offsetY )
        {
            TopLeft.Set( Rect.xMin + offsetX, Rect.yMax + offsetY );
            TopRight.Set( Rect.xMax + offsetX, Rect.yMax + offsetY );
            BottomRight.Set( Rect.xMax + offsetX, Rect.yMin + offsetY );
            BottomLeft.Set( Rect.xMin + offsetX, Rect.yMin + offsetY );
        }

        //this moves the quads by a certain offset
        public void CalculateVectorsToWholePixels( float offsetX, float offsetY )
        {
            float scaleInverse = FearsomeMonstrousBeast.displayScaleInverse;

            //the stuff is used to make sure the quad is resting on a whole pixel
            float xMod = ( Rect.xMin + offsetX ) % scaleInverse;
            float yMod = ( Rect.yMin + offsetY ) % scaleInverse;

            offsetX -= xMod;
            offsetY -= yMod;

            float roundedLeft = Rect.xMin + offsetX;
            float roundedRight = Rect.xMax + offsetX;
            float roundedTop = Rect.yMax + offsetY;
            float roundedBottom = Rect.yMin + offsetY;

            TopLeft.x = roundedLeft;
            TopLeft.y = roundedTop;

            TopRight.x = roundedRight;
            TopRight.y = roundedTop;

            BottomRight.x = roundedRight;
            BottomRight.y = roundedBottom;

            BottomLeft.x = roundedLeft;
            BottomLeft.y = roundedBottom;
        }
    }
}