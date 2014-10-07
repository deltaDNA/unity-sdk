
3.2 (2014/10/07)
================

Changes:
	
	* The `Init` method has be deprecated in favour of `StartSDK`, this will be removed in a future release.
	* The `TriggerEvent` method has been deprecated for `RecordEvent`, this will be removed in a future release.

Features:

	* `StartSDK` can be called during a game with a different User ID.
	* `StopSDK` method added, which sends a 'gameEnded' event before stopping background uploads.
	* `NewSession` will generate a new session ID for subsequent events.

