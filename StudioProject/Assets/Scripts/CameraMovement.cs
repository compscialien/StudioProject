using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	// Amount of time to wait before camera starts moving (optional)
	public float delay = 0;
	float startTime;

	// How fast the camera is moving. Default is 1, set new
	// values in GUI
	public float cameraSpeed = 1;


	// Use this for initialization
	void Start () {
		startTime = Time.time + delay;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > startTime) {
			transform.Translate (Vector2.right * cameraSpeed * Time.deltaTime);
		}
	}
}
