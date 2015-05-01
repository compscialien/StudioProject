﻿using UnityEngine;
using System.Collections;

public class EnemyPatrol : MonoBehaviour {

	/**
	 * Sets the walking speed of the enemy object.  Editable in the Unity
	 * editor directly.  Should not be changed in code.
	 * 
	 * Default should be 1, which is used as a bound on the x velocity.
	 */

	public float walkSpeed = 1f;

	/**
	 * When the enemy object begins patrolling, it will continue walking 
	 * this many "pixels" before turning around.
	 */
	public float patrolDistance = 32f;
	

	/** Sets the direction that the enemy should be walking when it starts.
	 */
	public bool movingRight = true;

	// Whether or not the enemy object is rotated 180 degrees in Z
	bool upsideDown = false;

	/* The minimum X position of this enemy object; set in the Start() method
	 */
	float minimumX;

	/* The maximum X position of this object
	 */
	float maximumX;

	/**
	 * A private reference to the animation controller that this enemy object uses.
	 */
	//Animator animator;


	// Use this for initialization
	void Start () {
		
		// Sets the variable animator to the animation controller assigned to this enemy object
		//animator = this.GetComponent<Animator> ();

		if (movingRight) {
			minimumX = this.transform.position.x;
			maximumX = minimumX + ( patrolDistance / 100 );
		} else {
			minimumX = this.transform.position.x - ( patrolDistance / 100 );
			maximumX = this.transform.position.x;
		}

		Debug.Log("Z rotation = " + this.transform.eulerAngles.z);

		if (this.transform.eulerAngles.z >= 90) {

			upsideDown = true;
			Vector3 theScale = transform.localScale;
			theScale.x *= -1;
			transform.localScale = theScale;

		}



	}
	
	// Update is called once per frame
	void Update () {

		if (movingRight)
			transform.Translate (Vector2.right * walkSpeed * Time.deltaTime);
		else
			transform.Translate (-Vector2.right * walkSpeed * Time.deltaTime);
		
		if(transform.position.x >= maximumX) {
			Flip ();
			// Without this next line, it is possible for the object to get stuck
			// just barely beyond maximumX, resulting in a vibrating effect
			transform.position = new Vector2 (maximumX, this.transform.position.y);
		}
		
		if(transform.position.x <= minimumX) {
			Flip ();
			// Without this next line, it is possible for the object to get stuck
			// just barely beyond minimumX, resulting in a vibrating effect
			transform.position = new Vector2 (minimumX, this.transform.position.y);
		}
		
		
	}

	void Flip()
	{
		// Switch the way the player is labeled as facing
		movingRight = !movingRight;
		
		// Multiply the player's x local scale by -1
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
