using System;
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

		public PopupEventArgs(string type, string value=null)
		{
			this.ActionType = type;
			this.ActionValue = value;
		}

		public GameObject GameObject { get; set; }
		public ImageAsset ImageAsset { get; set; }

		public string ActionType { get; set; }
		public string ActionValue { get; set; }
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
		void ShowPopup();
		void ClosePopup();

		void OnDismiss(PopupEventArgs eventArgs);
		void OnAction(PopupEventArgs eventArgs);

		GameObject Background { get; }
		GameObject Button1 { get; }
		GameObject Button2 { get; }
	}

	public class PopupException : Exception
	{
		public PopupException()
		{}
		
		public PopupException(string message)
			: base(message)
		{}
		
		public PopupException(string message, Exception inner)
			: base(message, inner)
		{}
	}
}

