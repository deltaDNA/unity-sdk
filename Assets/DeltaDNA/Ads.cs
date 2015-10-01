using UnityEngine;
using System;
using System.Collections.Generic;
using DeltaDNA;

namespace DeltaDNA
{
	/// <summary>
	/// The Ads class is deprecated and no longer does anything.  This class is provided to enable
	/// existing code to run.  It won't show any ads.  Use DeltaDNAAds instead.
	/// </summary>
	public class Ads : MonoBehaviour {
		
		#region Public interface
		
		public event Action OnDidRegisterForAds;
		public event Action<string> OnDidFailToRegisterForAds;
		public event Action OnAdOpened;
		public event Action OnAdFailedToOpen;
		public event Action OnAdClosed;
	
		[Obsolete("Use DDNAAds instead, this no longer does anything.")]
		public void RegisterForAds()
		{
			
		}
		
		[Obsolete("Use DDNAAds instead, this no longer does anything.")]
		public void ShowAd()
		{

		}
		
		[Obsolete("Use DDNAAds instead, this no longer does anything.")]
		public void ShowAd(string adPoint)
		{

		}
		
		#endregion
		
		void Awake()
		{
			gameObject.name = this.GetType().ToString();
			DontDestroyOnLoad(this);
		}
		
		void Update() 
		{

		}
		
		void OnApplicationPause(bool pauseStatus)
		{

		}
		
		void OnDestroy()
		{

		}
	}
}