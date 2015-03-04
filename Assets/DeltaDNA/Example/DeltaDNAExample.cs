using UnityEngine;
using System.Collections.Generic;
using DeltaDNA;
using DeltaDNA.Messaging;

public class DeltaDNAExample : MonoBehaviour {

	public const string ENVIRONMENT_KEY = "76410301326725846610230818914037";
	public const string COLLECT_URL = "http://collect2470ntysd.deltadna.net/collect/api";
	public const string ENGAGE_URL = "http://engage2470ntysd.deltadna.net";

	private string popupContent = "";
	private string popupTitle = "DeltaDNA Example";

	// Use this for initialization
	void Start () {

		DDNA.Instance.SetLoggingLevel(Logger.Level.DEBUG);
		DDNA.Instance.HashSecret = "1VLjWqChV2YC1sJ4EPKGzSF3TbhS26hq";
		DDNA.Instance.ClientVersion = "1.0.0";

		// TODO: Get the push notification token.
		NotificationServices.RegisterForRemoteNotificationTypes(
			RemoteNotificationType.Alert |
			RemoteNotificationType.Badge |
			RemoteNotificationType.Sound);

		// Try it with our own plugin...
		IOSPluginManager.RegisterForPushNotifications();

		DDNA.Instance.StartSDK(ENVIRONMENT_KEY, COLLECT_URL, ENGAGE_URL, DDNA.AUTO_GENERATED_USER_ID);

	}

	// Update is called once per frame
	void Update () {
		// Pickup up the Apple Push Notification Token
		// Putting this code here means the push notification won't be available
		// on the first play of the game, since the gameStarted event will have
		// already been sent.
		if (DDNA.Instance.PushNotificationToken == null) {
			byte[] token = NotificationServices.deviceToken;
			//Debug.Log("Push Token: "+token);
			if (token != null) {
				string tokenStr = System.BitConverter.ToString(token).Replace("-", "").ToLower();
				Debug.Log("Push Token: "+tokenStr);
				DDNA.Instance.PushNotificationToken = tokenStr;
			}
		}
	}

	void FixedUpdate() {
		// Make our cube rotate
		transform.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
	}

	void OnGUI() {

		int x = 10;
		int y = 10;
		int width = 150;
		int height = 50;
		int space = height + 5;

		GUI.skin.textField.wordWrap = true;

		if (GUI.Button(new Rect(x, y, width, height), "Simple Event")) {

			EventBuilder eventParams = new EventBuilder();
			eventParams.AddParam("option", "sword");
			eventParams.AddParam("action", "sell");

			DDNA.Instance.RecordEvent("options", eventParams);
		}

		if (GUI.Button(new Rect(x, y += space, width, height), "Achievement Event")) {

			EventBuilder achievementParams = new EventBuilder()
				.AddParam("achievementName", "Sunday Showdown Tournament Win")
				.AddParam("achievementID", "SS-2014-03-02-01")
				.AddParam("reward", new EventBuilder()
					.AddParam("rewardProducts", new ProductBuilder()
					 	.AddRealCurrency("USD", 5000)
					    .AddVirtualCurrency("VIP Points", "GRIND", 20)
					    .AddItem("Sunday Showdown Medal", "Victory Badge", 1))
					    .AddParam("rewardName", "Medal"));

			DDNA.Instance.RecordEvent("achievement", achievementParams);
		}

		if (GUI.Button(new Rect(x, y += space, width, height), "Transaction Event")) {

			EventBuilder transactionParams = new EventBuilder()
				.AddParam("transactionName", "Weapon type 11 manual repair")
				.AddParam("transactionID", "47891208312996456524019-178.149.115.237:51787")
				.AddParam("transactorID", "62.212.91.84:15116")
				.AddParam("productID", "4019")
				.AddParam("transactionType", "PURCHASE")
				.AddParam("paymentCountry", "GB")
				.AddParam("productsReceived", new ProductBuilder()
					.AddItem("WeaponMaxConditionRepair:11", "WeaponMaxConditionRepair", 5))
					.AddParam("productsSpent", new ProductBuilder()
						.AddVirtualCurrency("Credit", "GRIND", 710));

			DDNA.Instance.RecordEvent("transaction", transactionParams);

		}

		if (GUI.Button(new Rect(x, y += space, width, height), "Engagement")) {

			var engageParams = new Dictionary<string, object>()
			{
				{ "userLevel", 4 },
				{ "experience", 1000 },
				{ "missionName", "Disco Volante" }
			};

			SDK.Instance.RequestEngagement("gameLoaded", engageParams, (response) =>
			{
				popupContent = DeltaDNA.MiniJSON.Json.Serialize(response);
			});

			popupTitle = "Engage returned";
		}

		if (GUI.Button(new Rect(x, y += space, width, height), "Popup Image")) {

			var engageParams = new Dictionary<string, object>() {
				{ "userScore", 42 },
				{ "secondsPlayed", 20 }
			};

			// Create Popup Object
			IPopup imagePopup = new Popup();
			// Setup Events
			imagePopup.AfterPrepare += (sender, e) => {
				Debug.Log("Popup loaded resource");
				// Just show it, although you could do this later
				imagePopup.Show();
			};

			imagePopup.Dismiss += (sender, e) => {
				Debug.Log("Popup dismissed by "+e.ID);
			};

			imagePopup.Action += (sender, e) => {
				Debug.Log("Popup actioned by "+e.ID+" with command "+e.ActionValue);
			};
			// Start Request
			DDNA.Instance.RequestImageMessage("pickUp", engageParams, imagePopup);

		}

		if (GUI.Button(new Rect(x, y += space, width, height), "Notification Opened")) {
			var payload = new Dictionary<string, object>();
			payload.Add("_ddId", 1);
			payload.Add("_ddName", "Example Notification");
			payload.Add("_ddLaunch", true);
			DDNA.Instance.RecordPushNotification(payload);
		}

		if (GUI.Button(new Rect(x, y += space, width, height), "Upload Events")) {
			DDNA.Instance.Upload();
		}

		if (GUI.Button(new Rect(x, y += space, width, height), "Start SDK")) {
			DDNA.Instance.StartSDK(ENVIRONMENT_KEY, COLLECT_URL, ENGAGE_URL, DDNA.AUTO_GENERATED_USER_ID);
		}

		if (GUI.Button(new Rect(x, y += space, width, height), "Stop SDK")) {
			DDNA.Instance.StopSDK();
		}

		if (GUI.Button(new Rect(x, y += space, width, height), "New Session")) {
			DDNA.Instance.NewSession();
		}

		if (popupContent != "") {
			GUI.ModalWindow(0, new Rect(Screen.width/2-150, Screen.height/2-100, 300, 200), RenderPopupContent, popupTitle);
		}
	}

	void RenderPopupContent(int windowID) {
		if (GUI.Button(new Rect(248, 3, 50, 20), "Close")) {
			popupContent = "";
		}
		GUI.TextField(new Rect(0, 25, 300, 175), popupContent);
	}
}
