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
				{"width", 1024},
				{"height", 512},
				{"format", "png"},
				{"spritemap", new Dictionary<string, object>() {
					{"background", new Dictionary<string, object>() {
						{"x", 2},
						{"y", 52},
						{"width", 640},
						{"height", 400}
					}},
					{"buttons", new List<object>() {
						new Dictionary<string, object>() {
							{"x", 2},
							{"y", 2},
							{"width", 96},
							{"height", 48}
						},
						new Dictionary<string, object>() {
							{"x", 644},
							{"y", 404},
							{"width", 96},
							{"height", 48}
						}
					}}
				}},
				{"layout", new Dictionary<string, object>() {
					{"landscape", new Dictionary<string, object>() {
						{"cover", new Dictionary<string, object>() {
							{"valign", "center"},
							{"halign", "center"}
						}},
						{"background", new Dictionary<string, object>() {
							{"action", new Dictionary<string, object>() {
								{"type", "NONE"}
							}}
						}},
						{"buttons", new List<object>() {
							new Dictionary<string, object>() {
								{"x", 544},
								{"y", 0},
								{"action", new Dictionary<string, object>() {
									{"type", "ACTION"},
									{"value", "BUY_GOLD"}
								}},
							},
							new Dictionary<string, object>() {
								{"x", 544},
								{"y", 352},
								{"action", new Dictionary<string, object>() {
									{"type", "DISMISS"}
								}}
							}
						}}
					}}
				}},
				{"shim", new Dictionary<string, object>() {
					{"mask", "dimmed"},
					{"action", new Dictionary<string, object>() {
						{"type", "NONE"}
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

