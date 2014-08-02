// -------------------------------------------------------------------------------------------------
//  SRFButtonMain.cs
//  Created by Jesse Ozog (code@smashriot.com) on 2014/08/02
//  Copyright 2014 SmashRiot, LLC. All rights reserved.
// -------------------------------------------------------------------------------------------------
using UnityEngine;
using System;

public class SRFButtonMain : MonoBehaviour { 
	
	private FStage uiStage;	
	private FButton uiButton;
	private FButton mainButton;

	// ------------------------------------------------------------------------
	// ------------------------------------------------------------------------
	public void Start(){

		// init futile
		FutileParams fparms = new FutileParams(true,true,true,true);
		// AddResolutionLevel (float maxLength, float displayScale, float resourceScale, string resourceSuffix)
		fparms.AddResolutionLevel(1280.0f, 2.0f, 1.0f, "");
		fparms.origin = new Vector2(0.5f, 0.5f);
		Futile.instance.Init(fparms);
		Futile.instance.camera.depth = 1;
		//Futile.instance.camera.clearFlags = CameraClearFlags.Depth; //CameraClearFlags.Nothing;
		//Futile.instance.camera.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);

		// load image atlases
		Futile.atlasManager.LoadImage("Images/uibutton");
		Futile.atlasManager.LoadImage("Images/mainbutton");

		// add gameobject root for all gameobjects, and set the point to meter size
		FPWorld.Create(16.0f);

		// ui overlay --- after game is setup
		setupUserInterface();
	}

	// ------------------------------------------------------------------------
	// ------------------------------------------------------------------------
	private void setupUserInterface(){

		// stage
		FStage uiStage = new FStage("uiStage");
		uiStage.x = -10000;
		uiStage.y = -10000;
		Futile.AddStage(uiStage);

		// camera
		Camera uiCamera = Futile.instance.CreateNewCamera("UI Cam");
		uiCamera.orthographicSize = 90; // tight
		uiCamera.transform.position = new Vector3(uiStage.x, uiStage.y, -10.0f); 	
		uiCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		uiCamera.clearFlags = CameraClearFlags.Depth; 
		uiCamera.depth = 2; // above main camera

		// add ui button
		uiButton = new FButton("Images/uibutton", "Images/uibutton", "Images/uibutton", "select");
		uiButton.scale = 0.5f;
		uiButton.x = -32;
		uiButton.SetColors(Color.white, Color.green, Color.red);
		uiButton.SignalRelease += HandleUIButtonRelease;
		uiButton.offsetHitRect(19896, 20002); // holy shit we have some more magic numbers up in here
		uiStage.AddChild(uiButton);

		// add button to main camera
		mainButton = new FButton("Images/mainbutton", "Images/mainbutton", "Images/mainbutton", "select");
		mainButton.x = 32;
		mainButton.shouldSortByZ = true;
		mainButton.SetColors(Color.white, Color.green, Color.red);
		mainButton.SignalRelease += HandleMainButtonRelease;
		Futile.stage.AddChild(mainButton);
	}

	// ------------------------------------------------------------------------
	// button handlers
	// ------------------------------------------------------------------------
	private void HandleUIButtonRelease(FButton button){
		Debug.Log("HandleUIButtonRelease called");
	}
	private void HandleMainButtonRelease(FButton button){
		Debug.Log("HandleMainButtonRelease called");
	}
}