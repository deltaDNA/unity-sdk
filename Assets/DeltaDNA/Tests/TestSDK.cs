using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DeltaDNA;

public class TestSDK : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
		SDK.Instance.Settings.DebugMode = true;
		SDK.Instance.Settings.ResetTest = true;
		
		SDK.Instance.Settings.BackgroundEventUpload = true;
		
		//SDK.Instance.Settings.OnInitSendNewPlayerEvent = false;
	
		SDK.Instance.Init(
			"55822530117170763508653519413932", 				// iOS Test Dev				
			"http://collect2010stst.deltadna.net/collect/api",
			"KmMBBcNwStLJaq6KsEBxXc6HY3A4bhGw",
			"David - Test",
			"http://engage2010stst.deltadna.net"
		);
	
		
		// Send some more complicated events
		EventParams achievementParams = new EventParams()
			.AddParam("achievementName", "Sunday Showdown Tournament Win")
			.AddParam("achievementID", "SS-2014-03-02-01")
			.AddParam("reward", new EventParams()
				.AddParam("rewardProducts", new ProductParams()
					.AddRealCurrency("USD", 5000)
					.AddVirtualCurrency("VIP Points", "GRIND_CURRENCY", 20)
					.AddItem("Sunday Showdown Medal", "Victory Badge", 1))
				.AddParam("rewardName", "Medal"));
		
		SDK.Instance.TriggerEvent("achievement", achievementParams);	
		
		EventParams transactionParams = new EventParams()
			.AddParam("transactionName", "Weapon type 11 manual repair")
			.AddParam("transactionID", "47891208312996456524019-178.149.115.237:51787")
			.AddParam("transactorID", "62.212.91.84:15116")
			.AddParam("productID", "4019")
			.AddParam("transactionType", "PURCHASE")
			.AddParam("paymentCountry", "EN")
			.AddParam("productsReceived", new ProductParams()
				.AddItem("WeaponMaxConditionRepair:11", "WeaponMaxConditionRepair", 5))
			.AddParam("productsSpent", new ProductParams()
				.AddVirtualCurrency("Credit", "Grind", 710));
				
		SDK.Instance.TriggerEvent("transaction", transactionParams);
		
		// Play with Engage
		var engageParams = new Dictionary<string, object>()
		{
			{ "userLevel", 4 },
			{ "experience", 1000 },
			{ "missionName", "Disco Volante" }
		};
		
		SDK.Instance.RequestEngagement("gameLoaded", engageParams, (response) =>
		{
			string data = MiniJSON.Json.Serialize(response);
			Debug.Log("Engage returned '"+data+"'");
		});
		
		// Some time later...
		//SDK.Instance.Upload();
		
		//InvokeRepeating("TriggerEvent", 0, 5);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	private void TriggerEvent()
	{
		Debug.Log("Trigger Event...");
		SDK.Instance.TriggerEvent("emptySchema");
	}
	
}
