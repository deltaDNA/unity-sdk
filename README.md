![deltaDNA logo](https://deltadna.com/wp-content/uploads/2015/06/deltadna_www@1x.png)

## deltaDNA Analytics and SmartAds Unity SDK

The repository contains the sources for both the analytics and SmartAds SDKs.  They are packaged into separate unitypackages for easy installation.  The analytics can be installed independently, but the SmartAds depends on the analytics.  The unitypackages can be downloaded directly from GitHub by clicking the filename and then view raw.  Import into Unity with Assets->Import Package->Custom Package.  

The analytics SDK is supported in both Unity 4 and Unity 5, whereas SmartAds is only supported in Unity 5.

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

#### Custom Events

You can easily record custom events by using the `GameEvent` class.  Create a `GameEvent` with the name of your event schema.  Call `AddParam` to add custom event parameters to the event.  For example:

```csharp
var gameEvent = new GameEvent("myEvent")
    .AddParam("option", "sword")
    .AddParam("action", "sell");

DDNA.Instance.RecordEvent(gameEvent);
```

#### Engage

Change the behaviour of the game with an `Engagement`.  For example:

```csharp
var engagement = new Engagement("gameLoaded")
    .AddParam("userLevel", 4)
    .AddParam("experience", 1000)
    .AddParam("missionName", "Disco Volante");

DDNA.Instance.RequestEngagement(engagement, (response) =>
{
    // Response is a dictionary of key-values returned from Engage.
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

You can test if an interstitial ad is ready to be displayed with `DDNASmartAds.Instance.IsInterstitialAdAvailable()`.

Show an interstitial ad by calling `DDNASmartAds.Instance.ShowInterstitialAd()`.

You can test if a rewarded ad is ready to be displayed with `DDNASmartAds.Instance.IsRewardedAdAvailable()`.

Show a rewarded ad by calling `DDNASmartAds.Instance.ShowRewardedAd()`.

#### Events

Callbacks can be added to the following events to be notified when an ad has opened or closed.

* `OnDidRegisterForInterstitialAds` - Called when you have successfully enabled interstitial ads for your game.
* `OnDidFailToRegisterForInterstitialAds` - Called if interstitial ads are unavailable for some reason.  A string parameter reports a possible error.
* `OnInterstitialAdOpened` - Called when an interstitial ad is shown on screen.
* `OnInterstitialAdFailedToOpen` - Called if an interstitial ad fails to show.
* `OnInterstitialAdClosed` - Called when the user has closed an interstitial ad.
* `OnDidRegisterForRewardedAds` - Called when you have successfully enabled rewarded ads for your game.
* `OnDidFailToRegisterForRewardedAds` - Called if rewarded ads are unavailable for some reason.  A string parameter reports a possible error.
* `OnRewardedAdOpened` - Called when a rewarded ad is shown on screen.
* `OnRewardedAdFailedToOpen` - Called if a rewarded ad fails to show.
* `OnRewardedAdClosed` - Called when the user had closed a rewarded ad.  A boolean parameter indicates if the user had watched enough of the ad to be rewarded.

#### Decision Points

You can add control of which kinds players see ads by using *Decision Points*.  Show an ad with `ShowInterstitialAd("pointInGameToShowAnAd")` or `ShowRewardedAd("anotherPointInGameToShowAnAd")`, and register the decision point in Portal.  The SDK will ask if the segment this player is in should be shown the ad or not.  It's worth using decision points when you first integrate, if the decision point is not registered, it will be ignored and the ad always shown.

### iOS Integration

We use [CocoaPods](https://cocoapods.org/) to install our SmartAds library plus the 3rd party ad network libraries.  A minimal Podfile is included in DeltaDNAAds/Editor/iOS.  It will add our iOS SmartAds Pod to your XCode project along with all the ad networks we support.  A post process build hook prepares the XCode project Unity generates to support CocoaPods and adds the Podfile to the iOS build directory.  You must run `pod install` from the command line to install the Pods.  Finally open the *Unity-iPhone.xcworkspace* created by pod install.  A `pods.command` file is also included that runs the pod install and opens the XCode workspace for you.

The included Podfile will install support for all the ad networks deltaDNA supports.  You can customise the Podfile to download only the ad networks you require by using [Subspecs](https://guides.cocoapods.org/syntax/podfile.html#pod).  The process is the same as for the native [iOS SmartAds SDK](https://github.com/deltaDNA/ios-smartads-sdk) and more details on customising the Podfile can be found there.

### Android Integration

We provide a Python script to help manage the 3rd party ad network dependencies.  In `Assets\DeltaDNAAds\Editor\Android`, edit `config.json` to include the networks you wish to integrate.  Then from the command line run `download.py`.  This will download and copy the dependent AARs and Jar files into the `Assets\DeltaDNAAds\Plugins\Android` folder.  Unity will pick these up when you build the APK.

The SDK already pre-packages some dependencies for Google Play Services under `Assets\DeltaDNA\Plugins\Android` for push notifications (as well as SmartAds). If you would like to use your own version of Play Services, then you should remove the dependencies (ie play-services-base-7.8.0.aar, play-services-gcm-7.8.0.aar, etc) in order to avoid duplicate class definition errors during the build stage. Please note that we cannot guarantee other versions of Google Play Services than 7.8.0 to work correctly with our SDK.

## License

The sources are available under the Apache 2.0 license.
