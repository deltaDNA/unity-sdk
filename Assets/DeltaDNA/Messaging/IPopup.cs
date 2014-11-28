using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA.Messaging
{
	public class PopupEventArgs: EventArgs
	{
		public PopupEventArgs(string type, string value=null)
		{
			this.ActionType = type;
			this.ActionValue = value;
		}

		public string ActionType { get; set; }
		public string ActionValue { get; set; }
	}
		
	public interface IPopup
	{
		event EventHandler BeforePrepare;
		event EventHandler AfterPrepare;
		event EventHandler BeforeShow;
		event EventHandler BeforeClose;
		event EventHandler AfterClose;
		event EventHandler<PopupEventArgs> Dismiss;
		event EventHandler<PopupEventArgs> Action;

		void Prepare(Dictionary<string, object> configuration);
		void Show();
		void Close();

		void OnDismiss(PopupEventArgs eventArgs);
		void OnAction(PopupEventArgs eventArgs);
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

