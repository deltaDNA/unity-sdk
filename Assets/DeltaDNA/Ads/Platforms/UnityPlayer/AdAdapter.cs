//
// Copyright (c) 2017 deltaDNA Ltd. All rights reserved.
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
using UnityEngine;
using UnityEngine.UI;

namespace DeltaDNA.Ads.UnityPlayer {

    #if UNITY_EDITOR
    internal class AdAdapter {

        private readonly bool rewarded;
        private readonly string name;
        private GameObject gameObject;
        
        internal int shownCount;

        internal AdAdapter(bool rewarded, int number) {
            this.rewarded = rewarded;
            name = "Ad Adapter " + number;
        }

        internal void Load(AdListener listener) {
            listener.OnLoaded(this);
        }

        internal void Show(AdListener listener) {
            gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<GraphicRaycaster>();
            gameObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<Ad>().Build(
                gameObject,
                rewarded,
                name,
                () => { Close(listener, false); },
                () => { Close(listener, true); });

            listener.OnShowing(this);
        }

        private void Close(AdListener listener, bool complete) {
            UnityEngine.Object.Destroy(gameObject);
            listener.OnClosed(this, complete);
        }

        private class Ad : MonoBehaviour {

            private GameObject parent;
            private bool rewarded;
            private string label;
            private Action closed;
            private Action closedWithReward;

            internal void Build(
                GameObject parent,
                bool rewarded,
                string label,
                Action closed,
                Action closedWithReward) {
                this.parent = parent;
                this.rewarded = rewarded;
                this.label = label;
                this.closed = closed;
                this.closedWithReward = closedWithReward;
            }

            void Start() {
                var background = new GameObject("Background", typeof(RectTransform));
                Position(background, new Rect(0, 0, Screen.width, Screen.height));
                background.AddComponent<Image>().color = Color.grey;

                var label = new GameObject("Label", typeof(Text));
                var labelText = label.GetComponent<Text>();
                labelText.text = this.label;
                labelText.color = Color.black;
                labelText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                labelText.resizeTextForBestFit = true;
                labelText.alignment = TextAnchor.MiddleCenter;
                Position(label, new Rect(Screen.width/4, Screen.height/8, Screen.width/2, Screen.height/4));

                var button1 = new GameObject("Close", typeof(RectTransform));
                button1.AddComponent<Image>();
                var button1Button = button1.AddComponent<Button>();
                button1Button.onClick.AddListener(() => closed.Invoke());
                var button1Text = new GameObject("Close Text", typeof(RectTransform));
                button1Text.transform.SetParent(button1.transform);
                var button1TextText = button1Text.AddComponent<Text>();
                button1TextText.text = "Close";
                button1TextText.color = Color.black;
                button1TextText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                button1TextText.resizeTextForBestFit = true;
                button1TextText.horizontalOverflow = HorizontalWrapMode.Overflow;
                button1TextText.alignment = TextAnchor.MiddleCenter;
                Position(button1, new Rect(Screen.width/3, Screen.height/8 * 4, Screen.width/3, Screen.height/6));

                if (!rewarded) return;
                var button2 = new GameObject("Close with Reward", typeof(RectTransform));
                button2.AddComponent<Image>();
                var button2Button = button2.AddComponent<Button>();
                button2Button.onClick.AddListener(() => closedWithReward.Invoke());
                var button2Text = new GameObject("Close with Reward Text", typeof(RectTransform));
                button2Text.transform.SetParent(button2.transform);
                var button2TextText = button2Text.AddComponent<Text>();
                button2TextText.text = "Close with reward";
                button2TextText.color = Color.black;
                button2TextText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                button2TextText.resizeTextForBestFit = true;
                button2TextText.horizontalOverflow = HorizontalWrapMode.Overflow;
                button2TextText.alignment = TextAnchor.MiddleCenter;
                Position(button2, new Rect(Screen.width/3, Screen.height/8 * 6, Screen.width/3, Screen.height/6));
            }

            private void Position(GameObject obj, Rect position) {
                obj.transform.SetParent(parent.transform);

                var widthRatio = 1 / (float)Screen.width;
                var heightRatio = 1 / (float)Screen.height;

                var neededX = position.x * widthRatio;
                var neededY = position.y * heightRatio;
                var neededWidth = position.width * widthRatio;
                var neededHeight = position.height * heightRatio;

                obj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0f, 0f, 0f);
                obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);

                obj.GetComponent<RectTransform>().anchorMin = new Vector2(neededX, 1 - neededY - neededHeight);
                obj.GetComponent<RectTransform>().anchorMax = new Vector2(neededX + neededWidth, 1 - neededY);
            }
        }
    }
    #endif
}
