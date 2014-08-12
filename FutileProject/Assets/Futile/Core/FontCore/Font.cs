using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Futile.Core.AtlasCore;

//parts of this were inspired by https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UIElements/UIText.cs

//how to interpret BMFont files: http://www.gamedev.net/topic/284560-bmfont-and-how-to-interpret-the-fnt-file/
public class Font
{
    public const int ASCII_NEWLINE = 10;
    public const int ASCII_SPACE = 32;
    public const int ASCII_HYPHEN_MINUS = 45;
    public const int ASCII_LINEHEIGHT_REFERENCE = 77; //77 is the letter M
    
    private string _name;
    private AtlasElement _element;
    private string _configPath;
    private CharInfo[] _charInfos;
    private Dictionary<uint,CharInfo> _charInfosByID; //chars with the index of array being the char id
    private KerningInfo[] _kerningInfos;
    private int _kerningCount;
    private KerningInfo _nullKerning = new KerningInfo();
    private float _lineHeight;
    //private float _lineBase;
    private int _configWidth;
    //private int _configHeight;
    private float _configRatio;
    private TextParams _textParams;
    private float _offsetX;
    private float _offsetY;
    
    public Font( string name, AtlasElement element, string configPath, float offsetX, float offsetY, TextParams textParams )
    {
        _name = name;
        _element = element;
        _configPath = configPath;
        _textParams = textParams;

        _offsetX = offsetX; 
        _offsetY = offsetY;
        
        LoadAndParseConfigFile();
    }
    
    private void LoadAndParseConfigFile()
    {
        TextAsset asset = (TextAsset)Resources.Load( _configPath, typeof( TextAsset ) );
        
        if( asset == null )
        {
            throw new FutileException( "Couldn't find font config file " + _configPath );   
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
        
        int wordCount = 0;
        int c = 0;
        int k = 0;
        
        _charInfosByID = new Dictionary<uint, CharInfo>( 127 );
        
        //insert an empty char to be used when a character isn't in the font data file
        CharInfo emptyChar = new CharInfo();
        _charInfosByID[ 0 ] = emptyChar;
        
        float resourceScaleInverse = FearsomeMonstrousBeast.resourceScaleInverse;
        
        Vector2 textureSize = _element.atlas.TextureSize;

        bool wasKerningFound = false;
        
        int lineCount = lines.Length;
        
        for( int n = 0; n<lineCount; ++n )
        {
            string line = lines[ n ];
            
            string [] words = line.Split( new char[] {' '}, StringSplitOptions.RemoveEmptyEntries );
            
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
            
            if( words[ 0 ] == "common" ) //common lineHeight=168 base=26 scaleW=1024 scaleH=1024 pages=1 packed=0
            {
                //these are the height and width of the original atlas built by Hiero
                _configWidth = int.Parse( words[ 3 ].Split( '=' )[ 1 ] );
                //_configHeight = int.Parse(words[4].Split('=')[1]);
                
                //this is the ratio of the config vs the size of the actual texture element
                _configRatio = _element.sourcePixelSize.x / (float)_configWidth;

                _lineHeight = ( (float)int.Parse( words[ 1 ].Split( '=' )[ 1 ] ) ) * _configRatio * resourceScaleInverse;   
                //_lineBase = int.Parse(words[2].Split('=')[1]) * _configRatio; 
            }
            else if( words[ 0 ] == "chars" ) //chars count=92
            {
                int charCount = int.Parse( words[ 1 ].Split( '=' )[ 1 ] );
                _charInfos = new CharInfo[charCount + 1]; //gotta add 1 because the charCount seems to be off by 1
            }
            else if( words[ 0 ] == "char" ) //char id=32   x=0     y=0     width=0     height=0     xoffset=0     yoffset=120    xadvance=29     page=0  chnl=0 letter=a
            {
                CharInfo charInfo = new CharInfo();
                
                wordCount = words.Length;
                
                for( int w = 1; w<wordCount; ++w )
                {
                    string[] parts = words[ w ].Split( '=' );   
                    string partName = parts[ 0 ];
                    
                    if( partName == "letter" )
                    {
                        if( parts[ 1 ].Length >= 3 )
                        {
                            charInfo.letter = parts[ 1 ].Substring( 1, 1 );
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
                        charInfo.charID = partIntValue;
                    }
                    else if( partName == "x" )
                    {
                        charInfo.x = partFloatValue * _configRatio - _element.sourceRect.x * FearsomeMonstrousBeast.resourceScale; //offset to account for the trimmed atlas
                    }
                    else if( partName == "y" )
                    {
                        charInfo.y = partFloatValue * _configRatio - _element.sourceRect.y * FearsomeMonstrousBeast.resourceScale; //offset to account for the trimmed atlas
                    }
                    else if( partName == "width" )
                    {
                        charInfo.width = partFloatValue * _configRatio;
                    }
                    else if( partName == "height" )
                    {
                        charInfo.height = partFloatValue * _configRatio;
                    }
                    else if( partName == "xoffset" )
                    {
                        charInfo.offsetX = partFloatValue * _configRatio;
                    }
                    else if( partName == "yoffset" )
                    {
                        charInfo.offsetY = partFloatValue * _configRatio;
                    }
                    else if( partName == "xadvance" )
                    {
                        charInfo.xadvance = partFloatValue * _configRatio;
                    }
                    else if( partName == "page" )
                    {
                        charInfo.page = partIntValue;
                    }
                }

                Rect uvRect = new Rect  
                (
                    _element.uvRect.x + charInfo.x / textureSize.x,
                    ( textureSize.y - charInfo.y - charInfo.height ) / textureSize.y - ( 1.0f - _element.uvRect.yMax ),
                    charInfo.width / textureSize.x,
                    charInfo.height / textureSize.y
                );

                charInfo.uvRect = uvRect;
                
                charInfo.uvTopLeft.Set( uvRect.xMin, uvRect.yMax );
                charInfo.uvTopRight.Set( uvRect.xMax, uvRect.yMax );
                charInfo.uvBottomRight.Set( uvRect.xMax, uvRect.yMin );
                charInfo.uvBottomLeft.Set( uvRect.xMin, uvRect.yMin );

                //scale them AFTER they've been used for uvs
                charInfo.width *= resourceScaleInverse;
                charInfo.height *= resourceScaleInverse;
                charInfo.offsetX *= resourceScaleInverse;
                charInfo.offsetY *= resourceScaleInverse;
                charInfo.xadvance *= resourceScaleInverse;

                _charInfosByID[ (uint)charInfo.charID ] = charInfo;
                _charInfos[ c ] = charInfo;
                
                c++;
            }
            else if( words[ 0 ] == "kernings" ) //kernings count=169
            {
                wasKerningFound = true;
                int kerningCount = int.Parse( words[ 1 ].Split( '=' )[ 1 ] );
                _kerningInfos = new KerningInfo[kerningCount + 100]; //kerning count can be wrong so just add 100 items of potential fudge factor
            }
            else if( words[ 0 ] == "kerning" ) //kerning first=56  second=57  amount=-1
            {
                KerningInfo kerningInfo = new KerningInfo();
                
                kerningInfo.first = -1;
                
                wordCount = words.Length;
                
                for( int w = 1; w<wordCount; w++ )
                {
                    string[] parts = words[ w ].Split( '=' );   
                    if( parts.Length >= 2 )
                    {
                        string partName = parts[ 0 ];
                        int partValue = int.Parse( parts[ 1 ] );
                        
                        if( partName == "first" )
                        {
                            kerningInfo.first = partValue;
                        }
                        else if( partName == "second" )
                        {
                            kerningInfo.second = partValue;
                        }
                        else if( partName == "amount" )
                        {
                            kerningInfo.amount = ( (float)partValue ) * _configRatio * resourceScaleInverse;
                        }
                    }
                }
                
                if( kerningInfo.first != -1 )
                {
                    _kerningInfos[ k ] = kerningInfo;
                }
                
                k++;
            }
            
        }
        
        _kerningCount = k;
        
        
        if( !wasKerningFound ) //if there are no kernings at all (like in a pixel font), then make an empty kerning array
        {
            _kerningInfos = new KerningInfo[0]; 
        }
        
        //make sure the space character doesn't have offsetY and offsetX
        if( _charInfosByID.ContainsKey( ASCII_SPACE ) )
        {
            _charInfosByID[ ASCII_SPACE ].offsetX = 0;
            _charInfosByID[ ASCII_SPACE ].offsetY = 0;
        }
        
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
        for( int c = 0; c<lettersLength; ++c )
        {
            char letter = letters[ c ];
            
            if( letter == ASCII_NEWLINE )
            {
                lines[ lineCount ] = new LetterQuadLine();
                lines[ lineCount ].letterCount = letterCount;
                lines[ lineCount ].quads = new LetterQuad[letterCount];
                
                lineCount++;
                letterCount = 0;
            }
            else
            {
                letterCount++;  
            }
        }
        
        lines[ lineCount ] = new LetterQuadLine();
        lines[ lineCount ].letterCount = letterCount;
        lines[ lineCount ].quads = new LetterQuad[letterCount];
        
        LetterQuadLine[] oldLines = lines;
        lines = new LetterQuadLine[lineCount + 1];
         
        for( int c = 0; c<lineCount+1; ++c )
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
        
        float usableLineHeight = _lineHeight + labelTextParams.scaledLineHeightOffset + _textParams.scaledLineHeightOffset;

        for( int c = 0; c<lettersLength; ++c )
        {
            char letter = letters[ c ];
            
            if( letter == ASCII_NEWLINE )
            {   
                if( letterCount == 0 )
                {
                    lines[ lineCount ].bounds = new Rect( 0, 0, nextY, nextY - usableLineHeight );
                }
                else
                {
                    lines[ lineCount ].bounds = new Rect( minX, minY, maxX - minX, maxY - minY );
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
                KerningInfo foundKerning = _nullKerning;
                
                for( int k = 0; k<_kerningCount; k++ )
                {
                    KerningInfo kerningInfo = _kerningInfos[ k ];
                    if( kerningInfo.first == previousLetter && kerningInfo.second == letter )
                    {
                        foundKerning = kerningInfo;
                    }
                }
                
                LetterQuad letterQuad = new LetterQuad();

                if( !_charInfosByID.TryGetValue( letter, out charInfo ) )
                {
                    charInfo = _charInfosByID[ 0 ];
                }
                
                float totalKern = foundKerning.amount + labelTextParams.scaledKerningOffset + _textParams.scaledKerningOffset;

                if( letterCount == 0 )
                {
                    nextX = -charInfo.offsetX; //don't offset the first character
                }
                else
                {
                    nextX += totalKern; 
                }
                
                letterQuad.charInfo = charInfo;
                
                Rect quadRect = new Rect( nextX + charInfo.offsetX, nextY - charInfo.offsetY - charInfo.height, charInfo.width, charInfo.height );
            
                letterQuad.rect = quadRect;
                
                lines[ lineCount ].quads[ letterCount ] = letterQuad;   
                
                minX = Math.Min( minX, quadRect.xMin );
                maxX = Math.Max( maxX, quadRect.xMax );
                minY = Math.Min( minY, nextY - usableLineHeight );
                maxY = Math.Max( maxY, nextY );

                nextX += charInfo.xadvance;

                letterCount++;
            }
                        
            previousLetter = letter; 
        }
        
        if( letterCount == 0 ) //there were no letters, so minX and minY would be crazy if we used them
        {
            lines[ lineCount ].bounds = new Rect( 0, 0, nextY, nextY - usableLineHeight );
        }
        else
        {
            lines[ lineCount ].bounds = new Rect( minX, minY, maxX - minX, maxY - minY );
        }

        for( int n = 0; n<lineCount+1; n++ )
        {
            lines[ n ].bounds.height += labelTextParams.scaledLineHeightOffset + _textParams.scaledLineHeightOffset;
        }

        return lines;
    }

    public AtlasElement GetElementForChar( char character )
    {
        CharInfo charInfo;

        if( !_charInfosByID.TryGetValue( character, out charInfo ) )
        {
            return null;
        }

        AtlasElement charElement = new AtlasElement();
        charElement.atlas = _element.atlas;
        charElement.atlasIndex = _element.atlasIndex;
        charElement.indexInAtlas = -1;
        charElement.isTrimmed = true;
        charElement.name = ""; //_name + "_" + character.ToString();

        charElement.uvRect = charInfo.uvRect;
        charElement.sourceRect = new Rect( 0, 0, charInfo.width, charInfo.height );
        charElement.sourceSize = new Vector2( charInfo.width, charInfo.height );
        charElement.sourcePixelSize = new Vector2( charInfo.width * FearsomeMonstrousBeast.resourceScale, charInfo.height * FearsomeMonstrousBeast.resourceScale );

        Rect uvRect = charElement.uvRect;

        charElement.uvTopLeft.Set( uvRect.xMin, uvRect.yMax );
        charElement.uvTopRight.Set( uvRect.xMax, uvRect.yMax );
        charElement.uvBottomRight.Set( uvRect.xMax, uvRect.yMin );
        charElement.uvBottomLeft.Set( uvRect.xMin, uvRect.yMin );

        return charElement;
    }
    
    public string name
    {
        get { return _name;}    
    }
    
    public AtlasElement element
    {
        get { return _element;} 
    }
    
    public TextParams textParams
    {
        get { return _textParams;}  
    }
    
    public float offsetX
    {
        get { return _offsetX;} 
    }
    
    public float offsetY
    {
        get { return _offsetY;} 
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


