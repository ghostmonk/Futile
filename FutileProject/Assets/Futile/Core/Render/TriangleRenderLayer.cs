// //Written by ghostmonk

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriangleRenderLayer : FacetRenderLayer
{
	
	public TriangleRenderLayer (FStage stage, FacetType facetType, Atlas atlas, FShader shader)  : base (stage,facetType,atlas,shader)
	{
		
	}
	
	override protected void FillUnusedFacetsWithZeroes ()
	{
		_lowestZeroIndex = Math.Max (_nextAvailableFacetIndex, Math.Min (_maxFacetCount,_lowestZeroIndex));
		
		//Debug.Log ("FILLING FROM " + _nextAvailableFacetIndex + " to " + _lowestZeroIndex + " with zeroes!");
		
		for(int z = _nextAvailableFacetIndex; z<_lowestZeroIndex; z++)
		{
			int vertexIndex = z*3;	
			//the high 1000000 Z should make them get culled and not rendered because they're behind the camera 
			//need x to be 50 so they're "in screen" and not getting culled outside the bounds
			//because once something is marked outside the bounds, it won't get rendered until the next mesh.Clear()
			//TODO: test if the high z actually gives better performance or not
			_vertices[vertexIndex + 0].Set(50,0,1000000);	
			_vertices[vertexIndex + 1].Set(50,0,1000000);	
			_vertices[vertexIndex + 2].Set(50,0,1000000);
		}
		
		_lowestZeroIndex = _nextAvailableFacetIndex;
	}
	
	override protected void ShrinkMaxFacetLimit(int deltaDecrease)
	{
		if(deltaDecrease <= 0) return;
		
		_maxFacetCount = Math.Max (facetType.initialAmount, _maxFacetCount-deltaDecrease);
	
		//shrink the arrays
		Array.Resize (ref _vertices,_maxFacetCount*3);
		Array.Resize (ref _uvs,_maxFacetCount*3);
		Array.Resize (ref _colors,_maxFacetCount*3);
		Array.Resize (ref _triangles,_maxFacetCount*3);

		_didVertCountChange = true;
		_didVertsChange = true;
		_didUVsChange = true;
		_didColorsChange = true;
		_isMeshDirty = true;
		_doesMeshNeedClear = true; //we only need clear when shrinking the mesh size
	}
	
	override protected void ExpandMaxFacetLimit(int deltaIncrease)
	{
		if(deltaIncrease <= 0) return;
		
		int firstNewFacetIndex = _maxFacetCount;
		
		_maxFacetCount += deltaIncrease;
		
		//expand the arrays
		Array.Resize (ref _vertices,_maxFacetCount*3);
		Array.Resize (ref _uvs,_maxFacetCount*3);
		Array.Resize (ref _colors,_maxFacetCount*3);
		Array.Resize (ref _triangles,_maxFacetCount*3);
		
		//fill the triangles with the correct values
		for(int i = firstNewFacetIndex; i<_maxFacetCount; ++i)
		{
			int threei = i*3;
			
			_triangles[threei] = threei;	
			_triangles[threei + 1] = threei + 1;
			_triangles[threei + 2] = threei + 2;
		}
		
		_didVertCountChange = true;
		_didVertsChange = true;
		_didUVsChange = true;
		_didColorsChange = true;
		_isMeshDirty = true;
	}
}
