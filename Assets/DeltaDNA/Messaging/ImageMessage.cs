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

        private ImageMessage(JSONObject configuration, string name, int depth)
        {
            GameObject gameObject = new GameObject(name);
            SpriteMap spriteMap = gameObject.AddComponent<SpriteMap>();
            spriteMap.Build(configuration);

            this.configuration = configuration;
            this.gameObject = gameObject;
            this.spriteMap = spriteMap;
            this.depth = depth;
        }

        public static ImageMessage Create(Engagement engagement)
        {
            return Create(engagement, null);
        }

        public static ImageMessage Create(Engagement engagement, JSONObject options)
        {
            if (engagement == null || engagement.JSON == null || !engagement.JSON.ContainsKey("image")) return null;

            string name = "Image Message";
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
                    imageMessage = new ImageMessage(configuration, name, depth);
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
                    if (this.configuration.ContainsKey("shim")) {
                        ShimLayer shimLayer = this.gameObject.AddComponent<ShimLayer>();
                        shimLayer.Build(this, this.configuration["shim"] as JSONObject, this.depth);
                    }

                    JSONObject layout = this.configuration["layout"] as JSONObject;
                    object orientation;
                    if (!layout.TryGetValue("landscape", out orientation) && !layout.TryGetValue("portrait", out orientation)) {
                        throw new KeyNotFoundException("Layout missing orientation key.");
                    }

                    BackgroundLayer backgroundLayer = this.gameObject.AddComponent<BackgroundLayer>();
                    backgroundLayer.Build(this, orientation as JSONObject, this.spriteMap.Background, this.depth-1);

                    ButtonsLayer buttonLayer = this.gameObject.AddComponent<ButtonsLayer>();
                    buttonLayer.Build(this, orientation as JSONObject, this.spriteMap.Buttons, backgroundLayer, this.depth-2);

                    this.showing = true;

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
            if (this.showing) {
                foreach (var layer in this.gameObject.GetComponents<Layer>()) {
                    UnityEngine.Object.Destroy(layer);
                }
                this.showing = false;
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
                this.texture = new Texture2D(this.Width, this.Height);
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
                WWW www = new WWW(url);

                yield return www;

                if (www.error == null) {
                    www.LoadImageIntoTexture(texture);
                } else {
                    Logger.LogWarning("Failed to load resource "+url+" "+www.error);
                }

                callback(www.error);
            }

        }

        private class Layer : MonoBehaviour
        {
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
                                imageMessage.Close();
                            });
                            break;
                        }
                        default : { // "dismiss"
                            actions.Add(() => {
                                if (imageMessage.OnDismiss != null) {
                                    imageMessage.OnDismiss(eventArgs);
                                }
                                imageMessage.Close();
                            });
                            break;
                        }
                    }
                }
            }
        }

        private class ShimLayer : Layer
        {
            private Texture2D texture;
            private readonly byte dimmedMaskAlpha = 128;

            public void Build(ImageMessage imageMessage, JSONObject config, int depth)
            {
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

            public void OnGUI()
            {
                GUI.depth = this.depth;

                if (this.texture)
                {
                    Rect position = new Rect(0, 0, Screen.width, Screen.height);
                    GUI.DrawTexture(position, this.texture);
                    if (GUI.Button(position, "", GUIStyle.none)) {
                        if (this.actions.Count > 0) this.actions[0].Invoke();
                    }
                }
            }
        }

        private class BackgroundLayer : Layer
        {
            private Texture texture;
            private Rect position;
            private float scale;

            public void Build(ImageMessage imageMessage, JSONObject layout, Texture texture, int depth)
            {
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

            public void OnGUI()
            {
                GUI.depth = this.depth;

                if (this.texture)
                {
                    GUI.DrawTexture(this.position, this.texture);
                    if (GUI.Button(this.position, "", GUIStyle.none)) {
                        if (this.actions.Count > 0) this.actions[0].Invoke();
                    }
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

            public void Build(ImageMessage imageMessage, JSONObject orientation, List<Texture> textures, BackgroundLayer content, int depth)
            {
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

            public void OnGUI()
            {
                GUI.depth = this.depth;

                for (int i = 0; i < this.textures.Count; ++i)
                {
                    GUI.DrawTexture(this.positions[i], this.textures[i]);
                    if (GUI.Button(this.positions[i], "", GUIStyle.none)) {
                        this.actions[i].Invoke();
                    }
                }
            }
        }

    }
}
