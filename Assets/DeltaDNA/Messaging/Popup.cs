using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DeltaDNA.Messaging
{
	public class PopupEventArgs: EventArgs
	{
		public PopupEventArgs(GameObject gameObject, ImageAsset imageAsset)
		{
			this.GameObject = gameObject;
			this.ImageAsset = imageAsset;
		}

		public GameObject GameObject { get; set; }
		public ImageAsset ImageAsset { get; set; }
	}

	public interface IPopup
	{
		event EventHandler BeforeLoad;
		event EventHandler AfterLoad;
		event EventHandler BeforeShow;
		event EventHandler BeforeClose;
		event EventHandler AfterClose;
		event EventHandler<PopupEventArgs> Dismiss;
		event EventHandler<PopupEventArgs> Action;

		void LoadResource(ImageComposition image);
		void ShowPopup(Dictionary<string, object> options = null);
	}

	public class Popup : MonoBehaviour, IPopup 
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

		//public GameObject Background { get; private set; }
		//public GameObject Button1 { get; private set; }
		//public GameObject Button2 { get; private set; }
		private GameObject Background;
		private GameObject Button1;
		private GameObject Button2;

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

		public void LoadResource(ImageComposition image)
		{
			if (BeforeLoad != null) 
			{
				BeforeLoad(this, new EventArgs());
			}

			StartCoroutine(LoadResourceCoroutine(image));
		}

		public void ShowPopup(Dictionary<string, object> options = null)
		{
			if (HasLoadedResource) 
			{
				try {
					if (BeforeShow != null)
					{
						BeforeShow(this, new EventArgs());
					}

					// Background
					if (Image.Background != null) {
						DrawAsset(Background, Image.Background, _texture, 0);
						AddAction(Background, Image.Background);
					}

					// Button 1
					if (Image.Button1 != null) {
						DrawAsset(Button1, Image.Button1, _texture, 1);
						AddAction(Button1, Image.Button1);
					}

					// Button 2
					if (Image.Button2 != null) {
						DrawAsset(Button2, Image.Button2, _texture, 1);
						AddAction(Button2, Image.Button2);
					}
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}
		}

		private IEnumerator LoadResourceCoroutine(ImageComposition image)
		{
			_texture = new Texture2D(image.SpriteMap.Width, image.SpriteMap.Height);
			WWW www = new WWW(image.SpriteMap.Url);

			yield return www;

			try {
				if (www.error == null) {
					www.LoadImageIntoTexture(_texture);
					HasLoadedResource = true;

                    if (AfterLoad != null)
                    {
                        AfterLoad(this, new EventArgs());
                    }			
				}
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}

		private void DrawAsset(GameObject go, ImageAsset asset, Texture2D spriteMap, float z)
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
			Vector2 screen = new Vector2(Screen.width, Screen.height);
			Vector2 viewport = new Vector2(Image.Viewport.Width, Image.Viewport.Height);
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

		private void ClosePopup()
		{
			if (BeforeClose != null)
			{
				BeforeClose(this, new EventArgs());
			}

			Destroy(gameObject);

            if (AfterClose != null)
            {
                AfterClose(this, new EventArgs());
            }
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
