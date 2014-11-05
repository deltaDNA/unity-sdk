using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerController : MonoBehaviour 
{
	public float speed;
	public GUIText countText;
	public GUIText winText;
	private int score;
	private int pickups;
	private float secondsPlayed;
	private bool winnerFound;
	public DeltaDNA.SDK ddsdk;
	
	void Start()
	{
		score = 0;
		pickups = GameObject.FindGameObjectsWithTag("PickUp").Length;
		secondsPlayed = 0;
		SetCountText();
		winText.text = "";
		winnerFound = false;

		// -- Set up DeltaDNA SDK -- //
		ddsdk = DeltaDNA.SDK.Instance;

		ddsdk.Settings.DebugMode = true;
		ddsdk.Settings.BackgroundEventUploadRepeatRateSeconds = 10;

		ddsdk.StartSDK(
			"47331089073395586423522795814123",
            "http://collect2968rllbl.deltadna.net/collect/api",
            //"http://engage2968rllbl.deltadna.net",
			"http://192.168.30.72:9000/qa/engage",
            DeltaDNA.SDK.AUTO_GENERATED_USER_ID
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
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");

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
					
				ddsdk.RequestImageMessage("gameEnded", engageParams);
			}

			if (score % 3 == 0) {

				var engageParams = new Dictionary<string, object>() {
					{ "userScore", score },
					{ "secondsPlayed", Math.Floor(secondsPlayed) }
				};
					
				ddsdk.RequestImageMessage("pickUp", engageParams);
			}
		}
	}
	
	void SetCountText()
	{
		countText.text = String.Format("{0,2}s Count: {1}", Math.Floor(secondsPlayed), score);
		if (winnerFound) {
			winText.text = "YOU WIN!";
		}
	}

}
