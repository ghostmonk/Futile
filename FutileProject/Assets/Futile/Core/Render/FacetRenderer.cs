using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Futile.Core.AtlasCore;

namespace Futile.Core.Render
{
    public class FacetRenderer
    {   
        private List<FacetRenderLayer> _allLayers = new List<FacetRenderLayer>();
        private List<FacetRenderLayer> _liveLayers = new List<FacetRenderLayer>();
        private List<FacetRenderLayer> _previousLiveLayers = new List<FacetRenderLayer>();
        private List<FacetRenderLayer> _cachedLayers = new List<FacetRenderLayer>();
        private List<IRenderableLayer> _allRenderables = new List<IRenderableLayer>();
        private FacetRenderLayer _topLayer;
        private FStage _stage;
        
        public FacetRenderer( FStage stage )
        {
            _stage = stage;
        }

        public void Clear() //wipe the renderlayers and remove their gameobjects
        {
            int allLayerCount = _allLayers.Count;
            for( int a = 0; a<allLayerCount; ++a )
            {
                _allLayers[ a ].Destroy();
            }
            
            _allLayers.Clear();
            _liveLayers.Clear();
            _previousLiveLayers.Clear();
            _cachedLayers.Clear();
            _allRenderables.Clear();
        }

        public void ClearLayersThatUseAtlas( Atlas atlas )
        {
            bool didHaveLayerThatUsedAtlas = false;

            for( int a = _allLayers.Count - 1; a >= 0; --a )
            {
                if( _allLayers[ a ].atlas == atlas )
                {
                    didHaveLayerThatUsedAtlas = true;

                    FacetRenderLayer layer = _allLayers[ a ];

                    _liveLayers.Remove( layer );
                    _previousLiveLayers.Remove( layer );
                    _cachedLayers.Remove( layer );
                    _allRenderables.Remove( layer );
                    _allLayers.Remove( layer );

                    layer.Destroy();
                }
            }

            if( didHaveLayerThatUsedAtlas )
            {
                _stage.HandleFacetsChanged();
            }
        }

        public void UpdateLayerTransforms()
        {
            int allLayerCount = _allLayers.Count;
            for( int a = 0; a<allLayerCount; ++a )
            {
                _allLayers[ a ].UpdateTransform();
            }
        }
        
        public void StartRender()
        {
            //make the livelayers empty, put those layers in _previousLiveLayers
            List<FacetRenderLayer> swapLayers = _liveLayers;
            _liveLayers = _previousLiveLayers;
            _previousLiveLayers = swapLayers;
            
            _topLayer = null;
            
            _allRenderables.Clear();
        }
        
        public void EndRender()
        {
            int previousLiveLayerCount = _previousLiveLayers.Count;
            for( int p = 0; p<previousLiveLayerCount; ++p )
            {
                _previousLiveLayers[ p ].RemoveFromWorld();
                _cachedLayers.Add( _previousLiveLayers[ p ] );
            }
            
            _previousLiveLayers.Clear();
            
            if( _topLayer != null )
            {
                _topLayer.Close();
            }
            
        }
        
        protected FacetRenderLayer CreateFacetRenderLayer( FacetType facetType, int batchIndex, Atlas atlas, FShader shader )
        {
            //first, check and see if we already have a layer that matches the batchIndex
            int previousLiveLayerCount = _previousLiveLayers.Count;
            for( int p = 0; p<previousLiveLayerCount; ++p )
            {
                FacetRenderLayer previousLiveLayer = _previousLiveLayers[ p ];
                if( previousLiveLayer.batchIndex == batchIndex && previousLiveLayer.shader == shader )
                {
                    _previousLiveLayers.RemoveAt( p );
                    _liveLayers.Add( previousLiveLayer );
                    _allRenderables.Add( previousLiveLayer );
                    return previousLiveLayer;
                }
            }
            
            //now see if we have a cached (old, now unused layer) that matches the batchIndex
            int cachedLayerCount = _cachedLayers.Count;
            for( int c = 0; c<cachedLayerCount; ++c )
            {
                FacetRenderLayer cachedLayer = _cachedLayers[ c ];
                if( cachedLayer.batchIndex == batchIndex && cachedLayer.shader == shader )
                {
                    _cachedLayers.RemoveAt( c );
                    cachedLayer.AddToWorld();
                    _liveLayers.Add( cachedLayer );
                    _allRenderables.Add( cachedLayer );
                    return cachedLayer;
                }
            }
            
            //still no layer found? create a new one!
            
            FacetRenderLayer newLayer = facetType.createRenderLayer( _stage, facetType, atlas, shader );
            _liveLayers.Add( newLayer );
            _allLayers.Add( newLayer );
            _allRenderables.Add( newLayer );
            newLayer.AddToWorld();
            
            return newLayer;
        }
        
        public void GetFacetRenderLayer( out FacetRenderLayer renderLayer, out int firstFacetIndex, FacetType facetType, Atlas atlas, FShader shader, int numberOfFacetsNeeded )
        {
            //int batchIndex = facetType.index*10000000 + atlas.index*10000 + shader.index;
            int batchIndex = facetType.index * 10000000 + atlas.Index * 10000;
            
            if( _topLayer == null )
            {
                _topLayer = CreateFacetRenderLayer( facetType, batchIndex, atlas, shader );
                _topLayer.Open();
            }
            else
            {
                if( _topLayer.batchIndex != batchIndex || _topLayer.shader != shader ) //we're changing layers!
                {
                    _topLayer.Close(); //close the old layer 
                    
                    _topLayer = CreateFacetRenderLayer( facetType, batchIndex, atlas, shader );
                    _topLayer.Open(); //open the new layer
                }
            }
            
            renderLayer = _topLayer;
            firstFacetIndex = _topLayer.GetNextFacetIndex( numberOfFacetsNeeded );
        }

        public void AddRenderableLayer( IRenderableLayer renderableLayer )
        {
            _allRenderables.Add( renderableLayer );
            if( _topLayer != null )
            {
                _topLayer.Close();
                _topLayer = null;
            }
        }
        
        public void Update()
        {
            int allRenderablesCount = _allRenderables.Count;
            for( int a = 0; a<allRenderablesCount; a++ )
            {
                _allRenderables[ a ].Update( FearsomeMonstrousBeast.nextRenderLayerDepth++ ); 
            } 

            for( int a = 0; a<allRenderablesCount; a++ )
            {
                _allRenderables[ a ].PostUpdate();
            } 
        }
    }
}





