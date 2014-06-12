namespace DeltaDNA
{
	public static class Defaults
	{
		internal static readonly string SDK_VERSION = "Unity SDK v3.1";
		
		internal static readonly string ENGAGE_API_VERSION = "4";
		
		internal static readonly string EVENT_STORAGE_PATH = "{persistent_path}/ddsdk/events/";
		internal static readonly string ENGAGE_STORAGE_PATH = "{persistent_path}/ddsdk/engage/";
		internal static readonly string EVENT_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";
		internal static readonly string USERID_URL_PATTERN = "{host}/uuid";
		internal static readonly string COLLECT_URL_PATTERN = "{host}/{env_key}/bulk/hash/{hash}";
		internal static readonly string ENGAGE_URL_PATTERN = "{host}/{env_key}/hash/{hash}";
		
		static Defaults()
		{
			// defines default behaviour of the SDK
			
			DebugMode = false;
			ResetTest = false;
			
			OnFirstRunSendNewPlayerEvent = true;
			OnInitSendClientDeviceEvent = true;
			OnInitSendGameStartedEvent = true;
			
			HttpRequestRetryDelaySeconds = 2;
			HttpRequestMaxRetries = 5;
			
			BackgroundEventUpload = true;	// send events automatically by default
			BackgroundEventUploadStartDelaySeconds = 0;
			BackgroundEventUploadRepeatRateSeconds = 60;
		}
		
		/// <summary>
		/// Controls whether a 'newPlayer' event is sent the first time the game is played.
		/// </summary>
		public static bool OnFirstRunSendNewPlayerEvent { get; set; }
		
		/// <summary>
		/// Controls whether a 'clientDevice' event is sent after the Init call.
		/// </summary>
		public static bool OnInitSendClientDeviceEvent { get; set; }
		
		/// <summary>
		/// Controls whether a 'gameStarted' event is sent after the Init call.
		/// </summary>
		public static bool OnInitSendGameStartedEvent { get; set; }
		
		/// <summary>
		/// Controls if additional debug is output to the console.
		/// </summary>
		public static bool DebugMode { get; set; }
		
		/// <summary>
		/// If set, clear all persisted data on start up.
		/// </summary>
		public static bool ResetTest { get; set; }
		
		/// <summary>
		/// Controls the time in seconds between retrying a failed Http request.
		/// </summary>
		public static float HttpRequestRetryDelaySeconds { get; set; }
		
		/// <summary>
		/// Controls the number of times we retry an Http request before giving up.
		/// </summary>
		/// <value>The http request max retries.</value>
		public static int HttpRequestMaxRetries { get; set; }
		
		public static bool BackgroundEventUpload { get; set; }
		
		public static int BackgroundEventUploadStartDelaySeconds { get; set; }
		
		public static int BackgroundEventUploadRepeatRateSeconds { get; set; }
	}
}
