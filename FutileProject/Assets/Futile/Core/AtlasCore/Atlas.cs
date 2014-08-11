using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Atlas
{
    private Dictionary<string, AtlasElement> elementsByName;
    private bool isTextureAnAsset = false;

    public Atlas( string name, Texture texture, int index )
    {   
        IsSingleImage = true;
        Init( name, "", texture, index );
    }
    
    public Atlas( string name, string dataPath, Texture texture, int index, bool isSingleImage = false ) 
    {   
        Init( name, dataPath, texture, index ); 
    }
    
    public Atlas( string name, string imagePath, string dataPath, int index, bool isSingleImage )
    {   
        IsSingleImage = isSingleImage;
        isTextureAnAsset = true;
        ImagePath = imagePath;

        Texture texture = Resources.Load( imagePath, typeof( Texture ) ) as Texture;
        
        if( texture == null ) 
        {
            throw new FutileException( "Couldn't load the atlas texture from: " + ImagePath );  
        }

        Init( name, dataPath, texture, index );
    }

    private void Init( string name, string dataPath, Texture texture, int index )
    {
        elementsByName = new Dictionary<string, AtlasElement>();
        Elements = new List<AtlasElement>();
        Index = index;
        Name = name;
        ImagePath = "";
        DataPath = dataPath;
        Texture = texture;
        TextureSize = new Vector2( Texture.width, Texture.height );
        
        if( !IsSingleImage ) 
        {
            LoadAtlasData();
            return;
        }
        CreateElementForEntireAtlas();
    }
    
    public List<AtlasElement> Elements { get; private set; }
    
    public string Name { get; private set; }
    
    public int Index { get; private set; }
    
    public Texture Texture { get; private set; }
    
    public Vector2 TextureSize { get; private set; }
    
    public string ImagePath { get; private set; }
    
    public string DataPath { get; private set; }
    
    public bool IsSingleImage { get; private set; }
    
    public AtlasElement FullElement { get; private set; }
    
    private void LoadAtlasData()
    {
        TextAsset dataAsset = Resources.Load( DataPath, typeof( TextAsset ) ) as TextAsset;
        
        if( dataAsset == null ) {
            throw new FutileException( "Couldn't load the atlas data from: " + DataPath );
        }
        
        Dictionary<string,object> dict = dataAsset.text.dictionaryFromJson();
        
        if( dict == null ) {
            throw new FutileException( "The atlas at " + DataPath + " was not a proper JSON file. Make sure to select \"Unity3D\" in TexturePacker." );
        }
        
        Dictionary<string,object> frames = (Dictionary<string,object>)dict[ "frames" ];
        
        float scaleInverse = FutileEngine.resourceScaleInverse;
        
        int index = 0;
        
        foreach( KeyValuePair<string,object> item in frames ) {
            AtlasElement element = new AtlasElement();
             
            element.indexInAtlas = index++;
            
            string name = (string)item.Key;
            
            if( FutileEngine.shouldRemoveAtlasElementFileExtensions ) {
                int extensionPosition = name.LastIndexOf( "." );
                if( extensionPosition >= 0 )
                    name = name.Substring( 0, extensionPosition );
            }

            element.name = name;
            
            IDictionary itemDict = (IDictionary)item.Value;
            
            element.isTrimmed = (bool)itemDict[ "trimmed" ];
            
            if( (bool)itemDict[ "rotated" ] ) {
                throw new NotSupportedException( "Futile no longer supports TexturePacker's \"rotated\" flag. Please disable it when creating the " + DataPath + " atlas." );
            }

            //the uv coordinate rectangle within the atlas
            IDictionary frame = (IDictionary)itemDict[ "frame" ];
            
            float frameX = float.Parse( frame[ "x" ].ToString() );
            float frameY = float.Parse( frame[ "y" ].ToString() );
            float frameW = float.Parse( frame[ "w" ].ToString() );
            float frameH = float.Parse( frame[ "h" ].ToString() ); 
            
            Rect uvRect = new Rect
            (
                frameX / TextureSize.x,
                ( ( TextureSize.y - frameY - frameH ) / TextureSize.y ),
                frameW / TextureSize.x,
                frameH / TextureSize.y
            );
                
            element.uvRect = uvRect;
        
            element.uvTopLeft.Set( uvRect.xMin, uvRect.yMax );
            element.uvTopRight.Set( uvRect.xMax, uvRect.yMax );
            element.uvBottomRight.Set( uvRect.xMax, uvRect.yMin );
            element.uvBottomLeft.Set( uvRect.xMin, uvRect.yMin );


            //the source size is the untrimmed size
            IDictionary sourcePixelSize = (IDictionary)itemDict[ "sourceSize" ];

            element.sourcePixelSize.x = float.Parse( sourcePixelSize[ "w" ].ToString() );    
            element.sourcePixelSize.y = float.Parse( sourcePixelSize[ "h" ].ToString() );    

            element.sourceSize.x = element.sourcePixelSize.x * scaleInverse;    
            element.sourceSize.y = element.sourcePixelSize.y * scaleInverse;


            //this rect is the trimmed size and position relative to the untrimmed rect
            IDictionary sourceRect = (IDictionary)itemDict[ "spriteSourceSize" ];

            float rectX = float.Parse( sourceRect[ "x" ].ToString() ) * scaleInverse;
            float rectY = float.Parse( sourceRect[ "y" ].ToString() ) * scaleInverse;
            float rectW = float.Parse( sourceRect[ "w" ].ToString() ) * scaleInverse;
            float rectH = float.Parse( sourceRect[ "h" ].ToString() ) * scaleInverse;
            
            element.sourceRect = new Rect( rectX, rectY, rectW, rectH );

            Elements.Add( element );
            elementsByName.Add( element.name, element );
        }
        
        Resources.UnloadAsset( dataAsset );
    }
    
    private void CreateElementForEntireAtlas()
    {
        AtlasElement element = new AtlasElement();
        
        element.name = Name;
        element.indexInAtlas = 0;
        
        //TODO: may have to offset the rect slightly
        float scaleInverse = FutileEngine.resourceScaleInverse;
        
        Rect uvRect = new Rect( 0.0f, 0.0f, 1.0f, 1.0f );
        
        element.uvRect = uvRect;
        
        element.uvTopLeft.Set( uvRect.xMin, uvRect.yMax );
        element.uvTopRight.Set( uvRect.xMax, uvRect.yMax );
        element.uvBottomRight.Set( uvRect.xMax, uvRect.yMin );
        element.uvBottomLeft.Set( uvRect.xMin, uvRect.yMin );
        
        
        element.sourceSize = new Vector2( TextureSize.x * scaleInverse, TextureSize.y * scaleInverse );
        element.sourcePixelSize = new Vector2( TextureSize.x, TextureSize.y );

        element.sourceRect = new Rect( 0, 0, TextureSize.x * scaleInverse, TextureSize.y * scaleInverse );


        element.isTrimmed = false;
        
        Elements.Add( element );
        elementsByName.Add( element.name, element );

        FullElement = element;
    }

    public void UpdateElement( AtlasElement element, float leftX, float bottomY, float pixelWidth, float pixelHeight )
    {
        //      TODO: decide whether to scale by resScale or not
        //      float resScale = Futile.resourceScale;
        //      leftX *= resScale;
        //      bottomY *= resScale;
        //      pixelWidth *= resScale;
        //      pixelHeight *= resScale;
        
        element.atlas = this;
        element.atlasIndex = Index;
        
        float scaleInverse = FutileEngine.resourceScaleInverse;
        
        //the uv coordinate rectangle within the atlas
        Rect uvRect = new Rect
            (
                leftX / TextureSize.x,
                bottomY / TextureSize.y,
                pixelWidth / TextureSize.x,
                pixelHeight / TextureSize.y
        );

        element.uvRect = uvRect;
        
        element.uvTopLeft.Set( uvRect.xMin, uvRect.yMax );
        element.uvTopRight.Set( uvRect.xMax, uvRect.yMax );
        element.uvBottomRight.Set( uvRect.xMax, uvRect.yMin );
        element.uvBottomLeft.Set( uvRect.xMin, uvRect.yMin );
        
        //the source size is the untrimmed size
        element.sourcePixelSize.x = pixelWidth; 
        element.sourcePixelSize.y = pixelHeight;    
        
        element.sourceSize.x = element.sourcePixelSize.x * scaleInverse;    
        element.sourceSize.y = element.sourcePixelSize.y * scaleInverse;
        
        //sourceRect is the trimmed rect, inside the other rect, for now always as if untrimmed
        element.sourceRect = new Rect( 0, 0, pixelWidth * scaleInverse, pixelHeight * scaleInverse );
    }

    public AtlasElement CreateUnnamedElement( float leftX, float bottomY, float pixelWidth, float pixelHeight )
    {
        //note that this element has no name, so it is not stored in the atlas or atlasmanager

        AtlasElement element = new AtlasElement();

        element.atlas = this;
        element.atlasIndex = Index;

        UpdateElement( element, leftX, bottomY, pixelWidth, pixelHeight );

        return element;
    }

    public AtlasElement CreateNamedElement( string elementName, float leftX, float bottomY, float pixelWidth, float pixelHeight )
    {
        AtlasElement element = elementsByName[ elementName ];

        if( element == null ) { //it doesn't exist, so create it (if it does exist we just update it)
            element = new AtlasElement();
            element.name = elementName;
            element.atlas = this;
            element.atlasIndex = Index;
            
            elementsByName.Add( elementName, element );
            Elements.Add( element );
            FutileEngine.atlasManager.AddElement( element );
        }

        UpdateElement( element, leftX, bottomY, pixelWidth, pixelHeight );

        return element;
    }

    public void Unload()
    {
        if( isTextureAnAsset ) {
            Resources.UnloadAsset( Texture );
        }
    }
}


