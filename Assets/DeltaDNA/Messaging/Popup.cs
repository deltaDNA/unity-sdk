using System;
using UnityEngine;
using System.Collections;

namespace DeltaDNA.Messaging
{
	public class Popup : MonoBehaviour {

		public delegate void Action();
		public event Action BeforeShow;
		public event Action AfterShow;

		private ImageComposition _image;

		private GameObject _background;
		private GameObject _button1;
		private GameObject _button2;

		public PopupActionHandler Background 
		{
			get { return _background.GetComponent<PopupActionHandler>(); }
		}

		public PopupActionHandler Button1 
		{
			get { return _button1.GetComponent<PopupActionHandler>(); }
		}

		public PopupActionHandler Button2 
		{
			get { return _button2.GetComponent<PopupActionHandler>(); }
		}

		public bool HasLoadedSpriteMap { get; private set; }

		public void InitAndRun(ImageComposition image)
		{
			_image = image;

			Texture2D spriteMap = new Texture2D(image.SpriteMap.Width, image.SpriteMap.Height);
			WWW www = new WWW(image.SpriteMap.Url);
			StartCoroutine(LoadImageIntoTexture(spriteMap, www));
		}

		void Awake () 
		{
			_background = new GameObject("Background");
			_background.AddComponent<PopupActionHandler>();
			_background.transform.parent = gameObject.transform;
			_button1 = new GameObject("Button1");
			_button1.AddComponent<PopupActionHandler>();
			_button1.transform.parent = gameObject.transform;
			_button2 = new GameObject("Button2");
			_button2.AddComponent<PopupActionHandler>();
			_button2.transform.parent = gameObject.transform;
		}

		protected IEnumerator LoadImageIntoTexture(Texture2D texture, WWW www)
		{
			yield return www;

			try {
				if (www.error == null) {
					www.LoadImageIntoTexture(texture);
					HasLoadedSpriteMap = true;

					if (BeforeShow != null) BeforeShow();

					// Background
					if (_image.Background != null) {
						DrawAsset(_background, _image.Background, texture, 0);
						AddAction(_background, _image.Background);
					}

					// Button 1
					if (_image.Button1 != null) {
						DrawAsset(_button1, _image.Button1, texture, 1);
						AddAction(_button1, _image.Button1);
					}

					// Button 2
					if (_image.Button2 != null) {
						DrawAsset(_button2, _image.Button2, texture, 1);
						AddAction(_button2, _image.Button2);
					}
				}
			} catch (Exception) {

			}
		}

		protected void DrawAsset(GameObject go, ImageAsset asset, Texture2D spriteMap, float z)
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
			Vector2 viewport = new Vector2(_image.Viewport.Width, _image.Viewport.Height);
			Vector2 tl = new Vector2(asset.ImagePosition.x, asset.ImagePosition.y);
			Vector2 br = new Vector2(asset.ImagePosition.width, asset.ImagePosition.height);
			Vector2 middle = tl + (br / 2.0f);
			Vector2 distFromMiddleVP = middle - viewport / 2.0f;
			Vector2 normalised = new Vector2(distFromMiddleVP.x / screen.x, distFromMiddleVP.y / screen.y );
			Vector2 translatedToGUI = new Vector2(normalised.x+0.5f, (-1.0f * normalised.y)+0.5f);
			go.transform.position = new Vector3(translatedToGUI.x, translatedToGUI.y, z);
			go.transform.localScale = Vector3.zero;
		}
			
		protected Texture2D CopySubRegion(Texture2D texture, int x, int y, int width, int height)
		{
			Color[] pixels = texture.GetPixels(x, texture.height-y-height, width, height);
			Texture2D result = new Texture2D(width, height, texture.format, false);
			result.SetPixels(pixels);
			result.Apply();
			return result;
		}

		protected Texture2D CopySubRegion(Texture2D texture, Rect rect)
		{
			return CopySubRegion(
				texture, 
				Mathf.FloorToInt(rect.x), 
				Mathf.FloorToInt(rect.y), 
				Mathf.FloorToInt(rect.width), 
				Mathf.FloorToInt(rect.height));
		}

		protected void AddAction(GameObject obj, ImageAsset asset) {
			PopupActionHandler action = obj.GetComponent<PopupActionHandler>();
			if (action != null) {

				switch (asset.Action) {
					case ImageAsset.ActionType.DISMISS: {
						action.OnMouseDownAction += () => {
							ClosePopup();
						};
						break;
					}
					case ImageAsset.ActionType.LINK: {
						action.OnMouseDownAction += () => {
							Application.OpenURL(asset.ActionParam);
							ClosePopup();
						};
						break;
					}
					case ImageAsset.ActionType.CUSTOM: {
						// TODO: need access to a table of predefined functions
						ClosePopup();
						break;
					}
				}
			}
		}

		protected void ClosePopup()
		{
			Destroy(gameObject);
			if (AfterShow != null) AfterShow();
		}
	}

	public class PopupActionHandler : MonoBehaviour
	{
		public delegate void Action();
		public event Action OnMouseDownAction;

		public void OnMouseDown()
		{
			if (OnMouseDownAction != null) OnMouseDownAction();
		}
	}
}
