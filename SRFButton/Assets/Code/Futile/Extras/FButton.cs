using UnityEngine;
using System.Collections;
using System;

public class FButton : FContainer, FSingleTouchableInterface
{
	protected Rect _hitRect;
	protected bool _shouldUseCustomHitRect = false;
	
	protected FAtlasElement _upElement;
	protected FAtlasElement _downElement;
	protected FAtlasElement _overElement;

	protected bool _shouldUseCustomColors = false;
	protected Color _upColor = Color.white;
	protected Color _downColor = Color.white;
	protected Color _overColor = Color.white;
	
	protected FSprite _sprite;
	protected string _clickSoundName;
	protected FLabel _label;
	
	public delegate void ButtonSignalDelegate(FButton button);
	
	public event ButtonSignalDelegate SignalPress;
	public event ButtonSignalDelegate SignalRelease;
	public event ButtonSignalDelegate SignalReleaseOutside;

	private float _anchorX = 0.5f;
	private float _anchorY = 0.5f;
	
	public float expansionAmount = 10;
	
	protected bool _isEnabled = true;
	protected bool _supportsOver = false;
	protected bool _isTouchDown = false;

	public bool isTouchOver = false;
	
	public FButton (string upElementName, string downElementName, string overElementName, string clickSoundName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		
		if(overElementName != null)
		{
			_overElement = Futile.atlasManager.GetElementWithName(overElementName);
			_supportsOver = true;
		}
		
		_sprite = new FSprite(_upElement.name);
		_sprite.anchorX = _anchorX;
		_sprite.anchorY = _anchorY;
		AddChild(_sprite);
		
		_hitRect = _sprite.textureRect;

		_clickSoundName = clickSoundName;
		
		EnableSingleTouch();
		
		if(_supportsOver)
		{
			ListenForUpdate(HandleUpdate);
		}
	}
	
	// Simpler constructors
	public FButton (string upElementName) : 
		this(upElementName, upElementName, null, null) {}
	
	public FButton (string upElementName, string downElementName) : 
		this(upElementName, downElementName, null, null) {}
	
	public FButton (string upElementName, string downElementName, string clickSoundName) : 
		this(upElementName, downElementName, null, clickSoundName) {}
	
	virtual public void SetElements(string upElementName, string downElementName, string overElementName)
	{
		_upElement = Futile.atlasManager.GetElementWithName(upElementName);
		_downElement = Futile.atlasManager.GetElementWithName(downElementName);
		
		if(overElementName != null)
		{
			_overElement = Futile.atlasManager.GetElementWithName(overElementName);
			_supportsOver = true;
		}
		
		if(_isTouchDown)
		{
			_sprite.element = _downElement;	
		}
		else 
		{
			_sprite.element = _upElement;
		}
	}

	virtual public void SetColors(Color upColor, Color downColor)
	{
		SetColors(upColor, downColor, Color.white);
	}

	virtual public void SetColors(Color upColor, Color downColor, Color overColor)
	{
		_shouldUseCustomColors = true;

		_upColor = upColor;
		_downColor = downColor;
		_overColor = overColor;

		if(_isTouchDown)
		{
			_sprite.color = _downColor;	
		}
		else 
		{
			_sprite.color = _upColor;
		}
	}

	virtual public FLabel AddLabel (string fontName, string text, Color color)
	{
		return this.AddLabel(fontName,text,new FTextParams(),color);
	}
	
	virtual public FLabel AddLabel (string fontName, string text, FTextParams textParams, Color color)
	{
		if(_label != null) 
		{
			RemoveChild(_label);
		}

		_label = new FLabel(fontName, text, textParams);
		AddChild(_label);
		_label.color = color;
		_label.anchorX = _label.anchorY = 0.5f;
		_label.x = -_anchorX*_sprite.width+_sprite.width/2;
		_label.y = -_anchorY*_sprite.height+_sprite.height/2;
		
		return _label;
	}

	virtual protected void HandleUpdate()
	{
		UpdateOverState();
	}
	
	virtual protected void UpdateOverState()
	{
		if(_isTouchDown) return; //if the touch is down then we don't have to worry about over states
		
		Vector2 mousePos = GetLocalMousePosition();
		
		if(_hitRect.Contains(mousePos))
		{
			_sprite.element = _overElement;
			if (_shouldUseCustomColors)
			{
				_sprite.color = _overColor;
			}
		}
		else 
		{
			_sprite.element = _upElement;
			if (_shouldUseCustomColors)
			{
				_sprite.color = _upColor;
			}
		}
	}

	virtual protected void UpdateEnabled()
	{

	}
	
	virtual public bool HandleSingleTouchBegan(FTouch touch)
	{
		_isTouchDown = false;
		
		if(!IsAncestryVisible()) return false;
		
		if(!_shouldUseCustomHitRect)
		{
			_hitRect = _sprite.textureRect.CloneAndScale(_sprite.scaleX,_sprite.scaleY);
		}
		
		Vector2 touchPos = GetLocalTouchPosition(touch);
		Debug.Log("TOUCH " + touch.position + " AT: " + touchPos + " HIT RECT:" + _hitRect);
		
		if(_hitRect.Contains(touchPos))
		{

			//Debug.Log("TOUCH AT : " + touchPos + " HIT********");

			if(_isEnabled) //swallow touches all the time, but only listen to them when enabled
			{
				_sprite.element = _downElement;
				if (_shouldUseCustomColors)
				{
					_sprite.color = _downColor;
				}
				
				if(_clickSoundName != null) FSoundManager.PlaySound(_clickSoundName);
				
				if(SignalPress != null) SignalPress(this);
				
				isTouchOver = true;
				_isTouchDown = true;
			}
			
			return true;	
		}
		
		return false;
	}
	
	virtual public void HandleSingleTouchMoved(FTouch touch)
	{
        Vector2 touchPos = GetLocalTouchPosition(touch);
		
		//expand the hitrect so that it has more error room around the edges
		//this is what Apple does on iOS and it makes for better usability
		Rect expandedRect = _hitRect.CloneWithExpansion(expansionAmount);
		
		if(expandedRect.Contains(touchPos))
		{
			_sprite.element = _downElement;	
			if (_shouldUseCustomColors)
			{
				_sprite.color = _downColor;
			}
			isTouchOver = true;
			_isTouchDown = true;
		}
		else
		{
			_sprite.element = _upElement;	
			if (_shouldUseCustomColors)
			{
				_sprite.color = _upColor;
			}
			isTouchOver = false;
			_isTouchDown = false;
		}
	}
	
	virtual public void HandleSingleTouchEnded(FTouch touch)
	{
		_isTouchDown = false;
		isTouchOver = false;
		
		_sprite.element = _upElement;
		if (_shouldUseCustomColors)
		{
			_sprite.color = _upColor;
		}
		
        Vector2 touchPos = GetLocalTouchPosition(touch);
		
		//expand the hitrect so that it has more error room around the edges
		//this is what Apple does on iOS and it makes for better usability
		Rect expandedRect = _hitRect.CloneWithExpansion(expansionAmount);
		
		if(expandedRect.Contains(touchPos))
		{
			if(SignalRelease != null) SignalRelease(this);
			
			if(_supportsOver && _hitRect.Contains(touchPos)) //go back to the over image if we're over the button
			{
				_sprite.element = _overElement;	
				if (_shouldUseCustomColors)
				{
					_sprite.color = _overColor;
				}
			}
		}
		else
		{
			if(SignalReleaseOutside != null) SignalReleaseOutside(this);	
		}
	}
	
	virtual public void HandleSingleTouchCanceled(FTouch touch)
	{
		_isTouchDown = false;
		isTouchOver = false;
		
		_sprite.element = _upElement;
		if (_shouldUseCustomColors)
		{
			_sprite.color = _upColor;
		}
		if(SignalReleaseOutside != null) SignalReleaseOutside(this);
	}
	
	public FLabel label
	{
		get {return _label;}
	}
	
	public bool isEnabled
	{
		get {return _isEnabled;}
		set 
		{
			if(_isEnabled != value)
			{
				_isEnabled = value;
				UpdateEnabled();
			}
		}
	}
	
	public FSprite sprite
	{
		get { return _sprite;}
	}

	public float anchorX
	{
		set
		{
			_anchorX = value;
			_sprite.anchorX = _anchorX;
			if (_label != null) _label.x = -_anchorX*_sprite.width+_sprite.width/2;
		}
		get {return _anchorX;}
	}

	public float anchorY
	{
		set
		{
			_anchorY = value;
			_sprite.anchorY = _anchorY;
			if (_label != null) _label.y = -_anchorY*_sprite.height+_sprite.height/2;
		}
		get {return _anchorY;}
	}
	
	
	//you can set a custom hitRect to be used instead of the upElement's rect
	//but it's important to remember that the hitRect is in local coordinates
	public Rect hitRect
	{
		get {return _hitRect;}
		set 
		{
			_hitRect = value; 
			_shouldUseCustomHitRect = true;
		}
	}

	// ADDED for buttons on a different camera
	// TOUCH (74.5, -116.0) AT: (40084.9, -40104.0) HIT RECT:(x:39981.00, y:-40178.00, width:144.00, height:144.00)
	// the touch is screen relative, the ui camera is 20k away, mul by futile.display scale and take into account position 
	// of the sprite in that camera for the given ui camera orto size.  orthosize/360 was good for y, but x didn't follow.
	// so fuck me. hard coding offset using the click debug info for each button like a PRRROOOoooo!
	public void offsetHitRect(float offsetX, float offsetY){
		//Debug.Log("SPRITE POS: " + this.x + "," + this.y + " SPRITE SCALE: " + this.scaleX);
		_hitRect.x = offsetX; //this.x - (offsetX * Futile.displayScale);
		_hitRect.y = offsetY; //((this.y * Futile.displayScale)/0.70f) - (offsetY * Futile.displayScale);
		_hitRect.width = _sprite.width * Futile.displayScale;
		_hitRect.height = _sprite.height * Futile.displayScale;
		_shouldUseCustomHitRect = true;
		//Debug.Log("HIT RECT: " + _hitRect);
	}
	
	//for convenience
	public void SetAnchor(float newX, float newY)
	{
		this.anchorX = newX;
		this.anchorY = newY;
	}
	
	public void SetAnchor(Vector2 newAnchor)
	{
		this.anchorX = newAnchor.x;
		this.anchorY = newAnchor.y;
	}
	
	public Vector2 GetAnchor()
	{
		return new Vector2(_anchorX,_anchorY);	
	}
	
	
}


