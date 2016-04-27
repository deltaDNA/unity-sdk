# Change Log

## [Unreleased]() ()
### Added
- Added `InterstitialAd` and `RewardedAd` classes, which replace showing ads with the `DDNASmartAds`.  This idea is to split out the call to Engage and make it explicit.  This gives the game more flexibility in how it works with Engage to control if ads are to be shown to the player.
- `ImageMessage` class replaces the `Popup` interface and `BasicPopup`.  It's interface more closely matches how the ad classes work.
- Unity 5.3 has added support for some basic unit testing, so added a few tests.
### Changed
- `OnAdFailedToOpen` now reports the reason for the failure.
- Engage no longer returns null if the connection fails, it returns an empty dictionary.
- The `Engagement` records the response, http status code and any errors.  This works with an additional `RequestEngagement` method.
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
