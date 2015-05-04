using UnityEngine;
using System.Collections;
using System.IO;

public class ReplayController : MonoBehaviour
{

	/**
	 * The array list of replays that this controller will run
	 */
	ArrayList replays;

	/**
	 * Initialize the Replay controller by opening every available replay file in the 
	 * replay folder.
	 */
	void Start ()
	{
		// Get the info for the local Replays directory
		// Note that if the directory does not exist, an exception is thrown that
		// causes a NullPointerException with replays during FixedUpdate()
		var dirInfo = new DirectoryInfo ("./Replays");

		// Get an array of all of the FileInfo structs in the directory
		var files = dirInfo.GetFiles ();

		// A reference to the first existing player object to be found and
		// duplicated in the loop
		GameObject firstPlayer = null;

		// Initialize the array list with the size of the files array
		// By initializing it with the proper size, we will suffer no slowdowns
		// to the expansion of this list.
		replays = new ArrayList (files.Length);

		// For each file in the array:
		foreach (var file in files) {

			ReplayPlayer replay = new ReplayPlayer ();

			// Open a stream reader for the text file
			replay.reader = file.OpenText ();

			// If firstPlayer is null, then this is the first replay we've loaded
			if (firstPlayer == null) {

				// Find the player game object in the scene
				firstPlayer = GameObject.Find ("PlayerObject");

				// Get the first game object's controller
				replay.controller = firstPlayer.GetComponent<PlayerObjectController> ();
			}

			// Otherwise, we need to copy the firstPlayer object
			else {

				// This makes an exact copy of the firstPlayer object
				GameObject clone = Instantiate (firstPlayer);

				// This function handles getting its controller
				replay.controller = clone.GetComponent<PlayerObjectController> ();
			}

			// Set the replay controller to be true
			replay.controller.isReplay = true;

			// Add the reader to the list of readers
			replays.Add (replay);
		}
	}
	
	// FixedUpdate is called once per frame
	void FixedUpdate ()
	{
		// Living object flag set to true when an object is alive
		bool atLeastOneAlive = false;

		// Process each replay object for this frame
		foreach (ReplayPlayer replay in replays) {

			// Get the next line in the file
			string line = replay.reader.ReadLine ();

			// If the line is not empty and there is a controller and it is alive
			if (line != null && replay.controller != null && !replay.controller.isDead) {

				// Set the flag that at least one object is alive
				atLeastOneAlive = true;

				// If the line contains an "l"
				if (line.Contains ("l")) {

					// Signal that this player should move left
					replay.controller.signalLeft ();
				}

				// If the line contains an "r"
				if (line.Contains ("r")) {

					// Signal that this player should move right
					replay.controller.signalRight ();
				}

				// If the line contains an "j"
				if (line.Contains ("j")) {

					// Signal that this player should jump
					replay.controller.signalJump ();
				}
			}
		}

		// If there are no more objects still alive
		if (!atLeastOneAlive) {

			// We want to reload the level if there's none alive
			Application.LoadLevel ("Level01Replay");
		}
	}

	/**
	 * This structure links the stream reader of a file to the player object
	 * controller that will be receiving signals from the file.
	 */
	struct ReplayPlayer
	{
		/**
		 * The reader for this particular replay to get commands and signals from.
		 */
		public StreamReader reader;

		/**
		 * The reference to the object controller of the player governed by
		 * this replay.
		 */
		public PlayerObjectController controller;
	}
}
