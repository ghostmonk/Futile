using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Futile.Core.FontCore 
{
    public class CharInfo
    {
        public int charID;
        public float x;
        public float y;
        public float width;
        public float height;
        public Rect uvRect;
        public Vector2 uvTopLeft;
        public Vector2 uvTopRight;
        public Vector2 uvBottomRight;
        public Vector2 uvBottomLeft;
        public float offsetX;
        public float offsetY;
        public float xadvance;
        public int page;
        public string letter;
    }
}