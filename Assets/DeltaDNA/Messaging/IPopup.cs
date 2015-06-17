using System;
using System.Collections.Generic;

namespace DeltaDNA.Messaging
{
	public class PopupEventArgs: EventArgs
	{
		public PopupEventArgs(string id, string type, string value)
		{
			this.ID = id;
			this.ActionType = type;
			this.ActionValue = value;
		}

		public string ID { get; set; }
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
}

