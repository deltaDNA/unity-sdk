using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA.Messaging
{
	public class PopupEventArgs: EventArgs
	{
		public PopupEventArgs(GameObject gameObject, ImageAsset imageAsset)
		{
			this.GameObject = gameObject;
			this.ImageAsset = imageAsset;
		}

		public GameObject GameObject { get; set; }
		public ImageAsset ImageAsset { get; set; }
	}

	public interface IPopup
	{
		event EventHandler BeforeLoad;
		event EventHandler AfterLoad;
		event EventHandler BeforeShow;
		event EventHandler BeforeClose;
		event EventHandler AfterClose;
		event EventHandler<PopupEventArgs> Dismiss;
		event EventHandler<PopupEventArgs> Action;

		void LoadResource(ImageComposition image);
		void ShowPopup(Dictionary<string, object> options = null);
	}
}

