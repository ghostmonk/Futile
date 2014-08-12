using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Futile.Core.AtlasCore;

public class FacetType
{
    static public FacetType defaultFacetType;
    
    //facetType types
    public static FacetType Quad;
    public static FacetType Triangle;
    private static int _nextFacetTypeIndex = 0;
    private static List<FacetType> _facetTypes = new List<FacetType>();
    public int index;
    public string name;
    public int initialAmount;
    public int expansionAmount;
    public int maxEmptyAmount;
    
    public delegate FacetRenderLayer CreateRenderLayerDelegate (FStage stage,FacetType facetType,Atlas atlas,FShader shader);
    
    public CreateRenderLayerDelegate createRenderLayer;
    
    private FacetType( string name, int index, int initialAmount, int expansionAmount, int maxEmptyAmount, CreateRenderLayerDelegate createRenderLayer ) //only to be constructed by using CreateFacetType()
    {
        this.index = index;
        this.name = name;
        
        this.initialAmount = initialAmount;
        this.expansionAmount = expansionAmount;
        this.maxEmptyAmount = maxEmptyAmount;
        
        this.createRenderLayer = createRenderLayer;
    }
    
    public static void Init() //called by Futile
    {
        Quad = CreateFacetType( "Quad", 10, 10, 60, CreateQuadLayer );  
        Triangle = CreateFacetType( "Triangle", 16, 16, 64, CreateTriLayer );   
        
        defaultFacetType = Quad;
    }
    
    //create your own FFacetTypes by creating them here
    public static FacetType CreateFacetType( string facetTypeShortName, int initialAmount, int expansionAmount, int maxEmptyAmount, CreateRenderLayerDelegate createRenderLayer )
    {
        for( int s = 0; s<_facetTypes.Count; s++ )
        {
            if( _facetTypes[ s ].name == facetTypeShortName )
            {
                return _facetTypes[ s ];
            } //don't add it if we have it already  
        }
        
        FacetType newFacetType = new FacetType( facetTypeShortName, _nextFacetTypeIndex++, initialAmount, expansionAmount, maxEmptyAmount, createRenderLayer );
        _facetTypes.Add( newFacetType );
        
        return newFacetType;
    }
    
    static private FacetRenderLayer CreateQuadLayer( FStage stage, FacetType facetType, Atlas atlas, FShader shader )
    {
        return new QuadRenderLayer( stage, facetType, atlas, shader );
    }
    
    static private FacetRenderLayer CreateTriLayer( FStage stage, FacetType facetType, Atlas atlas, FShader shader )
    {
        return new TriangleRenderLayer( stage, facetType, atlas, shader );
    }
    
}


