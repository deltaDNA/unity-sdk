using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using DeltaDNA;
using DeltaDNA.Messaging;

public class PlayerController : MonoBehaviour 
{
	public float speed;
	public GUIText countText;
	public GUIText winText;
	private int score;
	private int pickups;
	private float secondsPlayed;
	private bool winnerFound;
	public SDK ddsdk;
	
	void Start()
	{
		score = 0;
		pickups = GameObject.FindGameObjectsWithTag("PickUp").Length;
		secondsPlayed = 0;
		SetCountText();
		winText.text = "";
		winnerFound = false;

		// -- Set up DeltaDNA SDK -- //
		ddsdk = SDK.Instance;

		ddsdk.Settings.DebugMode = true;
		ddsdk.Settings.BackgroundEventUploadRepeatRateSeconds = 10;
		
		// You don't want to do this, Collect will reject the events.
//		ddsdk.SetTimestampFunc(() => {
//			//DateTime dt = new DateTime(0);
//			//return dt;
//			return null;
//		});
		ddsdk.UseCollectTimestamp(true);

		ddsdk.HashSecret = "1VLjWqChV2YC1sJ4EPKGzSF3TbhS26hq";

		ddsdk.StartSDK(
			"76410301326725846610230818914037",
			"http://collect2470ntysd.deltadna.net/collect/api",
			"http://engage2470ntysd.deltadna.net",
			//"http://www.deltadna.net/qa/engage",
            SDK.AUTO_GENERATED_USER_ID
       	);
	}

	void Update ()
	{
		if (!winnerFound) {
				secondsPlayed += Time.deltaTime;
		}
		SetCountText();
	}
	
	void FixedUpdate()
	{
		#if UNITY_EDITOR || UNITY_WEBPLAYER
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		#else
		float moveHorizontal = Input.acceleration.x;
		float moveVertical = Input.acceleration.y;
		#endif

		Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
		
		rigidbody.AddForce(movement * speed * Time.deltaTime);
	}
	
	void OnTriggerEnter(Collider other) 
	{
		// do some engage magik!!
		if (other.gameObject.tag == "PickUp")
		{
			other.gameObject.SetActive(false);
			score += 1;
			SetCountText();

			ddsdk.RecordEvent("pickUp", new Dictionary<string, object>() {
				{ "userScore", score },
				{ "secondsPlayed", Math.Floor(secondsPlayed) }
			});

			if (score >= pickups) {
				winnerFound = true;
				ddsdk.RecordEvent("gameEnded", new Dictionary<string, object>() {
					{ "userScore", score },
					{ "secondsPlayed", Math.Floor(secondsPlayed) }
				});

				// Ask engage how well we did
				var engageParams = new Dictionary<string, object>() {
					{ "userScore", score },
					{ "secondsPlayed", Math.Floor(secondsPlayed) }
				};
					
				IPopup gameEndedPopup = new Popup();
				gameEndedPopup.AfterPrepare += new EventHandler(OnPopupLoaded);
				gameEndedPopup.BeforeClose += new EventHandler(OnGameEnded);
				ddsdk.RequestImageMessage("gameEnded", engageParams, gameEndedPopup, (response) => {
					Debug.Log ("gameEnded response: "+DeltaDNA.MiniJSON.Json.Serialize(response));
				});
			}

			if (score % 3 == 0) {

				var engageParams = new Dictionary<string, object>() {
					{ "userScore", score },
					{ "secondsPlayed", Math.Floor(secondsPlayed) }
				};
					
				// Create Popup Object
				IPopup myPopup = new Popup();
				// Setup Events
				myPopup.AfterPrepare += (sender, e) => {
					Debug.Log("Popup loaded resource");
					// Just show it, although you could do this later
					myPopup.Show();
				};

				myPopup.Dismiss += (sender, e) => {
					Debug.Log("Popup dismissed by "+e.ID);
				};

				myPopup.Action += (sender, e) => {
					Debug.Log("Popup actioned by "+e.ID+" with command "+e.ActionValue);
				};
				// Start Request
				ddsdk.RequestImageMessage("pickUp", engageParams, myPopup);
			}
		}
	}
	
	void SetCountText()
	{
		countText.text = String.Format("{0,2}s Remaining: {1}", Math.Floor(secondsPlayed), pickups - score);
		if (winnerFound) {
			winText.text = "YOU WIN!";
		}
	}

	void OnPopupLoaded(object sender, EventArgs e)
	{
		((IPopup)sender).Show();
	}

	void OnGameEnded(object sender, EventArgs e)
	{
		Application.LoadLevel(Application.loadedLevelName);
	}

}
