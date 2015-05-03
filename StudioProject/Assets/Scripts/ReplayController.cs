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
		var dirInfo = new DirectoryInfo ("./Replays");

		// Get an array of all of the FileInfo structs in the directory
		var files = dirInfo.GetFiles ();

		// Initialize the array list with the size of the files array
		// By initializing it with the proper size, we will suffer no slowdowns
		// to the expansion of this list.
		replays = new ArrayList (files.Length);

		// For each file in the array:
		foreach (var file in files) {

			ReplayPlayer replay = new ReplayPlayer ();

			// Open a stream reader for the text file
			replay.reader = file.OpenText ();

			// TODO create a new player object for each reader
			replay.controller = null;

			// Add the reader to the list of readers
			replays.Add (replay);
		}
	}
	
	// FixedUpdate is called once per frame
	void FixedUpdate ()
	{

		// Process each replay object for this frame
		foreach (ReplayPlayer replay in replays) {

			// Get the next line in the file
			string line = replay.reader.ReadLine ();

			// If the line is not empty and there is a controller
			if (line != null && replay.controller != null) {

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
