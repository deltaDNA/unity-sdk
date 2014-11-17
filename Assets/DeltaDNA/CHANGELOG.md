3.3 (2014/11/05)
================

Changes:

    * Moved our copy of MiniJSON inside DeltaDNA namespace to avoid namespace clashes.
    * If Collect and/or Engage URL's don't start with 'http://' I slightly prepend it.

Features:

    * Added support for rich messaging.  Call `RequestImageMessage` to display a popup image
        from an engagement.

3.2.1 (2014/10/14)
==================

Changes:

	* No longer need to wait for an Engagement to finish before making another.

3.2 (2014/10/07)
================

Changes:

	* The `Init` method has be deprecated in favour of `StartSDK`, this will be removed in a future release.
	* The `TriggerEvent` method has been deprecated for `RecordEvent`, this will be removed in a future release.

Features:

	* `StartSDK` can be called during a game with a different User ID.
	* `StopSDK` method added, which sends a 'gameEnded' event before stopping background uploads.
	* `NewSession` will generate a new session ID for subsequent events.
