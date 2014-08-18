using System;
using UnityEngine;
using System.Collections.Generic;
using Futile.Core.Exceptions;
using Futile.Core.FontCore;
using Font = Futile.Core.FontCore.Font;

namespace Futile.Core.AtlasCore
{
    public class AtlasManager
    {
        private static int NEXT_ATLAS_INDEX;

        private readonly Dictionary<string, AtlasElement> allElementsByName = new Dictionary<string, AtlasElement>();
        private readonly List<Atlas> atlases = new List<Atlas>();
        private readonly List<Font> fonts = new List<Font>();
        private readonly Dictionary<string, Font> fontsByName = new Dictionary<string, Font>();

        public Atlas GetAtlasWithName( string name )
        {
            int atlasCount = atlases.Count;
            for( int a = 0; a < atlasCount; ++a )
            {
                if( atlases[ a ].Name == name )
                {
                    return atlases[ a ];
                }
            }
            return null;
        }

        public bool DoesContainAtlas( string name )
        {
            int atlasCount = atlases.Count;
            for( int a = 0; a < atlasCount; ++a )
            {
                if( atlases[ a ].Name == name )
                {
                    return true;
                }
            }
            return false;
        }

        public Atlas LoadAtlasFromTexture( string name, Texture texture )
        {
            if( DoesContainAtlas( name ) )
            {
                return GetAtlasWithName( name );
            } //we already have it, don't load it again

            Atlas atlas = new Atlas( name, texture, NEXT_ATLAS_INDEX++ );

            AddAtlas( atlas );

            return atlas;
        }

        public Atlas LoadAtlasFromTexture( string name, string dataPath, Texture texture )
        {
            if( DoesContainAtlas( name ) )
            {
                return GetAtlasWithName( name );
            } //we already have it, don't load it again

            Atlas atlas = new Atlas( name, dataPath, texture, NEXT_ATLAS_INDEX++ );

            AddAtlas( atlas );

            return atlas;
        }

        public Atlas ActuallyLoadAtlasOrImage( string name, string imagePath, string dataPath )
        {
            if( DoesContainAtlas( name ) )
            {
                return GetAtlasWithName( name );
            } //we already have it, don't load it again

            //if dataPath is empty, load it as a single image
            bool isSingleImage = ( dataPath == "" );

            Atlas atlas = new Atlas( name, imagePath, dataPath, NEXT_ATLAS_INDEX++, isSingleImage );

            AddAtlas( atlas );

            return atlas;
        }

        private void AddAtlas( Atlas atlas )
        {
            int elementCount = atlas.Elements.Count;
            for( int e = 0; e < elementCount; ++e )
            {
                AtlasElement element = atlas.Elements[ e ];

                element.Atlas = atlas;
                element.AtlasIndex = atlas.Index;

                if( allElementsByName.ContainsKey( element.Name ) )
                {
                    throw new FutileException( "Duplicate element name '" + element.Name +
                                               "' found! All element names must be unique!" );
                }
                else
                {
                    allElementsByName.Add( element.Name, element );
                }
            }

            atlases.Add( atlas );
        }

        public Atlas LoadAtlas( string atlasPath )
        {
            return LoadAtlas( atlasPath, true );
        }

        public Atlas LoadAtlas( string atlasPath, bool shouldUseResourceSuffix )
        {
            if( DoesContainAtlas( atlasPath ) )
            {
                return GetAtlasWithName( atlasPath );
            } //we already have it, don't load it again

            string pathWithSuffix = shouldUseResourceSuffix
                ? atlasPath + FearsomeMonstrousBeast.resourceSuffix
                : atlasPath;
            string filePath = pathWithSuffix + "_png";

            TextAsset imageBytes = Resources.Load( filePath, typeof( TextAsset ) ) as TextAsset;

            if( imageBytes != null ) //do we have png bytes?
            {
                Texture2D texture = new Texture2D( 0, 0, TextureFormat.ARGB32, false );

                texture.LoadImage( imageBytes.bytes );

                Resources.UnloadAsset( imageBytes );

                texture.Apply( false, true );

                return LoadAtlasFromTexture( atlasPath, pathWithSuffix, texture );
            }
            else //load it as a normal Unity image asset
            {
                return ActuallyLoadAtlasOrImage( atlasPath, pathWithSuffix, pathWithSuffix );
            }
        }

        public Atlas LoadImage( string imagePath )
        {
            return LoadImage( imagePath, true );
        }

        public Atlas LoadImage( string imagePath, bool shouldUseResourceSuffix )
        {
            if( DoesContainAtlas( imagePath ) )
            {
                return GetAtlasWithName( imagePath );
            } //we already have it

            string pathWithSuffix = shouldUseResourceSuffix
                ? imagePath + FearsomeMonstrousBeast.resourceSuffix
                : imagePath;
            string filePath = pathWithSuffix + "_png";

            TextAsset imageBytes = Resources.Load( filePath, typeof( TextAsset ) ) as TextAsset;

            if( imageBytes != null ) //do we have png bytes?
            {
                Texture2D texture = new Texture2D( 0, 0, TextureFormat.ARGB32, false );

                texture.LoadImage( imageBytes.bytes );

                Resources.UnloadAsset( imageBytes );

                texture.Apply( false, true );

                return LoadAtlasFromTexture( imagePath, texture );
            }
            else //load it as a normal Unity image asset
            {
                return ActuallyLoadAtlasOrImage( imagePath, pathWithSuffix, "" );
            }
        }

        public void ActuallyUnloadAtlasOrImage( string name )
        {
            bool wasAtlasRemoved = false;

            int atlasCount = atlases.Count;

            for( int a = atlasCount - 1; a >= 0; a-- ) //reverse order so deletions ain't no thang
            {
                Atlas atlas = atlases[ a ];

                if( atlas.Name == name )
                {
                    int elementCount = atlas.Elements.Count;

                    for( int e = 0; e < elementCount; e++ )
                    {
                        allElementsByName.Remove( atlas.Elements[ e ].Name );
                    }

                    atlas.Unload();
                    atlases.RemoveAt( a );

                    wasAtlasRemoved = true;

                    FearsomeMonstrousBeast.stage.renderer.ClearLayersThatUseAtlas( atlas );
                }
            }

            if( wasAtlasRemoved )
            {
                Resources.UnloadUnusedAssets();
            }
        }

        //assuming resourceSuffix of _ipad, will attempt to load suffix _0_ipad, _1_ipad, etc until it misses
        //this is the TexturePacker default multiple atlas format

        public void LoadSequentialAtlases( string atlasPath )
        {
            LoadSequentialAtlases( atlasPath, true );
        }

        public void LoadSequentialAtlases( string atlasPath, bool shouldUseResourceSuffix )
        {
            int index = 0;
            while( true )
            {
                string filePath = atlasPath + "_" + index;
                if( DoesContainAtlas( atlasPath ) )
                {
                    return;
                } //we already have it, don't load it again

                string fullFilePath = shouldUseResourceSuffix
                    ? filePath + FearsomeMonstrousBeast.resourceSuffix
                    : filePath;

                TextAsset text = Resources.Load( fullFilePath, typeof( TextAsset ) ) as TextAsset;

                if( text == null )
                {
                    break;
                }
                else
                {
                    Resources.UnloadAsset( text );
                    LoadAtlas( filePath );
                }

                index++;
            }
        }

        public void UnloadAtlas( string atlasPath )
        {
            ActuallyUnloadAtlasOrImage( atlasPath );
        }

        public void UnloadImage( string imagePath )
        {
            ActuallyUnloadAtlasOrImage( imagePath );
        }

        public bool DoesContainElementWithName( string elementName )
        {
            return allElementsByName.ContainsKey( elementName );
        }

        public AtlasElement GetElementWithName( string elementName )
        {
            if( allElementsByName.ContainsKey( elementName ) )
            {
                return allElementsByName[ elementName ];
            }
            else
            {
                //Try to make an educated guess about what they were trying to load
                //First we get the last part of the path (the file name) and then we remove the extension
                //Then we check to see if that string is in any of our element names 
                //(perhaps they have the path wrong or are mistakenly using a .png extension)

                String lastChunk = null;

                if( elementName.Contains( "\\" ) )
                {
                    String[] chunks = elementName.Split( '\\' );
                    lastChunk = chunks[ chunks.Length - 1 ];
                }
                else
                {
                    String[] chunks = elementName.Split( '/' );
                    lastChunk = chunks[ chunks.Length - 1 ];
                }

                String replacementName = null;

                if( lastChunk != null )
                {
                    lastChunk = lastChunk.Split( '.' )[ 0 ]; //remove the extension

                    foreach( KeyValuePair<String, AtlasElement> pair in allElementsByName )
                    {
                        if( pair.Value.Name.Contains( lastChunk ) )
                        {
                            replacementName = pair.Value.Name;
                        }
                    }
                }

                if( replacementName == null )
                {
                    throw new FutileException( "Couldn't find element named '" + elementName +
                                               "'. \nUse Futile.atlasManager.LogAllElementNames() to see a list of all loaded elements names" );
                }
                else
                {
                    throw new FutileException( "Couldn't find element named '" + elementName + "'. Did you mean '" +
                                               replacementName +
                                               "'? \nUse Futile.atlasManager.LogAllElementNames() to see a list of all loaded element names." );
                }
            }
        }

        public Font GetFontWithName( string fontName )
        {
            if( fontsByName.ContainsKey( fontName ) )
            {
                return fontsByName[ fontName ];
            }
            else
            {
                throw new FutileException( "Couldn't find font named '" + fontName + "'" );
            }
        }

        public void LoadFont( string name, string elementName, string configPath, float offsetX, float offsetY )
        {
            LoadFont( name, elementName, configPath, offsetX, offsetY, new TextParams() );
        }

        public void LoadFont( string name, string elementName, string configPath, float offsetX, float offsetY,
                              TextParams textParams )
        {
            AtlasElement element = GetElementWithName( elementName );
            Font font = new Font( name, element, configPath, offsetX, offsetY, textParams );

            fonts.Add( font );
            fontsByName.Add( name, font );
        }

        public void AddElement( AtlasElement element ) //It's recommended to use myAtlas.CreateElement() instead of this
        {
            if( allElementsByName.ContainsKey( element.Name ) )
            {
                throw new FutileException( "Duplicate element name '" + element.Name +
                                           "' found! All element names must be unique!" );
            }
            else
            {
                allElementsByName.Add( element.Name, element );
            }
        }

        public void LogAllElementNames()
        {
            Debug.Log( "Logging all element names:" );

            foreach( KeyValuePair<String, AtlasElement> pair in allElementsByName )
            {
                Debug.Log( "'" + pair.Value.Name + "'" );
            }
        }
    }
}