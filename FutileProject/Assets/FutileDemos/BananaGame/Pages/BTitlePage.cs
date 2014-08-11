using UnityEngine;
using System.Collections;
using System;

public class BTitlePage : BPage
{
    private FSprite _background;
    private FContainer _logoHolder;
    private FSprite _logo;
    private FButton _startButton;
    private int _frameCount = 0;
    
    public BTitlePage()
    {
        ListenForUpdate( HandleUpdate );
        ListenForResize( HandleResize );
    }
    
    override public void Start()
    {
        _background = new FSprite( "JungleClearBG" );
        AddChild( _background );
        
        //this will scale the background up to fit the screen
        //but it won't let it shrink smaller than 100%
        
        _logoHolder = new FContainer();
        
        AddChild( _logoHolder );

        _logo = new FSprite( "MainLogo" );
        
        _logoHolder.AddChild( _logo );
        
        _startButton = new FButton( "YellowButton_normal", "YellowButton_down", "YellowButton_over", "ClickSound" );
        _startButton.AddLabel( "Franchise", "START", new Color( 0.45f, 0.25f, 0.0f, 1.0f ) );
        
        AddChild( _startButton );

        _startButton.SignalRelease += HandleStartButtonRelease;
        
        _logoHolder.scale = 0.0f;
        
        Go.to( _logoHolder, 0.5f, new TweenConfig().
            setDelay( 0.1f ).
            floatProp( "scale", 1.0f ).
            setEaseType( EaseType.BackOut ) );
        
        
        _startButton.scale = 0.0f;
        
        Go.to( _startButton, 0.5f, new TweenConfig().
            setDelay( 0.3f ).
            floatProp( "scale", 1.0f ).
            setEaseType( EaseType.BackOut ) );
        
        HandleResize( true ); //force resize to position everything at the start
    }
    
    protected void HandleResize( bool wasOrientationChange )
    {
        //this will scale the background up to fit the screen
        //but it won't let it shrink smaller than 100%
        _background.scale = Math.Max( 1.0f, Math.Max( FearsomeMonstrousBeast.screen.height / _background.textureRect.height, FearsomeMonstrousBeast.screen.width / _background.textureRect.width ) );
        
        _logoHolder.x = 0.0f;
        _logoHolder.y = 15.0f;
        
        _startButton.x = FearsomeMonstrousBeast.screen.halfWidth - 75.0f;
        _startButton.y = -FearsomeMonstrousBeast.screen.halfHeight + 35.0f;
        
        //scale the logo so it fits on the main screen 
        _logo.scale = Math.Min( 1.0f, FearsomeMonstrousBeast.screen.width / _logo.textureRect.width );
        
    }

    private void HandleStartButtonRelease( FButton button )
    {
        BMain.instance.GoToPage( BPageType.InGamePage );
    }
    
    protected void HandleUpdate()
    {
        _logo.rotation = -5.0f + RXMath.PingPong( _frameCount, 300 ) * 10.0f;
        
        _frameCount++;
    }

}

