using System;
using System.Collections;
using System.Collections.Generic;
using Futile.Core.Exceptions;
using UnityEngine;

namespace Futile.Core.AtlasCore
{
    public class Atlas
    {
        private readonly bool isTextureAnAsset;
        private Dictionary<string, AtlasElement> elementsByName;

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

            var texture = Resources.Load( imagePath, typeof( Texture ) ) as Texture;

            if( texture == null )
            {
                throw new FutileException( "Couldn't load the atlas texture from: " + ImagePath );
            }

            Init( name, dataPath, texture, index );
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

        private void LoadAtlasData()
        {
            var dataAsset = Resources.Load( DataPath, typeof( TextAsset ) ) as TextAsset;

            if( dataAsset == null )
            {
                throw new FutileException( "Couldn't load the atlas data from: " + DataPath );
            }

            Dictionary<string, object> dict = dataAsset.text.dictionaryFromJson();

            if( dict == null )
            {
                throw new FutileException(
                    string.Format(
                        "The atlas at {0} was not a proper JSON file. Make sure to select \"Unity3D\" in TexturePacker.",
                        DataPath ) );
            }

            var frames = (Dictionary<string, object>)dict[ "frames" ];

            float scaleInverse = FearsomeMonstrousBeast.resourceScaleInverse;

            int index = 0;

            foreach( var item in frames )
            {
                var element = new AtlasElement
                {
                    IndexInAtlas = index++
                };

                string name = item.Key;

                if( FearsomeMonstrousBeast.shouldRemoveAtlasElementFileExtensions )
                {
                    int extensionPosition = name.LastIndexOf( ".", StringComparison.Ordinal );
                    if( extensionPosition >= 0 )
                        name = name.Substring( 0, extensionPosition );
                }

                element.Name = name;

                var itemDict = (IDictionary)item.Value;

                element.IsTrimmed = (bool)itemDict[ "trimmed" ];

                if( (bool)itemDict[ "rotated" ] )
                {
                    throw new NotSupportedException(
                        "Futile no longer supports TexturePacker's \"rotated\" flag. Please disable it when creating the " +
                        DataPath + " atlas." );
                }

                //the uv coordinate rectangle within the atlas
                var frame = (IDictionary)itemDict[ "frame" ];

                float frameX = float.Parse( frame[ "x" ].ToString() );
                float frameY = float.Parse( frame[ "y" ].ToString() );
                float frameW = float.Parse( frame[ "w" ].ToString() );
                float frameH = float.Parse( frame[ "h" ].ToString() );

                var uvRect = new Rect
                    (
                    frameX / TextureSize.x,
                    ( ( TextureSize.y - frameY - frameH ) / TextureSize.y ),
                    frameW / TextureSize.x,
                    frameH / TextureSize.y
                    );

                element.UvRect = uvRect;

                element.UvTopLeft.Set( uvRect.xMin, uvRect.yMax );
                element.UvTopRight.Set( uvRect.xMax, uvRect.yMax );
                element.UvBottomRight.Set( uvRect.xMax, uvRect.yMin );
                element.UvBottomLeft.Set( uvRect.xMin, uvRect.yMin );


                //the source size is the untrimmed size
                var sourcePixelSize = (IDictionary)itemDict[ "sourceSize" ];

                element.SourcePixelSize.x = float.Parse( sourcePixelSize[ "w" ].ToString() );
                element.SourcePixelSize.y = float.Parse( sourcePixelSize[ "h" ].ToString() );

                element.SourceSize.x = element.SourcePixelSize.x * scaleInverse;
                element.SourceSize.y = element.SourcePixelSize.y * scaleInverse;


                //this rect is the trimmed size and position relative to the untrimmed rect
                var sourceRect = (IDictionary)itemDict[ "spriteSourceSize" ];

                float rectX = float.Parse( sourceRect[ "x" ].ToString() ) * scaleInverse;
                float rectY = float.Parse( sourceRect[ "y" ].ToString() ) * scaleInverse;
                float rectW = float.Parse( sourceRect[ "w" ].ToString() ) * scaleInverse;
                float rectH = float.Parse( sourceRect[ "h" ].ToString() ) * scaleInverse;

                element.SourceRect = new Rect( rectX, rectY, rectW, rectH );

                Elements.Add( element );
                elementsByName.Add( element.Name, element );
            }

            Resources.UnloadAsset( dataAsset );
        }

        private void CreateElementForEntireAtlas()
        {
            var element = new AtlasElement();

            element.Name = Name;
            element.IndexInAtlas = 0;

            //TODO: may have to offset the rect slightly
            float scaleInverse = FearsomeMonstrousBeast.resourceScaleInverse;

            var uvRect = new Rect( 0.0f, 0.0f, 1.0f, 1.0f );

            element.UvRect = uvRect;

            element.UvTopLeft.Set( uvRect.xMin, uvRect.yMax );
            element.UvTopRight.Set( uvRect.xMax, uvRect.yMax );
            element.UvBottomRight.Set( uvRect.xMax, uvRect.yMin );
            element.UvBottomLeft.Set( uvRect.xMin, uvRect.yMin );


            element.SourceSize = new Vector2( TextureSize.x * scaleInverse, TextureSize.y * scaleInverse );
            element.SourcePixelSize = new Vector2( TextureSize.x, TextureSize.y );

            element.SourceRect = new Rect( 0, 0, TextureSize.x * scaleInverse, TextureSize.y * scaleInverse );


            element.IsTrimmed = false;

            Elements.Add( element );
            elementsByName.Add( element.Name, element );

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

            element.Atlas = this;
            element.AtlasIndex = Index;

            float scaleInverse = FearsomeMonstrousBeast.resourceScaleInverse;

            //the uv coordinate rectangle within the atlas
            var uvRect = new Rect
                (
                leftX / TextureSize.x,
                bottomY / TextureSize.y,
                pixelWidth / TextureSize.x,
                pixelHeight / TextureSize.y
                );

            element.UvRect = uvRect;

            element.UvTopLeft.Set( uvRect.xMin, uvRect.yMax );
            element.UvTopRight.Set( uvRect.xMax, uvRect.yMax );
            element.UvBottomRight.Set( uvRect.xMax, uvRect.yMin );
            element.UvBottomLeft.Set( uvRect.xMin, uvRect.yMin );

            //the source size is the untrimmed size
            element.SourcePixelSize.x = pixelWidth;
            element.SourcePixelSize.y = pixelHeight;

            element.SourceSize.x = element.SourcePixelSize.x * scaleInverse;
            element.SourceSize.y = element.SourcePixelSize.y * scaleInverse;

            //sourceRect is the trimmed rect, inside the other rect, for now always as if untrimmed
            element.SourceRect = new Rect( 0, 0, pixelWidth * scaleInverse, pixelHeight * scaleInverse );
        }

        public AtlasElement CreateUnnamedElement( float leftX, float bottomY, float pixelWidth, float pixelHeight )
        {
            //note that this element has no name, so it is not stored in the atlas or atlasmanager

            var element = new AtlasElement();

            element.Atlas = this;
            element.AtlasIndex = Index;

            UpdateElement( element, leftX, bottomY, pixelWidth, pixelHeight );

            return element;
        }

        public AtlasElement CreateNamedElement( string elementName, float leftX, float bottomY, float pixelWidth,
                                                float pixelHeight )
        {
            AtlasElement element = elementsByName[ elementName ];

            if( element == null )
            {
                //it doesn't exist, so create it (if it does exist we just update it)
                element = new AtlasElement();
                element.Name = elementName;
                element.Atlas = this;
                element.AtlasIndex = Index;

                elementsByName.Add( elementName, element );
                Elements.Add( element );
                FearsomeMonstrousBeast.atlasManager.AddElement( element );
            }

            UpdateElement( element, leftX, bottomY, pixelWidth, pixelHeight );

            return element;
        }

        public void Unload()
        {
            if( isTextureAnAsset )
            {
                Resources.UnloadAsset( Texture );
            }
        }
    }
}