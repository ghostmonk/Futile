using System;
using UnityEngine;

public class NodeEnabler
{
	public NodeEnabler ()
	{
	}
	
	virtual public void Connect()
	{
		
	}
	
	virtual public void Disconnect()
	{
		
	}
}

public class NodeEnablerForPreUpdate : NodeEnabler
{
    public FutileEngine.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForPreUpdate(FutileEngine.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FutileEngine.instance.SignalPreUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FutileEngine.instance.SignalPreUpdate -= handleUpdateCallback;
	}
}


public class NodeEnablerForUpdate : NodeEnabler
{
    public FutileEngine.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForUpdate(FutileEngine.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FutileEngine.instance.SignalUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FutileEngine.instance.SignalUpdate -= handleUpdateCallback;
	}
}

public class NodeEnablerForAfterUpdate : NodeEnabler
{
    public FutileEngine.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForAfterUpdate(FutileEngine.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FutileEngine.instance.SignalAfterUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FutileEngine.instance.SignalAfterUpdate -= handleUpdateCallback;
	}
}


public class NodeEnablerForLateUpdate : NodeEnabler
{
    public FutileEngine.FutileUpdateDelegate handleUpdateCallback;
    public NodeEnablerForLateUpdate(FutileEngine.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FutileEngine.instance.SignalLateUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FutileEngine.instance.SignalLateUpdate -= handleUpdateCallback;
	}
}

public class NodeEnablerForAfterDraw : NodeEnabler
{
    public FutileEngine.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForAfterDraw(FutileEngine.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FutileEngine.instance.SignalAfterDraw += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FutileEngine.instance.SignalAfterDraw -= handleUpdateCallback;
	}
}
public class NodeEnablerForFixedUpdate : NodeEnabler
{
    public FutileEngine.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForFixedUpdate(FutileEngine.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FutileEngine.instance.SignalFixedUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FutileEngine.instance.SignalFixedUpdate -= handleUpdateCallback;
	}
}

public class NodeEnablerForSingleTouch : NodeEnabler
{
	public FSingleTouchableInterface singleTouchable;
	
	public NodeEnablerForSingleTouch(FNode node)
	{
		singleTouchable = node as FSingleTouchableInterface;
		if(singleTouchable == null)
		{
			throw new FutileException("Trying to enable single touch on a node that doesn't implement FSingleTouchableInterface");	
		}
	}
	
	override public void Connect()
	{
        FutileEngine.touchManager.AddSingleTouchTarget(singleTouchable);	
	}
	
	override public void Disconnect()
	{
        FutileEngine.touchManager.RemoveSingleTouchTarget(singleTouchable);	
	}
}

public class NodeEnablerForMultiTouch : NodeEnabler
{
	public FMultiTouchableInterface multiTouchable;
	
	public NodeEnablerForMultiTouch(FNode node)
	{
		multiTouchable = node as FMultiTouchableInterface;
		
		if(multiTouchable == null)
		{
			throw new FutileException("Trying to enable multi touch on a node that doesn't implement FMultiTouchableInterface");	
		}
	}
	
	override public void Connect()
	{
		FutileEngine.touchManager.AddMultiTouchTarget(multiTouchable);	
	}
	
	override public void Disconnect()
	{
        FutileEngine.touchManager.RemoveMultiTouchTarget(multiTouchable);	
	}
}

public class NodeEnablerForSmartTouch : NodeEnabler
{
	public FSmartTouchableInterface smartTouchable;
	
	public NodeEnablerForSmartTouch(FNode node)
	{
		smartTouchable = node as FSmartTouchableInterface;
		if(smartTouchable == null)
		{
			throw new FutileException("Trying to enable single touch on a node that doesn't implement FSmartTouchableInterface");	
		}
	}
	
	override public void Connect()
	{
        FutileEngine.touchManager.AddSmartTouchTarget(smartTouchable);	
	}
	
	override public void Disconnect()
	{
        FutileEngine.touchManager.RemoveSmartTouchTarget(smartTouchable);	
	}
}
public class NodeEnablerForResize : NodeEnabler
{
	public FScreen.ScreenResizeDelegate handleResizeCallback;
	
	public NodeEnablerForResize(FScreen.ScreenResizeDelegate handleResizeCallback)
	{
		this.handleResizeCallback = handleResizeCallback;	
	}
	
	override public void Connect()
	{
        FutileEngine.screen.SignalResize += handleResizeCallback;
	}
	
	override public void Disconnect()
	{
        FutileEngine.screen.SignalResize -= handleResizeCallback;
	}
}

public class NodeEnablerForOrientationChange : NodeEnabler
{
	public FScreen.ScreenOrientationChangeDelegate handleOrientationChangeCallback;
	
	public NodeEnablerForOrientationChange(FScreen.ScreenOrientationChangeDelegate handleOrientationChangeCallback)
	{
		this.handleOrientationChangeCallback = handleOrientationChangeCallback;	
	}
	
	override public void Connect()
	{
        FutileEngine.screen.SignalOrientationChange += handleOrientationChangeCallback;
	}
	
	override public void Disconnect()
	{
        FutileEngine.screen.SignalOrientationChange -= handleOrientationChangeCallback;
	}
}

public class NodeEnablerForAddedOrRemoved : NodeEnabler
{
	public delegate void Delegate(bool wasAdded);

	public Delegate handleAddedOrRemovedCallback;

	public NodeEnablerForAddedOrRemoved(Delegate handleAddedOrRemovedCallback)
	{
		this.handleAddedOrRemovedCallback = handleAddedOrRemovedCallback;	
	}

	override public void Connect()
	{
		handleAddedOrRemovedCallback.Invoke(true);
	}

	override public void Disconnect()
	{
		handleAddedOrRemovedCallback.Invoke(false);
	}
}



