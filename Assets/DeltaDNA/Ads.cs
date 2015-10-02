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
	public class Ads {
		
		#region Public interface
		
		[Obsolete("Use DDNAAds instead, this no longer does anything.")]
		public event Action OnDidRegisterForAds;
		[Obsolete("Use DDNAAds instead, this no longer does anything.")]
		public event Action<string> OnDidFailToRegisterForAds;
		[Obsolete("Use DDNAAds instead, this no longer does anything.")]
		public event Action OnAdOpened;
		[Obsolete("Use DDNAAds instead, this no longer does anything.")]
		public event Action OnAdFailedToOpen;
		[Obsolete("Use DDNAAds instead, this no longer does anything.")]
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
		
	}
}