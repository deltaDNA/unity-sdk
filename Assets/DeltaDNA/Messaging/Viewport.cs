using System;
using System.Collections.Generic;

namespace DeltaDNA.Messaging
{
	public class Viewport
	{
		public int Width { get; set; }
		public int Height { get; set; }	

		public static Viewport BuildFromDictionary(Dictionary<string, object> d)
		{
			ViewportMarshaller marshaller = new ViewportMarshaller();
			Viewport result = marshaller.FromDictionary(d);
			if (result == null) {
				throw new ArgumentException(marshaller.GetErrorResponse());
			}
			return result;
		}
	}

	internal class ViewportMarshaller : BaseMarshaller
	{
		public Viewport FromDictionary(Dictionary<string, object> d)
		{
			if (d == null) return null;

			Viewport result = new Viewport();

			if (d.ContainsKey("width")) {
				int width;
				if (!int.TryParse(d["width"].ToString(), out width)) {
					LogError("width", "width is not a valid number");
				} else {
					result.Width = width;
				}
			} else {
				LogError("width", "width is missing");
			}

			if (d.ContainsKey("height")) {
				int height;
				if (!int.TryParse(d["height"].ToString(), out height)) {
					LogError("height", "height is not a valid number");
				} else {
					result.Height = height;
				}
			} else {
				LogError("height", "height is missing");
			}

			return HasErrors() ? null : result;
		}
	}
}

