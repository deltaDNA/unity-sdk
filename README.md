![deltaDNA logo](https://deltadna.com/wp-content/uploads/2015/06/deltadna_www@1x.png)

## deltaDNA Analytics and SmartAds Unity SDK

The repository contains the sources for both the analytics and SmartAds SDKs.  They are packaged into separate unitypackages for easy installation.  The analytics can be installed independently, but the SmartAds depends on the analytics.  The unitypackages can be downloaded directly from GitHub by clicking the filename and then view raw.  Import into Unity with Assets->Import Package->Custom Package.

The analytics SDK is supported in both Unity 4 and Unity 5, whereas SmartAds is only supported in Unity 5.

## Contents

* [Analytics](#analytics)
* [Quick Start](#quick-start)
 * [Custom Events](#custom-events)
 * [Engage](#engage)
* [SmartAds](#smartads)
 * [Usage](#usage)
 * [Create an Interstitial Ad](#create-an-interstitial-ad)
 * [Create a Rewarded Ad](#create-a-rewarded-ad)
 * [Working with Engage](#working-with-engage)
 * [Legacy Interface](#legacy-interface)
 * [Events](#events)
* [iOS Integration](#ios-integration)
 * [Push Notifications](#push-notifications)
 * [SmartAds on iOS](#smartads-on-ios)
* [Android Integration](#android-integration)
 * [Push Notifications](#push-notifications)
 * [SmartAds on Android](#smartads-on-android)
 * [Permissions](#permissions)
* [Migrations](#migrations)
 * [4.2](#version-4.2)
 * [4.3](#version-4.3)
* [License](#license)

## Analytics

Our analytics SDK is written entirely in Unity with no native code requirements.  Out of the box it runs on any platform that Unity supports.  The easiest way to get started is to download the `deltadna-sdk-*.unitypackage` from this repository, and import into your Unity project.

## Quick Start

For all the information on how to use the analytics SDK, refer to our documentation [portal](http://docs.deltadna.com/advanced-integration/unity-sdk/).

Checkout the example in `Assets\DeltaDNA\Example` to see how to use the SDK.  At a minimum you will want to set the Client Version and start the SDK from a custom `MonoBehaviour`.

```csharp
DDNA.Instance.ClientVersion = "1.0.0";
DDNA.Instance.StartSDK("YOUR_ENVIRONMENT_KEY",
                       "YOUR_COLLECT_URL",
                       "YOUR_ENGAGE_URL");
```

On the first run this will create new user id and send a `newPlayer` event. On every call it will send a `gameStarted` and `clientDevice` event.

### Custom Events

You can easily record custom events by using the `GameEvent` class.  Create a `GameEvent` with the name of your event schema.  Call `AddParam` to add custom event parameters to the event.  For example:

```csharp
var gameEvent = new GameEvent("myEvent")
    .AddParam("option", "sword")
    .AddParam("action", "sell");

DDNA.Instance.RecordEvent(gameEvent);
```

### Engage

Change the behaviour of the game with an `Engagement`.  For example:

```csharp
var engagement = new Engagement("gameLoaded")
    .AddParam("userLevel", 4)
    .AddParam("experience", 1000)
    .AddParam("missionName", "Disco Volante");

DDNA.Instance.RequestEngagement(engagement, (response) =>
{
    // Response is a Dictionary<string, object> of key-values returned from Engage.
    // It will be empty if no matching campaign was found or an error occurred.
});
```

If you need more control over the response from Engage use `DDNA.Instance.RequestEngagement(Engagement engagement, Action<Engagement> onCompleted, Action<Exception> onError)`.  This calls the onCompleted callback with the Engagement containing the response from Engage.  You can also handle if any errors occur.  With this method it is possible to optionally create an `ImageMessage` if the Engagement supports it.  For example:

```csharp
var engagement = new Engagement("imageMessage")
    .AddParam("userLevel", 4)
    .AddParam("experience", 1000)
    .AddParam("missionName", "Disco Volante");

DDNA.Instance.RequestEngagement(engagement, (response) => {

    ImageMessage imageMessage = ImageMessage.Create(response);

    // Check we got an engagement with a valid image message.
    if (imageMessage != null) {
        imageMessage.OnDidReceiveResources += () => {
            // Can show once we've got the resources.
            imageMessage.Show();
        };
        // Download the image message resources.
        imageMessage.FetchResources();
    }
    else {
        // Engage didn't return an image message.
    }
}, (exception) => {
    Debug.Log("Engage reported an error: "+exception.Message);
});
```

## SmartAds

Integrating SmartAds into your Unity project requires native code extensions which we supply separately.  More information on how to access our SmartAds platform is [here](http://docs.deltadna.com/advanced-integration/smart-ads/).  To add the Unity extensions download and import the `deltadna-smartads-*.unitypackage`.  We support iOS and Android platforms.

### Usage

The quickest way to learn how to use SmartAds is to checkout out the example scene in `Assets\DeltaDNAAds\Example`.  The `AdsDemo` class shows how to use both interstitial and rewarded ads.  Support for SmartAds is enabled by calling `RegisterForAds`.  This *must* be called after starting the analytics SDK.  The `DDNASmartAds` class defines a number of events which you can register callbacks with to be notified when an ad has opened or closed.

Start the analytics SDK.

```csharp
DDNA.Instance.ClientVersion = "1.0.0";
DDNA.Instance.StartSDK("YOUR_ENVIRONMENT_KEY",
                       "YOUR_COLLECT_URL",
                       "YOUR_ENGAGE_URL");
```

Register for ads.

```csharp
DDNASmartAds.Instance.RegisterForAds();
```

If everything went well the SmartAds service will start fetching ads in the background.  The `DDNASmartAds` class provides the following delegates to report if the service was successfully configured:

* `OnDidRegisterForInterstitialAds` - Called when interstitial ads have been successfully configured.
* `OnDidFailToRegisterForInterstitialAds` - Called if interstitial ads can't be configured for some reason.
* `OnDidRegisterForRewardedAds` - Called when rewarded ads have successfully been configured.
* `OnDidFailToRegisterForRewardedAds` - Called when rewarded ads can't be configured for some reason.

### Create an Interstitial Ad

An interstitial ad is a fullscreen popup that the player can dismiss from a close button.  In order to show an interstitial ad, try to create an `InterstitialAd` and then show it.

```csharp
var interstitialAd = InterstitialAd.Create();
if (interstitialAd != null) {
    interstitialAd.Show();
}
```

`Create` checks that the game has permission to show an ad at this point.  It checks that an ad has loaded, that the number of ads for this session hasn't been exceeded and that it's not too soon since the last ad was shown.  If a non null object is returned you are allowed to show an ad.  This allows you to easily control the number and frequency of ads shown to your players from our platform.

The following events can be added to an `InterstitialAd`:

* `OnInterstitialAdOpened` - Called when the ad is showing on screen.
* `OnInterstitialAdFailedToOpen` - Called if the ad fails to open for some reason.
* `OnInterstitialAdClosed` - Called when the ad has been closed.

### Create a Rewarded Ad

A rewarded ad is a short video, typically 30 seconds in length that the player must watch before being able to dismiss.  To show a rewarded ad, try to create a `RewardedAd` and then show it.

```csharp
var rewardedAd = RewardedAd.Create();
if (rewardedAd != null) {
    rewardedAd.Show();
}
```

As with `InterstitialAd` the `Create` method will only return an object if you're allowed to show a rewarded ad and there is one available to show at this point.  So, for example if you get a non null object you can present a UI to the player that offers them a rewarded ad to watch.

The following events can be added to a `RewardedAd`:

* `OnRewardedAdOpened` - Called when the ad is showing on screen.
* `OnRewardedAdFailedToOpen` - Called if the ad fails to open for some reason.
* `OnRewardedAdClosed` - Called when the ad is finished.  A boolean reward flag indicates if the ad was watched enough that you can reward the player.

### Working with Engage

To fully take advantage of deltaDNA's SmartAds you want to work with our Engage service.  The game can ask Engage if it should show an ad for this particular player.  Engage will tailor its response according to which campaigns are running and which segment this player is in.  You try to create an ad from an `Engagement` object, it will only succeed if the Engage response allows it and the session, time and loaded constraints are satisfied.  We can also add additional parameters into the Engage response which the game can use, perhaps to customise the reward for this player.

```csharp
var engagement = new Engagement("showRewarded");

DDNA.Instance.RequestEngagement(engagement, response => {

    var rewardedAd = RewardedAd.Create(response);

    if (rewardedAd != null) {

        // See what reward is being offered
        if (rewardedAd.Parameters.ContainsKey("rewardAmount")) {
            int rewardAmount = System.Convert.ToInt32(rewardedAd.Parameters["rewardAmount"]);

            // Present offer to player...

            // If they choose to watch the add
            rewardedAd.Show();
        }
    }

}, exception => {
    Debug.Log("Engage encountered an error: "+exception.Message);
});
```

Checkout the included example project for more details.

### Legacy Interface

Prior to the inclusion of the `InterstitialAd` and `RewardedAd` classes you could show ads directly from the `DDNASmartAds` object.  This still works since this is what the ad classes use, but it's preferred to use the separate classes.

You can test if an interstitial ad has loaded with `DDNASmartAds.Instance.IsInterstitialAdAvailable()`.  Show an interstitial ad by calling `DDNASmartAds.Instance.ShowInterstitialAd()`.  You can test if a rewarded ad has loaded with `DDNASmartAds.Instance.IsRewardedAdAvailable()`.  Show a rewarded ad by calling `DDNASmartAds.Instance.ShowRewardedAd()`.  These calls don't tell you in advance if showing the ad will fail because of session and time limits, another reason why we recommend using the `InterstitialAd` and `RewardedAd` classes.

The additional show methods that use Decision Points are now deprecated, since they hide what Engage is returning which prevents you from controlling if and when to show the ad in your game.

### Events

Callbacks can be added to the following events to be notified when an ad has opened or closed.

* `OnDidRegisterForInterstitialAds` - Called when you have successfully enabled interstitial ads for your game.
* `OnDidFailToRegisterForInterstitialAds` - Called if interstitial ads are unavailable for some reason.  A string parameter reports a possible error.
* ~~`OnInterstitialAdOpened` - Called when an interstitial ad is shown on screen.~~  Prefer `InterstitialAd.OnInterstitialAdOpened`.
* ~~`OnInterstitialAdFailedToOpen` - Called if an interstitial ad fails to show.~~ Prefer `InterstitialAd.OnInterstitialAdFailedToOpen`.
* ~~`OnInterstitialAdClosed` - Called when the user has closed an interstitial ad.~~ Prefer `InterstitialAd.OnInterstitialAdClosed`.
* `OnDidRegisterForRewardedAds` - Called when you have successfully enabled rewarded ads for your game.
* `OnDidFailToRegisterForRewardedAds` - Called if rewarded ads are unavailable for some reason.  A string parameter reports a possible error.
* ~~`OnRewardedAdOpened` - Called when a rewarded ad is shown on screen.~~ Prefer `RewardedAd.OnRewardedAdOpened`.
* ~~`OnRewardedAdFailedToOpen` - Called if a rewarded ad fails to show.~~ Prefer `RewardedAd.OnRewardedAdFailedToOpen`.
* ~~`OnRewardedAdClosed` - Called when the user had closed a rewarded ad.  A boolean parameter indicates if the user had watched enough of the ad to be rewarded.~~ See `RewardedAd.OnRewardedAdClosed`.

## iOS Integration

### Push Notifications

To support iOS push notifications you need to call `IosNotifications.RegisterForPushNotifications()`.  This uses Unity's `NotificationServices` to request a push token and then reports it back to us in a `notificationServices` event.  You will also need to enter the game's associated APNs certificate into our platform.

We record if your game was started by the player clicking on a push notification.  However to make this work properly the `DDNA` game object has to be loaded early on in the scene which the game launches with.  This can be achieved by adding a delegate to `OnDidLaunchWithPushNotification` in the `Awake` method of a game object that manages the SDK.

### SmartAds on iOS

We use [CocoaPods](https://cocoapods.org/) to install our SmartAds library plus the 3rd party ad network libraries.  The included Podfile will add our iOS SmartAds Pod to your XCode project along with all the ad networks we support.  A post process build hook prepares the XCode project Unity generates to support CocoaPods and adds the Podfile to the iOS build directory.  It then runs `pod install` to download the dependencies and create the *Unity-iPhone.xcworkspace*.  You will need to open the workspace file since Unity doesn't know about this.  Clicking *build and run* is therefore not supported.

__The ad networks require a minimum target version of 7, and ideally 8 to get the latest sdks.  If the default 6 is used cocoapods will fail and no xcworkspace file will be generated.__

__If updating from a previous SDK version, run `pod repo update` to update your local cache and ensure you build with the latest dependencies.  Since CocoaPods v1.0 this no longer happens by default on `pod install`.__

To select which ad networks should be included in the game select *DeltaDNA* from the Unity menu bar, navigate to *SmartAds -> Select Networks*, which will open a tab with the settings. The ad networks can now be selected or deselected, and clicking *Apply* will persist the changes.

If you make changes to the enabled networks the changes to the Podfile should be committed to version control.

#### UnityAds

The latest versions of Unity cause conflict with Unity's internal UnityAds plugin.  An error can occur when the PostBuildProcess methods are run.  I've resolved this by having the `pod install` process run last.  You may need to change the order of the PostBuildProcess if your game includes multiple libraries using PostProcessBuild calls.

## Android Integration

### Android Dependencies Google Firebase/Play Services Libraries

Any library dependencies such as Google's Firebase (Google Play Services) are handled by Google's [Unity Jar Resolver](https://github.com/googlesamples/unity-jar-resolver) plugin. The libraries will be automatically downloaded into the *Assets/Plugins/Android* folder. If you have other Unity plugins in your application which don't use the Resolver for downloading dependencies you may want to consider using the Resolver to get manage their dependencies as well, otherwise you may have to manually resolve any conflicts.

### Push Notifications

Our push notifications use Firebase messaging (this was changed in version 4.3, if you're upgrading see the migration [guide](#version-4.3) below). In order to configure notifications you will need to set the *Application* and *Sender IDs* from the configuration screen, which can be accessed from the Unity Editor menu under *DeltaDNA -> Notifications -> Android -> Configure*. The IDs can be found in the Firebase Console for your application ([1](Docs/firebase_console_1.png), [2](Docs/firebase_console_2.png), and [3](Docs/firebase_console_3.png)). Pressing *Apply* will persist the changes to resource files in your project, which should be committed to source control.

If your application is setup using the Google Cloud Console you can find instructions [here](https://developers.google.com/cloud-messaging/android/android-migrate-fcm#import_your_gcm_project_as_a_firebase_project) on how to migrate the project to Firebase. Firebase projects are backwards compatible with applications using Google Cloud Messaging.

The style of the push notifications can be changed by overriding the behaviour of the library. Instructions on how to do this can be found [here](https://github.com/deltaDNA/android-sdk/tree/master/library-notifications#unity). Once you have added either the modified library or added the new classes as a separate library you will need to change the *Listener Service* field in the configuration to the fully qualified name of your new class.

If you no longer wish to use push notifications on Android then you can remove the *Assets/DeltaDNA/Plugins/Android* and *Assets/Plugins/Android/deltadna-sdk-unity-notifications* folders from the project to decrease the number of methods and the APK size of your game.

### SmartAds on Android

The ad networks you wish to build into your app can be selected from *DeltaDNA -> SmartAds -> Select Networks*. After applying the changes the SDK will download the latest libraries from the Maven repository. If you make changes to the enabled networks the changes to the *build.gradle* file should be committed to version control.

The libraries can be downloaded anytime from the *DeltaDNA -> SmartAds -> Android -> Download Libraries* menu item. We recommend doing this after updating the DeltaDNA SDK, or after pulling changes from version control. The SDK will try to detect when the downloaded libraries are stale and log a warning in the Editor console.

In order for the library download function to properly work you need to have the Android SDK installed and setup for your Unity project. From the Android SDK you will need to have a version of *build-tools* and an *SDK Platform* installed, as well as recent versions of the *Android Support Repository* and *Google Repository*.

### MultiDex; Working Around Android's 65k Method Limit
1. Export your Unity project using the *Gradle* build system. These options can be found in the *Build Settings* dialog.
2. Open the exported project in Android Studio and select to use the Gradle wrapper if asked to.
3. Open the top-level *build.gradle* file for your project and apply the MultiDex workaround as described [here](https://developer.android.com/studio/build/multidex.html#mdex-gradle).

### Permissions

The permissions which the Android libraries request can be overriden through the use of the [Android manifest merger](http://tools.android.com/tech-docs/new-build-system/user-guide/manifest-merger). For example, if you would like to remove the `maxSdkVersion` attribute for the `WRITE_EXTERNAL_STORAGE` permission then you can specify the following in your manifest file:
```xml
<uses-permission
    android:name="android.permission.WRITE_EXTERNAL_STORAGE"
    tools:remove="android:maxSdkVersion"/>
```

In case the above still causes conflicts during manifest merging then the following can be used in the manifest file instead:
```xml
<uses-permission
    android:name="android.permission.WRITE_EXTERNAL_STORAGE"
    tools:merge="override"/>
```

## Migrations

### Version 4.2
Configuring which networks should be used for SmartAds has been changed by adding menu items to the Unity Editor, which removes some of the error-prone manual steps. For Android there's no longer the need to have Python installed or set the Android SDK directory in order to download the library dependencies, as the menu item for this task will take care of the steps. If you make changes to the selected networks you will need to commit the changes made to the build.gradle and/or Podfile to your version control, in order for the rest of your team to use the changes.

Since we've had to change how the SmartAds networks are defined you may need to look over the selected networks in case you had previously removed any of them for your project.

### Version 4.3
Between version 4.2 and version 4.3 we updated our push notifications to use Firebase (play-services-*-10.2).  This requires changing the way push notification integration works.  To better manage the Android dependencies we now use Google's [Unity Jar Resolver](https://github.com/googlesamples/unity-jar-resolver).  This allows other plugins to also specify dependencies on the Firebase/Play-Services libraries and the Unity Jar Resolver will work out which library to use, hopefully reducing duplicate library errors at build time.

#### SDK Health Check
You can run a health check once you've upgraded the SDK to identify mistakes related to previous versions, such as conflicting configuration entries and duplicate libraries. It can be accessed from the Editor menu under *DeltaDNA -> Health Check SDK*. Please note that there could still be issues with your project which the utility may be unable to detect. Always consult the documentation for more details.

#### Android Dependencies
After importing the new DeltaDNA SDK package into your project make sure to remove the old *deltadna-sdk-notifications* AAR file from *Assets/DeltaDNA/Plugins/Android*. You also need to remove any *play-services* and *support* AAR and JAR libraries in that location as they will cause conflicts with the libraries downloaded by the Unity Jar Resolver.

As with any SDK update you should update the Android SmartAds libraries from *DeltaDNA -> SmartAds -> Android -> Download Libraries*.

#### Android Notifications
We have added a UI for configuring push notifications on Android, which can be accessed from the menu of the Unity Editor under *DeltaDNA -> Notifications -> Android -> Configure*. You will need to fill in the Application and Sender IDs from the Firebase Console for your application if you'd like to use notifications or have been using them with a previous version of our SDK.

We highly recommend removing any entries previously added for DeltaDNA notifications from the *AndroidManifest.xml* file in *Assets/Plugins/Android* as they may conflict with the Firebase implementation. If you never added anything else to the manifest file then you can probably remove it altogether. For more details on which XML attributes to remove take a look [here](https://github.com/deltaDNA/android-sdk/blob/master/docs/migrations/4.3.md#manifest). In addition you will also be able to remove the *string* resource from *Assets/Plugins/Android/res/values* which contains your application's Sender ID.

If you no longer wish to use notifications then remove the *Assets/Plugins/Android/deltadna-sdk-unity-notifications* and *Assets/DeltaDNA/Plugins/Android* folders from your project.

## License

The sources are available under the Apache 2.0 license.
