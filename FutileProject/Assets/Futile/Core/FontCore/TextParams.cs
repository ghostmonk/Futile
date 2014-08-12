using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//parts of this were inspired by https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UIElements/UIText.cs

//how to interpret BMFont files: http://www.gamedev.net/topic/284560-bmfont-and-how-to-interpret-the-fnt-file/

public class TextParams
{
    public float scaledLineHeightOffset = 0;
    public float scaledKerningOffset = 0;
    private float _lineHeightOffset = 0;
    private float _kerningOffset = 0;
    
    public float kerningOffset
    {
        get { return _kerningOffset;}
        set
        {
            _kerningOffset = value; 
            scaledKerningOffset = value;
        }
    }
    
    public float lineHeightOffset
    {
        get { return _lineHeightOffset;}
        set
        {
            _lineHeightOffset = value; 
            scaledLineHeightOffset = value;
        }
    }
}
