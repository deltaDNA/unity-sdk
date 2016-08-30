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
* [Android Integration](#android-integration)
 * [Push Notifications](#push-notifications)
 * [SmartAds on Android](#smartads-on-android)
* [Migrations](#migrations)
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

**Unity iOS builds have a [bug](https://issuetracker.unity3d.com/issues/ios-itunes-connect-rejects-all-unity-apps-that-are-referencing-watchconnectivity-dot-framework) with apps that use the WatchConnectivity framework.  Flurry has a dependency on that framework, so for now you should exclude the Flurry SDK from your stack.  See [iOS Integration](#ios-integration) for how to customise your Podfile.**

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

We use [CocoaPods](https://cocoapods.org/) to install our SmartAds library plus the 3rd party ad network libraries.  The included Podfile will add our iOS SmartAds Pod to your XCode project along with all the ad networks we support.  A post process build hook prepares the XCode project Unity generates to support CocoaPods and adds the Podfile to the iOS build directory.  It then runs `pod install` to download the dependencies and create the *Unity-iPhone.xcworkspace*.  You will need to open the workspace file since Unity doesn't know about this.  Clicking *build and run* is therefore not supported.

To select which ad networks should be included in the game select *DeltaDNA* from the Unity menu bar, navigate to *SmartAds -> Select Networks*, which will open a tab with the settings. The ad networks can now be selected or deselected, and clicking *Apply* will persist the changes.

If you make changes to the enabled networks the changes to the Podfile should be committed to version control.

## Android Integration

### Push Notifications

In order to use push notifications on Android you will need to add an AndroidManifest.xml to your project under `Assets/Plugins/Android` in order to register broadcast receivers and services for your game. You can take a look [here](Assets/Plugins/Android/) for an example configuration which has been made to work with the example packaged in the SDK. Please take a look at the [integration section](https://github.com/deltaDNA/android-sdk/tree/master/library-notifications#integration) for push notifications, which is also relevant to the Analytics Unity SDK on Android, containing integration steps with more details.

The SDK already pre-packages some dependencies for Google Play Services under `Assets\DeltaDNA\Plugins\Android` for push notifications (as well as SmartAds). If you would like to use your own version of Play Services, then you should remove the dependencies (ie play-services-base-7.8.0.aar, play-services-gcm-7.8.0.aar, etc) in order to avoid duplicate class definition errors during the build stage. Please note that we cannot guarantee other versions of Google Play Services than 7.8.0 to work correctly with our SDK.

If you do not wish to use push notifications on Android then you can remove the files from the `Assets\DeltaDNA\Plugins\Android` folder and the customised `AndroidManifest.xml` to decrease the APK size of your game.

### SmartAds on Android

As with SmartAds on iOS the same settings from *DeltaDNA -> SmartAds -> Select Networks* can be used to select which networks should be used. After applying the changes the SDK will automatically download the ad network libraries from a Maven repository.

The Android libraries can also be downloaded from the *DeltaDNA -> SmartAds -> Android -> Download Libraries* menu item. We recommend doing this after updating the DeltaDNA SDK, or after pulling changes from version control. The SDK will automatically try to detect when the downloaded libraries may be stale and will show a warning in the Editor console.

If you make changes to the enabled networks the changes to the build.gradle file should be committed to version control.

In order for the menu items to work you will need to have the Android SDK installed and setup for your Unity project. From the Android SDK you will need to have a version of build-tools and an SDK platform installed, as well as recent versions of the *Android Support Repository* and *Google Repository*.

## Migrations

### Version 4.2
Configuring which networks should be used for SmartAds has been changed by adding menu items to the Unity Editor, which removes some of the error-prone manual steps. For Android there's no longer the need to have Python installed or set the Android SDK directory in order to download the library dependencies, as the menu item for this task will take care of the steps. If you make changes to the selected networks you will need to commit the changes made to the build.gradle and/or Podfile to your version control, in order for the rest of your team to use the changes.

Since we've had to change how the SmartAds networks are defined you may need to look over the selected networks in case you had previously removed any of them for your project.

## License

The sources are available under the Apache 2.0 license.
