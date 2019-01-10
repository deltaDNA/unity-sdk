# Change Log

## [4.12.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.12-0) (YYYY-MM-DD)
### Added
- Support for cross promotion.
- Support for image message store action.

### Fixed
- Image messages not redrawing correctly on device re-orientation 

## [4.11.6](https://github.com/deltaDNA/unity-sdk/releases/tag/4.11.6)
### Added
- Support for removing iOS push notifications from build.

### Fixed
- Engage requests resulting in client error responses will no longer use the Engage cache.

## [4.11.5](https://github.com/deltaDNA/unity-sdk/releases/tag/4.11.5) (2018-11-19)
### Fixed
- Occasional freeze when closing a Unity ad on iOS.

## [4.11.4](https://github.com/deltaDNA/unity-sdk/releases/tag/4.11.4) (2018-11-07)
### Fixed
- Missing fields in ddnaEventTriggeredAction event.

## [4.11.3](https://github.com/deltaDNA/unity-sdk/releases/tag/4.11.3) (2018-11-02)
### Fixed
- Crash when performing Engage requests.
- Invalid event schema for forget me events.
- IsolatedStorageException errors when saving the Engage cache.

## [4.11.2](https://github.com/deltaDNA/unity-sdk/releases/tag/4.11.2) (2018-09-20)
### Fixed
- Facebook ads dependency resolution on Android.
- Missing Amazon, iPad, and iPhone identifiers for 2018 devices.

## [4.11.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.11.1) (2018-08-31)
### Fixed
- Compatibility with Unity 5.4 and 5.5.

## [4.11.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.11.0) (2018-08-22)
### Added
- Sends advertising id with Forget Me event when using SmartAds. 

### Removed
- MachineZone ads.

### Updated
- Android notifications library.
- AdColony, AdMob, AppLovin, Chartboost, InMobi, IronSource, LoopMe, MobFox, MoPub, Tapjoy, Unity, and Vungle ads with GDPR compatibility on Android.
- Facebook and Flurry ads on Android.

## [4.10.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.10.0) (2018-08-07)
### Added
- Session configuration.
- Event and decision point whitelisting.
- Event-Triggered Campaign support.
- Image message asset caching.

### Changed
- Engage responses will be evicted from cache after 12 hours (configurable).

## [4.9.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.9.1) (2018-07-18)
### Fixed
- Gradle template not setting app version on Unity 2017.1+.
- Gradle template causing build error on Unity 2018.2.
- Wrong notification received callback invoked on iOS.

## [4.9.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.9.0) (2018-05-21)
### Added
- API for forgetting the user and stopping tracking (GDPR).
- Settings for advertising tracking consent and age restriction (GDPR).

### Fixed
- Crash in Editor when Android notifications are removed from project.
- Crash in Editor when debug notifications are enabled for iOS.
- Unity Jar Resolver looping when debug notifications are enabled.

### Changed
- Unity Jar Resolver updated to 1.2.71.

## [4.8.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.8.1) (2018-04-30)
### Fixed
- Prevent Smartads debug notifications being triggered by other push notification libraries.

## [4.8.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.8.0) (2018-04-18)
### Added
- Configuration UI for the SDK.
- Automatic registration for ads.
- Test ads support for when running in the Unity Editor.

### Fixed
- Namespace clash with Tuple classes.

## [4.7.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.7.1) (2018-02-06)
### Fixed
- Disabled chunked transfer for Unity 2017.3.

### Changed
- Collect and Engage URLs will be forced to use HTTPS.

## [4.7.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.7.0) (2018-01-25)
### Added
- MachineZone ad network.
- SmartAds debug diagnostics on iOS and Android.

### Fixed
- Crash on Android when notifications are removed from project.

### Changed
- Updated runtime libraries on Android.
- Updated AdColony ads on Android.
- Updated AdMob ads on Android.
- Updated AppLovin ads on Android.
- Updated Chartboost ads on Android.
- Updated Facebook ads on Android.
- Updated Flyrry ads on Android.
- Updated HyprMX ads on Android.
- Updated InMobi ads on Android.
- Updated IronSource ads on Android.
- Updated MobFox ads on Android.
- Updated MoPub ads on Android.
- Updated Tapjoy ads on Android.
- Updated Unity ads on Android.
- Updated Vungle ads on Android.
- Updated IronSource to set mediation type.

## [4.6.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.6.1) (2017-12-01)
### Fixed
- Support for Unity 4.7.2
- InMobi completed callbacks not being invoked on Android.
- Push notification broadcast events if targeting Android API 26 or higher.
- Push notifications not opening app if targeting Android API 26 or higher.
- Push notification events to not be broadcast outside of app on Android.
- Crash when notifications icon defined as drawable resource on Android.
- Possibility of leaking cursors on Android.

### Changed
- Updated Unity Jar Resolver to v1.2.59.0.

## [4.6.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.6.0) (2017-11-13)
### Added
- Added support for Amazon platform option.

### Fixed
- Support for Unity 2017.2

### Changed
- Updated ad networks on Android.
- Updated Unity Jar Resolver to v1.2.52.0.
- Unity Jar Resolver downloads all Android dependencies from Maven.
- Unity Jar Resolver handles SmartAds libraries for iOS.

### Removed
- Android libraries from packages.
- PostProcessBuild script for iOS.

## [4.5.4](https://github.com/deltaDNA/unity-sdk/releases/tag/4.5.4) (2017-09-20)
### Fixed
- Triggering a new session before marking the sdk as started.
- Unable to send long values as virtual currency.
- Tapjoy causing a hardware not supported error on 64-bit architectures.

### Changed
- Updated AdColony ads on Android.
- Updated iOS SmartAds to 1.5.2.

## [4.5.3](https://github.com/deltaDNA/unity-sdk/releases/tag/4.5.3) (2017-09-11)
### Fixed
- Fixed missing resources in Tapjoy on Android.

## [4.5.2](https://github.com/deltaDNA/unity-sdk/releases/tag/4.5.2) (2017-08-22)
### Added
- Tapjoy ad network.
- LoopMe ad network.
- HyprMX ad network.

## [4.5.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.5.1) (2017-08-10)
### Fixed
- Support for Unity 2017.1
- Missing device manufacturer on Android.
- Crash on Mac when running in Editor on non-GMT timezones.
- Crash when running examples in the Editor.
- Notifications not registered on iOS before the SDK is initialised.

### Changed
- Updated Unity Jar Resolver to v1.2.31.

## [4.5.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.5.0) (2017-06-27)
### Added
- Send imageMessageAction event when interacting with image messages.

### Fixed
- Fixed event sending on notification interactions in Android.

### Changed
- Image Message uses new UI.
- Using new Unity UI for examples.
- Replaced WWW with UnityWebRequest for 5.6.2 and above.
- Updated Unity Jar Resolver to v1.2.29.

### Removed
- Register and unregister notification methods from iOS native bridge.

## [4.4.2](https://github.com/deltaDNA/unity-sdk/releases/tag/4.4.2) (2017-06-14)
### Added
- Detection and removal of crashed ad networks on Android.

### Fixed
- AppLovin reporting duplicate ad loads on Android.
- Chartboost misreporting show failures as load failures on Android.
- Vungle, IronSource, and Unity callbacks not invoked after first ad show on Android.
- Stuck download of SmartAds libraries on Android in some cases.

### Updated
- Unity Jar Resolver.
- Google Play Services and Firebase dependencies for Android.

## [4.4.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.4.1) (2017-05-04)
### Fixed
- Fixed Chartboost, Unity, and Vungle callbacks on Android.
- Fixed misreporting of ad show events on Android.
- Updated iOS device definitions

## [4.4.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.4.0) (2017-04-17)
### Added
- Facebook ad network.
- IronSource ad network.

### Fixed
- Rejected events due to missing timezone offset on Windows.
- Rejected events due to long device details on Windows.
- Android Play Services and Support not cleared out after ad network changes.
- Overwriting of notifications configuration on Android after updating SDK.
- Package name conflict on Android when exporting as a Grandle project.

### Changed
- Updated Android dependencies.
- Updated Jar Resolver.
- Uses 'pod update' instead of 'pod install' to ensure latest iOS dependencies.

## [4.3.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.3.0) (2017-03-13)
### Added
- Support for AppLovin SDK.
- Support for ThirdPresence SDK.
- Push notifications configuration UI for Android.
- Unity Play Resolver for downloading Android and Google dependencies.
- Editor menu item for checking SDK configuration errors.

### Fixed
- Notifications from campaigns overwriting other notifications on Android.
- Application icon not being picked up for notifications on Android.
- Fixed Android notifications project not merging on Unity 5.4.

### Changed
- Notifications use newer Firebase libraries on Android.
- Updated SmartAds library dependencies on Android.
- Removed manifest file for push notifications on Android.
- Remove Android and Google libraries for push notifications on Android.
- Removed deprecated engage and image message methods.

## [4.2.13](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.13) (2017-02-16)
### Fixed
- IL2CPP support for Android.
- Additional event store corruption checks.

## [4.2.12](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.12) (2017-02-01)
### Fixed
- SmartAd adapters will respect their waterfall index on Android.
- SmartAd requests will not be made after reaching the session limit on Android.

### Changed
- Removed MobFox ad network.

## [4.2.11](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.11) (2016-12-21)
### Changed
- Updated Chartboost network dependency on Android.
- Prefer deltaDNA's CocoaPods repository for conflict resolution.

## [4.2.10](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.10) (2016-12-13)
### Fixed
- SmartAds not using cached responses on Android.
- SmartAds configuration retries on Android.

### Changed
- Allow Platform field to be set by clients.
- Default urls without protocol to HTTPS.
- Minor changes to support Unity 5.5.

## [4.2.9](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.9) (2016-11-24)
### Fixed
- MobFox crash on Android.
- Amazon ad events on Android.
- Vungle misreporting ad watched status on Android.

### Changed
- Updated Android SmartAds ad network dependencies.

## [4.2.8](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.8) (2016-11-14)
### Fixed
- Serialisation of decimal point values.
- Performance when first creating a Product instance.
- Pass iOS supported version to CocoaPods build.

## [4.2.7](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.7) (2016-10-07)
### Fixed
- Application.persistentDataPath can return empty or read only path.

## [4.2.6](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.6) (2016-10-03)
### Fixed
- SmartAds iOS pauses Unity whilst showing an ad, which now matches the Android behaviour.

## [4.2.5](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.5) (2016-09-29)
### Fixed
- SmartAds iOS PostProcessBuild conflicting with Unity's internal UnityAds post process.

## [4.2.4](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.4) (2016-09-29)
### Fixed
- Currency conversion on Windows.

## [4.2.3](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.3) (2016-09-16)
### Fixed
- Unrealiable Android push notification callbacks.
- Exceptions when running analytics in the Editor Player.
- Better support for Unity 4.7.

### Changed
- Updated Android SmartAds ad network dependencies.

## [4.2.2](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.2) (2016-09-13)
### Fixed
- Crash when receiving notifications on Android.

## [4.2.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.1) (2016-09-09)
### Fixed
- Handling of push notifications from other senders on Android.

## [4.2.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.0) (2016-09-05)
### Added
- Menu for setting up SmartAds networks into the Unity Editor.
- Menu for downloading Android SmartAds library dependencies.
- Utility method in Product for converting currencies from a floating point representation.
- Method added to Transaction for setting the receipt signature.
- Warning when clearing persistent data while SDK is started.

### Fixed
- Compilation errors in SmartAds when missing iOS build support.
- iOS notifications not working with Unity 5.4 and newer.

### Changed
- Removed Python dependency and download script for Android.

## [4.1.7](https://github.com/deltaDNA/unity-sdk/releases/tag/4.1.7) (2016-08-18)
### Fixed
- Empty notification title on Android.
- Crash when no networks are added from the configuration on Android.

## [4.1.6](https://github.com/deltaDNA/unity-sdk/releases/tag/4.1.6) (2016-07-28)
### Fixed
- Crash when showing a MoPub ad on Android.

## [4.1.5](https://github.com/deltaDNA/unity-sdk/releases/tag/4.1.5) (2016-06-06)
### Fixed
- Ad show set to false not being respected on Android.
- Boolean native iOS calls always returning true.

## [4.1.4](https://github.com/deltaDNA/unity-sdk/releases/tag/4.1.4) (2016-05-27)
### Added
- `DDNASmartAds.IsInterstitialAdAllowed` and `DDNASmartAds.IsRewardedAdAllowed` which report if an ad is allowed to show.
- Calls `RegisterForAds` again on a new session.

### Fixed
- Ad network cycling on Android.
- Ad configuration not being read correctly on Android.
- Memory leak for `InterstitialAd` and `RewardedAd`.

### Changed
- `InterstitialAd` and `RewardedAd` will only be created if the time and session limits have not been reached, an ad has loaded, and if an Engagement is used, that it doesn't disable the ad.

## [4.1.3](https://github.com/deltaDNA/unity-sdk/releases/tag/4.1.3) (2016-05-18)
### Added
- Missing callbacks for push notification events on Android.

## [4.1.2](https://github.com/deltaDNA/unity-sdk/releases/tag/4.1.2) (2016-05-12)
### Fixed
- AdColony on Android reporting potentially wrong ad shown state.
- Minimum interval between ads not being respected on Android.

## [4.1.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.1.1) (2016-05-09)
### Fixed
- Crash on Android when requesting ads without a network connection.

## [4.1.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.1.0) (2016-05-06)
### Added
- Added `InterstitialAd` and `RewardedAd` classes, which replace showing ads with the `DDNASmartAds`.  This idea is to split out the call to Engage and make it explicit.  This gives the game more flexibility in how it works with Engage to control if ads are to be shown to the player.
- `ImageMessage` class replaces the `Popup` interface and `BasicPopup`.  It's interface more closely matches how the ad classes work.
- Unity 5.3 has added support for some basic unit testing, so added a few tests.
- Will automatically generate a new session id if the app has been in the background for more than 5 minutes.  This behaviour can be configured from the settings.

### Changed
- `OnAdFailedToOpen` now reports the reason for the failure.
- Engage no longer returns null if the connection fails, it returns an empty dictionary.
- The `Engagement` records the response, http status code and any errors.  This works with an additional `RequestEngagement` method.
- For iOS OnPostprocessBuild calls pod install for you automatically.  This generates an 'Invalid PBX project' exception which can be ignored.  Instead you must open the generated workspace yourself.
- The automated event uploading no longer retries by default on a network connection error, instead it relies on the background timer to try again later. The default timeout has also been increased.

### Fixed
- 'isCachedResponse' is now injected into the Engage response if the cache is used.

## [4.0.1](https://github.com/deltaDNA/unity-sdk/releases/tag/4.0.1) (2016-03-29)
### Added
- Limit max event size to 1MB.
- Support event deduplication with eventUUID field.

### Fixed
- AdColony ads not reporting rewarded completion

## [4.0.0](https://github.com/deltaDNA/unity-sdk/releases/tag/4.0.0) (2016-03-15)
Initial version 4.0 release.
