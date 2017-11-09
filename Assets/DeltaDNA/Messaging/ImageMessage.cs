//
// Copyright (c) 2016 deltaDNA Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_5_6_OR_NEWER
using UnityEngine.Networking;
#endif

namespace DeltaDNA {

    using JSONObject = System.Collections.Generic.Dictionary<string, object>;

    public class ImageMessage {

        public event Action OnDidReceiveResources;
        public event Action<string> OnDidFailToReceiveResources;
        public event Action<EventArgs> OnDismiss;
        public event Action<EventArgs> OnAction;

        private JSONObject configuration;
        private GameObject gameObject;
        private SpriteMap spriteMap;

        private int depth;
        private bool resourcesLoaded = false;
        private bool showing = false;
        private Engagement engagement;

        public class EventArgs: System.EventArgs
        {
            public EventArgs(string id, string type, string value)
            {
                this.ID = id;
                this.ActionType = type;
                this.ActionValue = value;
            }

            public string ID { get; set; }
            public string ActionType { get; set; }
            public string ActionValue { get; set; }
        }

        private ImageMessage(JSONObject configuration, string name, int depth, Engagement engagement)
        {
            gameObject = new GameObject(name, typeof(RectTransform));
            SpriteMap spriteMap = gameObject.AddComponent<SpriteMap>();
            spriteMap.Build(configuration);

            this.configuration = configuration;
            this.spriteMap = spriteMap;
            this.depth = depth;
            this.engagement = engagement;
        }

        public static ImageMessage Create(Engagement engagement)
        {
            return Create(engagement, null);
        }

        public static ImageMessage Create(Engagement engagement, JSONObject options)
        {
            if (engagement == null || engagement.JSON == null || !engagement.JSON.ContainsKey("image")) return null;

            string name = "DeltaDNA Image Message";
            int depth = 0;

            if (options != null) {
                if (options.ContainsKey("name")) {
                    name = options["name"] as string;
                }
                if (options.ContainsKey("depth")) {
                    depth = (int)options["depth"];
                }
            }

            ImageMessage imageMessage = null;

            try {
                var configuration = engagement.JSON["image"] as JSONObject;
                if (ValidConfiguration(configuration)) {
                    imageMessage = new ImageMessage(configuration, name, depth, engagement);
                    if (engagement.JSON.ContainsKey("parameters")) {
                        imageMessage.Parameters = engagement.JSON["parameters"] as JSONObject;
                    }
                } else {
                    Logger.LogWarning("Invalid image message configuration.");
                }
            } catch (Exception exception) {
                Logger.LogWarning("Failed to create image message: "+exception.Message);
            }

            return imageMessage;
        }

        private static bool ValidConfiguration(JSONObject c)
        {
            if (!c.ContainsKey("url") ||
                !c.ContainsKey("height") ||
                !c.ContainsKey("width") ||
                !c.ContainsKey("spritemap") ||
                !c.ContainsKey("layout")) return false;

            JSONObject layout = c["layout"] as JSONObject;

            if (!layout.ContainsKey("landscape") && !layout.ContainsKey("portrait")) return false;

            JSONObject spritemap = c["spritemap"] as JSONObject;

            if (!spritemap.ContainsKey("background")) return false;

            return true;
        }

        public void FetchResources()
        {
            this.spriteMap.LoadResource((error) => {
                if (error == null) {
                    this.resourcesLoaded = true;
                    if (this.OnDidReceiveResources != null) {
                        this.OnDidReceiveResources();
                    }
                } else {
                    if (this.OnDidFailToReceiveResources != null) {
                        this.OnDidFailToReceiveResources(error);
                    }
                }
            });
        }

        public bool IsReady()
        {
            return this.resourcesLoaded;
        }

        public void Show()
        {
            if (this.resourcesLoaded) {
                try {
                    gameObject.AddComponent<Canvas>();
                    gameObject.AddComponent<GraphicRaycaster>();
                    gameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

                    if (this.configuration.ContainsKey("shim")) {
                        ShimLayer shimLayer = gameObject.AddComponent<ShimLayer>();
                        shimLayer.Build(gameObject, this, this.configuration["shim"] as JSONObject, this.depth);
                    }

                    JSONObject layout = this.configuration["layout"] as JSONObject;
                    object orientation;
                    if (!layout.TryGetValue("landscape", out orientation) && !layout.TryGetValue("portrait", out orientation)) {
                        throw new KeyNotFoundException("Layout missing orientation key.");
                    }

                    BackgroundLayer backgroundLayer = gameObject.AddComponent<BackgroundLayer>();
                    backgroundLayer.Build(gameObject, this, orientation as JSONObject, this.spriteMap.Background, this.depth-1);

                    ButtonsLayer buttonLayer = gameObject.AddComponent<ButtonsLayer>();
                    buttonLayer.Build(gameObject, this, orientation as JSONObject, this.spriteMap.Buttons, backgroundLayer, this.depth-2);

                    showing = true;
                } catch (KeyNotFoundException exception) {
                    Logger.LogWarning("Failed to show image message, invalid format: "+exception.Message);
                } catch (Exception exception) {
                    Logger.LogWarning("Failed to show image message: "+exception.Message);
                }
            }
        }

        public bool IsShowing() {
            return this.showing;
        }

        public void Close() {
            if (showing) {
                UnityEngine.Object.Destroy(gameObject);
                showing = false;
            }
        }

        public JSONObject Parameters { get; private set; }

        private class SpriteMap : MonoBehaviour
        {
            private JSONObject configuration;
            private Texture2D texture;

            public string URL { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }

            public void Build(JSONObject configuration)
            {
                try {
                    this.URL = configuration["url"] as string;
                    this.Width = (int)((long)configuration["width"]);
                    this.Height = (int)((long)configuration["height"]);
                    this.configuration = configuration["spritemap"] as JSONObject;

                } catch (KeyNotFoundException exception) {
                    Logger.LogError("Invalid format: "+exception.Message);
                }
            }

            public void LoadResource(Action<string> callback)
            {
                #if !UNITY_2017_1_OR_NEWER
                this.texture = new Texture2D(this.Width, this.Height);
                #endif
                StartCoroutine(LoadResourceCoroutine(this.URL, callback));
            }

            public Texture Texture {
                get {
                    return this.texture;
                }
            }

            public Texture Background {
                get {
                    try {
                        JSONObject background = this.configuration["background"] as JSONObject;
                        int x = (int)((long)background["x"]);
                        int y = (int)((long)background["y"]);
                        int w = (int)((long)background["width"]);
                        int h = (int)((long)background["height"]);
                        return this.GetSubRegion(x, y, w, h);
                    } catch (KeyNotFoundException exception) {
                        Logger.LogError("Invalid format, background not found: "+exception.Message);
                    }
                    return null;
                }
            }

            public List<Texture> Buttons {
                get {
                    List<Texture> textures = new List<Texture>();
                    if (this.configuration.ContainsKey("buttons")) {
                        try {
                            var buttons = this.configuration["buttons"] as List<object>;
                            foreach (var button in buttons) {
                                int x = (int)((long)((JSONObject)button)["x"]);
                                int y = (int)((long)((JSONObject)button)["y"]);
                                int w = (int)((long)((JSONObject)button)["width"]);
                                int h = (int)((long)((JSONObject)button)["height"]);
                                textures.Add(this.GetSubRegion(x, y, w, h));
                            }
                        } catch (KeyNotFoundException exception) {
                            Logger.LogError("Invalid format, button not found: "+exception.Message);
                        }
                    }
                    return textures;
                }
            }

            public Texture2D GetSubRegion(int x, int y, int width, int height)
            {
                Color[] pixels = texture.GetPixels(x, texture.height-y-height, width, height);
                Texture2D result = new Texture2D(width, height, texture.format, false);
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

            private IEnumerator LoadResourceCoroutine(string url, Action<string> callback)
            {
                #if UNITY_2017_1_OR_NEWER
                using (var www = UnityWebRequestTexture.GetTexture(url))
                {
                    #if UNITY_2017_2_OR_NEWER
                    yield return www.SendWebRequest();
                    #else
                    yield return www.Send();
                    #endif

                    if (www.isNetworkError || www.isHttpError) {
                        Logger.LogWarning("Failed to load resource "+url+" "+www.error);
                    } else {
                        this.texture = DownloadHandlerTexture.GetContent(www);
                    }

                    callback(www.error);
                }

                #else
                WWW www = new WWW(url);

                yield return www;

                if (www.error == null) {
                    www.LoadImageIntoTexture(texture);
                } else {
                    Logger.LogWarning("Failed to load resource "+url+" "+www.error);
                }

                callback(www.error);
                #endif
            }

        }

        private class Layer : MonoBehaviour {

            protected GameObject parent;
            protected ImageMessage imageMessage;
            protected List<Action> actions = new List<Action>();
            protected int depth = 0;

            protected void RegisterAction()
            {
                actions.Add(() => {});
            }

            protected void RegisterAction(JSONObject action, string id)
            {
                object typeObj, valueObj;
                action.TryGetValue("value", out valueObj);

                if (action.TryGetValue("type", out typeObj)) {

                    ImageMessage.EventArgs eventArgs = new ImageMessage.EventArgs(id, (string)typeObj, (string)valueObj);

                    GameEvent actionEvent = new GameEvent("imageMessageAction");
                    if (imageMessage.engagement.JSON.ContainsKey("eventParams")) {
                        var eventParams = imageMessage.engagement.JSON["eventParams"] as Dictionary<string, object>;
                        actionEvent.AddParam("responseDecisionpointName", eventParams["responseDecisionpointName"]);
                        actionEvent.AddParam("responseEngagementID", eventParams["responseEngagementID"]);
                        actionEvent.AddParam("responseEngagementName", eventParams["responseEngagementName"]);
                        actionEvent.AddParam("responseEngagementType", eventParams["responseEngagementType"]);
                        actionEvent.AddParam("responseMessageSequence", eventParams["responseMessageSequence"]);
                        actionEvent.AddParam("responseVariantName", eventParams["responseVariantName"]);
                        actionEvent.AddParam("responseTransactionID", eventParams["responseTransactionID"]);
                    }

                    actionEvent.AddParam("imActionName", id);
                    actionEvent.AddParam("imActionType", (string)typeObj);
                    if (!String.IsNullOrEmpty((string)valueObj) && (string)typeObj != "dismiss") {
                        actionEvent.AddParam("imActionValue", (string)valueObj);
                    }

                    switch ((string)typeObj) {
                        case "none": {
                            actions.Add(() => {});
                            break;
                        }
                        case "action": {
                            actions.Add(() => {
                                if (valueObj != null) {
                                    if (imageMessage.OnAction != null) {
                                        imageMessage.OnAction(eventArgs);
                                    }
                                }
                                DDNA.Instance.RecordEvent(actionEvent);
                                imageMessage.Close();
                            });
                            break;
                        }
                        case "link": {
                            actions.Add(() => {
                                if (imageMessage.OnAction != null) {
                                    imageMessage.OnAction(eventArgs);
                                }
                                if (valueObj != null) {
                                    Application.OpenURL((string)valueObj);
                                }
                                DDNA.Instance.RecordEvent(actionEvent);
                                imageMessage.Close();
                            });
                            break;
                        }
                        default : { // "dismiss"
                            actions.Add(() => {
                                if (imageMessage.OnDismiss != null) {
                                    imageMessage.OnDismiss(eventArgs);
                                }
                                DDNA.Instance.RecordEvent(actionEvent);
                                imageMessage.Close();
                            });
                            break;
                        }
                    }
                }
            }

            protected void PositionObject(GameObject obj, Rect position) {
                obj.transform.SetParent(parent.transform);

                var widthRatio = 1 / (float) Screen.width;
                var heightRatio = 1 / (float) Screen.height;

                var neededX = position.x * widthRatio;
                var neededY = position.y * heightRatio;
                var neededWidth = position.width * widthRatio;
                var neededHeight = position.height * heightRatio;

                obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0f, 0f, 0f);
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);

                // our coordinates go from top-left to bottom-right (render-like),
                // as opposed to bottom-left to top-right (graph-like) in unity
                obj.GetComponent<RectTransform>().anchorMin = new Vector2(neededX, 1 - neededY - neededHeight);
                obj.GetComponent<RectTransform>().anchorMax = new Vector2(neededX + neededWidth, 1 - neededY);
            }
        }

        private class ShimLayer : Layer
        {
            private Texture2D texture;
            private readonly byte dimmedMaskAlpha = 128;

            public void Build(GameObject parent, ImageMessage imageMessage, JSONObject config, int depth)
            {
                this.parent = parent;
                this.imageMessage = imageMessage;
                this.depth = depth;

                object mask;
                if (config.TryGetValue("mask", out mask)) {
                    bool show = true;
                    Color32[] colours = new Color32[1];
                    switch ((string)mask)
                    {
                        case "dimmed": {
                            colours[0] = new Color32(0, 0, 0, this.dimmedMaskAlpha);
                            break;
                        }
                        case "clear": {
                            colours[0] = new Color32(0, 0, 0, 0);
                            break;
                        }
                        default: {  // "none"
                            show = false;
                            break;
                        }
                    }
                    if (show) {
                        this.texture = new Texture2D(1, 1);
                        this.texture.SetPixels32(colours);
                        this.texture.Apply();
                    }
                }

                object actionObj;
                if (config.TryGetValue("action", out actionObj)) {
                    RegisterAction((JSONObject)actionObj, "shim");
                }
                else {
                    RegisterAction();
                }
            }

            void Start() {
                if (texture) {
                    var obj = new GameObject("Shim", typeof(RectTransform));
                    PositionObject(obj, new Rect(0, 0, Screen.width, Screen.height));

                    obj.AddComponent<Button>();
                    obj.AddComponent<Image>();

                    obj.GetComponent<Button>().onClick.AddListener(() => {
                        if (actions.Count > 0) actions[0].Invoke();
                    });
                    obj.GetComponent<Image>().sprite = Sprite.Create(
                        texture as Texture2D,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                }
            }
        }

        private class BackgroundLayer : Layer
        {

            private Texture texture;
            private Rect position;
            private float scale;

            public void Build(GameObject parent, ImageMessage imageMessage, JSONObject layout, Texture texture, int depth)
            {
                this.parent = parent;
                this.imageMessage = imageMessage;
                this.texture = texture;
                this.depth = depth;

                object backgroundObj;
                if (layout.TryGetValue("background", out backgroundObj)) {
                    var background = backgroundObj as JSONObject;

                    object actionObj;
                    if ((background).TryGetValue("action", out actionObj)) {
                        RegisterAction((JSONObject)actionObj, "background");
                    }
                    else {
                        RegisterAction();
                    }

                    object rulesObj;
                    if (background.TryGetValue("cover", out rulesObj)) {
                        this.position = RenderAsCover((JSONObject)rulesObj);
                    }
                    else if (background.TryGetValue("contain", out rulesObj)) {
                        this.position = RenderAsContain((JSONObject)rulesObj);
                    }
                    else {
                        Logger.LogError("Invalid layout");
                    }
                }
                else {
                    RegisterAction();
                }
            }

            public Rect Position { get { return this.position; }}

            public float Scale { get { return this.scale; }}

            void Start() {
                if (texture) {
                    var obj = new GameObject("Background", typeof(RectTransform));
                    PositionObject(obj, position);

                    obj.AddComponent<Button>();
                    obj.AddComponent<Image>();

                    obj.GetComponent<Button>().onClick.AddListener(() => {
                        if (actions.Count > 0) actions[0].Invoke();
                    });
                    obj.GetComponent<Image>().sprite = Sprite.Create(
                        texture as Texture2D,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                }
            }

            private Rect RenderAsCover(JSONObject rules)
            {
                this.scale = Math.Max((float)Screen.width / (float)this.texture.width, (float)Screen.height / (float)this.texture.height);
                float width = this.texture.width * this.scale;
                float height = this.texture.height * this.scale;

                float top = Screen.height / 2.0f - height / 2.0f;   // default "center"
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

            private Rect RenderAsContain(JSONObject rules)
            {
                float lc = 0, rc = 0, tc = 0, bc = 0;
                object l, r, t, b;
                if (rules.TryGetValue("left", out l)) {
                    lc = GetConstraintPixels((string)l, Screen.width);
                }
                if (rules.TryGetValue("right", out r)) {
                    rc = GetConstraintPixels((string)r, Screen.width);
                }

                float ws = ((float)Screen.width - lc - rc) / (float)this.texture.width;

                if (rules.TryGetValue("top", out t)) {
                    tc = GetConstraintPixels((string)t, Screen.height);
                }
                if (rules.TryGetValue("bottom", out b)) {
                    bc = GetConstraintPixels((string)b, Screen.height);
                }

                float hs = ((float)Screen.height - tc - bc) / (float)this.texture.height;

                this.scale = Math.Min(ws, hs);
                float width = this.texture.width * this.scale;
                float height = this.texture.height * this.scale;

                float top = ((Screen.height - tc - bc) / 2.0f - height / 2.0f) + tc;    // default "center"
                float left = ((Screen.width - lc - rc) / 2.0f - width / 2.0f) + lc;     // default "center"

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

        private class ButtonsLayer : Layer
        {
            private List<Texture> textures = new List<Texture>();
            private List<Rect> positions = new List<Rect>();

            public void Build(GameObject parent, ImageMessage imageMessage, JSONObject orientation, List<Texture> textures, BackgroundLayer content, int depth)
            {
                this.parent = parent;
                this.imageMessage = imageMessage;
                this.depth = depth;

                object buttonsObj;
                if (orientation.TryGetValue("buttons", out buttonsObj)) {
                    var buttons = buttonsObj as List<object>;
                    for (int i = 0; i < buttons.Count; ++i) {
                        var button = buttons[i] as JSONObject;
                        float left = 0, top = 0;
                        object x, y;
                        if (button.TryGetValue("x", out x)) {
                            left = (int)((long)x) * content.Scale + content.Position.xMin;
                        }
                        if (button.TryGetValue("y", out y)) {
                            top = (int)((long)y) * content.Scale + content.Position.yMin;
                        }
                        this.positions.Add(new Rect(left, top, textures[i].width * content.Scale, textures[i].height * content.Scale));

                        object actionObj;
                        if (button.TryGetValue("action", out actionObj)) {
                            RegisterAction((JSONObject)actionObj, "button"+(i+1));
                        }
                        else {
                            RegisterAction();
                        }
                    }
                    this.textures = textures;
                }
            }

            void Start() {
                for (int i = 0; i < textures.Count; ++i) {
                    var obj = new GameObject("Button", typeof(RectTransform));
                    PositionObject(obj, positions[i]);

                    obj.AddComponent<Button>();
                    obj.AddComponent<Image>();

                    var action = actions[i];
                    obj.GetComponent<Button>().onClick.AddListener(() => {
                        action.Invoke();
                    });
                    obj.GetComponent<Image>().sprite = Sprite.Create(
                        textures[i] as Texture2D,
                        new Rect(0, 0, textures[i].width, textures[i].height),
                        new Vector2(0.5f, 0.5f));
                }
            }
        }
    }
}
