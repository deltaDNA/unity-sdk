using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA.Messaging
{

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

		public Popup() : this(new Dictionary<string, object>())
		{

		}

		public Popup(Dictionary<string, object> options)
		{
			string name = (options.ContainsKey("name")) ? options["name"] as string : "Popup";
			_popup = new GameObject(name);
			_behaviour = _popup.AddComponent<PopupBehaviour>();
		}

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

		public void ShowPopup()
		{
			if (HasLoadedResource)
			{
				try {
					if (BeforeShow != null)
					{
						BeforeShow(this, new EventArgs());
					}
					_behaviour.ShowPopup(Image);
					IsShowingPopup = true;
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}
		}

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

		public GameObject Background { get { return _behaviour.Background; }}

		public GameObject Button1 { get { return _behaviour.Button1; }}

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

		public void ShowPopup(ImageComposition image)
		{
			Vector2 screen = new Vector2(Screen.width, Screen.height);
			Vector2 viewport = new Vector2(image.Viewport.Width, image.Viewport.Height);
				
			if (image.Background != null) {
				DrawAsset(Background, image.Background, _texture, screen, viewport, 0);
			}
				
			if (image.Button1 != null) {
				DrawAsset(Button1, image.Button1, _texture, screen, viewport, 1);
			}
				
			if (image.Button2 != null) {
				DrawAsset(Button2, image.Button2, _texture, screen, viewport, 1);
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
