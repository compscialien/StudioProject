using UnityEngine;
using System.Collections;

public class PlayerObjectController : MonoBehaviour
{

	/**
	 * This is the acceleration that walking has.  Higher value means higher
	 * acceleration.
	 * 
	 * Default should be 100, which provides enough thrust to real max speed
	 * right away.
	 */
	public float walkThrust;

	/**
	 * Sets the walking speed of the player object.  Editable in the Unity
	 * editor directly.  Should not be changed in code.
	 * 
	 * Default should be 1, which is used as a bound on the x velocity.
	 */
	public float walkSpeed;

	/**
	 * The upward force to add when jumping.  Editable in the Unity editor,
	 * should not be changed in code.
	 * 
	 * Default should be 100, which provides a good amount of thrust to jump
	 * up into the air.
	 */
	public float jumpThrust;

	/**
	 * The y value of the player from the previous frame. Used to compare with
	 * current frame's y value and determine apex of jump.
	 */
	public float yPrevious;

	/**
	 * Stores and holds animation state until it is changed by a signal.
	 */
	public int currentAnimState = 0;

	/**
	 * A private reference to the RigidBody2D that this PlayerObject uses.  This
	 * reference means that we do not have to repeatedly use GetComponent to
	 * reacquire the rigid body.
	 */
	Rigidbody2D rbody;

	/**
	 * A private reference to the animation controller that this PlayerObject uses.
	 */
	private Animator animator;
	
	/**
	 * Handles initialization.  Called when the PlayerObject is spawned.  Used to
	 * acquire any references required and run any set up code we may need.
	 */
	void Start ()
	{
	
		// Use GetComponent to get the rigid body of this PlayerObject
		this.rbody = GetComponent<Rigidbody2D> ();

		// Sets the variable animator to the animation controller assigned to this PlayerObject
		animator = this.GetComponent<Animator> ();
	}

	/**
	 * This is the per-frame update function.  Note that it used the timelocked FixedUpdate
	 * version, instead of Update().  This prevents issues with rigid bodies and the physics.
	 */
	void FixedUpdate ()
	{
		if (IsOnGround () == false) {
			checkJumpApex (this.rbody.position.y);
		}

		if (isMoving () == false && IsOnGround ()) {
			currentAnimState = 0;
		}
		animator.SetInteger ("AnimState", currentAnimState);

		//Debug.Log("real AnimState = " + animator.GetInteger("AnimState"));

	}

	/**
	 * This update function runs after all other update functions.  It is used to cap the max speed
	 * of the PlayerObject.
	 */
	void LateUpdate ()
	{

		// Get the velocity vector
		Vector2 velocity = this.rbody.velocity;

		// Find the maximum velocity in the x direction
		// Vector math accounts for direction as well
		Vector2 maxVelocity = new Vector2 (velocity.x, 0).normalized * walkSpeed;

		// The y value must be copied separately so it is not factored into the normalization
		// of the vector.
		maxVelocity.y = velocity.y;

		// Cap the maximum velocity
		if ((velocity.x > 0 && velocity.x > maxVelocity.x) || (velocity.x < 0 && velocity.x < maxVelocity.x)) {

			this.rbody.velocity = maxVelocity;
		}
	}

	/**
	 * Used by the UserController and ReplayController to make the PlayerObject move as desired.
	 * This function signals the player object to move to the right.
	 */
	public void signalRight ()
	{

		// Check that the rigidbody component was acquired in Start(), or throw an exception.
		if (this.rbody == null) {
			throw new System.NullReferenceException ("Rigidbody2D component not acquired at Start()");
		}

		// Get a copy of the player's local scale vector
		Vector3 ls = transform.localScale;



		// If the x scale is negative, meaning the object is facing left
		if (ls.x < 0) {
			
			// Reverse the x scaling to make the object face the other direction
			ls.x = -ls.x;
			
			// Save the new local scale vector to the transformation
			transform.localScale = ls;
		}

		// Apply the walking thrust as a force to the rigid body
		this.rbody.AddForce (walkThrust * Vector2.right);

		// Turns on the "move" animation state
		setAnimMove ();
	}

	/**
	 * Used by the UserController and ReplayController to make the PlayerObject move as desired.
	 * This function signals the player object to move to the left.
	 */
	public void signalLeft ()
	{
		
		// Check that the rigidbody component was acquired in Start(), or throw an exception.
		if (this.rbody == null) {
			throw new System.NullReferenceException ("Rigidbody2D component not acquired at Start()");
		}


		// Get a copy of the player's local scale vector
		Vector3 ls = transform.localScale;


		// If the x scale is positive, meaning the object is facing right
		if (ls.x > 0) {

			// Reverse the x scaling to make the object face the other direction
			ls.x = -ls.x;

			// Save the new local scale vector to the transformation
			transform.localScale = ls;
		}


		// Apply the walking thrust as a force to the rigid body
		this.rbody.AddForce (-walkThrust * Vector2.right);

		// Turns on the "move" animation state
		setAnimMove ();

	}

	/**
	 * Returns true if the object is on the ground, or false otherwise.
	 */
	bool IsOnGround ()
	{
		// TODO this currently is hitting something and always giving true
		// TODO make the vector check again y + image height converted to in-game units
		return Physics2D.Raycast (new Vector2 (transform.position.x, transform.position.y), -Vector2.up, 0.001f);
	}

	/**
	 * Used by the UserController and ReplayController to make the PlayerObject move as desired.
	 * This function signals the player object to jump.
	 */
	public void signalJump ()
	{

		// Check that the rigidbody component was acquired in Start(), or throw an exception.
		if (this.rbody == null) {
			throw new System.NullReferenceException ("Rigidbody2D component not acquired at Start()");
		}

		// Check if this object is on the ground
		if (this.IsOnGround ()) {

			// Apply the jumping thrust as a force to the rigid body
			this.rbody.AddForce (jumpThrust * Vector2.up);

			// Turns on the "jump" animation state
			setAnimJump();
		}
	}

	/**
	 * Compares Y value for current frame to Y value from previous frame
	 * to determine if character is falling instead of ascending
	 * 
	 * TODO this should only be called if PlayerObject is not on ground.
	 */
	public void checkJumpApex ( float yCurrent ) {
		if (yPrevious != null) {

			if (yCurrent < yPrevious) {
				setAnimFall();
			}

			yPrevious = yCurrent;

		}
	}

	public bool isMoving () {
		if (this.rbody.velocity.x != 0 && IsOnGround()) {
			Debug.Log("Moving");
			return true;

		} else {
			return false;
		}
	}

	/**
	 * The following "setAnim" classes set an animation state for the PlayerObject,
	 * which in turn triggers an associated animation:
	 * 0 = idle
	 * 1 = moving
	 * 2 = jump up
	 * 3 = jump down
	 * 4 = death
	 */

	public void setAnimIdle () {
		currentAnimState = 0;
	}

	public void setAnimMove () {
		currentAnimState = 1;
	}

	public void setAnimJump () {
		//if (!(this.IsOnGround ())) {
			currentAnimState = 2;
		//}
	}

	public void setAnimFall () {
		//if (!(this.IsOnGround ())) {
			currentAnimState = 3;
		//}
	}

}