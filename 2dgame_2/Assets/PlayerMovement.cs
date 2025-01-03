using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public CharacterController2D controller;
	public Animator animator;

	public float runSpeed = 0f;

	float horizontalMove = 0f;
	bool jump = false;
	bool crouch = false;
	
	void Start () {

	}

	// Update is called once per frame
	void Update () {

		//Debug.Log(Input.GetAxisRaw("Horizontal"));
		animator.SetFloat("Speed", runSpeed);
		horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

		if (!Input.anyKey) {
		runSpeed = 0;
		//animator.SetBool("Is Moving", false);
		}

		 if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
			 runSpeed = 20;
			 if (Input.GetKey(KeyCode.DownArrow)) {
			 runSpeed = 10;
			 animator.SetBool("Is Crouching", true);
			 }
			 else if (Input.GetKey("z")) {
			 runSpeed = 40;
			 animator.SetBool("Is Sprinting", true);
			 }
			 animator.SetBool("Is Moving", true);
		 }
		 else if (Input.GetButtonUp("Horizontal")) {
			 animator.SetBool("Is Moving", false);
		 }

		 if (Input.GetKey("z")) {
		 	runSpeed = 40;
			animator.SetBool("Is Sprinting", true);
			if (!(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))) {
			runSpeed = 0;
			animator.SetBool("Is Moving", false);
			}
		 }
		 else if (Input.GetButtonUp("Sprint")) {
		 	runSpeed = 20;
			animator.SetBool("Is Sprinting", false);
		 }

		if (Input.GetKey(KeyCode.UpArrow))
		 {
			jump = true;
		 }

		 if (Input.GetKey(KeyCode.DownArrow))
		 {
			runSpeed = 10;
			crouch = true;
			animator.SetBool("Is Crouching", crouch);
			if (!(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))) {
			runSpeed = 0;
			animator.SetBool("Is Moving", false);
			}
		 } 
		 else if (Input.GetKeyUp("down"))
		 {
			crouch = false;
			animator.SetBool("Is Crouching", crouch);
			if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
			runSpeed = 20;
			}
			if (Input.GetKey("z")) {
			runSpeed = 40;
			}
		 }
	}

	 void FixedUpdate ()
	 {
	 	// Move our character
	 	controller.inBorder();
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
		jump = false;

	 }
}