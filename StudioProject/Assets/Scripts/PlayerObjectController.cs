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
	 * Reference to the empty object below the player for collision
	 */
	public Transform below;

	/**
	 * The y value of the player from the previous frame. Used to compare with
	 * current frame's y value and determine apex of jump.
	 */
	float yPrevious = 0.0f;

	/**
	 * Stores and holds animation state until it is changed by a signal.
	 */
	int currentAnimState = 0;

	/**
	 * boolean to define which direction the character is facing. Defaults to 
	 * right-facing at start of game.
	 */
	bool rightFacing = true;

	/**
	 * A private reference to the RigidBody2D that this PlayerObject uses.  This
	 * reference means that we do not have to repeatedly use GetComponent to
	 * reacquire the rigid body.
	 */
	Rigidbody2D rbody;

	/**
	 * A private reference to the animation controller that this PlayerObject uses.
	 */
	Animator animator;
	
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
		// If the player is not on the groumd, check the jump arc and update the animation
		// accordingly
		if (isOnGround () == false) {

			this.checkFalling (this.rbody.position.y);
		}

		// Otherwise, if the player is not moving, go to the idle animation
		else if (!isMoving ()) { 

			this.setAnimIdle ();
		}
	}

	/**
	 * Called when the player object collides with another object.  Used to set animation state
	 * when landing on the ground, as well as for hitting death triggers
	 */
	void OnCollisionEnter2D (Collision2D col)
	{
		// If the player object is now grounded
		if (this.isOnGround ()) {

			// If the player object is moving, set to the moving animation
			if (this.isMoving ()) {

				this.setAnimMove ();
			}

			// Otherwise go to the idle animation
			else {

				this.setAnimIdle ();
			}
		}
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

		// Apply the walking thrust as a force to the rigid body
		this.rbody.AddForce (walkThrust * Vector2.right);

		// Turn on the rightFacing flag so that animations face right
		rightFacing = true;

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

		// Apply the walking thrust as a force to the rigid body
		this.rbody.AddForce (-walkThrust * Vector2.right);

		// Flip the rightFacing flag so that animations face left
		rightFacing = false;

		// Turns on the "move" animation state
		setAnimMove ();

	}

	/**
	 * Returns true if the object is on the ground, or false otherwise.
	 */
	bool isOnGround ()
	{
		return Physics2D.Raycast (new Vector2 (below.position.x, below.position.y), -Vector2.up, 0.001f);
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
		if (this.isOnGround ()) {

			// Apply the jumping thrust as a force to the rigid body
			this.rbody.AddForce (jumpThrust * Vector2.up);

			// Turns on the "jump" animation state
			setAnimJump ();
		}
	}

	/**
	 * Compares Y value for current frame to Y value from previous frame
	 * to determine if character is falling instead of ascending, and then sets
	 * the new animation state of the object
	 */
	void checkFalling (float yCurrent)
	{
		// Check that the current y position is less than the previous,
		// if true, then the object is falling
		if (yCurrent < yPrevious) {

			setAnimFall ();
		}

		// Capture the current y position for the next check
		yPrevious = yCurrent;
	}

	/**
	 * Returns true if the player object is moving in the x direction above a small
	 * threshold and on the ground.
	 */
	public bool isMoving ()
	{
		return (Mathf.Abs (this.rbody.velocity.x) >= 0.00001);
	}

	/**
	 * The following "setAnim" classes set an animation state for the PlayerObject,
	 * which in turn triggers an associated animation:
	 * -5, 5 = idle left, right
	 * -1, 1 = moving left, right
	 * -2, 2 = jump up left, right
	 * -3, 3 = jump down left, right
	 * 4     = death
	 */

	/**
	 * Sets the player's animation state to the idle animation.
	 * Handles finding the directional version of the animation.
	 */
	public void setAnimIdle ()
	{
		// Figure out the correct version for checks and sets
		int aniState = (rightFacing ? 5 : -5);

		// If we're already in the animation, don't reenter it
		if (this.currentAnimState != aniState) {

			// Set the currentAnimState to our new state
			this.currentAnimState = aniState;

			// Sets the animation state
			animator.SetInteger ("AnimState", this.currentAnimState);
		}
	}

	/**
	 * Sets the player's animation state to the moving animation.
	 * Handles finding the directional version of the animation.
	 * Assumes the player object is already grounded.
	 */
	public void setAnimMove ()
	{
		// Figure out the correct version for checks and sets
		int aniState = (rightFacing ? 1 : -1);

		// If we're already in the animation, don't reenter it
		if (this.currentAnimState != aniState) {

			// Set the currentAnimState to our new state
			this.currentAnimState = aniState;

			// Sets the animation state
			animator.SetInteger ("AnimState", this.currentAnimState);
		}
	}

	/**
	 * Sets the player's animation state to the jumping animation.
	 * Handles finding the directional version of the animation.
	 * Assumes the player object is already ungrounded.
	 */
	public void setAnimJump ()
	{
		// Figure out the correct version for checks and sets
		int aniState = (rightFacing ? 2 : -2);

		// If we're already in the animation, don't reenter it
		if (this.currentAnimState != aniState) {

			// Set the currentAnimState to our new state
			this.currentAnimState = aniState;

			// Sets the animation state
			animator.SetInteger ("AnimState", this.currentAnimState);
		}
	}

	/**
	 * Sets the player's animation state to the falling animation.
	 * Handles finding the directional version of the animation.
	 * Assumes the player object is already ungrounded.
	 */
	public void setAnimFall ()
	{
		// Figure out the correct version for checks and sets
		int aniState = (rightFacing ? 3 : -3);

		// If we're already in the animation, don't reenter it
		if (this.currentAnimState != aniState) {

			// Set the currentAnimState to our new state
			this.currentAnimState = aniState;

			// Sets the animation state
			animator.SetInteger ("AnimState", this.currentAnimState);
		}
	}
}