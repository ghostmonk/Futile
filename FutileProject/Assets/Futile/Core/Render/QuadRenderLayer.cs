using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Futile.Core.AtlasCore;

public class QuadRenderLayer : FacetRenderLayer
{
    
    public QuadRenderLayer( FStage stage, FacetType facetType, Atlas atlas, FShader shader )  : base (stage,facetType,atlas,shader)
    {
        
    }
    
    override protected void FillUnusedFacetsWithZeroes()
    {
        _lowestZeroIndex = Math.Max( _nextAvailableFacetIndex, Math.Min( _maxFacetCount, _lowestZeroIndex ) );
        
        for( int z = _nextAvailableFacetIndex; z<_lowestZeroIndex; z++ )
        {
            int vertexIndex = z * 4;    
            //the high 1000000 Z should make them get culled and not rendered because they're behind the camera 
            //need x to be 50 so they're "in screen" and not getting culled outside the bounds
            //because once something is marked outside the bounds, it won't get rendered until the next mesh.Clear()
            //TODO: test if the high z actually gives better performance or not
            _vertices[ vertexIndex + 0 ].Set( 50, 0, 1000000 ); 
            _vertices[ vertexIndex + 1 ].Set( 50, 0, 1000000 ); 
            _vertices[ vertexIndex + 2 ].Set( 50, 0, 1000000 ); 
            _vertices[ vertexIndex + 3 ].Set( 50, 0, 1000000 ); 
        }
        
        _lowestZeroIndex = _nextAvailableFacetIndex;
    }
    
    override protected void ShrinkMaxFacetLimit( int deltaDecrease )
    {
        if( deltaDecrease <= 0 )
        {
            return;
        }
        
        _maxFacetCount = Math.Max( facetType.initialAmount, _maxFacetCount - deltaDecrease );
    
        //shrink the arrays
        Array.Resize( ref _vertices, _maxFacetCount * 4 );
        Array.Resize( ref _uvs, _maxFacetCount * 4 );
        Array.Resize( ref _colors, _maxFacetCount * 4 );
        Array.Resize( ref _triangles, _maxFacetCount * 6 );

        _didVertCountChange = true;
        _didVertsChange = true;
        _didUVsChange = true;
        _didColorsChange = true;
        _isMeshDirty = true;
        _doesMeshNeedClear = true; //we only need clear when shrinking the mesh size
    }
    
    override protected void ExpandMaxFacetLimit( int deltaIncrease )
    {
        if( deltaIncrease <= 0 )
        {
            return;
        }
        
        int firstNewFacetIndex = _maxFacetCount;
        
        _maxFacetCount += deltaIncrease;
        
        //expand the arrays
        Array.Resize( ref _vertices, _maxFacetCount * 4 );
        Array.Resize( ref _uvs, _maxFacetCount * 4 );
        Array.Resize( ref _colors, _maxFacetCount * 4 );
        Array.Resize( ref _triangles, _maxFacetCount * 6 );
        
        //fill the triangles with the correct values
        for( int i = firstNewFacetIndex; i<_maxFacetCount; ++i )
        {
            _triangles[ i * 6 + 0 ] = i * 4 + 0;    
            _triangles[ i * 6 + 1 ] = i * 4 + 1;
            _triangles[ i * 6 + 2 ] = i * 4 + 2;
            
            _triangles[ i * 6 + 3 ] = i * 4 + 0;    
            _triangles[ i * 6 + 4 ] = i * 4 + 2;
            _triangles[ i * 6 + 5 ] = i * 4 + 3;
        }
        
        _didVertCountChange = true;
        _didVertsChange = true;
        _didUVsChange = true;
        _didColorsChange = true;
        _isMeshDirty = true;
    }
}
