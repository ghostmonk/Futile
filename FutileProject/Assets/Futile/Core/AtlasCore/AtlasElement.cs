// //Written by ghostmonk

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasElement
{
	public string name;
	
	public int indexInAtlas;

	public Atlas atlas;
	public int atlasIndex;
	
	public Rect uvRect;
	public Vector2 uvTopLeft;
	public Vector2 uvTopRight;
	public Vector2 uvBottomRight;
	public Vector2 uvBottomLeft;
	
	public Rect sourceRect;
	public Vector2 sourceSize;
	public Vector2 sourcePixelSize;
	public bool isTrimmed;
	//public bool isRotated;
	
	public AtlasElement Clone()
	{
		AtlasElement element = new AtlasElement();
		
		element.name = name;
		
		element.indexInAtlas = indexInAtlas;
		
		element.atlas = atlas;
		element.atlasIndex = atlasIndex;
		
		element.uvRect = uvRect;
		element.uvTopLeft = uvTopLeft;
		element.uvTopRight = uvTopRight;
		element.uvBottomRight = uvBottomRight;
		element.uvBottomLeft = uvBottomLeft;
		
		element.sourceRect = sourceRect;
		element.sourceSize = sourceSize;
		element.isTrimmed = isTrimmed;
		
		return element;
	}

	public void UseTrimmedSizeAsBounds()//this will alter the element so that it uses the trimmed area as its bounds
	{
		sourceSize.x = sourceRect.width; 
		sourceSize.y = sourceRect.height;
	}
}
