using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DeltaDNA.Messaging
{
	public class BaseMarshaller
	{
		private SortedDictionary<string, string> _errors = new SortedDictionary<string, string>();
	
		public SortedDictionary<string, string> GetErrors()
		{
			return _errors;
		}

		public string GetErrorResponse()
		{
			StringBuilder errorResponse = new StringBuilder();
			bool firstTime = true;
			foreach (KeyValuePair<string, string> error in _errors) {
				if (firstTime) {
					firstTime = false;
				} else {
					errorResponse.Append(", ");
				}
				errorResponse.AppendFormat("{0}", error.Value);
			}

			return errorResponse.ToString();
		}

		public bool HasErrors()
		{
			return _errors.Count > 0;
		}

		protected void LogError(string field, string message)
		{
			_errors.Add(field, message);
		}
	}
}

