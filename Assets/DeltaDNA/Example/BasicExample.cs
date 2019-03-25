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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DeltaDNA {

public class BasicExample : MonoBehaviour {

    [SerializeField]
    private Transform cubeObj;
    [SerializeField]
    private GameObject popUpObj;
    [SerializeField]
    private Text popUpContent;
    [SerializeField]
    private Text popUpTitle;

    // Use this for initialization
    void Start () {
        // Configure the SDK
        DDNA.Instance.SetLoggingLevel(Logger.Level.DEBUG);

        // Enable push notifications
        #if !DDNA_IOS_PUSH_NOTIFICATIONS_REMOVED
        DDNA.Instance.IosNotifications.OnDidRegisterForPushNotifications += (string n) => {
            Debug.Log("Got an iOS push token: " + n);
        };
        DDNA.Instance.IosNotifications.OnDidReceivePushNotification += (string n) => {
            Debug.Log("Got an iOS push notification! " + n);
        };
        DDNA.Instance.IosNotifications.OnDidLaunchWithPushNotification += (string n) => {
            Debug.Log("Launched with an iOS push notification: " + n);
        };
        DDNA.Instance.IosNotifications.RegisterForPushNotifications();
        #endif

        DDNA.Instance.AndroidNotifications.OnDidRegisterForPushNotifications += (string n) => {
            Debug.Log("Got an Android registration token: " + n);
        };
        DDNA.Instance.AndroidNotifications.OnDidFailToRegisterForPushNotifications += (string n) => {
            Debug.Log("Failed getting an Android registration token: " + n);
        };
        DDNA.Instance.AndroidNotifications.OnDidReceivePushNotification += (string n) => {
            Debug.Log("Got an Android push notification: " + n);
        };
        DDNA.Instance.AndroidNotifications.OnDidLaunchWithPushNotification += (string n) => {
            Debug.Log("Launched with an Android push notification: " + n);
        };
        DDNA.Instance.AndroidNotifications.RegisterForPushNotifications();

        //Register default handlers for event triggered campaigns. These will be candidates for handling ANY Event-Triggered Campaigns. 
        //Any handlers added to RecordEvent() calls with the .Add method will be evaluated before these default handlers. 
        DDNA.Instance.Settings.DefaultImageMessageHandler =
            new ImageMessageHandler(DDNA.Instance, imageMessage =>{
                // the image message is already prepared so it will show instantly
                imageMessage.Show();
            });
        DDNA.Instance.Settings.DefaultGameParameterHandler = new GameParametersHandler(gameParameters =>{
            // do something with the game parameters
            Logger.LogInfo("Received game parameters from event trigger: " + gameParameters);
        });
        // Start the SDK. We recommend using the configuration UI for setting your game's
        // keys and calling StartSDK() or StartSDK(userID) instead.
        DDNA.Instance.StartSDK(new Configuration() {
            environmentKeyDev = "76410301326725846610230818914037",
            environmentKey = 0,
            collectUrl = "https://collect2470ntysd.deltadna.net/collect/api",
            engageUrl = "https://engage2470ntysd.deltadna.net",
            useApplicationVersion = true
        });
    }

    void FixedUpdate() {
        // Make our cube rotate
        if (DDNA.Instance.HasStarted)
        {
            cubeObj.Rotate(new Vector3(15, 30, 45) * Time.deltaTime);
        }
    }

    public void OnSimpleEventBtn_Clicked() {
        var gameEvent = new GameEvent("options")
                .AddParam("option", "sword")
                .AddParam("action", "sell");

        DDNA.Instance.RecordEvent(gameEvent);
    }

    public void OnAchievementEventBtn_Clicked() {
        var gameEvent = new GameEvent("achievement")
            .AddParam("achievementName", "Sunday Showdown Tournament Win")
            .AddParam("achievementID", "SS-2014-03-02-01")
            .AddParam("reward", new Params()
                .AddParam("rewardName", "Medal")
                .AddParam("rewardProducts", new Product()
                    .AddVirtualCurrency("VIP Points", "GRIND", 20)
                    .AddItem("Sunday Showdown Medal", "Victory Badge", 1)
                )
            );

        DDNA.Instance
                .RecordEvent(gameEvent)
                .Run();
    }

    public void OnTransactionEventBtn_Clicked() {
        var transaction = new Transaction(
                "Weapon type 11 manual repair",
                "PURCHASE",
                new Product()
                    .AddItem("WeaponsMaxConditionRepair:11", "WeaponMaxConditionRepair", 5)
                    .AddVirtualCurrency("Credit", "PREMIUM", 710),
                new Product()
                    .SetRealCurrency("USD", Product.ConvertCurrency("USD", 12.34m))) // $12.34
            .SetTransactorId("2.212.91.84:15116")
            .SetProductId("4019")
            .AddParam("paymentCountry", "GB");

        DDNA.Instance.RecordEvent(transaction);
    }

    public void OnEngagementBtn_Clicked() {
        var customParams = new Params()
            .AddParam("userLevel", 4)
            .AddParam("experience", 1000)
            .AddParam("missionName", "Disco Volante");

        DDNA.Instance.EngageFactory.RequestGameParameters("gameLoaded", customParams, (gameParameters) => {
            popUpContent.text = MiniJSON.Json.Serialize(gameParameters);
        });

        popUpTitle.text = "Engage returned";
        popUpObj.SetActive(true);
    }

    public void OnImageMessageBtn_Clicked() {
        var customParams = new Params()
            .AddParam("userLevel", 4)
            .AddParam("experience", 1000)
            .AddParam("missionName", "Disco Volante");

        DDNA.Instance.EngageFactory.RequestImageMessage("testImageMessage", customParams, (imageMessage) => {

            // Check we got an engagement with a valid image message.
            if (imageMessage != null) {
                Debug.Log("Engage returned a valid image message.");

                // This example will show the image as soon as the background
                // and button images have been downloaded.
                imageMessage.OnDidReceiveResources += () => {
                    Debug.Log("Image Message loaded resources.");
                    imageMessage.Show();
                };

                // Add a handler for the 'dismiss' action.
                imageMessage.OnDismiss += (ImageMessage.EventArgs obj) => {
                    Debug.Log("Image Message dismissed by " + obj.ID);
                };

                // Add a handler for the 'action' action.
                imageMessage.OnAction += (ImageMessage.EventArgs obj) => {
                    Debug.Log("Image Message actioned by " + obj.ID + " with command " + obj.ActionValue);
                };

                // Download the image message resources.
                imageMessage.FetchResources();
            } else {
                Debug.Log("Engage didn't return an image message.");
            }
        });
    }

    public void OnUploadEventsBtn_Clicked() {
        DDNA.Instance.Upload();
    }

    public void OnStartSDKBtn_Clicked() {
        // Start the SDK. We recommend using the configuration UI for setting your game's
        // keys and calling StartSDK() or StartSDK(userID) instead.
        DDNA.Instance.StartSDK(new Configuration() {
            environmentKeyDev = "76410301326725846610230818914037",
            environmentKey = 0,
            collectUrl = "https://collect2470ntysd.deltadna.net/collect/api",
            engageUrl = "https://engage2470ntysd.deltadna.net",
            useApplicationVersion = true
        });
    }

    public void OnStartSDKNewUserBtn_Clicked() {
        // Start the SDK. We recommend using the configuration UI for setting your game's
        // keys and calling StartSDK() or StartSDK(userID) instead.
        DDNA.Instance.StartSDK(new Configuration() {
            environmentKeyDev = "76410301326725846610230818914037",
            environmentKey = 0,
            collectUrl = "https://collect2470ntysd.deltadna.net/collect/api",
            engageUrl = "https://engage2470ntysd.deltadna.net",
            useApplicationVersion = true
        }, System.Guid.NewGuid().ToString());
    }

    public void OnStopSDKBtn_Clicked() {
        DDNA.Instance.StopSDK();
    }

    public void OnNewSessionBtn_Clicked() {
        DDNA.Instance.NewSession();
    }

    public void OnForgetMeBtn_Clicked() {
        DDNA.Instance.ForgetMe();
    }

    public void OnClearPersistentDataBtn_Clicked() {
        DDNA.Instance.ClearPersistentData();
    }
}
} // namespace DeltaDNA
