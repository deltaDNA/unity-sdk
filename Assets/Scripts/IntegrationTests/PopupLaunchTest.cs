using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityTest;

namespace DeltaDNA.Messaging
{
	// Mimicks the Engage Response handler, creates an Image Composition
	// that can be drawn for the Popup.
	public class PopupLaunchTest : MonoBehaviour {

		// Use this for initialization
		void Awake () {
			
			// Build a Composition and Launch the popup
			// Test assets stored in StreamingAssetsPath allows WWW to be used as in real code.
			string spriteMapPath = "file://" + Path.Combine(Application.streamingAssetsPath, "Images/Popup1.png");

			ImageComposition c = ImageComposition.BuildFromDictionary(new Dictionary<string, object>(){
				{"url", spriteMapPath},
				{"width", 1024},
				{"height", 512},
				{"format", "PNG"},
				{"viewport", new Dictionary<string, object>() {
					{"width", 960},
					{"height", 480}
				}},
				{"background", new Dictionary<string, object>() {
					{"x", 2},
					{"y", 52},
					{"imgX", 160},
					{"imgY", 40},
					{"width", 640},
					{"height", 400},
					{"actionType", "NONE"}
				}},
				{"button1", new Dictionary<string, object>() {
					{"x", 2},
					{"y", 2},
					{"imgX", 244},
					{"imgY", 310},
					{"width", 96},
					{"height", 48},
					{"label", "Purchase"},
					{"name", "purchase"},
					{"actionType", "CUSTOM"},
					{"actionParam", "DO_SOMETHING"}
				}},
				{"button2", new Dictionary<string, object>() {
					{"x", 644},
					{"y", 404},
					{"imgX", 600},
					{"imgY", 310},
					{"width", 96},
					{"height", 48},
					{"label", "Ignore"},
					{"name", "cancel"},
					{"actionType", "DISMISS"}
				}}
			});
				
			IPopup popup = new Popup();
			popup.Action += (sender, e) => {
				if (e.GameObject == popup.Button1)
				{
					PassTest();
				}
			};
			popup.AfterLoad += (sender, e) => {
				((Popup)sender).ShowPopup();
			};
			popup.LoadResource(c);

		}		

		void Update()
		{
			// After awhile simulate a button press...
			if (Time.time > 5) {
				GameObject btn1 = GameObject.Find("Button1");
				if (btn1 != null) {
					btn1.gameObject.GetComponent<PopupActionHandler>().OnMouseDown();
				}
			}
		}

		void OnGUI () 
		{
	        // Make a background box
	        GUI.Box(new Rect(150,110,100,90), "Loader Menu");
	    
	        // Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
	        if(GUI.Button(new Rect(160,140,80,20), "Level 1")) {
	            Debug.Log("Level 1 Selected");
	        }
	    
	        // Make the second button.
	        if(GUI.Button(new Rect(160,170,80,20), "Level 2")) {
	            Debug.Log("Level 2 Selected");
	        }
	    }

		void PassTest()
		{
			IntegrationTest.Pass();
		}
	}
}