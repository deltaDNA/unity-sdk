using UnityEngine;
using System;
using System.Collections.Generic;
using DeltaDNA;

namespace DeltaDNAAds
{
	public class DDNASmartAds : Singleton<DDNASmartAds> {

		public const string SMARTADS_VERSION = "SmartAds v0.8.1";

		#if UNITY_ANDROID
		private DeltaDNAAds.Android.AdService adService;
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
				DeltaDNA.Logger.LogError("The DeltaDNA SDK must be started before calling RegisterForAds.");
				return;
			}

			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				adService = new DeltaDNAAds.Android.AdService(new DeltaDNAAds.Android.AdServiceListener(this));
				adService.RegisterForAds();
				#endif
			}
			else {
				DeltaDNA.Logger.LogWarning("SmartAds is not currently supported on "+Application.platform);
			}
		}

		public void ShowAd()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.ShowAd();
				} else {
					DeltaDNA.Logger.LogError("RegisterForAds must be called before calling ShowAd.");
				}
				#endif
			}
			else {
				this.AdFailedToOpen();
			}
		}

		public void ShowAd(string adPoint)
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.ShowAd(adPoint);
				} else {
					DeltaDNA.Logger.LogError("RegisterForAds must be called before calling ShowAd.");
				}
				#endif
			}
			else {
				this.AdFailedToOpen();
			}
		}

		#endregion

		#region Native Bridge

		// Methods will be called from the Android UI thread, so must pass them back to UnityMain thread
		internal void DidRegisterForAds()
		{
			actions.Enqueue(() => {
				DeltaDNA.Logger.LogDebug("Did register for ads");
				if (OnDidRegisterForAds != null) {
					OnDidRegisterForAds();
				}
			});
		}

		internal void DidFailToRegisterForAds(string reason)
		{
			actions.Enqueue(() => {
				DeltaDNA.Logger.LogDebug("Did fail to register for ads: "+reason);
				if (OnDidFailToRegisterForAds != null) {
					OnDidFailToRegisterForAds(reason);
				}
			});
		}

		internal void AdOpened()
		{
			actions.Enqueue(() => {
				DeltaDNA.Logger.LogDebug("Did open an ad");
				if (OnAdOpened != null) {
					OnAdOpened();
				}
			});
		}

		internal void AdFailedToOpen()
		{
			actions.Enqueue(() => {
				DeltaDNA.Logger.LogDebug("Did fail to open an ad");
				if (OnAdFailedToOpen != null) {
					OnAdFailedToOpen();
				}
			});
		}

		internal void AdClosed()
		{
			actions.Enqueue(() => {
				DeltaDNA.Logger.LogDebug("Did close an ad");
				if (OnAdClosed != null) {
					OnAdClosed();
				}
			});
		}

		internal void RecordEvent(string eventName, Dictionary<string,object> eventParams)
		{
			actions.Enqueue(() => {
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

		public override void OnDestroy()
		{
			if (Application.platform == RuntimePlatform.Android) {
				#if UNITY_ANDROID
				if (adService != null) {
					adService.OnDestroy();
				}
				#endif
			}

			base.OnDestroy();
		}
	}
}
