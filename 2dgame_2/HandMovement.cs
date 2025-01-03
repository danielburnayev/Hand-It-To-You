using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMovement : MonoBehaviour {

    public CharacterController2D controller;
	public Animator animator;

    private Rigidbody2D handRigidbody;

    bool idle = true;
    bool jump = false;
    bool crouch = false;
    bool sprint = false;
    bool moving = false;
    bool underSomething = false;

    private void Awake() {
        handRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start() {

    }

    private void Update() {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
            handRigidbody.velocity = new Vector2(0.0f, 3.14f);
        }
    }
    
    private void FixedUpdate() {

    }

    public void getInput() {
        
    }
    
    public void moveHand() {

    }

    public void flipHand() {
        // Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
    }

    public void rotateHand() {

    }

}