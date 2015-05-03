using UnityEngine;
using System.Collections;

public class UserController : MonoBehaviour
{

	public GameObject player;

	float restartTime;

	// Use this for initialization
	void Start ()
	{
	
		player = GameObject.Find ("PlayerObject");

		restartTime = -500.0f;
	}
	
	/**
	 * This is the per-frame function for reading and using the controller state.
	 * We want to apply any actions done to our owned PlayerObject
	 */
	void FixedUpdate ()
	{
	
		// If the user presses right on a joystick or the right arrow key or the 'd' key
		if (Input.GetAxis ("Horizontal") > 0) {

			// Send a right movement signal to the player object
			player.GetComponent<PlayerObjectController> ().signalRight ();
		}

		// If the user presses left on a joystick or the left arrow key or the 'a' key
		else if (Input.GetAxis ("Horizontal") < 0) {

			// Send a left movement signal to the player object
			player.GetComponent<PlayerObjectController> ().signalLeft ();
		}

		// If the user presses the 'space' key
		// TODO add joystick jump button
		if (Input.GetButton ("Jump")) {

			// Send a jump movement signal to the player object
			player.GetComponent<PlayerObjectController> ().signalJump ();
		}

		// If the owned player is dead
		if (player.GetComponent<PlayerObjectController> ().isDead) {

			if (restartTime < 0.0f) {

				restartTime = Time.time + 2.75f;
			}
			else if (Time.time >= restartTime) {

				// Reload the start screen level
				Application.LoadLevel ("StartScreen");
			}
		}
	}	
}
