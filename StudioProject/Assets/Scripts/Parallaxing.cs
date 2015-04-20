using UnityEngine;
using System.Collections;

public class Parallaxing : MonoBehaviour {

	public Transform[] backgrounds; 	//List of backgrounds and forgrounds being parallaxed
	private float[] parallaxScales; 	//Proportion of camera's movement to move grounds
	public float smoothing = 1f; 		//Parallaxing amount - how smooth the parallax will be (set above 0)

	private Transform cam;				//Reference to main camera's transform
	private Vector3 previousCamPos; 	//Store the position of the camera in the previous frame

	//Called before start, for references
	void Awake (){
		//Setup cam reference
		cam = Camera.main.transform;
	}

	// Used for initialization
	void Start () {
		//Store previous frame with the current frames camera pos
		previousCamPos = cam.position;
		parallaxScales = new float[backgrounds.Length];
		//Assigning corresponding parallax scales
		for (int i = 0; i < backgrounds.Length; i++) {
			parallaxScales[i] = backgrounds[i].position.z*-1;
		}
	}
	
	// Update is called once per frame
	void Update () {
		//For each background
		for (int i = 0; i < backgrounds.Length; i++) {
			//The parallax is the opposite of the camera movement
			float parallax = (previousCamPos.x - cam.position.x) * parallaxScales[i];
			//Set a target x position(current position + parallax)
			float backgroundTargetPosX = backgrounds[i].position.x + parallax;
			//Create a target position(background current position with its target x pos)
			Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, backgrounds[i].position.y, backgrounds[i].position.z);
			//Fade between current position and target pos using lerp
			backgrounds[i].position = Vector3.Lerp (backgrounds[i].position, backgroundTargetPos, smoothing * Time.deltaTime);
		}
		//Set previousCamPos to the camera's position at end of frame
		previousCamPos = cam.position;
	}
}
