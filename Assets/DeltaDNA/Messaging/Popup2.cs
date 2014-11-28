using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using System.Text.RegularExpressions;
using UnityEditor.AnimatedValues;

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

		private GameObject _gameObject;
		private SpriteMapManager _spritemap;

		public Popup2() : this(new Dictionary<string, object>())
		{

		}

		public Popup2(Dictionary<string, object> options)
		{
			string name = (options.ContainsKey("name")) ? options["name"] as string : "Popup";
			_gameObject = new GameObject(name);
			_spritemap = _gameObject.AddComponent<SpriteMapManager>();
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
					
				SpriteMapManager spriteMapMgr = _gameObject.AddComponent<SpriteMapManager>();
				spriteMapMgr.Init(resource);
				spriteMapMgr.LoadResource(() => {
					Debug.Log("Resource loaded...");
					HasLoadedResource = true;
					if (AfterLoad != null) {
                        AfterLoad(this, new EventArgs());
                    }	
				});

				_spritemap = spriteMapMgr;
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

					object screenDict;
					if (Resource.TryGetValue("screen", out screenDict)) {
						ScreenLayer screen = _gameObject.AddComponent<ScreenLayer>();
						screen.Init(this, (Dictionary<string, object>)screenDict);
					}

					object layoutDictObj;
					if (Resource.TryGetValue("layout", out layoutDictObj)) {
						var layoutDict = layoutDictObj as Dictionary<string, object>;
						object orientationDictObj;
						if ((layoutDict).TryGetValue("landscape", out orientationDictObj) ||
							(layoutDict).TryGetValue("portrait", out orientationDictObj)) {

							var orientationDict = orientationDictObj as Dictionary<string, object>;

							BackgroundLayer background = _gameObject.AddComponent<BackgroundLayer>();
							background.Init(this, orientationDict, _spritemap.GetBackground());

							ButtonsLayer buttons = _gameObject.AddComponent<ButtonsLayer>();
							buttons.Init(this, orientationDict, _spritemap.GetButtons(), background);
						
							IsShowingPopup = true;
						} 
						else {
							Debug.LogError("No layout orientation found.");
						} 
					}
					else {
						Debug.LogError("No layout found.");
					}
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}
		}

		public void ClosePopup()
		{
			Debug.Log("Closing popup");
			if (IsShowingPopup) {
				if (BeforeClose != null) {
					BeforeClose(this, new EventArgs());
				}

				UnityEngine.Object.Destroy(_gameObject);

				if (AfterClose != null) {
					AfterClose(this, new EventArgs());
				}
			}
		}

		public void OnDismiss(PopupEventArgs eventArgs)
		{
			if (Dismiss != null) {
				Dismiss(this, eventArgs);
			}
		}

		public void OnAction(PopupEventArgs eventArgs)
		{
			if (Action != null) {
				Action(this, eventArgs);
			}
		}
	}

	internal class SpriteMapManager : MonoBehaviour
	{
		private Dictionary<string, object> _spriteMapDict;

		public Texture2D SpriteMap { get; private set; }
		public string URL { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public void Init(Dictionary<string, object> message)
		{
			object url, width, height;
			if (message.TryGetValue("url", out url) &&
			    message.TryGetValue("width", out width) &&
			    message.TryGetValue("height", out height)) {

			    URL = (string)url;
			    Width = (int)width;
			    Height = (int)height;
			}
			else {
				Debug.Log("Invalid image message format.");
			}

			object spriteMapDict;
			if (message.TryGetValue("spritemap", out spriteMapDict)) {
				_spriteMapDict = (Dictionary<string, object>)spriteMapDict;
			}
			else {
				Debug.Log("Invalid message format, missing 'spritemap' object");
			}
		}
	
		public void LoadResource(Action callback)
		{
			SpriteMap = new Texture2D(Width, Height);
			StartCoroutine(LoadResourceCoroutine(URL, callback));
		}

		public Texture GetBackground()
		{
			object backgroundDict;
			if (_spriteMapDict.TryGetValue("background", out backgroundDict)) {
				object x, y, width, height;
				if (((Dictionary<string, object>)backgroundDict).TryGetValue("x", out x) &&
					((Dictionary<string, object>)backgroundDict).TryGetValue("y", out y) &&
					((Dictionary<string, object>)backgroundDict).TryGetValue("width", out width) &&
					((Dictionary<string, object>)backgroundDict).TryGetValue("height", out height)) {

				    return GetSubRegion((int)x, (int)y, (int)width, (int)height);
				}
			}
			else {
				Debug.LogError("Background not found in spritemap object.");
			}

			return null;
		}

		public List<Texture> GetButtons()
		{
			List<Texture> textures = new List<Texture>();

			object buttonList;
			if (_spriteMapDict.TryGetValue("buttons", out buttonList)) {
				foreach (object buttonDict in (List<object>)buttonList) {
					object x, y, width, height;
					if (((Dictionary<string, object>)buttonDict).TryGetValue("x", out x) &&
						((Dictionary<string, object>)buttonDict).TryGetValue("y", out y) &&
						((Dictionary<string, object>)buttonDict).TryGetValue("width", out width) &&
						((Dictionary<string, object>)buttonDict).TryGetValue("height", out height)) {

						textures.Add(GetSubRegion((int)x, (int)y, (int)width, (int)height));
					}
				}
			}

			return textures;
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

	internal class Layer : MonoBehaviour
	{
		protected IPopup _popup;
		protected List<Action> _actions = new List<Action>();

		protected void RegisterAction()
		{
			_actions.Add(() => {});
		}

		protected void RegisterAction(Dictionary<string, object> action)
		{
			object typeObj, valueObj;
			action.TryGetValue("value", out valueObj);
		
			if (action.TryGetValue("type", out typeObj)) {

				PopupEventArgs eventArgs = new PopupEventArgs((string)typeObj, (string)valueObj);

				switch ((string)typeObj) {
					case "NONE": {
						_actions.Add(() => {});
						break;
					}
					case "ACTION": {
						_actions.Add(() => {
							if (valueObj != null) {
								_popup.OnAction(eventArgs);
							}
							_popup.ClosePopup();
						});
						break;
					}
					case "LINK": {
						_actions.Add(() => {
							_popup.OnAction(eventArgs);
							if (valueObj != null) {
								Application.OpenURL((string)valueObj);
							}
							_popup.ClosePopup();
						});
						break;
					}
					default : {	// "DISMISS"
						_actions.Add(() => {
							_popup.OnDismiss(eventArgs);
							_popup.ClosePopup();
						});
						break;
					}
				}
			}
		}
	}

	internal class ScreenLayer : Layer
	{
		private Texture2D _texture;
		private const byte _dimmedMaskAlpha = 128;

		public void Init(IPopup popup, Dictionary<string, object> config)
		{
			_popup = popup;

			object mask;
			if (config.TryGetValue("mask", out mask)) {
				bool show = true;
				Color32[] colours = new Color32[1];
				switch ((string)mask) 
				{
					case "dimmed": {
						colours[0] = new Color32(0, 0, 0, _dimmedMaskAlpha); 
						break;
					}
					case "clear": {
						colours[0] = new Color32(0, 0, 0, 0); 
						break;
					}
					default: {	// "none"
						show = false;
						break;
					}
				}
				if (show) {
					_texture = new Texture2D(1, 1);
					_texture.SetPixels32(colours);
					_texture.Apply();
				}
			}

			object actionObj;
			if (config.TryGetValue("action", out actionObj)) {
				RegisterAction((Dictionary<string, object>)actionObj);
			} 
			else {
				RegisterAction();
			}
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
					if (_actions.Count > 0) _actions[0].Invoke();
				}
			}
		}
	}

	internal class BackgroundLayer : Layer
	{
		private Texture _texture;
		private Rect _position;
		private float _scale;

		public void Init(IPopup popup, Dictionary<string, object> layout, Texture texture)
		{
			_popup = popup;
			_texture = texture;

			object rules;
			if (layout.TryGetValue("cover", out rules)) {
				_position = RenderAsCover((Dictionary<string, object>)rules);
			} 
			else if (layout.TryGetValue("contain", out rules)) {
				_position = RenderAsContain((Dictionary<string, object>)rules);
			}
			else {
				Debug.Log("Invalid layout");
			}

			object backgroundObj;
			if (layout.TryGetValue("background", out backgroundObj)) {
				object actionObj;
				if (((Dictionary<string, object>)backgroundObj).TryGetValue("action", out actionObj)) {
					RegisterAction((Dictionary<string, object>)actionObj);
				} 
				else {
					RegisterAction();
				}
			}
			else {
				RegisterAction();
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
					if (_actions.Count > 0) _actions[0].Invoke();
				}
			}
		}

		private Rect RenderAsCover(Dictionary<string, object> rules)
		{
			_scale = Math.Max((float)Screen.width / (float)_texture.width, (float)Screen.height / (float)_texture.height);
			float width = _texture.width * _scale;
			float height = _texture.height * _scale;

			float top = Screen.height / 2.0f - height / 2.0f;	// default "center"
			float left = Screen.width / 2.0f - width / 2.0f;
			object valign;
			if (rules.TryGetValue("valign", out valign))
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
				}
			}
			object halign;
			if (rules.TryGetValue("halign", out halign))
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
				}
			}

			return new Rect(left, top, width, height);
		}
			
		private Rect RenderAsContain(Dictionary<string, object> rules)
		{
			float lc = 0, rc = 0, tc = 0, bc = 0;
			object l, r, t, b;
			if (rules.TryGetValue("left", out l)) {
				lc = GetConstraintPixels((string)l, Screen.width);
			}
			if (rules.TryGetValue("right", out r)) {
				rc = GetConstraintPixels((string)r, Screen.width);
			}

			float ws = ((float)Screen.width - lc - rc) / (float)_texture.width;

			if (rules.TryGetValue("top", out t)) {
				tc = GetConstraintPixels((string)t, Screen.height);
			}
			if (rules.TryGetValue("bottom", out b)) {
				bc = GetConstraintPixels((string)b, Screen.height);
			}

			float hs = ((float)Screen.height - tc - bc) / (float)_texture.height;

			_scale = Math.Min(ws, hs);
			float width = _texture.width * _scale;
			float height = _texture.height * _scale;

			float top = ((Screen.height - tc - bc) / 2.0f - height / 2.0f) + tc;	// default "center"
			float left = ((Screen.width - lc - rc) / 2.0f - width / 2.0f) + lc; 	// default "center"

			object valign;
			if (rules.TryGetValue("valign", out valign))
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
			if (rules.TryGetValue("halign", out halign))
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

	internal class ButtonsLayer : Layer
	{
		private List<Texture> _textures = new List<Texture>();
		private List<Rect> _positions = new List<Rect>();

		public void Init(IPopup popup, Dictionary<string, object> orientation, List<Texture> textures, BackgroundLayer content)
		{
			_popup = popup;
			
			object buttonsObj;
			if (orientation.TryGetValue("buttons", out buttonsObj)) {
				var buttons = buttonsObj as List<object>;
				for (int i = 0; i < buttons.Count; ++i) {
					var button = buttons[i] as Dictionary<string, object>;
					float left = 0, top = 0;
					object x, y;
					if (button.TryGetValue("x", out x)) {
						left = (int)x * content.Scale + content.Position.xMin;
					}
					if (button.TryGetValue("y", out y)) {
						top = (int)y * content.Scale + content.Position.yMin;
					}
					_positions.Add(new Rect(left, top, textures[i].width * content.Scale, textures[i].height * content.Scale));
				
					object actionObj;
					if (button.TryGetValue("action", out actionObj)) {
						RegisterAction((Dictionary<string, object>)actionObj);
					} 
					else {
						RegisterAction();
					}
				}
				_textures = textures;
			}
		}

		public void OnGUI()
		{
			GUI.depth = 0;

			for (int i = 0; i < _textures.Count; ++i)
			{
				if (GUI.Button(_positions[i], _textures[i], GUIStyle.none)) {
					Debug.Log("Button "+(i+1)+" clicked");
					_actions[i].Invoke();
				}
			}
		}
	}

}

