using UnityEngine;
using System.Collections;

public class Scrolling : MonoBehaviour {

	public float speed = 0.05f;

	public float delay;

	float startTime;

	// Use this for initialization
	void Start () {
	
		startTime = Time.time + delay;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (Time.time > startTime) {
			Vector2 offset = new Vector2 ((Time.time - startTime) * speed, 0);
			GetComponent<Renderer> ().material.mainTextureOffset = offset;
		}
	}
}
