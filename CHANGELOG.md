# Change Log

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
