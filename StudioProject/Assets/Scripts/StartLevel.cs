using UnityEngine;
using System.Collections;

public class StartLevel : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButton("Jump")){
			Application.LoadLevel ("Level01");
		}
	}
}
