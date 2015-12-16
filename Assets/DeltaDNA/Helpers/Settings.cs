namespace DeltaDNA
{
	public class Settings
	{
		internal static readonly string SDK_VERSION = "Unity SDK v3.7.2";

		internal static readonly string ENGAGE_API_VERSION = "4";

		internal static readonly string EVENT_STORAGE_PATH = "{persistent_path}/ddsdk/events/";
		internal static readonly string ENGAGE_STORAGE_PATH = "{persistent_path}/ddsdk/engage/";
		internal static readonly string LEGACY_SETTINGS_STORAGE_PATH = "{persistent_path}/GASettings.ini";
		internal static readonly string EVENT_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";
		internal static readonly string USERID_URL_PATTERN = "{host}/uuid";
		internal static readonly string COLLECT_URL_PATTERN = "{host}/{env_key}/bulk";
		internal static readonly string COLLECT_HASH_URL_PATTERN = "{host}/{env_key}/bulk/hash/{hash}";
		internal static readonly string ENGAGE_URL_PATTERN = "{host}/{env_key}";
		internal static readonly string ENGAGE_HASH_URL_PATTERN = "{host}/{env_key}/hash/{hash}";

		private bool _debugMode = false;

		internal Settings()
		{
			// defines default behaviour of the SDK

			DebugMode = false;

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
		public bool OnFirstRunSendNewPlayerEvent { get; set; }

		/// <summary>
		/// Controls whether a 'clientDevice' event is sent after the Init call.
		/// </summary>
		public bool OnInitSendClientDeviceEvent { get; set; }

		/// <summary>
		/// Controls whether a 'gameStarted' event is sent after the Init call.
		/// </summary>
		public bool OnInitSendGameStartedEvent { get; set; }

		/// <summary>
		/// Controls if additional debug is output to the console.
		/// </summary>
		public bool DebugMode
		{
			get
			{
				return _debugMode;
			}
			set
			{
				Logger.SetLogLevel(value ? Logger.Level.DEBUG : Logger.Level.WARNING);
				_debugMode = value;
			}
		}

		/// <summary>
		/// Controls the time in seconds between retrying a failed Http request.
		/// </summary>
		public float HttpRequestRetryDelaySeconds { get; set; }

		/// <summary>
		/// Controls the number of times we retry an Http request before giving up.
		/// </summary>
		public int HttpRequestMaxRetries { get; set; }

		/// <summary>
		/// Controls if events are uploaded automatically in the background.
		/// </summary>
		public bool BackgroundEventUpload { get; set; }

		/// <summary>
		/// Controls how long after the <see cref="Init"/> call we wait before
		/// sending the first event upload.
		/// </summary>
		public int BackgroundEventUploadStartDelaySeconds { get; set; }

		/// <summary>
		/// Controls how fequently events are uploaded automatically.
		/// </summary>
		public int BackgroundEventUploadRepeatRateSeconds { get; set; }
	}
}
