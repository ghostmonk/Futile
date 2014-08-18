using System;
using UnityEngine;
using System.Collections.Generic;
using Futile.Core.AtlasCore;
using Futile.Core.Exceptions;

namespace Futile.Core.FontCore
{
    //parts of this were inspired by https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UIElements/UIText.cs
    //how to interpret BMFont files: http://www.gamedev.net/topic/284560-bmfont-and-how-to-interpret-the-fnt-file/
    public class Font
    {
        public const int ASCII_NEWLINE = 10;
        public const int ASCII_SPACE = 32;
        public const int ASCII_HYPHEN_MINUS = 45;
        public const int ASCII_LINEHEIGHT_REFERENCE = 77; //77 is the letter M

        private readonly string configPath;
        private readonly AtlasElement element;
        private readonly string name;
        private readonly KerningInfo nullKerning = new KerningInfo();
        private readonly float offsetX;
        private readonly float offsetY;
        private readonly TextParams textParams;

        private CharInfo[] charInfos;
        private Dictionary<uint, CharInfo> charInfosById; //chars with the index of array being the char id
        private float configRatio;
        private int configWidth;
        private int kerningCount;
        private KerningInfo[] kerningInfos;
        private float lineHeight;

        public Font( string name, AtlasElement element, string configPath, float offsetX, float offsetY, TextParams textParams )
        {
            this.name = name;
            this.element = element;
            this.configPath = configPath;
            this.textParams = textParams;

            this.offsetX = offsetX;
            this.offsetY = offsetY;

            LoadAndParseConfigFile();
        }

        public string Name { get { return name; } }

        public AtlasElement Element { get { return element; } }

        public TextParams TextParams { get { return textParams; } }

        public float OffsetX { get { return offsetX; } }

        public float OffsetY { get { return offsetY; } }

        private void LoadAndParseConfigFile()
        {
            TextAsset asset = (TextAsset)Resources.Load( configPath, typeof( TextAsset ) );

            if( asset == null )
            {
                throw new FutileException( "Couldn't find font config file " + configPath );
            }

            string[] separators = new string[1];

            separators[ 0 ] = "\n"; //osx
            string[] lines = asset.text.Split( separators, StringSplitOptions.RemoveEmptyEntries );

            if( lines.Length <= 1 ) //osx line endings didn't work, try windows
            {
                separators[ 0 ] = "\r\n";
                lines = asset.text.Split( separators, StringSplitOptions.RemoveEmptyEntries );
            }

            if( lines.Length <= 1 ) //those line endings didn't work, so we're on a magical OS
            {
                separators[ 0 ] = "\r";
                lines = asset.text.Split( separators, StringSplitOptions.RemoveEmptyEntries );
            }

            if( lines.Length <= 1 ) //WHAT
            {
                throw new FutileException( "Your font file is messed up" );
            }

            int c = 0;
            int k = 0;

            charInfosById = new Dictionary<uint, CharInfo>( 127 );

            //insert an empty char to be used when a character isn't in the font data file
            CharInfo emptyChar = new CharInfo();
            charInfosById[ 0 ] = emptyChar;

            float resourceScaleInverse = FearsomeMonstrousBeast.resourceScaleInverse;

            Vector2 textureSize = element.Atlas.TextureSize;

            bool wasKerningFound = false;

            int lineCount = lines.Length;

            for( int n = 0; n < lineCount; ++n )
            {
                string line = lines[ n ];

                string[] words = line.Split( new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );

                /* we don't care about these, or else they could be in the elseif
                if(words[0] == "info") //info face="Franchise Bold" size=160 bold=0 italic=0 charset="" unicode=0 stretchH=100 smooth=1 aa=1 padding=0,0,0,0 spacing=1,1
                {
                    //do nothing
                }
                else if(words[0] == "page") //page id=0 file="FranchiseLarge"
                {
                    //do nothing
                }
                */

                if( words[ 0 ] == "common" ) 
                {
                    //common lineHeight=168 base=26 scaleW=1024 scaleH=1024 pages=1 packed=0
                    //these are the height and width of the original atlas built by Hiero
                    configWidth = int.Parse( words[ 3 ].Split( '=' )[ 1 ] );
                    //_configHeight = int.Parse(words[4].Split('=')[1]);

                    //this is the ratio of the config vs the size of the actual texture element
                    configRatio = element.SourcePixelSize.x / (float)configWidth;

                    lineHeight = ( (float)int.Parse( words[ 1 ].Split( '=' )[ 1 ] ) ) * configRatio *
                                  resourceScaleInverse;
                    //_lineBase = int.Parse(words[2].Split('=')[1]) * _configRatio; 
                }
                else if( words[ 0 ] == "chars" ) 
                {
                    //chars count=92
                    int charCount = int.Parse( words[ 1 ].Split( '=' )[ 1 ] );
                    charInfos = new CharInfo[charCount + 1]; //gotta add 1 because the charCount seems to be off by 1
                }
                else
                {
                    int wordCount = 0;
                    if( words[ 0 ] == "char" )
                    {
                        //char id=32   x=0     y=0     width=0     height=0     xoffset=0     yoffset=120    xadvance=29     page=0  chnl=0 letter=a
                        CharInfo charInfo = new CharInfo();

                        wordCount = words.Length;

                        for( int w = 1; w < wordCount; ++w )
                        {
                            string[] parts = words[ w ].Split( '=' );
                            string partName = parts[ 0 ];

                            if( partName == "letter" )
                            {
                                if( parts[ 1 ].Length >= 3 )
                                {
                                    charInfo.Letter = parts[ 1 ].Substring( 1, 1 );
                                }
                                continue; //we don't care about the letter  
                            }

                            if( partName == "\r" )
                            {
                                continue;
                            } //something weird happened with linebreaks, meh!

                            int partIntValue = int.Parse( parts[ 1 ] );
                            float partFloatValue = (float)partIntValue;

                            if( partName == "id" )
                            {
                                charInfo.CharId = partIntValue;
                            }
                            else if( partName == "x" )
                            {
                                charInfo.X = partFloatValue * configRatio -
                                             element.SourceRect.x * FearsomeMonstrousBeast.resourceScale;
                                //offset to account for the trimmed atlas
                            }
                            else if( partName == "y" )
                            {
                                charInfo.Y = partFloatValue * configRatio -
                                             element.SourceRect.y * FearsomeMonstrousBeast.resourceScale;
                                //offset to account for the trimmed atlas
                            }
                            else if( partName == "width" )
                            {
                                charInfo.Width = partFloatValue * configRatio;
                            }
                            else if( partName == "height" )
                            {
                                charInfo.Height = partFloatValue * configRatio;
                            }
                            else if( partName == "xoffset" )
                            {
                                charInfo.OffsetX = partFloatValue * configRatio;
                            }
                            else if( partName == "yoffset" )
                            {
                                charInfo.OffsetY = partFloatValue * configRatio;
                            }
                            else if( partName == "xadvance" )
                            {
                                charInfo.Xadvance = partFloatValue * configRatio;
                            }
                            else if( partName == "page" )
                            {
                                charInfo.Page = partIntValue;
                            }
                        }

                        Rect uvRect = new Rect
                            (
                            element.UvRect.x + charInfo.X / textureSize.x,
                            ( textureSize.y - charInfo.Y - charInfo.Height ) / textureSize.y -
                            ( 1.0f - element.UvRect.yMax ),
                            charInfo.Width / textureSize.x,
                            charInfo.Height / textureSize.y
                            );

                        charInfo.UvRect = uvRect;

                        charInfo.UvTopLeft.Set( uvRect.xMin, uvRect.yMax );
                        charInfo.UvTopRight.Set( uvRect.xMax, uvRect.yMax );
                        charInfo.UvBottomRight.Set( uvRect.xMax, uvRect.yMin );
                        charInfo.UvBottomLeft.Set( uvRect.xMin, uvRect.yMin );

                        //scale them AFTER they've been used for uvs
                        charInfo.Width *= resourceScaleInverse;
                        charInfo.Height *= resourceScaleInverse;
                        charInfo.OffsetX *= resourceScaleInverse;
                        charInfo.OffsetY *= resourceScaleInverse;
                        charInfo.Xadvance *= resourceScaleInverse;

                        charInfosById[ (uint)charInfo.CharId ] = charInfo;
                        charInfos[ c ] = charInfo;

                        c++;
                    }
                    else if( words[ 0 ] == "kernings" ) //kernings count=169
                    {
                        wasKerningFound = true;
                        //kerning count can be wrong so just add 100 items of potential fudge factor
                        int count = int.Parse( words[ 1 ].Split( '=' )[ 1 ] ) + 100;
                        kerningInfos = new KerningInfo[ count ];
                    }
                    else if( words[ 0 ] == "kerning" ) //kerning first=56  second=57  amount=-1
                    {
                        KerningInfo kerningInfo = new KerningInfo { First = -1 };

                        wordCount = words.Length;

                        for( int w = 1; w < wordCount; w++ )
                        {
                            string[] parts = words[ w ].Split( '=' );
                            if( parts.Length >= 2 )
                            {
                                string partName = parts[ 0 ];
                                int partValue = int.Parse( parts[ 1 ] );

                                if( partName == "first" )
                                {
                                    kerningInfo.First = partValue;
                                }
                                else if( partName == "second" )
                                {
                                    kerningInfo.Second = partValue;
                                }
                                else if( partName == "amount" )
                                {
                                    kerningInfo.Amount = ( (float)partValue ) * configRatio * resourceScaleInverse;
                                }
                            }
                        }

                        if( kerningInfo.First != -1 )
                        {
                            kerningInfos[ k ] = kerningInfo;
                        }

                        k++;
                    }
                }
            }

            kerningCount = k;


            if( !wasKerningFound )
                //if there are no kernings at all (like in a pixel font), then make an empty kerning array
            {
                kerningInfos = new KerningInfo[0];
            }

            //make sure the space character doesn't have offsetY and offsetX
            if( !charInfosById.ContainsKey( ASCII_SPACE ) ) return;
            
            charInfosById[ ASCII_SPACE ].OffsetX = 0;
            charInfosById[ ASCII_SPACE ].OffsetY = 0;
        }

        public LetterQuadLine[] GetQuadInfoForText( string text, TextParams labelTextParams )
        {
            int lineCount = 0;
            int letterCount = 0;

            char[] letters = text.ToCharArray();

            //at some point these should probably be pooled and reused so we're not allocing new ones all the time
            //now they're structs though, so it might not be an issue
            LetterQuadLine[] lines = new LetterQuadLine[10];

            int lettersLength = letters.Length;
            for( int c = 0; c < lettersLength; ++c )
            {
                char letter = letters[ c ];

                if( letter == ASCII_NEWLINE )
                {
                    lines[ lineCount ] = new LetterQuadLine();
                    lines[ lineCount ].LetterCount = letterCount;
                    lines[ lineCount ].Quads = new LetterQuad[letterCount];

                    lineCount++;
                    letterCount = 0;
                }
                else
                {
                    letterCount++;
                }
            }

            lines[ lineCount ] = new LetterQuadLine();
            lines[ lineCount ].LetterCount = letterCount;
            lines[ lineCount ].Quads = new LetterQuad[letterCount];

            LetterQuadLine[] oldLines = lines;
            lines = new LetterQuadLine[lineCount + 1];

            for( int c = 0; c < lineCount + 1; ++c )
            {
                lines[ c ] = oldLines[ c ];
            }

            lineCount = 0;
            letterCount = 0;

            float nextX = 0;
            float nextY = 0;

            CharInfo charInfo;

            char previousLetter = '\0';

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            float usableLineHeight = lineHeight + labelTextParams.ScaledLineHeightOffset +
                                     textParams.ScaledLineHeightOffset;

            for( int c = 0; c < lettersLength; ++c )
            {
                char letter = letters[ c ];

                if( letter == ASCII_NEWLINE )
                {
                    if( letterCount == 0 )
                    {
                        lines[ lineCount ].Bounds = new Rect( 0, 0, nextY, nextY - usableLineHeight );
                    }
                    else
                    {
                        lines[ lineCount ].Bounds = new Rect( minX, minY, maxX - minX, maxY - minY );
                    }

                    minX = float.MaxValue;
                    maxX = float.MinValue;
                    minY = float.MaxValue;
                    maxY = float.MinValue;

                    nextX = 0;
                    nextY -= usableLineHeight;

                    lineCount++;
                    letterCount = 0;
                }
                else
                {
                    KerningInfo foundKerning = nullKerning;

                    for( int k = 0; k < kerningCount; k++ )
                    {
                        KerningInfo kerningInfo = kerningInfos[ k ];
                        if( kerningInfo.First == previousLetter && kerningInfo.Second == letter )
                        {
                            foundKerning = kerningInfo;
                        }
                    }

                    LetterQuad letterQuad = new LetterQuad();

                    if( !charInfosById.TryGetValue( letter, out charInfo ) )
                    {
                        charInfo = charInfosById[ 0 ];
                    }

                    float totalKern = foundKerning.Amount + labelTextParams.ScaledKerningOffset +
                                      textParams.ScaledKerningOffset;

                    if( letterCount == 0 )
                    {
                        nextX = -charInfo.OffsetX; //don't offset the first character
                    }
                    else
                    {
                        nextX += totalKern;
                    }

                    letterQuad.CharInfo = charInfo;

                    Rect quadRect = new Rect( 
                        nextX + charInfo.OffsetX, 
                        nextY - charInfo.OffsetY - charInfo.Height,
                        charInfo.Width, 
                        charInfo.Height );

                    letterQuad.Rect = quadRect;

                    lines[ lineCount ].Quads[ letterCount ] = letterQuad;

                    minX = Math.Min( minX, quadRect.xMin );
                    maxX = Math.Max( maxX, quadRect.xMax );
                    minY = Math.Min( minY, nextY - usableLineHeight );
                    maxY = Math.Max( maxY, nextY );

                    nextX += charInfo.Xadvance;

                    letterCount++;
                }

                previousLetter = letter;
            }

            if( letterCount == 0 ) //there were no letters, so minX and minY would be crazy if we used them
            {
                lines[ lineCount ].Bounds = new Rect( 0, 0, nextY, nextY - usableLineHeight );
            }
            else
            {
                lines[ lineCount ].Bounds = new Rect( minX, minY, maxX - minX, maxY - minY );
            }

            for( int n = 0; n < lineCount + 1; n++ )
            {
                lines[ n ].Bounds.height += labelTextParams.ScaledLineHeightOffset + textParams.ScaledLineHeightOffset;
            }

            return lines;
        }

        public AtlasElement GetElementForChar( char character )
        {
            CharInfo charInfo;

            if( !charInfosById.TryGetValue( character, out charInfo ) )
            {
                return null;
            }

            AtlasElement charElement = new AtlasElement();
            charElement.Atlas = element.Atlas;
            charElement.AtlasIndex = element.AtlasIndex;
            charElement.IndexInAtlas = -1;
            charElement.IsTrimmed = true;
            charElement.Name = ""; //_name + "_" + character.ToString();

            charElement.UvRect = charInfo.UvRect;
            charElement.SourceRect = new Rect( 0, 0, charInfo.Width, charInfo.Height );
            charElement.SourceSize = new Vector2( charInfo.Width, charInfo.Height );
            charElement.SourcePixelSize = new Vector2( charInfo.Width * FearsomeMonstrousBeast.resourceScale,
                                                       charInfo.Height * FearsomeMonstrousBeast.resourceScale );

            Rect uvRect = charElement.UvRect;

            charElement.UvTopLeft.Set( uvRect.xMin, uvRect.yMax );
            charElement.UvTopRight.Set( uvRect.xMax, uvRect.yMax );
            charElement.UvBottomRight.Set( uvRect.xMax, uvRect.yMin );
            charElement.UvBottomLeft.Set( uvRect.xMin, uvRect.yMin );

            return charElement;
        }

        //  Not gonna deal with this stuff unless it's actually needed
        //  In theory it'll handle nice quotes
        //
        //  private int forceLowAsciiChar(int charID)
        //  {
        //      if(charID < 8200) return charID; //short circuit so we don't branch 6 times
        //      
        //      if(charID == 8211) return 150;
        //      if(charID == 8212) return 151;
        //      if(charID == 8216) return 145;
        //      if(charID == 8217) return 146;
        //      if(charID == 8220) return 147;
        //      if(charID == 8221) return 148;
        //          
        //      return charID;  
        //  }
    }
}