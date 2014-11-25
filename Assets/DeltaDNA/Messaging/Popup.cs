using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA.Messaging
{
	/// <summary>
	/// Builds a popup to display an image message from Engage.  Each popup has a background
	/// and two buttons.  Their images are determined by an ImageComposition object.  A 
	/// number of event handlers are available to customise the behaviour for your game.
	///
	/// Known limitations:
	/// 	The Popup uses GUITextures to render the background and buttons.  These will
	/// interfere with UI components drawn in OnGUI.
	/// </summary>
	public class Popup : IPopup
	{
		public event EventHandler BeforeLoad;
		public event EventHandler AfterLoad;
		public event EventHandler BeforeShow;
		public event EventHandler BeforeClose;
		public event EventHandler AfterClose;
		public event EventHandler<PopupEventArgs> Dismiss;
		public event EventHandler<PopupEventArgs> Action;

		public ImageComposition Image { get; set; }
		public bool HasLoadedResource { get; set; }
		public bool IsShowingPopup { get; set; }

		private GameObject _popup;
		private PopupBehaviour _behaviour;
		private float _zAxis = 1;

		/// <summary>
		/// Creates a new Popup object with default behaviour.
		/// </summary>
		public Popup() : this(new Dictionary<string, object>())
		{

		}

		/// <summary>
		/// Creates a new Popup object with a set of options.
		///
		/// Available options:
		///		* name - override the name of GameObject (default: Popup)
		///		* zAxis - the position the GUITextures are drawn at (default: 1).
		/// </summary>
		/// <param name="options">The dictionary of options.</param>
		public Popup(Dictionary<string, object> options)
		{
			string name = (options.ContainsKey("name")) ? options["name"] as string : "Popup";
			_popup = new GameObject(name);
			_behaviour = _popup.AddComponent<PopupBehaviour>();
			if (options.ContainsKey("zAxis")) float.TryParse(options["zAxis"] as string, out _zAxis);
		}

		/// <summary>
		/// Fetchs the image resource from our servers.  BeforeLoad will be called before
		/// the request is made.  AfterLoad will be called once the image has been
		/// downloaded.
		/// </summary>
		/// <param name="image">Image.</param>
		public void LoadResource(ImageComposition image)
		{
			try {
				Image = image;
				if (BeforeLoad != null) 
				{
					BeforeLoad(this, new EventArgs());
				}

				_behaviour.LoadResource(image, () => {
					HasLoadedResource = true;
					if (AfterLoad != null)
                    {
                        AfterLoad(this, new EventArgs());
                    }	
				});

				if (image.Background != null) {
					AddAction(_behaviour.Background, image.Background);
				}
					
				if (image.Button1 != null) {
					AddAction(_behaviour.Button1, image.Button1);
				}
					
				if (image.Button2 != null) {
					AddAction(_behaviour.Button2, image.Button2);
				}
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Renders the popup to screen.  BeforeShow is called before the 
		/// popup is rendered.
		/// </summary>
		public void ShowPopup()
		{
			if (HasLoadedResource)
			{
				try {
					if (BeforeShow != null)
					{
						BeforeShow(this, new EventArgs());
					}
					_behaviour.ShowPopup(Image, _zAxis);
					IsShowingPopup = true;
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}
		}

		/// <summary>
		/// Closes the popup.  This is normally called by the Popup but is available for 
		/// other usecases.  BeforeClose is called before the Popop is closed.  AfterClose
		/// is called after the Popup has closed.
		/// </summary>
		public void ClosePopup()
		{
			if (IsShowingPopup)
			{
				if (BeforeClose != null)
				{
					BeforeClose(this, new EventArgs());
				}

				UnityEngine.Object.Destroy(_popup);
				IsShowingPopup = false;

	            if (AfterClose != null)
	            {
	                AfterClose(this, new EventArgs());
	            }
			}
		}

		/// <summary>
		/// Gets the background game object.  Use this reference to determine if it was selected.
		/// </summary>
		public GameObject Background { get { return _behaviour.Background; }}

		/// <summary>
		/// Gets the button1 game object.  Use this reference to determine if it was selected.
		/// </summary>
		public GameObject Button1 { get { return _behaviour.Button1; }}

		/// <summary>
		/// Get the button2 game object.  Use this reference to determine if it was selected.
		/// </summary>
		public GameObject Button2 { get { return _behaviour.Button2; }}

		private void AddAction(GameObject obj, ImageAsset asset) {
			PopupActionHandler action = obj.GetComponent<PopupActionHandler>();
			if (action != null) {

				PopupEventArgs eventArgs = new PopupEventArgs(obj, asset);

				switch (asset.Action) {
					case ImageAsset.ActionType.DISMISS: {
						action.OnMouseDownAction += () => {
							if (Dismiss != null)
							{
								Dismiss(this, eventArgs);
							}
							ClosePopup();
						};
						break;
					}
					case ImageAsset.ActionType.LINK: {
						action.OnMouseDownAction += () => {
							if (Action != null)
							{
								Action(this, eventArgs);
							}
							Application.OpenURL(asset.ActionParam);
							ClosePopup();
						};
						break;
					}
					case ImageAsset.ActionType.CUSTOM: {
						action.OnMouseDownAction += () => {
							if (Action != null)
							{
								Action(this, eventArgs);
							}
							ClosePopup();
						};
						break;
					}
				}
			}
		}
	}

	public class PopupBehaviour : MonoBehaviour
	{
		public GameObject Background { get; set; }
		public GameObject Button1 { get; set; }
		public GameObject Button2 { get; set; }

		private Texture2D _texture;

		void Awake () 
		{
			Background = new GameObject("Background");
			Background.AddComponent<PopupActionHandler>();
			Background.transform.parent = gameObject.transform;
			Button1 = new GameObject("Button1");
			Button1.AddComponent<PopupActionHandler>();
			Button1.transform.parent = gameObject.transform;
			Button2 = new GameObject("Button2");
			Button2.AddComponent<PopupActionHandler>();
			Button2.transform.parent = gameObject.transform;
		}

		public void LoadResource(ImageComposition image, Action callback)
		{
			StartCoroutine(LoadResourceCoroutine(image, callback));
		}

		public void ShowPopup(ImageComposition image, float zAxis)
		{
			Vector2 screen = new Vector2(Screen.width, Screen.height);
			Vector2 viewport = new Vector2(image.Viewport.Width, image.Viewport.Height);
				
			if (image.Background != null) {
				DrawAsset(Background, image.Background, _texture, screen, viewport, zAxis);
			}
				
			if (image.Button1 != null) {
				DrawAsset(Button1, image.Button1, _texture, screen, viewport, zAxis+1);
			}
				
			if (image.Button2 != null) {
				DrawAsset(Button2, image.Button2, _texture, screen, viewport, zAxis+1);
			}
		}

		private IEnumerator LoadResourceCoroutine(ImageComposition image, Action callback)
		{
			_texture = new Texture2D(image.SpriteMap.Width, image.SpriteMap.Height);
			WWW www = new WWW(image.SpriteMap.Url);

			yield return www;

			if (www.error == null) 
			{
				www.LoadImageIntoTexture(_texture);
				callback();
			}
			else 
			{
				throw new PopupException("Failed to load resource "+image.SpriteMap.Url+" "+www.error);
			}
		}

		private void DrawAsset(GameObject go, ImageAsset asset, Texture2D spriteMap, Vector2 screen, Vector2 viewport, float z)
		{
			Texture2D texture = CopySubRegion(spriteMap, asset.GlobalPosition);
			GUITexture gui = go.AddComponent<GUITexture>();
			gui.texture = texture;
			gui.border = new RectOffset(0, 0, 0, 0);
			// centre texture
			float width = asset.ImagePosition.width;
			float height = asset.ImagePosition.height;
			gui.pixelInset = new Rect(0.0f-width/2.0f, 0.0f-height/2.0f, width, height);

			// find middle of asset
			// find distance to middle of viewpoint
			// position assets scaled normalised distance from middle

			Vector2 tl = new Vector2(asset.ImagePosition.x, asset.ImagePosition.y);
			Vector2 br = new Vector2(asset.ImagePosition.width, asset.ImagePosition.height);
			Vector2 middle = tl + (br / 2.0f);
			Vector2 distFromMiddleVP = middle - viewport / 2.0f;
			Vector2 normalised = new Vector2(distFromMiddleVP.x / screen.x, distFromMiddleVP.y / screen.y );
			Vector2 translatedToGUI = new Vector2(normalised.x+0.5f, (-1.0f * normalised.y)+0.5f);
			go.transform.position = new Vector3(translatedToGUI.x, translatedToGUI.y, z);
			go.transform.localScale = Vector3.zero;
		}
			
		private Texture2D CopySubRegion(Texture2D texture, int x, int y, int width, int height)
		{
			Color[] pixels = texture.GetPixels(x, texture.height-y-height, width, height);
			Texture2D result = new Texture2D(width, height, texture.format, false);
			result.SetPixels(pixels);
			result.Apply();
			return result;
		}

		private Texture2D CopySubRegion(Texture2D texture, Rect rect)
		{
			return CopySubRegion(
				texture, 
				Mathf.FloorToInt(rect.x), 
				Mathf.FloorToInt(rect.y), 
				Mathf.FloorToInt(rect.width), 
				Mathf.FloorToInt(rect.height));
		}
	}

	public class PopupActionHandler : MonoBehaviour
	{
		public event Action OnMouseDownAction;

		public void OnMouseDown()
		{
            if (OnMouseDownAction != null)
            {
                OnMouseDownAction();
            }
		}
	}
}
