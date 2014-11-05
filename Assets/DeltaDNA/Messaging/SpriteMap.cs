using System;
using System.Collections.Generic;

namespace DeltaDNA.Messaging
{
	public class SpriteMap
	{
		public string Url { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string Format { get; set; }

		public static SpriteMap BuildFromDictionary(Dictionary<string, object> d)
		{
			SpriteMapMarshaller marshaller = new SpriteMapMarshaller();
			SpriteMap result = marshaller.FromDictionary(d);
			if (result == null) {
				throw new ArgumentException(marshaller.GetErrorResponse());
			}
			return result;
		}
	}

	internal class SpriteMapMarshaller : BaseMarshaller
	{
		public SpriteMap FromDictionary(Dictionary<string, object> d)
		{
			if (d == null) return null;

			SpriteMap result = new SpriteMap();

			if (d.ContainsKey("url")) {
				result.Url = d["url"] as string;
			} else {
				LogError("url", "url is missing");
			}

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

			if (d.ContainsKey("format")) {
				string format = d["format"] as string;
				if (format.ToUpper() != "JPG" && format.ToUpper() != "PNG") {
					LogError("format", format+" is not a supported image format");
				} else {
					result.Format = format;
				}
			} else {
				LogError("format", "format is missing");
			}

			return HasErrors() ? null : result;
		}
	}
}

