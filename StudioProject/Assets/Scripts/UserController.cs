using UnityEngine;
using System.Collections;

public class UserController : MonoBehaviour
{

	public GameObject player;

	// Use this for initialization
	void Start ()
	{
	
		player = GameObject.Find ("PlayerObject");
	}

	int c = 0;
	
	/**
	 * This is the per-frame function for reading and using the controller state.
	 * We want to apply any actions done to our owned PlayerObject
	 */
	void FixedUpdate ()
	{
	
		// TODO implement controller usage.
		if (c++ < 10)
			player.GetComponent<PlayerObjectController> ().signalJump ();
		//if (c >= 700 && c++ < 1500)
		//player.GetComponent<PlayerObjectController> ().signalLeft ();

	}
}
