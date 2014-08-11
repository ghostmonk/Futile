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
    public FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForPreUpdate(FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FearsomeMonstrousBeast.instance.SignalPreUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.instance.SignalPreUpdate -= handleUpdateCallback;
	}
}


public class NodeEnablerForUpdate : NodeEnabler
{
    public FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForUpdate(FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FearsomeMonstrousBeast.instance.SignalUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.instance.SignalUpdate -= handleUpdateCallback;
	}
}

public class NodeEnablerForAfterUpdate : NodeEnabler
{
    public FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForAfterUpdate(FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FearsomeMonstrousBeast.instance.SignalAfterUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.instance.SignalAfterUpdate -= handleUpdateCallback;
	}
}


public class NodeEnablerForLateUpdate : NodeEnabler
{
    public FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback;
    public NodeEnablerForLateUpdate(FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FearsomeMonstrousBeast.instance.SignalLateUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.instance.SignalLateUpdate -= handleUpdateCallback;
	}
}

public class NodeEnablerForAfterDraw : NodeEnabler
{
    public FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForAfterDraw(FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FearsomeMonstrousBeast.instance.SignalAfterDraw += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.instance.SignalAfterDraw -= handleUpdateCallback;
	}
}
public class NodeEnablerForFixedUpdate : NodeEnabler
{
    public FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback;
	
    public NodeEnablerForFixedUpdate(FearsomeMonstrousBeast.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
        FearsomeMonstrousBeast.instance.SignalFixedUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.instance.SignalFixedUpdate -= handleUpdateCallback;
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
        FearsomeMonstrousBeast.touchManager.AddSingleTouchTarget(singleTouchable);	
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.touchManager.RemoveSingleTouchTarget(singleTouchable);	
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
		FearsomeMonstrousBeast.touchManager.AddMultiTouchTarget(multiTouchable);	
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.touchManager.RemoveMultiTouchTarget(multiTouchable);	
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
        FearsomeMonstrousBeast.touchManager.AddSmartTouchTarget(smartTouchable);	
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.touchManager.RemoveSmartTouchTarget(smartTouchable);	
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
        FearsomeMonstrousBeast.screen.SignalResize += handleResizeCallback;
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.screen.SignalResize -= handleResizeCallback;
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
        FearsomeMonstrousBeast.screen.SignalOrientationChange += handleOrientationChangeCallback;
	}
	
	override public void Disconnect()
	{
        FearsomeMonstrousBeast.screen.SignalOrientationChange -= handleOrientationChangeCallback;
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



