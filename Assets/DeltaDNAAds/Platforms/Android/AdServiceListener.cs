﻿#if UNITY_ANDROID

using System.Collections.Generic;
using UnityEngine;
using DeltaDNA;
using DeltaDNA.MiniJSON;

namespace DeltaDNAAds.Android
{
	
	internal class AdServiceListener : AndroidJavaProxy {
	
		private DDNAAds ads;
		
		internal AdServiceListener(DDNAAds ads)
			: base(Utils.AdServiceListenerClassName) 
		{
			this.ads = ads;
		}
		
		void onRegisteredForAds() {
			ads.DidRegisterForAds();
		}
		
		void onFailedToRegisterForAds(string reason) {
			ads.DidFailToRegisterForAds(reason);
		}
		
		void onAdOpened() {
			ads.AdOpened();
		}
		
		void onAdFailedToOpen() {
			ads.AdFailedToOpen();
		}
		
		void onAdClosed() {
			ads.AdClosed();
		}
		
		string toString() {
			return "AdListener";
		}
		
		void onRecordEvent(string eventName, string eventParamsJson) {
			var eventParams = Json.Deserialize(eventParamsJson) as Dictionary<string,object>;
			ads.RecordEvent(eventName, eventParams);
		}
		
	}
	
}

#endif