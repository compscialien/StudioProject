using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class UserController : MonoBehaviour
{

	/**
	 * A reference to the game object of the player
	 */
	public GameObject player;

	/**
	 * The random generator used in generating file names
	 */
	UnityEngine.Random random;

	/**
	 * The UTF8 encoder used when writing the signals to the file
	 */
	UTF8Encoding encoding;

	/**
	 * The stream to the file for the replay of this run of the game
	 */
	FileStream stream;

	/**
	 * Initializes values used by the UserController
	 */
	void Start ()
	{
	
		// Find the player object in the scene
		player = GameObject.Find ("PlayerObject");

		// Initialize the random generator
		random = new UnityEngine.Random ();

		// Initialize the encoder
		encoding = new UTF8Encoding (true);

		// Create a stream to the file with a randomly generated name
		stream = new FileInfo ("./Replays/" + MakeRandomFileName ()).Create ();
	}

	/**
	 * Makes a random, unique file name that will be used for recording the signals
	 * from this object.
	 */
	string MakeRandomFileName ()
	{

		// The length of the random part of the file name
		const int length = 10;
		
		// All of the characters allowed in the filename
		const char[] characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxy0123456789";
		
		// The ending of the file name
		const string extension = ".txt";

		// Continue until we return from the function
		while (true) {

			// The char array of the selected characters to use
			char[] selected = new char[length];

			// Each letter of the char array must be set
			for (int i = 0; i < length; i++) {

				// Select a char from characters from [0, characters.Length) using the floored int value to eliminate
				// the potential decimal values from random.Range
				selected [i] = characters [Mathf.FloorToInt (random.Range (0, characters.Length))];
			}

			// Convert from char array to string and add the extension
			string filename = new string (selected) + extension;

			// Check if the filename is unique
			if (!File.Exists ("./Replays/" + filename)) {

				// Return the file name if it is unique
				return filename;
			}
		}
	}
	
	/**
	 * This is the per-frame function for reading and using the controller state.
	 * We want to apply any actions done to our owned PlayerObject
	 */
	void FixedUpdate ()
	{

		// The string of signals to write out to the file
		string signals = "";
	
		// If the user presses right on a joystick or the right arrow key or the 'd' key
		if (Input.GetAxis ("Horizontal") > 0) {

			// Send a right movement signal to the player object
			player.GetComponent<PlayerObjectController> ().signalRight ();

			// Add a "r" to the signals string
			signals = signals + "r";
		}

		// If the user presses left on a joystick or the left arrow key or the 'a' key
		else if (Input.GetAxis ("Horizontal") < 0) {

			// Send a left movement signal to the player object
			player.GetComponent<PlayerObjectController> ().signalLeft ();

			// Add a "l" to the signals string
			signals = signals + "l";
		}

		// If the user presses the 'space' key
		if (Input.GetButton ("Jump")) {

			// Send a jump movement signal to the player object
			player.GetComponent<PlayerObjectController> ().signalJump ();

			// Adds a "j" to the signals string
			signals = signals + "j";
		}

		// Use the correct version of the new line for the environment ("\n" or "\r\n")
		signals = signals + Environment.NewLine;

		// Convert the signals string into bytes encoded in UTF8
		Byte[] signalBytes = encoding.GetBytes (signals);

		// Write the signals string to a file
		stream.Write (signalBytes);
	}
}
