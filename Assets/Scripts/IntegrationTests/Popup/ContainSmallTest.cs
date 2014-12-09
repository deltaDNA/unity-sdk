using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityTest;

namespace DeltaDNA.Messaging
{
	public class ContainSmallTest : MonoBehaviour
	{
		public Popup popup = new Popup();

		void Awake()
		{
			string spriteMapPath = "file://" + Path.Combine(Application.streamingAssetsPath, "Images/Popup2.png");

			string json = "{ \"transactionID\": 42, \"image\": { \"width\": 512, \"height\": 256, \"format\": \"png\", \"spritemap\": { \"background\": { \"x\": 2, \"y\": 34, \"width\": 275, \"height\": 183 }, \"buttons\": [ { \"x\": 2, \"y\": 2, \"width\": 30, \"height\": 30 }, { \"x\": 2, \"y\": 2, \"width\": 30, \"height\": 30 } ] }, \"layout\": { \"landscape\": { \"background\": { \"contain\": { \"halign\": \"left\", \"valign\": \"top\", \"left\": \"5%\", \"right\": \"20%\", \"top\": \"5%\", \"bottom\": \"20%\" }, \"action\": { \"type\": \"dismiss\" } }, \"buttons\": [ { \"x\": 49, \"y\": 142, \"action\": { \"type\": \"dismiss\" } }, { \"x\": 11, \"y\": 142, \"action\": { \"type\": \"dismiss\" } } ] } }, \"shim\": { \"mask\": \"clear\", \"action\": { \"type\": \"none\" } }, \"url\": \""+spriteMapPath+"\" }, \"parameters\": {} }";

			var response = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;

			if (response != null && response.ContainsKey("image"))
			{
				var image = response["image"] as Dictionary<string, object>;

				popup.AfterPrepare += (sender, e) => {
					((Popup)sender).Show();
				};
				popup.Action += (sender, e) => {
					Debug.Log("Action => "+e.ID+" "+e.ActionType+" "+e.ActionValue);
				};
				popup.Dismiss += (sender, e) => {
					Debug.Log("Dismiss => "+e.ID);
				};
				popup.AfterClose += (sender, e) => {
					IntegrationTest.Pass();
				};
				popup.Prepare(image);
			}
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

		void Update()
		{
			if (Time.time > 5) {
				popup.Close();
			}
		}
	}
}

