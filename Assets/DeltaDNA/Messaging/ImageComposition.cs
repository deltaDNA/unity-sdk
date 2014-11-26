using System;
using System.Collections.Generic;

namespace DeltaDNA.Messaging
{
	public class Composition
	{
		public string Url { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string Format { get; set; }
		public SpriteMap SpriteMap { get; set; }
		public Layout Layout { get; set; }

	}


	public class ImageComposition
	{
		public SpriteMap SpriteMap { get; set; }
		public Viewport Viewport { get; set; }
		public ImageAsset Background { get; set; }
		public ImageAsset Button1 { get; set; }
		public ImageAsset Button2 { get; set; }

		public static ImageComposition BuildFromDictionary(Dictionary<string, object> d)
		{
			ImageCompositionMarshaller marshaller = new ImageCompositionMarshaller();
			ImageComposition result = marshaller.FromDictionary(d);
			if (result == null) {
				throw new ArgumentException(marshaller.GetErrorResponse());
			}
			return result;
		}
	}

	internal class ImageCompositionMarshaller : BaseMarshaller
	{
		public ImageComposition FromDictionary(Dictionary<string, object> d)
		{
			if (d == null) return null;

			ImageComposition result = new ImageComposition();

			try {
				result.SpriteMap = SpriteMap.BuildFromDictionary(d);
			} catch (Exception ex) {
				LogError("image", ex.Message);
			}

			if (d.ContainsKey("viewport")) {
				try {
					var viewport = d["viewport"] as Dictionary<string, object>;
					result.Viewport = Viewport.BuildFromDictionary(viewport);
				} catch (Exception ex) {
					LogError("viewport", ex.Message);
				}
			} else {
				LogError("viewport", "viewport is missing");
			}

			if (d.ContainsKey("background")) {
				try {
					var background = d["background"] as Dictionary<string, object>;
					result.Background = ImageAsset.BuildFromDictionary(background);
				} catch (Exception ex) {
					LogError("background", ex.Message);
				}
			} else {
				LogError("background", "background is missing");
			}

			if (d.ContainsKey("button1")) {
				try {
					var button = d["button1"] as Dictionary<string, object>;
					result.Button1 = ImageAsset.BuildFromDictionary(button);
				} catch (Exception ex) {
					LogError("button1", ex.Message);
				}
			} 

			if (d.ContainsKey("button2")) {
				try {
					var button = d["button2"] as Dictionary<string, object>;
					result.Button2 = ImageAsset.BuildFromDictionary(button);
				} catch (Exception ex) {
					LogError("button2", ex.Message);
				}
			} 

			return HasErrors() ? null : result;
		}
	}
}

