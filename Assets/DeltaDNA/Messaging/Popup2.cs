using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace DeltaDNA.Messaging
{
	public class Popup2 : IPopup
	{
		public event EventHandler BeforeLoad;
		public event EventHandler AfterLoad;
		public event EventHandler BeforeShow;
		public event EventHandler BeforeClose;
		public event EventHandler AfterClose;
		public event EventHandler<PopupEventArgs> Dismiss;
		public event EventHandler<PopupEventArgs> Action;

		public Dictionary<string, object> Resource { get; private set; }
		public bool HasLoadedResource { get; private set; }
		public bool IsShowingPopup { get; private set; }

		public GameObject Background { get; set; }
		public GameObject Button1 { get; set; }
		public GameObject Button2 { get; set; }
		// should be a list of buttons going forward...

		private GameObject _popup;
		private SpriteMapManager _spritemap;

		public Popup2() : this(new Dictionary<string, object>())
		{

		}

		public Popup2(Dictionary<string, object> options)
		{
			string name = (options.ContainsKey("name")) ? options["name"] as string : "Popup";
			_popup = new GameObject(name);
			_spritemap = _popup.AddComponent<SpriteMapManager>();
		}

		public void LoadResource(ImageComposition image)
		{
			// see if this can be redundant...
		}

		public void LoadResource(Dictionary<string, object> resource)
		{
			try {
				if (BeforeLoad != null) {
					BeforeLoad(this, new EventArgs());
				}
					
				object url, width, height;
				if (resource.TryGetValue("url", out url) &&
				    resource.TryGetValue("width", out width) &&
				    resource.TryGetValue("height", out height)) {
					
					Debug.Log("Loading resource...");
					_spritemap.LoadResource((string)url, (int)width, (int)height, () => 
					{
						Debug.Log("Resource loaded...");
						HasLoadedResource = true;
						if (AfterLoad != null) {
	                        AfterLoad(this, new EventArgs());
	                    }	
					});
				} else {
					Debug.LogWarning("Failed to load resource "+url);
				}
				// else what if the dictionary is corrupt?

				// Add action behaviour to the buttons?

				Resource = resource;

			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}

		public void ShowPopup()
		{
			if (HasLoadedResource)
			{
				try {
					if (BeforeShow != null) {
						BeforeShow(this, new EventArgs());
					}
					//_behaviour.ShowPopup(Resource);

					// Build layers
					Background background = _popup.AddComponent<Background>();
					//background.Init(Resource);

					Container container = _popup.AddComponent<Container>();
					Texture containerTexture = _spritemap.GetSubRegion(new Rect(2, 52, 640, 400));

					Buttons buttons = _popup.AddComponent<Buttons>();
					List<Texture> buttonTextures = new List<Texture>();
					buttonTextures.Add(_spritemap.GetSubRegion(new Rect(2, 2, 96, 48)));
					buttonTextures.Add(_spritemap.GetSubRegion(new Rect(644, 404, 96, 48)));

					// Container Layout
					object layout;
					if (Resource.TryGetValue("layout", out layout)) {
						// Not caring about orientation of device just now
						Dictionary<string, object> l = layout as Dictionary<string, object>;
						Dictionary<string, object> orientation = null;
						if (l.ContainsKey("landscape")) {
							orientation = l["landscape"] as Dictionary<string, object>;
						}
						else if (l.ContainsKey("portrait")) {
							orientation = l["portrait"] as Dictionary<string, object>;
						}

						if (orientation != null) {
							container.Init(orientation, containerTexture);

							object btns;
							if (orientation.TryGetValue("buttons", out btns)) {
								Debug.Log(container.Position);
								buttons.Init(
									(List<object>)btns, 
									buttonTextures, 
									container);
							}


						} else {
							Debug.LogError("No layout orientation found");
						}
					} else {
						Debug.LogError("No layout found");
					}


					// float scale = container.Scale...

					//Buttons buttons = _popup.AddComponent<Buttons>();
					//buttons.Init(Resource);

					IsShowingPopup = true;
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}
		}
	}

	public class SpriteMapManager : MonoBehaviour
	{
		public Texture2D SpriteMap { get; set; }
	
		public void LoadResource(string url, int width, int height, Action callback)
		{
			SpriteMap = new Texture2D(width, height);
			StartCoroutine(LoadResourceCoroutine(url, callback));
		}

		public Texture2D GetSubRegion(int x, int y, int width, int height)
		{
			Color[] pixels = SpriteMap.GetPixels(x, SpriteMap.height-y-height, width, height);
			Texture2D result = new Texture2D(width, height, SpriteMap.format, false);
			result.SetPixels(pixels);
			result.Apply();
			return result;
		}

		public Texture2D GetSubRegion(Rect rect)
		{
			return GetSubRegion(
				Mathf.FloorToInt(rect.x), 
				Mathf.FloorToInt(rect.y), 
				Mathf.FloorToInt(rect.width), 
				Mathf.FloorToInt(rect.height));
		}

		private IEnumerator LoadResourceCoroutine(string url, Action callback)
		{
			WWW www = new WWW(url);

			yield return www;

			if (www.error == null) {
				www.LoadImageIntoTexture(SpriteMap);
			} else {
				throw new PopupException("Failed to load resource "+url+" "+www.error);
			}
		
			callback();
		}

	}

	public class Background : MonoBehaviour
	{
		private Texture2D _texture = new Texture2D(1, 1);

		public void Awake()
		{
			Color32[] colours = new Color32[1];
			colours[0] = new Color32(0, 0, 0, 128); 
			_texture.SetPixels32(colours);
			_texture.Apply();
		}

		public void OnGUI()
		{
			GUI.depth = 2;

			if (_texture)
			{
				Rect position = new Rect(0, 0, Screen.width, Screen.height);
				GUI.DrawTexture(position, _texture);
				if (GUI.Button(position, "", GUIStyle.none)) {
					Debug.Log("Background clicked");
				}
			}
		}
	}

	public class Container : MonoBehaviour
	{
		private Texture _texture;
		private Rect _position;
		private float _scale;

		public void Init(Dictionary<string, object> layout, Texture texture)
		{
			_texture = texture;

			object rules;
			if (layout.TryGetValue("cover", out rules)) {
				_position = RenderAsCover((Dictionary<string, object>)rules);
			} 
			else if (layout.TryGetValue("contain", out rules)) {
				_position = RenderAsContain((Dictionary<string, object>)rules);
			}
			else if (layout.TryGetValue("constrain", out rules)) {
				_position = RenderAsConstrain((Dictionary<string, object>)rules);
			}
			else {
				Debug.Log("Invalid layout");
			}
		}

		public Rect Position { get { return _position; }}

		public float Scale { get { return _scale; }}

		public void OnGUI()
		{
			GUI.depth = 1;

			if (_texture)
			{
				//if (GUI.Button(_position, _texture, GUIStyle.none)) {
				//	Debug.Log("Container clicked");
				//}
				// Unity feature: You can't scale a Button's texture bigger than the original
				// but a normal texture is quite happy.  
				GUI.DrawTexture(_position, _texture);
				if (GUI.Button(_position, "", GUIStyle.none)) {
					Debug.Log("container clicked");
				}
			}
		}

		private Rect RenderAsCover(Dictionary<string, object> rules)
		{
			_scale = Math.Max((float)Screen.width / (float)_texture.width, (float)Screen.height / (float)_texture.height);
			float width = _texture.width * _scale;
			float height = _texture.height * _scale;

			float top = 0, left = 0; 
			object valign;
			if (rules.TryGetValue("v", out valign))
			{
				switch ((string)valign)
				{
					case "top": {
						top = 0;
						break;
					}
					case "bottom": {
						top = Screen.height - height;
						break;
					}
					default: { // "center"
						top = Screen.height / 2.0f - height / 2.0f;
						break;
					}
				}
			}
			object halign;
			if (rules.TryGetValue("h", out halign))
			{
				switch ((string)halign)
				{
					case "left": {
						left = 0;
						break;
					}
					case "right": {
						left = Screen.width - width;
						break;
					}
					default: { // "center"
						left = Screen.width / 2.0f - width / 2.0f;
						break;
					}
				}
			}

			return new Rect(left, top, width, height);
		}

		private Rect RenderAsContain(Dictionary<string, object> rules)
		{
			_scale = Math.Min((float)Screen.width / (float)_texture.width, (float)Screen.height / (float)_texture.height);
			float width = _texture.width * _scale;
			float height = _texture.height * _scale;

			float top = 0, left = 0; 
			object valign;
			if (rules.TryGetValue("v", out valign))
			{
				switch ((string)valign)
				{
					case "top": {
						top = 0;
						break;
					}
					case "bottom": {
						top = Screen.height - height;
						break;
					}
					default: { // "center"
						top = Screen.height / 2.0f - height / 2.0f;
						break;
					}
				}
			}
			object halign;
			if (rules.TryGetValue("h", out halign))
			{
				switch ((string)halign)
				{
					case "left": {
						left = 0;
						break;
					}
					case "right": {
						left = Screen.width - width;
						break;
					}
					default: { // "center"
						left = Screen.width / 2.0f - width / 2.0f;
						break;
					}
				}
			}
			
			return new Rect(left, top, width, height);
		}

		private Rect RenderAsConstrain(Dictionary<string, object> rules)
		{
			//float scale = Math.Min((float)Screen.width / (float)_texture.width, (float)Screen.height / (float)_texture.height);
			// find max scale that satifies the contraints
			float lc = 0, rc = 0, tc = 0, bc = 0;
			object l, r, t, b;
			if (rules.TryGetValue("l", out l)) {
				lc = GetConstraintPixels((string)l, Screen.width);
			}
			if (rules.TryGetValue("r", out r)) {
				rc = GetConstraintPixels((string)r, Screen.width);
			}

			float ws = ((float)Screen.width - lc - rc) / (float)_texture.width;

			if (rules.TryGetValue("t", out t)) {
				tc = GetConstraintPixels((string)t, Screen.height);
			}
			if (rules.TryGetValue("b", out b)) {
				bc = GetConstraintPixels((string)b, Screen.height);
			}

			float hs = ((float)Screen.height - tc - bc) / (float)_texture.height;

			_scale = Math.Min(ws, hs);	// This is Max if you want to do cover (only works if no contraints since can't crop)
			float width = _texture.width * _scale;
			float height = _texture.height * _scale;

			float top = ((Screen.height - tc - bc) / 2.0f - height / 2.0f) + tc;	// default "center"
			float left = ((Screen.width - lc - rc) / 2.0f - width / 2.0f) + lc; 	// default "center"

			object valign;
			if (rules.TryGetValue("v", out valign))
			{
				switch ((string)valign)
				{
					case "top": {
						top = tc;
						break;
					}
					case "bottom": {
						top = Screen.height - height - bc;
						break;
					}
				}
			}
			object halign;
			if (rules.TryGetValue("h", out halign))
			{
				switch ((string)halign)
				{
					case "left": {
						left = lc;
						break;
					}
					case "right": {
						left = Screen.width - width - rc;
						break;
					}
				}
			}

			return new Rect(left, top, width, height);
		}

		private float GetConstraintPixels(string constraint, float edge)
		{
			float val = 0;
			Regex rgx = new Regex(@"(\d+)(px|%)", RegexOptions.IgnoreCase);
			var match = rgx.Match(constraint);
			if (match != null && match.Success) {
				var groups = match.Groups;
				Debug.Log(groups[1].Value +" "+groups[2].Value);

				if (float.TryParse(groups[1].Value, out val)) {
					if (groups[2].Value == "%") {
						return edge * val / 100.0f;
					} else {
					return val;
					}
				}
			}
			return val;
		}
	}

	public class Buttons : MonoBehaviour
	{
		private List<Texture> _textures = new List<Texture>();
		private List<Rect> _positions = new List<Rect>();
					
		public void Init(List<object> buttons, List<Texture> textures, Container container)
		{
			for (int i = 0; i < buttons.Count; ++i) {
				float left = 0, top = 0;
				object x, y;
				if (((Dictionary<string, object>)buttons[i]).TryGetValue("x", out x)) {
					left = (int)x * container.Scale + container.Position.xMin;
				}
				if (((Dictionary<string, object>)buttons[i]).TryGetValue("y", out y)) {
					top = (int)y * container.Scale + container.Position.yMin;
				}
				_positions.Add(new Rect(left, top, textures[i].width * container.Scale, textures[i].height * container.Scale));
			}

			_textures = textures;
		}

		public void OnGUI()
		{
			GUI.depth = 0;

			for (int i = 0; i < _textures.Count; ++i)
			{
				if (GUI.Button(_positions[i], _textures[i], GUIStyle.none)) {
					Debug.Log("Button "+(i+1)+" clicked");
				}
			}
		}
	}

}

