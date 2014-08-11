using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FNode
{
    protected float _x = 0f;
    protected float _y = 0f;
    protected float _scaleX = 1f;
    protected float _scaleY = 1f;
    protected float _rotation = 0f;
    protected float _meshZ = 0f;
    protected float _sortZ = 0f; //sortZ is used for depth sorting but ONLY if the node container's shouldSortByZ = true;
    
    protected bool _isMatrixDirty = false;
    protected FContainer _container = null;
    protected Matrix _matrix;
    protected Matrix _concatenatedMatrix;
    protected Matrix _inverseConcatenatedMatrix = null;
    protected Matrix _screenConcatenatedMatrix = null;
    protected Matrix _screenInverseConcatenatedMatrix = null;
    protected bool _needsSpecialMatrices = false;
    protected float _alpha = 1f;
    protected float _concatenatedAlpha = 1f;
    protected bool _isAlphaDirty = false;
    protected float _visibleScale = 1f;
    protected bool _isOnStage = false;
    protected int _depth = 0;
    protected FStage _stage = null; //assigned in HandleAddedToStage
    
    protected bool _isVisible = true;
    public object data = null; //the user can put whatever data they want here
    
    private List<NodeEnabler> _enablers = null;
    
    public FNode()
    {
        _matrix = new Matrix();
        _concatenatedMatrix = new Matrix();

        #if UNITY_EDITOR
        if(FutileEngine.instance.shouldTrackNodesInRXProfiler) 
            RXProfiler.TrackLifeCycle(this);
        #endif 
    }
    
    public void AddEnabler( NodeEnabler enabler )
    {
        if( _enablers == null )
        {
            _enablers = new List<NodeEnabler>();
        }
        
        _enablers.Add( enabler );
        
        if( _isOnStage )
        {
            enabler.Connect();  
        }
    }
    
    public void RemoveEnabler( NodeEnabler enabler )
    {
        if( _enablers == null )
        {
            return;
        }
        
        if( _isOnStage )
        {
            enabler.Disconnect();   
        }
        
        _enablers.Remove( enabler );
    }
    
    public void RemoveEnablerOfType( Type enablerType )
    {
        if( _enablers == null )
        {
            return;
        }
        
        for( int e = _enablers.Count-1; e>= 0; e-- ) //reverse order for easy removal
        {
            if( _enablers[ e ].GetType() == enablerType )
            {
                if( _isOnStage )
                {
                    _enablers[ e ].Disconnect();
                }
                
                _enablers.RemoveAt( e );
            }
        }
    }
    
    public void ListenForResize( FScreen.ScreenResizeDelegate handleResizeCallback )
    {
        RemoveEnablerOfType( typeof( NodeEnablerForResize ) );
        AddEnabler( new NodeEnablerForResize( handleResizeCallback ) );
    }
    
    public void RemoveListenForResize()
    {   
        RemoveEnablerOfType( typeof( NodeEnablerForResize ) );
    }
    
    public void ListenForOrientationChange( FScreen.ScreenOrientationChangeDelegate handleOrientationChangeCallback )
    {
        RemoveEnablerOfType( typeof( NodeEnablerForOrientationChange ) );
        AddEnabler( new NodeEnablerForOrientationChange( handleOrientationChangeCallback ) );
    }
    
    public void RemoveListenForOrientationChange()
    {   
        RemoveEnablerOfType( typeof( NodeEnablerForOrientationChange ) );
    }

    public void ListenForPreUpdate( FutileEngine.FutileUpdateDelegate handleUpdateCallback )
    {
        RemoveEnablerOfType( typeof( NodeEnablerForPreUpdate ) );
        AddEnabler( new NodeEnablerForPreUpdate( handleUpdateCallback ) );
    }
    
    public void RemoveListenForPreUpdate()
    {   
        RemoveEnablerOfType( typeof( NodeEnablerForPreUpdate ) );
    }
    
    public void ListenForUpdate( FutileEngine.FutileUpdateDelegate handleUpdateCallback )
    {
        RemoveEnablerOfType( typeof( NodeEnablerForUpdate ) );
        AddEnabler( new NodeEnablerForUpdate( handleUpdateCallback ) );
    }
    
    public void RemoveListenForUpdate()
    {   
        RemoveEnablerOfType( typeof( NodeEnablerForUpdate ) );
    }

    public void ListenForAfterUpdate( FutileEngine.FutileUpdateDelegate handleUpdateCallback )
    {
        RemoveEnablerOfType( typeof( NodeEnablerForAfterUpdate ) );
        AddEnabler( new NodeEnablerForAfterUpdate( handleUpdateCallback ) );
    }
    
    public void RemoveListenForAfterUpdate()
    {   
        RemoveEnablerOfType( typeof( NodeEnablerForAfterUpdate ) );
    }
    
    public void ListenForLateUpdate( FutileEngine.FutileUpdateDelegate handleUpdateCallback )
    {
        RemoveEnablerOfType( typeof( NodeEnablerForLateUpdate ) );
        AddEnabler( new NodeEnablerForLateUpdate( handleUpdateCallback ) );
    }
    
    public void RemoveListenForLateUpdate()
    {   
        RemoveEnablerOfType( typeof( NodeEnablerForLateUpdate ) );
    }
    
    public void ListenForFixedUpdate( FutileEngine.FutileUpdateDelegate handleUpdateCallback )
    {
        RemoveEnablerOfType( typeof( NodeEnablerForFixedUpdate ) );
        AddEnabler( new NodeEnablerForFixedUpdate( handleUpdateCallback ) );
    }
    
    public void RemoveListenForFixedUpdate()
    {   
        RemoveEnablerOfType( typeof( NodeEnablerForFixedUpdate ) );
    }
    
    public void EnableSingleTouch()
    {
        DisableSingleTouch(); //clear any old ones first
        AddEnabler( new NodeEnablerForSingleTouch( this ) );
    }
    
    public void DisableSingleTouch()
    {
        RemoveEnablerOfType( typeof( NodeEnablerForSingleTouch ) );
    }
    
    public void EnableMultiTouch()
    {
        DisableMultiTouch(); //clear any old ones first
        AddEnabler( new NodeEnablerForMultiTouch( this ) );
    }
    
    public void DisableMultiTouch()
    {
        RemoveEnablerOfType( typeof( NodeEnablerForMultiTouch ) );
    }

    public void EnableSmartTouch()
    {
        DisableSmartTouch(); //clear any old ones first
        AddEnabler( new NodeEnablerForSmartTouch( this ) );
    }
    
    public void DisableSmartTouch()
    {
        RemoveEnablerOfType( typeof( NodeEnablerForSmartTouch ) );
    }

    public void ListenForAddedOrRemoved( NodeEnablerForAddedOrRemoved.Delegate handleAddedOrRemoved )
    {
        RemoveEnablerOfType( typeof( NodeEnablerForAddedOrRemoved ) );
        AddEnabler( new NodeEnablerForAddedOrRemoved( handleAddedOrRemoved ) );
    }

    public void RemoveListenForAddedOrRemoved()
    {
        RemoveEnablerOfType( typeof( NodeEnablerForAddedOrRemoved ) );
    }
    
    virtual public void HandleAddedToStage()
    {
        _isOnStage = true;
        
        if( _enablers != null )
        {
            int count = _enablers.Count;
            for( int e = 0; e< count; e++ )
            {
                _enablers[ e ].Connect();
            }
        }
    }

    virtual public void HandleRemovedFromStage()
    {
        _isOnStage = false;
        
        if( _enablers != null )
        {
            int count = _enablers.Count;
            for( int e = 0; e< count; e++ )
            {
                _enablers[ e ].Disconnect();
            }
        }
    }
    
    public Vector2 LocalToScreen( Vector2 localVector ) //for sending local points back to screen coords
    {
        if( _container != null )
        {
            _container.UpdateMatrix();
        }
        _isMatrixDirty = true;
        UpdateMatrix();
        
        //the offsets account for the camera's 0,0 point (eg, center, bottom left, etc.)
        float offsetX = -FutileEngine.screen.originX * FutileEngine.screen.pixelWidth;
        float offsetY = -FutileEngine.screen.originY * FutileEngine.screen.pixelHeight;
        
        localVector = this.screenConcatenatedMatrix.GetNewTransformedVector( localVector );
        
        return new Vector2
        (
                ( localVector.x / FutileEngine.displayScaleInverse ) - offsetX, 
                ( localVector.y / FutileEngine.displayScaleInverse ) - offsetY
        );
    }
    
    public Vector2 ScreenToLocal( Vector2 screenVector ) //for transforming mouse or touch points to local coords
    {
        if( _container != null )
        {
            _container.UpdateMatrix();
        }
        _isMatrixDirty = true;
        UpdateMatrix();
        
        //the offsets account for the camera's 0,0 point (eg, center, bottom left, etc.)
        float offsetX = -FutileEngine.screen.originX * FutileEngine.screen.pixelWidth;
        float offsetY = -FutileEngine.screen.originY * FutileEngine.screen.pixelHeight;
        
        screenVector = new Vector2
        (
                ( screenVector.x + offsetX ) * FutileEngine.displayScaleInverse, 
                ( screenVector.y + offsetY ) * FutileEngine.displayScaleInverse
        );
        
        return this.screenInverseConcatenatedMatrix.GetNewTransformedVector( screenVector );
    }
    
    public Vector2 LocalToStage( Vector2 localVector )
    {
        if( _container != null )
        {
            _container.UpdateMatrix();
        }
        _isMatrixDirty = true;
        UpdateMatrix();
        return _concatenatedMatrix.GetNewTransformedVector( localVector );
    }
    
    public Vector2 StageToLocal( Vector2 globalVector )
    {
        if( _container != null )
        {
            _container.UpdateMatrix();
        }
        _isMatrixDirty = true;
        UpdateMatrix();
        //using "this" so the getter is called (because it checks if the matrix exists and lazy inits it if it doesn't)
        return this.inverseConcatenatedMatrix.GetNewTransformedVector( globalVector );
    }
    
    public Vector2 LocalToGlobal( Vector2 localVector )
    {
        if( _container != null )
        {
            _container.UpdateMatrix();
        }
        _isMatrixDirty = true;
        UpdateMatrix();
        //using "this" so the getter is called (because it checks if the matrix exists and lazy inits it if it doesn't)
        return this.screenConcatenatedMatrix.GetNewTransformedVector( localVector );
    }
    
    public Vector2 GlobalToLocal( Vector2 globalVector )
    {
        if( _container != null )
        {
            _container.UpdateMatrix();
        }
        _isMatrixDirty = true;
        UpdateMatrix();
        //using "this" so the getter is called (because it checks if the matrix exists and lazy inits it if it doesn't)
        return this.screenInverseConcatenatedMatrix.GetNewTransformedVector( globalVector );
    }
    
    public Vector2 OtherToLocal( FNode otherNode, Vector2 otherVector ) //takes a point in another node and converts it to a point in this node
    {
        return GlobalToLocal( otherNode.LocalToGlobal( otherVector ) );
    }
    
    public Vector2 LocalToOther( Vector2 localVector, FNode otherNode ) //takes a point in this node and converts it to a point in another node
    {
        return otherNode.GlobalToLocal( LocalToGlobal( localVector ) );
    }
    
    public Vector2 GetLocalMousePosition()
    {
        return ScreenToLocal( Input.mousePosition );    
    }
    
    public Vector2 GetLocalTouchPosition( FTouch touch )
    {
        return GlobalToLocal( touch.position ); 
    }
    
    public void UpdateMatrix()
    {
        if( !_isMatrixDirty )
        {
            return;
        }
        
        //do NOT set _isMatrixDirty to false here because it is used in the redraw loop and will be set false then

        _matrix.SetScaleThenRotate( _x, _y, _scaleX * _visibleScale, _scaleY * _visibleScale, _rotation * -0.01745329f ); //0.01745329 is RXMath.DTOR
            
        if( _container != null )
        {
            _concatenatedMatrix.ConcatAndCopyValues( _matrix, _container.concatenatedMatrix );
        }
        else
        {
            _concatenatedMatrix.CopyValues( _matrix );  
        }
        
        if( _needsSpecialMatrices )
        {
            _inverseConcatenatedMatrix.InvertAndCopyValues( _concatenatedMatrix );

            if( _isOnStage )
            {
                _screenConcatenatedMatrix.ConcatAndCopyValues( _concatenatedMatrix, _stage.screenConcatenatedMatrix );
            }
            else
            {
                _screenConcatenatedMatrix.CopyValues( _concatenatedMatrix ); //if it's not on the stage, just use its normal matrix
            }

            _screenInverseConcatenatedMatrix.InvertAndCopyValues( _screenConcatenatedMatrix );
        }
    }
    
    virtual protected void UpdateDepthMatrixAlpha( bool shouldForceDirty, bool shouldUpdateDepth )
    {
        if( shouldUpdateDepth )
        {
            _depth = _stage.nextNodeDepth++;    
        }
        
        if( _isMatrixDirty || shouldForceDirty )
        {
            _isMatrixDirty = false;
            
            _matrix.SetScaleThenRotate( _x, _y, _scaleX * _visibleScale, _scaleY * _visibleScale, _rotation * -0.01745329f ); //0.01745329 is RXMath.DTOR
            
            if( _container != null )
            {
                _concatenatedMatrix.ConcatAndCopyValues( _matrix, _container.concatenatedMatrix );
            }
            else
            {
                _concatenatedMatrix.CopyValues( _matrix );  
            }
        }
        
        if( _needsSpecialMatrices )
        {
            _inverseConcatenatedMatrix.InvertAndCopyValues( _concatenatedMatrix );
            _screenConcatenatedMatrix.ConcatAndCopyValues( _concatenatedMatrix, _stage.screenConcatenatedMatrix );
            _screenInverseConcatenatedMatrix.InvertAndCopyValues( _screenConcatenatedMatrix );
        }
        
        if( _isAlphaDirty || shouldForceDirty )
        {
            _isAlphaDirty = false;
            
            if( _container != null )
            {
                _concatenatedAlpha = _container.concatenatedAlpha * _alpha;
            }
            else
            {
                _concatenatedAlpha = _alpha;
            }
        }   
    }
    
    virtual public void Redraw( bool shouldForceDirty, bool shouldUpdateDepth )
    {   
        UpdateDepthMatrixAlpha( shouldForceDirty, shouldUpdateDepth );
    }
    
    virtual public void HandleAddedToContainer( FContainer container )
    {
        if( _container != container )
        {
            if( _container != null ) //remove from the old container first if there is one
            {
                _container.RemoveChild( this ); 
            }
            
            _container = container;
        }
    }
    
    virtual public void HandleRemovedFromContainer()
    {
        _container = null;  
    }
    
    public void RemoveFromContainer()
    {
        if( _container != null )
        {
            _container.RemoveChild( this );
        }
    }
    
    public void MoveToFront()
    {
        if( _container != null )
        {
            _container.AddChild( this );
        }
    }

    public void MoveToBack()
    {
        if( _container != null )
        {
            _container.AddChildAtIndex( this, 0 );
        }   
    }

    public void MoveInFrontOfOtherNode( FNode otherNode )
    {
        if( _container == null )
        {
            return;
        } //we have no container
        if( otherNode.container != _container )
        {
            return;
        } //we don't share a container
        _container.AddChildAtIndex( this, _container.GetChildIndex( otherNode ) + 1 );
    }

    public void MoveBehindOtherNode( FNode otherNode )
    {
        if( _container == null )
        {
            return;
        } //we have no container
        if( otherNode.container != _container )
        {
            return;
        } //we don't share a container
        _container.AddChildAtIndex( this, _container.GetChildIndex( otherNode ) );
    }

    public bool isVisible
    {
        get { return _isVisible;}
        set
        { 
            if( _isVisible != value )
            {
                _isVisible = value;
                _visibleScale = _isVisible ? 1f : 0f;
                _isMatrixDirty = true;
            }
        }
    }

    //check if the ancestry of this node *including this node* is visible. 
    public bool IsAncestryVisible()
    {
        if( _isVisible )
        {
            if( container != null )
            {
                return container.IsAncestryVisible();
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }
    
    public float x
    {
        get { return _x;}
        set { _x = value;
            _isMatrixDirty = true;}
    }
    
    public float y
    {
        get { return _y;}
        set { _y = value;
            _isMatrixDirty = true;}
    }

    public float meshZ
    {
        get { return _meshZ;}
        set { _meshZ = value;
            _isMatrixDirty = true;}
    }
    
    virtual public float sortZ
    {
        get { return _sortZ;}
        set { _sortZ = value;} 
    }
    
    public float scaleX
    {
        get { return _scaleX;}
        set { _scaleX = value;
            _isMatrixDirty = true;}
    }
    
    public float scaleY
    {
        get { return _scaleY;}
        set { _scaleY = value;
            _isMatrixDirty = true;}
    }
    
    public float scale
    {
        get { return scaleX;} //return scaleX because if we're using this, x and y should be the same anyway
        set { _scaleX = value;
            scaleY = value;
            _isMatrixDirty = true;}
    }
    
    public float rotation
    {
        get { return _rotation;}
        set { _rotation = value;
            _isMatrixDirty = true;}
    }
    
    public bool isMatrixDirty
    {
        get { return _isMatrixDirty;}   
    }
    
    public FContainer container
    {
        get { return _container;}   
    }
    
    public int depth
    {
        get { return _depth;}   
    }
    
    virtual public int touchPriority
    {
        get { return _depth;}   
    }
    
    virtual public Matrix matrix
    {
        get { return _matrix; }
    }
    
    virtual public Matrix concatenatedMatrix
    {
        get { return _concatenatedMatrix; }
    }
    
    protected void CreateSpecialMatrices()
    {
        _needsSpecialMatrices = true; //after now the matrices will be updated on redraw     
        
        _inverseConcatenatedMatrix = new Matrix();
        _screenConcatenatedMatrix = new Matrix();
        _screenInverseConcatenatedMatrix = new Matrix();

        if( _isOnStage )
        {
            _inverseConcatenatedMatrix.InvertAndCopyValues( _concatenatedMatrix );
            _screenConcatenatedMatrix.ConcatAndCopyValues( _concatenatedMatrix, _stage.screenConcatenatedMatrix );
            _screenInverseConcatenatedMatrix.InvertAndCopyValues( _screenConcatenatedMatrix );
        }
        else
        {
            Debug.LogWarning( "Futile: Warning! You're probably trying to use GlobalToLocal/LocalToLocal with an object that isn't currently part of the display list" );
        }
    }
    
    virtual public Matrix inverseConcatenatedMatrix
    {
        get
        { 
            if( !_needsSpecialMatrices )
            {
                CreateSpecialMatrices();
            } //only it create if needed
            
            return _inverseConcatenatedMatrix; 
        }
    }
    
    virtual public Matrix screenConcatenatedMatrix
    {
        get
        { 
            if( !_needsSpecialMatrices )
            {
                CreateSpecialMatrices();
            } //only it create if needed
            
            return _screenConcatenatedMatrix; 
        }
    }
    
    virtual public Matrix screenInverseConcatenatedMatrix
    {
        get
        { 
            if( !_needsSpecialMatrices )
            {
                CreateSpecialMatrices();
            } //only it create if needed

            return _screenInverseConcatenatedMatrix; 
        }
    }
    
    public float alpha
    {
        get { return _alpha; }
        set
        { 
            float newAlpha = Math.Max( 0.0f, Math.Min( 1.0f, value ) );
            if( _alpha != newAlpha )
            {
                _alpha = newAlpha; 
                _isAlphaDirty = true;
            }
        }
    }
    
    public float concatenatedAlpha
    {
        get { return _concatenatedAlpha; }
    }
    
    public FStage stage
    {
        get { return _stage;}
        set { _stage = value;}
    }

    public Vector2 GetPositionRelativeToAncestor( FContainer ancestor )
    {
        FNode target = this;

        Vector2 position = new Vector2( 0, 0 );
        
        FContainer container;
        
        while( true )
        {
            position += target.GetPosition();
            container = target.container;
            if( container == null )
            {
                break;
            }
            if( container == ancestor )
            {
                break;
            }
            target = container;
        }
        
        return position;
    }
    
    //use node.LocalToLocal to use a point from a different coordinate space
    public void RotateAroundPointRelative( Vector2 localPoint, float relativeDegrees )
    {
        Matrix tempMatrix = Matrix.tempMatrix;
        
        tempMatrix.ResetToIdentity();
        tempMatrix.SetScaleThenRotate( 0, 0, _scaleX, _scaleY, _rotation * -RXMath.DTOR );
        Vector2 firstVector = tempMatrix.GetNewTransformedVector( new Vector2( -localPoint.x, -localPoint.y ) );
        
        _rotation += relativeDegrees;
        
        tempMatrix.ResetToIdentity();
        tempMatrix.SetScaleThenRotate( 0, 0, _scaleX, _scaleY, _rotation * -RXMath.DTOR );
        Vector2 secondVector = tempMatrix.GetNewTransformedVector( new Vector2( -localPoint.x, -localPoint.y ) );
        
        _x += secondVector.x - firstVector.x;
        _y += secondVector.y - firstVector.y;
        
        _isMatrixDirty = true;
    }
    
    //use node.LocalToLocal to use a point from a different coordinate space
    public void RotateAroundPointAbsolute( Vector2 localPoint, float absoluteDegrees )
    {
        RotateAroundPointRelative( localPoint, absoluteDegrees - _rotation );
    }
    
    //use node.LocalToLocal to use a point from a different coordinate space
    public void ScaleAroundPointRelative( Vector2 localPoint, float relativeScaleX, float relativeScaleY )
    {
        Matrix tempMatrix = Matrix.tempMatrix;
        
        tempMatrix.ResetToIdentity();
        tempMatrix.SetScaleThenRotate( 0, 0, ( relativeScaleX - 1.0f ), ( relativeScaleY - 1.0f ), _rotation * -RXMath.DTOR );
        Vector2 moveVector = tempMatrix.GetNewTransformedVector( new Vector2( localPoint.x * _scaleX, localPoint.y * _scaleY ) );   

        _x += -moveVector.x;
        _y += -moveVector.y;

        _scaleX *= relativeScaleX;
        _scaleY *= relativeScaleY;
        
        _isMatrixDirty = true;
    }
    
    //use node.LocalToLocal to use a point from a different coordinate space
    public void ScaleAroundPointAbsolute( Vector2 localPoint, float absoluteScaleX, float absoluteScaleY )
    {
        ScaleAroundPointRelative( localPoint, absoluteScaleX / _scaleX, absoluteScaleX / _scaleY );
    }
    
    //for convenience
    public void SetPosition( float newX, float newY )
    {
        this.x = newX;
        this.y = newY;
    }
    
    public void SetPosition( Vector2 newPosition )
    {
        this.x = newPosition.x;
        this.y = newPosition.y;
    }
    
    public Vector2 GetPosition()
    {
        return new Vector2( _x, _y );   
    }

    public bool isOnStage
    {
        get { return _isOnStage;}
    }
}
