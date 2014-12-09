using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityTest;

namespace DeltaDNA.Messaging
{
	public class PopupRenderTest : MonoBehaviour 
	{
		public Popup popup = new Popup();

		void Awake()
		{
			string spriteMapPath = "file://" + Path.Combine(Application.streamingAssetsPath, "Images/Popup1.png");

			// Need to make int longs to behave as MiniJSON does
			var image = new Dictionary<string, object>() {
				{"url", spriteMapPath},
				{"width", 1024L},
				{"height", 512L},
				{"format", "png"},
				{"spritemap", new Dictionary<string, object>() {
					{"background", new Dictionary<string, object>() {
						{"x", 2L},
						{"y", 52L},
						{"width", 640L},
						{"height", 400L}
					}},
					{"buttons", new List<object>() {
						new Dictionary<string, object>() {
							{"x", 2L},
							{"y", 2L},
							{"width", 96L},
							{"height", 48L}
						},
						new Dictionary<string, object>() {
							{"x", 644L},
							{"y", 404L},
							{"width", 96L},
							{"height", 48L}
						}
					}}
				}},
				{"layout", new Dictionary<string, object>() {
					{"landscape", new Dictionary<string, object>() {
						{"background", new Dictionary<string, object>() {
							{"contain", new Dictionary<string, object>() {
								{"left", "20%"},
								{"right", "20%"},
								{"top", "10px"},
								{"bottom", "10px"},
								{"valign", "center"},
								{"halign", "center"}
							}},
							{"action", new Dictionary<string, object>() {
								{"type", "none"}
							}}
						}},
						{"buttons", new List<object>() {
							new Dictionary<string, object>() {
								{"x", 544L},
								{"y", 0L},
								{"action", new Dictionary<string, object>() {
									{"type", "action"},
									{"value", "BUY_GOLD"}
								}},
							},
							new Dictionary<string, object>() {
								{"x", 544L},
								{"y", 352L},
								{"action", new Dictionary<string, object>() {
									{"type", "dismiss"}
								}}
							}
						}}
					}}
				}},
				{"shim", new Dictionary<string, object>() {
					{"mask", "dimmed"},
					{"action", new Dictionary<string, object>() {
						{"type", "none"}
					}}
				}}
			};


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

