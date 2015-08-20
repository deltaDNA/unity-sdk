using UnityEngine;
using System;
using System.Collections.Generic;

namespace DeltaDNA
{
	public class Ads : MonoBehaviour {
	
		#if UNITY_ANDROID
		private DeltaDNA.Android.AdService adService;	
		#endif
		
		private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
		
		#region Public interface
		
		public event Action OnDidRegisterForAds;
		public event Action<string> OnDidFailToRegisterForAds;
		public event Action OnAdOpened;
		public event Action OnAdFailedToOpen;
		public event Action OnAdClosed;
	
		public void RegisterForAds()
		{
			if (!DDNA.Instance.IsInitialised) {
				Logger.LogError("You must first start the SDK.");
				return;
			}
		
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				adService = new DeltaDNA.Android.AdService(new DeltaDNA.Android.AdListener(this));	
				adService.RegisterForAds();	
				#endif
			}
		}
		
		public void ShowAd()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.ShowAd();
				} else {
					Logger.LogError("You must first register for ads.");
				}
				#else 
				this.AdFailedToOpen();
				#endif
			}
		}
		
		public void ShowAd(string adPoint)
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.ShowAd(adPoint);
				} else {
					Logger.LogError("You must first register for ads.");
				}
				#else
				this.AdFailedToOpen();
				#endif
			}
		}
		
		#endregion
		
		#region Native Bridge
		
		// Methods will be called from the Android UI thread, so must pass them back to UnityMain thread
		internal void DidRegisterForAds()
		{	
			actions.Enqueue(() => { 
				Logger.LogDebug("Did register for ads");
				if (OnDidRegisterForAds != null) {
					OnDidRegisterForAds(); 
				}
			});
		}
		
		internal void DidFailToRegisterForAds(string reason)
		{
			actions.Enqueue(() => { 
				Logger.LogDebug("Did fail to register for ads: "+reason);
				if (OnDidFailToRegisterForAds != null) {
					OnDidFailToRegisterForAds(reason); 
				}	
			});
		}
		
		internal void AdOpened()
		{
			actions.Enqueue(() => {
				Logger.LogDebug("Did open an ad");
				if (OnAdOpened != null) {
					OnAdOpened();
				}
			});
		}
		
		internal void AdFailedToOpen()
		{	
			actions.Enqueue(() => {
				Logger.LogDebug("Did fail to open an ad");
				if (OnAdFailedToOpen != null) {
					OnAdFailedToOpen();
				}
			});
		}
		
		internal void AdClosed()
		{
			actions.Enqueue(() => {
				Logger.LogDebug("Did close an ad");
				if (OnAdClosed != null) {
					OnAdClosed();
				}
			});
		}
		
		internal void RecordEvent(string eventName, Dictionary<string,object> eventParams)
		{					
			actions.Enqueue(() => {
				Logger.LogDebug("Recording Android event "+eventName);
				DDNA.Instance.RecordEvent(eventName, eventParams);
			});
		}
		
		#endregion
		
		void Awake()
		{
			gameObject.name = this.GetType().ToString();
			DontDestroyOnLoad(this);
		}
		
		void Update() 
		{
			// Action tasks from Android thread
			while (actions.Count > 0) {
				Logger.LogDebug("Processing Android thread action");
				Action action = actions.Dequeue();
				action();
			}
		}
		
		void OnApplicationPause(bool pauseStatus)
		{
			if (Application.platform == RuntimePlatform.Android) {
				
				#if UNITY_ANDROID
				if (adService != null) {
					if (pauseStatus) {
						adService.OnPause();
					} else {
						adService.OnResume();
					}
				}
				#endif 	
			}
		}
		
		void OnDestroy()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.OnDestroy();
				}			
				#endif
			}
		}
	}
}