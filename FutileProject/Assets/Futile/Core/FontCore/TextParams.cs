using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Futile.Core.FontCore 
{
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
}