using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Futile.Core.AtlasCore;

public class FacetRenderLayer : IRenderableLayer
{
    public int batchIndex;
    public FStage stage;
    public FacetType facetType;
    public Atlas atlas;
    public FShader shader;
    protected GameObject _gameObject;
    protected Transform _transform;
    protected Material _material;
    protected MeshFilter _meshFilter;
    protected MeshRenderer _meshRenderer;
    protected Mesh _mesh;
    
    //Mesh stuff
    protected Vector3[] _vertices = new Vector3[0];
    protected int[] _triangles = new int[0];
    protected Vector2[] _uvs = new Vector2[0];
    protected Color[] _colors = new Color[0];
    protected bool _isMeshDirty = false;
    protected bool _didVertsChange = false;
    protected bool _didUVsChange = false;
    protected bool _didColorsChange = false;
    protected bool _didVertCountChange = false;
    protected bool _doesMeshNeedClear = false;
    protected int _expansionAmount;
    protected int _maxEmptyFacets;
    protected int _maxFacetCount = 0;
    protected int _depth = -1;
    protected int _nextAvailableFacetIndex;
    protected int _lowestZeroIndex = 0;

    public FacetRenderLayer( FStage stage, FacetType facetType, Atlas atlas, FShader shader )
    {
        this.stage = stage;
        
        this.facetType = facetType;
        this.atlas = atlas;
        this.shader = shader;
        
        _expansionAmount = facetType.expansionAmount;
        _maxEmptyFacets = facetType.maxEmptyAmount;
        
        this.batchIndex = facetType.index * 10000000 + atlas.Index * 10000;
        
        _gameObject = new GameObject( "FRenderLayer (" + stage.name + ") (" + facetType.name + ")" );
        _transform = _gameObject.transform;
        
        _transform.parent = FearsomeMonstrousBeast.instance.gameObject.transform;
        
        _meshFilter = _gameObject.AddComponent<MeshFilter>();
        _meshRenderer = _gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.castShadows = false;
        _meshRenderer.receiveShadows = false;
        
        _mesh = _meshFilter.mesh;

        //we could possibly create a pool of materials so they can be reused, 
        //but that would create issues when unloading textures, so it's probably not worth it
        _material = new Material( shader.shader );
        _material.mainTexture = atlas.Texture;
        
        _meshRenderer.renderer.sharedMaterial = _material;
        
        #if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
            _gameObject.active = false;
        #else
        _gameObject.SetActive( false );
        _mesh.MarkDynamic();
        #endif
        
        ExpandMaxFacetLimit( facetType.initialAmount );
        
        UpdateTransform();
    }

    public void Destroy()
    {
        UnityEngine.Object.Destroy( _gameObject );

        UnityEngine.Object.Destroy( _mesh );
        UnityEngine.Object.Destroy( _material );
    }

    public void UpdateTransform()
    {
        _transform.position = stage.transform.position;
        _transform.rotation = stage.transform.rotation;
        _transform.localScale = stage.transform.localScale;

        _gameObject.layer = stage.layer;
    }
    
    public void AddToWorld() //add to the transform etc
    {
        #if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
            _gameObject.active = true;
        #else
        _gameObject.SetActive( true );
        #endif
    }
    
    public void RemoveFromWorld() //remove it from the root transform etc
    {
        #if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
            _gameObject.active = false;
        #else
        _gameObject.SetActive( false );
        #endif
        #if UNITY_EDITOR
            //some debug code so that layers are sorted by depth properly
            _gameObject.name = "FRenderLayer X ("+stage.name+") (" + atlas.Name + " " + shader.name+" "+facetType.name+ ")";
        #endif
    }

    public void Open()
    {
        _nextAvailableFacetIndex = 0;
    }
    
    public int GetNextFacetIndex( int numberOfFacetsNeeded )
    {       
        int indexToReturn = _nextAvailableFacetIndex;
        _nextAvailableFacetIndex += numberOfFacetsNeeded;
        
        //expand the layer (if needed) now that we know how many facets we need to fit
        if( _nextAvailableFacetIndex - 1 >= _maxFacetCount )
        {
            int deltaNeeded = ( _nextAvailableFacetIndex - _maxFacetCount ) + 1;
            ExpandMaxFacetLimit( Math.Max( deltaNeeded, _expansionAmount ) ); //expand it by expansionAmount or the amount needed
        } 
            
        return indexToReturn;
    }

    public void Close() //fill remaining facets with 0,0,0
    {
        //if we have a ton of empty facets
        //shrink the facets
        if( _nextAvailableFacetIndex < _maxFacetCount - _maxEmptyFacets )
        {
            ShrinkMaxFacetLimit( Math.Max( 0, ( _maxFacetCount - _nextAvailableFacetIndex ) - _expansionAmount ) ); 
        }
        
        FillUnusedFacetsWithZeroes();
        
        #if UNITY_EDITOR
            //some debug code so that layers are sorted by depth properly
            _gameObject.name = "FRenderLayer "+_depth+" ("+stage.name+") ["+_nextAvailableFacetIndex+"/"+_maxFacetCount+"] (" + atlas.Name + " " + shader.name+" "+facetType.name+ ")";
        #endif
    }

    virtual protected void FillUnusedFacetsWithZeroes()
    {
        throw new NotImplementedException( "Override me!" );
    }
    
    virtual protected void ShrinkMaxFacetLimit( int deltaDecrease )
    {
        throw new NotImplementedException( "Override me!" );
    }
    
    virtual protected void ExpandMaxFacetLimit( int deltaIncrease )
    {
        throw new NotImplementedException( "Override me!" );
    }
    
    //ACTUAL RENDERING GOES HERE
    
    public void Update( int depth ) //called by the engine
    {
        if( _depth != depth )
        {
            _depth = depth; 
    
            //this will set the render order correctly based on the depth
            _material.renderQueue = FearsomeMonstrousBeast.baseRenderQueueDepth + _depth;
            
            #if UNITY_EDITOR
                //some debug code so that layers are sorted by depth properly
                _gameObject.name = "FRenderLayer "+_depth+" ("+stage.name+") ["+_nextAvailableFacetIndex+"/"+_maxFacetCount+"] (" + atlas.Name + " " + shader.name+" "+facetType.name+ ")";
            #endif
        }
        
        if( _isMeshDirty )
        {
            UpdateMeshProperties();
        }

        if( shader.needsApply )
        {
            shader.Apply( _material );
        }
    }

    virtual public void PostUpdate()
    {
        shader.needsApply = false;
    }
    
    protected void UpdateMeshProperties()
    {
        _isMeshDirty = false;
        
        // Were changes made to the mesh since last time?
        
        if( _didVertCountChange )
        {
            _didVertCountChange = false;
            _didColorsChange = false;
            _didVertsChange = false;
            _didUVsChange = false;
            
            //in theory we shouldn't need clear because we KNOW everything is correct
            //see http://docs.unity3d.com/Documentation/ScriptReference/Mesh.html
            if( _doesMeshNeedClear )
            {
                _mesh.Clear();
            } 
            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;
            _mesh.uv = _uvs;

            //make the bounds huge so it won't ever be culled (I tried using float.MaxValue here but it made the Unity editor crash)
            _mesh.bounds = new Bounds( Vector3.zero, new Vector3( 9999999999, 9999999999, 9999999999 ) );

            //TODO: switch to using colors32 at some point for performance
            //see http://docs.unity3d.com/Documentation/ScriptReference/Mesh-colors32.html
            _mesh.colors = _colors;
        }
        else
        {
            if( _didVertsChange )
            {
                _didVertsChange = false;
                _mesh.vertices = _vertices;
            }
        
            if( _didColorsChange )
            {
                _didColorsChange = false;
                _mesh.colors = _colors;
            }
            
            if( _didUVsChange )
            {
                _didUVsChange = false;
                _mesh.uv = _uvs;
            }
        } 

    }

    public void HandleVertsChange()
    {
        _didVertsChange = true;
        _didUVsChange = true;
        _didColorsChange = true;
        _isMeshDirty = true;
    }
    
    public int expansionAmount
    {
        set { _expansionAmount = value;}
        get { return _expansionAmount;}
    }
    
    public Vector3[] vertices
    {
        get { return _vertices;}
    }
    
    public Vector2[] uvs
    {
        get { return _uvs;}
    }
    
    public Color[] colors
    {
        get { return _colors;}
    }
}






