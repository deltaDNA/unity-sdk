# Change Log

## [4.2.11](https://github.com/deltaDNA/unity-sdk/releases/tag/4.2.11) (YYYY-MM-DD)
### Changed
- Updated Chartboost network dependency on Android.

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
