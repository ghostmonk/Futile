using UnityEngine;
using System;

public class FFacetNode : FNode
{
	protected Atlas _atlas = null;
	protected FShader _shader = null;
	
	protected int _firstFacetIndex = -1;
	protected int _numberOfFacetsNeeded;
	
	protected FacetRenderLayer _renderLayer;
	
	protected FacetType _facetType;
	
	private bool _hasInited = false;
	
	public FFacetNode ()
	{
		
	}
	
	virtual protected void Init(FacetType facetType, Atlas atlas, int numberOfFacetsNeeded)
	{
		_facetType = facetType;
		
		_atlas = atlas;
		if(_shader == null) _shader = FShader.defaultShader;
		_numberOfFacetsNeeded = numberOfFacetsNeeded; 
		
		_hasInited = true;
	}
	
	protected void UpdateFacets()
	{
		if(!_hasInited) return;
		
		_stage.renderer.GetFacetRenderLayer(out _renderLayer, out _firstFacetIndex, _facetType, _atlas, _shader, _numberOfFacetsNeeded);
	}
	
	virtual public int firstFacetIndex
	{
		get {return _firstFacetIndex;}	
	}
	
	virtual public void PopulateRenderLayer()
	{
		//override in parent, this is when you set the quads of the render layer
	}
	
	override public void HandleAddedToStage()
	{
		if(!_isOnStage)
		{
			base.HandleAddedToStage();
			_stage.HandleFacetsChanged();
		}
	}
	
	override public void HandleRemovedFromStage()
	{
		if(_isOnStage)
		{
			base.HandleRemovedFromStage();
			_stage.HandleFacetsChanged();
		}
	}
	
	public FShader shader
	{
		get { return _shader;}
		set
		{
			if(_shader != value)
			{
				_shader = value;
				if(_isOnStage) _stage.HandleFacetsChanged();	
			}
		}
	}
	
	public FacetType facetType
	{
		get {return _facetType;}	
	}
}

//FFacetNode handles only a single element
public class FFacetElementNode : FFacetNode
{
	protected AtlasElement _element;
	
	protected void Init(FacetType facetType, AtlasElement element, int numberOfFacetsNeeded)
	{
		_element = element;
		
		base.Init(facetType,_element.atlas,numberOfFacetsNeeded);
		
		HandleElementChanged();
	}
	
	public void SetElementByName(string elementName)
	{
        this.element = FutileEngine.atlasManager.GetElementWithName(elementName);
	}
	
	public AtlasElement element
	{
		get { return _element;}
		set
		{
			if(_element != value)
			{
				bool isAtlasDifferent = (_element.atlas != value.atlas);
	
				_element = value;	
				
				if(isAtlasDifferent)
				{
					_atlas = _element.atlas;
					if(_isOnStage) _stage.HandleFacetsChanged();	
				}
				
				HandleElementChanged();
			}
		}
	}
	
	virtual public void HandleElementChanged()
	{
		//override by parent things
	}
}


