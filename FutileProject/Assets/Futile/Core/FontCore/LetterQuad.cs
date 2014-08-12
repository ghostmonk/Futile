using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//parts of this were inspired by https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UIElements/UIText.cs

//how to interpret BMFont files: http://www.gamedev.net/topic/284560-bmfont-and-how-to-interpret-the-fnt-file/

public class LetterQuad
{
    public CharInfo charInfo;
    public Rect rect;
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomRight;
    public Vector2 bottomLeft;
    
    public void CalculateVectors()
    {
        topLeft.Set( rect.xMin, rect.yMax );
        topRight.Set( rect.xMax, rect.yMax );
        bottomRight.Set( rect.xMax, rect.yMin );
        bottomLeft.Set( rect.xMin, rect.yMin );
    }
    
    public void CalculateVectors( float offsetX, float offsetY )
    {
        topLeft.Set( rect.xMin + offsetX, rect.yMax + offsetY );
        topRight.Set( rect.xMax + offsetX, rect.yMax + offsetY );
        bottomRight.Set( rect.xMax + offsetX, rect.yMin + offsetY );
        bottomLeft.Set( rect.xMin + offsetX, rect.yMin + offsetY );
    }
    
    //this moves the quads by a certain offset
    public void CalculateVectorsToWholePixels( float offsetX, float offsetY )
    {
        float scaleInverse = FearsomeMonstrousBeast.displayScaleInverse;
        
        //the stuff is used to make sure the quad is resting on a whole pixel
        float xMod = ( rect.xMin + offsetX ) % scaleInverse;
        float yMod = ( rect.yMin + offsetY ) % scaleInverse;
        
        offsetX -= xMod;
        offsetY -= yMod;
        
        float roundedLeft = rect.xMin + offsetX;
        float roundedRight = rect.xMax + offsetX;
        float roundedTop = rect.yMax + offsetY;
        float roundedBottom = rect.yMin + offsetY;
        
        topLeft.x = roundedLeft;
        topLeft.y = roundedTop;
        
        topRight.x = roundedRight;
        topRight.y = roundedTop;
        
        bottomRight.x = roundedRight;
        bottomRight.y = roundedBottom;
        
        bottomLeft.x = roundedLeft;
        bottomLeft.y = roundedBottom;
    }
    
}
