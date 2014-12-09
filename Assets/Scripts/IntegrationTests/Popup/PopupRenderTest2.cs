using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTest;

namespace DeltaDNA.Messaging
{
	public class PopupRenderTest2 : MonoBehaviour
	{
		public Popup popup = new Popup();

		void Awake()
		{
			string json = "{ \"transactionID\": 42, \"image\": { \"width\": 1024, \"height\": 2048, \"format\": \"png\", \"spritemap\": { \"background\": { \"x\": 2, \"y\": 76, \"width\": 768, \"height\": 1024 }, \"buttons\": [ { \"x\": 2, \"y\": 2, \"width\": 128, \"height\": 72 } ] }, \"layout\": { \"landscape\": { \"background\": { \"contain\": { \"halign\": \"center\", \"valign\": \"center\", \"left\": \"20px\", \"right\": \"20px\", \"top\": \"20px\", \"bottom\": \"20px\" }, \"action\": { \"type\": \"dismiss\" } }, \"buttons\": [ { \"x\": 310, \"y\": 721, \"action\": { \"type\": \"dismiss\" } } ] } }, \"shim\": { \"mask\": \"dimmed\", \"action\": { \"type\": \"dismiss\" } }, \"url\": \"http://download.deltadna.net/engagements/132513322e774d358e60230fc7aeb273.png\" }, \"parameters\": {} }";
		
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

		void Update()
		{
			if (Time.time > 5) {
				popup.Close();
			}
		}
	}
}

