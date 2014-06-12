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
		var achievementParams = new Dictionary<string, object>()
		{
			{ "achievementName", "Sunday Showdown Tournament Win" },
			{ "achievementID", "SS-2014-03-02-01" },
			{ "reward", new Dictionary<string, object>()
				{
					{ "rewardProducts", new Dictionary<string, object>()
						{
							{ "realCurrency", new Dictionary<string, object>()
								{
									{ "realCurrencyType", "USD" },
									{ "realCurrencyAmount", 5000 }	
								}
							},
							{ "virtualCurrencies", new List<object>()
								{
									new Dictionary<string, object>()
									{
										{ "virtualCurrency", new Dictionary<string, object>()
											{
												{ "virtualCurrencyName", "VIP Points" },
												{ "virtualCurrencyType", "GRIND_CURRENCY" },
												{ "virtualCurrencyAmount", 10 }	
											}
										}
									}
								}
							},
							{ "items", new List<object>()
								{
									new Dictionary<string, object>()
									{
										{ "item", new Dictionary<string, object>()
											{
												{ "itemName", "Sunday Showdown Medal" },
												{ "itemType", "Victory Badge" },
												{ "itemAmount", 1 }	
											}
										}
									}
								}
							}	
						}
					}
				}
			}
		};
		
		//SDK.Instance.TriggerEvent("achievement", achievementParams);
		
		EventParams achievementParams2 = new EventParams()
			.AddParam("achievementName", "Sunday Showdown Tournament Win")
			.AddParam("achievementID", "SS-2014-03-02-01")
			.AddParam("reward", new ProductParams("rewardProducts")
				.AddRealCurrency("USD", 5000)
				.AddVirtualCurrency("VIP Points", "GRIND_CURRENCY", 20)
				.AddItem("Sunday Showdown Medal", "Victory Badge", 1));
		
		SDK.Instance.TriggerEvent("achievement", achievementParams2);	
		
		
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
