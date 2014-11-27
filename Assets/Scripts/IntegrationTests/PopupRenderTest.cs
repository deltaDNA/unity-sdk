using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityTest;

namespace DeltaDNA.Messaging
{
	public class PopupRenderTest : MonoBehaviour 
	{
		void Awake()
		{
			string spriteMapPath = "file://" + Path.Combine(Application.streamingAssetsPath, "Images/Popup1.png");

			var resource = new Dictionary<string, object>() {
				{"url", spriteMapPath},
				{"width", 1024},
				{"height", 512},
				{"format", "png"},
				{"layout", new Dictionary<string, object>() {
					{"landscape", new Dictionary<string, object>() {
						{"constrain", new Dictionary<string, object>() {
							{"l", "20%"},
							{"r", "20%"},
							{"t", "10px"},
							{"b", "10px"},
							{"v", "top"},
							{"h", "left"}
						}},
						{"buttons", new List<object>() {
							new Dictionary<string, object>() {
								{"x", 544},
								{"y", 0}
							},
							new Dictionary<string, object>() {
								{"x", 544},
								{"y", 352}
							}
						}}
					}}
				}}
			};

			Popup2 popup = new Popup2();
			popup.AfterLoad += (sender, e) => {
				((Popup2)sender).ShowPopup();
			};
			popup.LoadResource(resource);
		}

		void OnGUI () 
		{
			GUI.depth = 5;

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
	}
}

