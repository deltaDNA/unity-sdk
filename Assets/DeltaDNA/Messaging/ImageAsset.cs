using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA.Messaging
{
	public class ImageAsset
	{
		public enum ActionType { NONE, DISMISS, LINK, CUSTOM };

		public Rect GlobalPosition { get; set; }
		public Rect ImagePosition { get; set; }
		public ActionType Action { get; set; }
		public String ActionParam { get; set; }

		public static ImageAsset BuildFromDictionary(Dictionary<string, object> d)
		{
			ImageAssetMarshaller marshaller = new ImageAssetMarshaller();
			ImageAsset result = marshaller.FromDictionary(d);

			if (result == null) {
				throw new ArgumentException(marshaller.GetErrorResponse());
			}
			return result;
		}
	}

	public class ImageAssetMarshaller : BaseMarshaller
	{
		public ImageAsset FromDictionary(Dictionary<string, object> d)
		{
			if (d == null) return null;

			ImageAsset result = new ImageAsset();

			float x = 0, y = 0, imgX = 0, imgY = 0, width = 0, height = 0;
			if (d.ContainsKey("x")) {
				if (!float.TryParse(d["x"].ToString(), out x)) {
					LogError("x", "x is not a valid number");	
				}
			} else {
				LogError("x", "x position missing");
			}

			if (d.ContainsKey("y")) {
				if (!float.TryParse(d["y"].ToString(), out y)) {
					LogError("y", "y is not a valid number");
				}
			} else {
				LogError("y", "y is missing");
			}
			
			if (d.ContainsKey("imgX")) {
				if (!float.TryParse(d["imgX"].ToString(), out imgX)) {
					LogError("imgX", "imgX is not a valid number");
				}
			} else {
				LogError("imgX", "imgX is missing");
			}

			if (d.ContainsKey("imgY")) {
				if (!float.TryParse(d["imgY"].ToString(), out imgY)) {
					LogError("imgY", "imgY is not a valid number");
				}
			} else {
				LogError("imgY", "imgY is missing");
			}

			if (d.ContainsKey("width")) {
				if (!float.TryParse(d["width"].ToString(), out width)) {
					LogError("width", "width is not a valid number");
				}
			} else {
				LogError("width", "width is missing");
			}

			if (d.ContainsKey("height")) {
				if (!float.TryParse(d["height"].ToString(), out height)) {
					LogError("height", "height is not a valid number");
				}
			} else {
				LogError("height", "height is missing");
			}

			if (d.ContainsKey("actionType")) {
				string actionTypeStr = d["actionType"] as string;
				ImageAsset.ActionType actionType;
				try {
					actionType = (ImageAsset.ActionType)Enum.Parse(typeof(ImageAsset.ActionType), actionTypeStr, true);
					result.Action = actionType;
				
					if (result.Action == ImageAsset.ActionType.LINK || result.Action == ImageAsset.ActionType.CUSTOM) {
						if (d.ContainsKey("actionParam")) {
							result.ActionParam = d["actionParam"] as string;
						} else {
							LogError("actionParam", "actionType "+result.Action+ " requires missing actionParam");
						}
					}
				} catch (Exception) {
					LogError("actionType", "actionType "+actionTypeStr+ " is not recognised");
				}
			} else {
				LogError("actionType", "actionType is missing");
			}

			if (!HasErrors()) {
				result.GlobalPosition = new Rect(x, y, width, height);
				result.ImagePosition = new Rect(imgX, imgY, width, height);
				return result;
			}
			return null;
		}
	}
}

