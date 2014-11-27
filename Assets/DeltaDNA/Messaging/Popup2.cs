using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
					//Background background = _popup.AddComponent<Background>();
					//background.Init(Resource);

					Container container = _popup.AddComponent<Container>();
					Texture containerTexture = _spritemap.GetSubRegion(new Rect(2, 52, 640, 400));

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

//		public void ShowPopup(Dictionary<string, object> resource)
//		{
//			Debug.Log("Asked to show popup");
//
//			_container = TextureHelpers.CopySubRegion(_spritemap, new Rect(2, 52, 640, 400));
//			_button = TextureHelpers.CopySubRegion(_spritemap, new Rect(2, 2, 96, 48));
//		}
//
//		public void OnGUI()
//		{
//			// This works!  You must set the value lower than the other gui elements,
//			// so it will need to be an input to the configuration.
//			GUI.depth = 0;
//
//			if (_container) {
//				//GUI.DrawTextureWithTexCoords(position, _container, texCoords);
//				if (GUI.Button(new Rect(30, 30, 320, 200), _container, GUIStyle.none)) {
//					Debug.Log("Container was clicked");
//				}
//			}
//
//			if (_button) {
//				if (GUI.Button(new Rect(50, 50, 48, 24), _button, GUIStyle.none)) {
//					Debug.Log("Button was clicked");
//				}
//			}
//		}


	}

	public class Background : MonoBehaviour
	{
		private Texture _texture;

		public void OnGUI()
		{
			GUI.depth = 0;

			GUI.color = new Color32(255, 255, 255, 100);

			if (_texture)
			{
				Rect position = new Rect(0, 0, Screen.width, Screen.height);
				if (GUI.Button(position, _texture, GUIStyle.none)) {
					Debug.Log("Background clicked");
				}
			}
		}
	}

	public class Container : MonoBehaviour
	{
		private Texture _texture;
		private Rect _position;

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

			}
			else {
				Debug.Log("Invalid layout");
			}
		}

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
			float scale = Math.Max((float)Screen.width / (float)_texture.width, (float)Screen.height / (float)_texture.height);
			float width = _texture.width * scale;
			float height = _texture.height * scale;

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
			float scale = Math.Min((float)Screen.width / (float)_texture.width, (float)Screen.height / (float)_texture.height);
			float width = _texture.width * scale;
			float height = _texture.height * scale;

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
	}

	public class Buttons : MonoBehaviour
	{
		private List<Texture> _texture = new List<Texture>();
		private List<Rect> _position = new List<Rect>();

		public void OnGUI()
		{
			GUI.depth = 2;

			for (int i = 0; i < _texture.Count; ++i)
			{
				if (GUI.Button(_position[i], _texture[i], GUIStyle.none)) {
					Debug.Log("Button "+i+1+" clicked");
				}
			}
		}
	}

}

