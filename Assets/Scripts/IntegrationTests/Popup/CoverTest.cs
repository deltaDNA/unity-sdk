using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityTest;

namespace DeltaDNA.Messaging
{
	public class CoverTest : MonoBehaviour
	{
		public Popup popup = new Popup();

		void Awake()
		{
			string spriteMapPath = "file://" + Path.Combine(Application.streamingAssetsPath, "Images/Popup1.png");

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
							{"cover", new Dictionary<string, object>() {
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

		void Update()
		{
			if (Time.time > 5) {
				popup.Close();
			}
		}
	}


}

