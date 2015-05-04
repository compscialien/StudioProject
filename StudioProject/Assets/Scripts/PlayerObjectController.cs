using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]

public class PlayerObjectController : MonoBehaviour
{
	// Bring in the screaming audio
	public AudioClip clip;

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
	 * A vector that controls drag in a vector that is convoluted with velocity.
	 * Each component is a linear scale of the amount of speed to mantain on each
	 * update.
	 * This is in scale with the linear drag of the rigidbody.
	 */
	public Vector2 convolutionDrag;

	/**
	 * Reference to the empty object below the player for collision
	 */
	public Transform below;

	/**
	 * Reference to the empty object to the right of the player for collision
	 */
	public Transform right;

	/**
	 * Reference to the empty object to the left of the player for collision
	 */
	public Transform left;

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
	 * Stores whether the object was moving in this/the last frame.
	 */
	bool movingThisFrame = false;
	bool movingLastFrame = false;

	/**
	 * Stores whether this player is dead
	 */
	[HideInInspector]
	public bool
		isDead = false;

	/**
	 * Flag of whether this instance is a replay or not, set by the controller that
	 * owns the object
	 */
	[HideInInspector]
	public bool
		isReplay;

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
	 * This update function is called as fast as possible.  Used for animation and non-gameplay updates
	 * so that they can be updated as soon as possible.
	 */
	void Update ()
	{
		// If the player is not on the groumd, check the jump arc and update the animation
		// accordingly
		if (!this.isOnGround ()) {
			
			this.checkFalling (this.rbody.position.y);
		}
		
		// Otherwise, if the player is not moving, go to the idle animation
		else if (!isMoving ()) { 
			
			this.setAnimIdle ();
		}
	}

	/**
	 * This is the per-frame update function.  Used to apply the convolution drag.
	 */
	void FixedUpdate ()
	{
		this.rbody.velocity = Vector2.Scale (this.rbody.velocity, this.convolutionDrag);
	}

	/**
	 * Called when the player object collides with another object.  Used to set animation state
	 * when landing on the ground
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
	 * Called when the player enters a trigger - the death trigger zones
	 */
	void OnTriggerEnter2D (Collider2D other)
	{
		// The player entered a Death flag
		if (other.tag == "Death" && !this.isDead) {

			this.signalDeath ();
		}
	}

	/**
	 * This update function runs after all other update functions.
	 */
	void LateUpdate ()
	{
		// If we are moving left and can't or are moving right and can't, set velocity to 0.
		if (((this.cantMoveLeft () && this.rbody.velocity.x < 0)
			|| (this.cantMoveRight () && this.rbody.velocity.x > 0))
			|| (!this.movingLastFrame && !this.movingThisFrame)) {

			this.rbody.velocity = new Vector2 (0, this.rbody.velocity.y);
		}

		// Copy current moving state to last state position
		this.movingLastFrame = this.movingThisFrame;
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

		// If we can't move right due to an obstruction, or if the player is dead, leave the function
		if (this.cantMoveRight () || this.isDead) {

			return;
		}

		// Apply the walking thrust as a force to the rigid body
		//this.rbody.AddForce (walkThrust * Vector2.right);
		this.rbody.velocity = new Vector2 (this.walkSpeed, this.rbody.velocity.y);

		// Turn on the rightFacing flag so that animations face right
		this.rightFacing = true;

		this.movingThisFrame = true;

		// Turns on the "move" animation state if the player is grounded
		if (this.isOnGround ()) {

			this.setAnimMove ();
		}
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

		// If we can't move left due to an obstruction, or if the player is dead, leave the function
		if (this.cantMoveLeft () || this.isDead) {

			return;
		}

		// Apply the walking thrust as a force to the rigid body
		//this.rbody.AddForce (-walkThrust * Vector2.right);
		this.rbody.velocity = new Vector2 (-this.walkSpeed, this.rbody.velocity.y);

		// Flip the rightFacing flag so that animations face left
		this.rightFacing = false;

		this.movingThisFrame = true;

		// Turns on the "move" animation state if the player is grounded
		if (this.isOnGround ()) {

			this.setAnimMove ();
		}
	}

	/**
	 * Signals that this object has died
	 */
	void signalDeath ()
	{
		// Play the death audio only if this is not in replay mode
		if (!isReplay) {

			// Plays the audio at the player position
			AudioSource.PlayClipAtPoint (clip, transform.position);
		}

		// Sets the velocity to zero to stop the player
		this.rbody.velocity = Vector2.zero;

		// Sets the isDead flag to stop certain functions from triggering
		this.isDead = true;

		// Sets the animation state to the dead state
		this.setAnimDead ();
	}

	/**
	 * Returns true if the object is on the ground, or false otherwise.
	 */
	bool isOnGround ()
	{
		return Physics2D.Raycast (new Vector2 (below.position.x, below.position.y), -Vector2.up, 0.001f);
	}

	/**
	 * Returns true if there is nothing directly to the right of the player.
	 */
	bool cantMoveRight ()
	{

		return Physics2D.Raycast (new Vector2 (right.position.x, right.position.y), Vector2.right, 0.001f);
	}

	/**
	 * Returns true if there is nothing directly to the left of the player.
	 */
	bool cantMoveLeft ()
	{

		return Physics2D.Raycast (new Vector2 (left.position.x, left.position.y), -Vector2.right, 0.001f);
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
		if (this.isOnGround () && !isDead) {

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
		return (Mathf.Abs (this.rbody.velocity.x) >= 0.001);
	}

	/**
	 * The following "setAnim" classes set an animation state for the PlayerObject,
	 * which in turn triggers an associated animation:
	 * -5, 5 = idle left, right
	 * -1, 1 = moving left, right
	 * -2, 2 = jump up left, right
	 * -3, 3 = jump down left, right
	 *  4    = death
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
		if (this.currentAnimState != aniState && !this.isDead) {

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
		if (this.currentAnimState != aniState && !this.isDead) {

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
		if (this.currentAnimState != aniState && !this.isDead) {

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
		if (this.currentAnimState != aniState && !this.isDead) {

			// Set the currentAnimState to our new state
			this.currentAnimState = aniState;

			// Sets the animation state
			animator.SetInteger ("AnimState", this.currentAnimState);
		}
	}

	/**
	 * Sets the player's animation state to the dead animation.
	 */
	public void setAnimDead ()
	{
		// Figure out the correct version for checks and sets
		int aniState = 4;

		// If we're already in the animation, don't reenter it
		if (this.currentAnimState != aniState) {

			// Set the currentAnimState to our new state
			this.currentAnimState = aniState;

			// Sets the animation state
			animator.SetInteger ("AnimState", this.currentAnimState);
		}
	}
}