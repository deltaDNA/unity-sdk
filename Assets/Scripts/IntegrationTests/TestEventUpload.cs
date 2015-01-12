using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTest;

namespace DeltaDNA {

class TestEventUpload : MonoBehaviour {

	public void Start() {
	
		SDK.Instance.Settings.DebugMode = true;
		SDK.Instance.Settings.BackgroundEventUpload = false;
		SDK.Instance.Settings.OnFirstRunSendNewPlayerEvent = false;
		SDK.Instance.Settings.OnInitSendClientDeviceEvent = false;
		SDK.Instance.Settings.OnInitSendGameStartedEvent = false;
		
		SDK.Instance.ClearPersistentData();
		
		SDK.Instance.ClientVersion = "1.0";
		SDK.Instance.HashSecret = "1VLjWqChV2YC1sJ4EPKGzSF3TbhS26hq";
		SDK.Instance.StartSDK(
			"76410301326725846610230818914037", 					// Environment Key	(UnitySDK)		
			"collect2470ntysd.deltadna.net/collect/api",			// Collect URI
			"engage2470ntysd.deltadna.net",							// Engage URI
			"TestEventUpload"
		);
			
		StartCoroutine(RunTestCoroutine());
	
	}
	
	private IEnumerator RunTestCoroutine() {
	
		SDK.Instance.RecordEvent("gameStarted");
		SDK.Instance.Upload();
		SDK.Instance.RecordEvent("testUploadEvent1");
		
		while (SDK.Instance.IsUploading) {
			yield return new WaitForSeconds(3);
		}
		
		SDK.Instance.Upload();
		SDK.Instance.RecordEvent("testUploadEvent2");
		
		while (SDK.Instance.IsUploading) {
			yield return new WaitForSeconds(3);
		}
		
		SDK.Instance.Upload();
		SDK.Instance.RecordEvent("testUploadEvent3");
		
		while (SDK.Instance.IsUploading) {
			yield return new WaitForSeconds(3);
		}
		
		SDK.Instance.RecordEvent("gameEnded", new Dictionary<string,object>() {{"userScore", 0}});
		SDK.Instance.Upload();
		
		while (SDK.Instance.IsUploading) {
			yield return new WaitForSeconds(3);
		}
	
		IntegrationTest.Pass(gameObject);
	}
};

}
