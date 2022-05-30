# Changelog

## [6.0.3]

### Fixed
- Event trigger counts will now be correctly persisted across sessions
- iOS Rich Push Notifications will build correctly when project is moved between different systems between Unity and Xcode builds

## [6.0.2]

### Fixed
- When switching environments between Dev and Live, data will be cleaned so that events are not sent to the wrong environment during testing
- Fixed a potential bug in Android push notifications where opening a notification on the latest version of Android could cause an exception

## [6.0.1]

### Fixed
- Fixed a bug where in certain cases devices wouldn't send events from locations requiring PIPL consent, even if consent was provided.

## [6.0.0]

### New

**Breaking Change**: New APIs provided for checking if PIPL consent is required, and recording users' consent. Note that
it is now required to check if PIPL consent is required, and provide that consent if necessary, before collect and engage
requests will be sent from the SDK.

### Fixed
- iOS notifications will show in the foreground as previously on devices running iOS 10+

## [5.0.10-preview]

### Fixed
- notificationOpened events will now be sent correctly when a notification is opened on iOS 10+
- Bundle identifier for notification services will be properly respected in Xcode

### Known Issues
- When upgrading from a previous version, you may get warnings about changes in GUIDs for folders within the SDK. These should have
no effect on the behaviour of the SDK, and should be able to be safely dismissed. These should only occur on upgrade to this version,
and should not occur in subsequent upgrades or usage.

## [5.0.9-preview] - 2021-08-30

### Fixed 
- Xcode builds will now default to the non-legacy build system on supported Unity versions

## [5.0.8-preview] - 2021-07-06

### Fixed
- Fixed issue where entering and leaving Play Mode with Domain Reload disabled would prevent SDK from starting on returning to Play Mode
- Timezone offset now InvariantCulture. 
- Locale fix so backend doesn't reject country codes.
- Expired credentials removed from Android Notifications gradle template

**NOTE:** After updating to this version of the SDK you will need to manually add your GitHub Packages credentials to the generated gradle template when using Android push notifications.
More details can be found in the README.

## [5.0.7-preview] - 2021-04-29

### Changed
- Use Unity's in-built gradle instead of Unity Jar Resolver for resolving android dependencies (due to bintray deprecation, reconfiguring android notifications required)

### Fixed
- Correctly report IDFA in signal events on iOS 14.5+

## [5.0.6-preview] - 2021-04-27

### Fixed
- Fixed a crash that occurred when using a non-ISO combination of language and country codes

## [5.0.5-preview] - 2021-04-23

### New
- Signal purchase method will now send a verifiable transaction 

## [5.0.4-preview] - 2021-04-16

### Fixed
- Fixed issue where parsing of some cached dates would throw an exception for certain user cultures
- Fixed PlayServicesResolver being placed within the DeltaDNA folder instead of at the project root
- Fixed default setting for "use application version" being false instead of true
- Fixed some legal terms in LICENSE and README files
- Fixed issue in Audience Pinpointer helpers where the ATT status was not reported correctly

## [5.0.2-preview] - 2021-02-18

### Fixed
- BasicExample scene has been reinstated inside the Samples folder.
- Android dependency has been updated to work the latest version of the Android SDK (4.13.4).
- Fixed an issue where device locales were not correctly identified.

## [5.0.1-preview] - 2021-01-13

### Fixed
- Various warnings and errors resolved.
- Android notifications credentials will now be correctly passed to the built project (note that you may need to reconfigure android notifications in the deltaDNA control panel when upgrading to v5)
- Included missing parameters at SDK start-up to enable use of Session Parameters for campaign segmentation.

## [5.0.0-preview] - 2020-09-11

NOTE: Some files have been moved in this release. If you have trouble uprading, please delete your existing version of the SDK and reinstall cleanly.

### Changed
- Package structure has been adjusted so that all relevant files are contained by the deltaDNA folder for convenience and to support assembly definitions
- SDK configuration is now stored in a ScriptableObject (Assets/Resources/ddna_configuration.asset) instead of an XML file

### Added
- Events Manager for simple event and parameter creation
- Automatic 'heartbeat' event that is recorded every 60 seconds to ensure sessions are tracked more accurately
- Automatic starting of new session if game is paused (e.g. mobile app sent to background) for more than 5 minutes
- Automatic recording of gameEnded event with 'paused' state when game is paused
- Automatic recording of gameEnded event with 'stopped' state when game is exited (NOTE: this will not be uploaded until the game is started again later)
- Assembly Definition files for deltaDNA and deltaDNA.Editor assemblies

### Fixed
- Errors when attempting to build for Android API level greater than 28

### Removed
- Obsolete SmartAds files
