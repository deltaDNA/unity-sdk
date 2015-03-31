using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeltaDNA.Messaging
{
	public class Popup : IPopup
	{
		public event EventHandler BeforePrepare;
		public event EventHandler AfterPrepare;
		public event EventHandler BeforeShow;
		public event EventHandler BeforeClose;
		public event EventHandler AfterClose;
		public event EventHandler<PopupEventArgs> Dismiss;
		public event EventHandler<PopupEventArgs> Action;

		public Dictionary<string, object> Configuration { get; private set; }
		public bool IsReady { get; private set; }
		public bool IsShowing { get; private set; }

		private GameObject _gameObject;
		private SpriteMap _spritemap;
		private string _name = "Popup";
		private int _depth = 0;

		public Popup() : this(new Dictionary<string, object>()) {}

		public Popup(Dictionary<string, object> options)
		{
			object name;
			if (options.TryGetValue("name", out name)) {
				_name = (string)name;
			}

			object depth;
			if (options.TryGetValue("depth", out depth)) {
				_depth = (int)depth;
			}
		}

		public void Prepare(Dictionary<string, object> configuration)
		{
			try {
				if (BeforePrepare != null) {
					BeforePrepare(this, new EventArgs());
				}

				_gameObject = new GameObject(_name);
				SpriteMap spriteMap = _gameObject.AddComponent<SpriteMap>();
				spriteMap.Init(configuration);
				spriteMap.LoadResource(() => {
					IsReady = true;
					if (AfterPrepare != null) {
                        AfterPrepare(this, new EventArgs());
                    }	
				});

				_spritemap = spriteMap;
				Configuration = configuration;

			} catch (Exception ex) {
				Logger.LogError("Preparing popup configuration failed: "+ex.Message);
			}
		}

		public void Show()
		{
			if (IsReady)
			{
				try {
					if (BeforeShow != null) {
						BeforeShow(this, new EventArgs());
					}

					object shimDictObj;
					if (Configuration.TryGetValue("shim", out shimDictObj)) {
						var shimDict = shimDictObj as Dictionary<string, object>;
						ShimLayer shim = _gameObject.AddComponent<ShimLayer>();
						shim.Init(this, shimDict, _depth);
					}

					object layoutDictObj;
					if (Configuration.TryGetValue("layout", out layoutDictObj)) {
						var layoutDict = layoutDictObj as Dictionary<string, object>;
						object orientationDictObj;
						if ((layoutDict).TryGetValue("landscape", out orientationDictObj) ||
							(layoutDict).TryGetValue("portrait", out orientationDictObj)) {

							var orientationDict = orientationDictObj as Dictionary<string, object>;

							BackgroundLayer background = _gameObject.AddComponent<BackgroundLayer>();
							background.Init(this, orientationDict, _spritemap.GetBackground(), _depth-1);

							ButtonsLayer buttons = _gameObject.AddComponent<ButtonsLayer>();
							buttons.Init(this, orientationDict, _spritemap.GetButtons(), background, _depth-2);
						
							IsShowing = true;
						} 
						else {
							Logger.LogError("No layout orientation found.");
						} 
					}
					else {
						Logger.LogError("No layout found.");
					}
				} catch (Exception ex) {
					Logger.LogError("Showing popup failed: "+ex.Message);
				}
			}
		}

		public void Close()
		{
			if (IsShowing) {
				if (BeforeClose != null) {
					BeforeClose(this, new EventArgs());
				}

				foreach (var layer in _gameObject.GetComponents<Layer>()) {
					UnityEngine.Object.Destroy(layer);
				}

				if (AfterClose != null) {
					AfterClose(this, new EventArgs());
				}
				IsShowing = false;
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

	internal class SpriteMap : MonoBehaviour
	{
		private Dictionary<string, object> _spriteMapDict;
		private Texture2D _spriteMap;

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
			    Width = (int)((long)width);
			    Height = (int)((long)height);
			}
			else {
				Logger.LogError("Invalid image message format.");
			}

			object spriteMapDict;
			if (message.TryGetValue("spritemap", out spriteMapDict)) {
				_spriteMapDict = (Dictionary<string, object>)spriteMapDict;
			}
			else {
				Logger.LogError("Invalid message format, missing 'spritemap' object");
			}
		}
	
		public void LoadResource(Action callback)
		{
			_spriteMap = new Texture2D(Width, Height);
			StartCoroutine(LoadResourceCoroutine(URL, callback));
		}

		public Texture GetSpriteMap()
		{
			return _spriteMap;
		}

		public Texture GetBackground()
		{
			object backgroundDictObj;
			if (_spriteMapDict.TryGetValue("background", out backgroundDictObj)) {
				var backgroundDict = backgroundDictObj as Dictionary<string, object>;
				object x, y, width, height;
				if (backgroundDict.TryGetValue("x", out x) &&
					backgroundDict.TryGetValue("y", out y) &&
					backgroundDict.TryGetValue("width", out width) &&
					backgroundDict.TryGetValue("height", out height)) {

				    return GetSubRegion((int)((long)x), (int)((long)y), (int)((long)width), (int)((long)height));
				}
			}
			else {
				Logger.LogError("Background not found in spritemap object.");
			}

			return null;
		}

		public List<Texture> GetButtons()
		{
			List<Texture> textures = new List<Texture>();

			object buttonListObj;
			if (_spriteMapDict.TryGetValue("buttons", out buttonListObj)) {
				var buttonList = buttonListObj as List<object>;
				foreach (object buttonDictObj in buttonList) {
					var buttonDict = buttonDictObj as Dictionary<string, object>;
					object x, y, width, height;
					if (buttonDict.TryGetValue("x", out x) &&
						buttonDict.TryGetValue("y", out y) &&
						buttonDict.TryGetValue("width", out width) &&
						buttonDict.TryGetValue("height", out height)) {

						textures.Add(GetSubRegion((int)((long)x), (int)((long)y), (int)((long)width), (int)((long)height)));
					}
				}
			}

			return textures;
		}

		public Texture2D GetSubRegion(int x, int y, int width, int height)
		{
			Color[] pixels = _spriteMap.GetPixels(x, _spriteMap.height-y-height, width, height);
			Texture2D result = new Texture2D(width, height, _spriteMap.format, false);
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
				www.LoadImageIntoTexture(_spriteMap);
			} else {
				Logger.LogError("Failed to load resource "+url+" "+www.error);
				yield break;
			}
		
			callback();
		}

	}

	internal class Layer : MonoBehaviour
	{
		protected IPopup _popup;
		protected List<Action> _actions = new List<Action>();
		protected int _depth = 0;

		protected void RegisterAction()
		{
			_actions.Add(() => {});
		}

		protected void RegisterAction(Dictionary<string, object> action, string id)
		{
			object typeObj, valueObj;
			action.TryGetValue("value", out valueObj);
		
			if (action.TryGetValue("type", out typeObj)) {

				PopupEventArgs eventArgs = new PopupEventArgs(id, (string)typeObj, (string)valueObj);

				switch ((string)typeObj) {
					case "none": {
						_actions.Add(() => {});
						break;
					}
					case "action": {
						_actions.Add(() => {
							if (valueObj != null) {
								_popup.OnAction(eventArgs);
							}
							_popup.Close();
						});
						break;
					}
					case "link": {
						_actions.Add(() => {
							_popup.OnAction(eventArgs);
							if (valueObj != null) {
								Application.OpenURL((string)valueObj);
							}
							_popup.Close();
						});
						break;
					}
					default : {	// "dismiss"
						_actions.Add(() => {
							_popup.OnDismiss(eventArgs);
							_popup.Close();
						});
						break;
					}
				}
			}
		}
	}

	internal class ShimLayer : Layer
	{
		private Texture2D _texture;
		private const byte _dimmedMaskAlpha = 128;

		public void Init(IPopup popup, Dictionary<string, object> config, int depth)
		{
			_popup = popup;
			_depth = depth;

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
				RegisterAction((Dictionary<string, object>)actionObj, "shim");
			} 
			else {
				RegisterAction();
			}
		}

		public void OnGUI()
		{
			GUI.depth = _depth;

			if (_texture)
			{
				Rect position = new Rect(0, 0, Screen.width, Screen.height);
				GUI.DrawTexture(position, _texture);
				if (GUI.Button(position, "", GUIStyle.none)) {
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

		public void Init(IPopup popup, Dictionary<string, object> layout, Texture texture, int depth)
		{
			_popup = popup;
			_texture = texture;
			_depth = depth;

			object backgroundObj;
			if (layout.TryGetValue("background", out backgroundObj)) {
				var background = backgroundObj as Dictionary<string, object>;

				object actionObj;
				if ((background).TryGetValue("action", out actionObj)) {
					RegisterAction((Dictionary<string, object>)actionObj, "background");
				} 
				else {
					RegisterAction();
				}

				object rulesObj;
				if (background.TryGetValue("cover", out rulesObj)) {
					_position = RenderAsCover((Dictionary<string, object>)rulesObj);
				} 
				else if (background.TryGetValue("contain", out rulesObj)) {
					_position = RenderAsContain((Dictionary<string, object>)rulesObj);
				}
				else {
					Logger.LogError("Invalid layout");
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
			GUI.depth = _depth;

			if (_texture)
			{
				GUI.DrawTexture(_position, _texture);
				if (GUI.Button(_position, "", GUIStyle.none)) {
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

		public void Init(IPopup popup, Dictionary<string, object> orientation, List<Texture> textures, BackgroundLayer content, int depth)
		{
			_popup = popup;
			_depth = depth;
			
			object buttonsObj;
			if (orientation.TryGetValue("buttons", out buttonsObj)) {
				var buttons = buttonsObj as List<object>;
				for (int i = 0; i < buttons.Count; ++i) {
					var button = buttons[i] as Dictionary<string, object>;
					float left = 0, top = 0;
					object x, y;
					if (button.TryGetValue("x", out x)) {
						left = (int)((long)x) * content.Scale + content.Position.xMin;
					}
					if (button.TryGetValue("y", out y)) {
						top = (int)((long)y) * content.Scale + content.Position.yMin;
					}
					_positions.Add(new Rect(left, top, textures[i].width * content.Scale, textures[i].height * content.Scale));
				
					object actionObj;
					if (button.TryGetValue("action", out actionObj)) {
						RegisterAction((Dictionary<string, object>)actionObj, "button"+(i+1));
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
			GUI.depth = _depth;

			for (int i = 0; i < _textures.Count; ++i)
			{
				GUI.DrawTexture(_positions[i], _textures[i]);
				if (GUI.Button(_positions[i], "", GUIStyle.none)) {
					_actions[i].Invoke();
				}
			}
		}
	}

}

