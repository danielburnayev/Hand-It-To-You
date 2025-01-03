using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HandMovement : MonoBehaviour {

	public Animator animator;
	public Transform GroundCheck;
	public Transform CeilingCheck;
	public Transform RightCheck;
    public Transform BLborder;
	public Transform TRborder;
    public LayerMask Ground;
    public LayerMask Climbable;
    public LayerMask CheckPoint;
    public LayerMask Damaging;
    public TextMesh TRHandHealthText;
    public TextMesh TLHandHealthText;
    public TextMesh SRHandHealthText;
    public TextMesh SLHandHealthText;
    public GameObject kickFromHand;
    public GameObject Exit;
    public float jumpRate = 0.5f;
    public float waitJumpTime = 2.0f;
    public float waitPressTime = 10.0f;
    public float waitRespawnTime = 3.0f;
    public float waitDamageTime = 1.0f;
    public float waitDamageBySpikesTime = 2.0f;
    public float waitKickTime = 0.5f;
    public float waitHealingTime = 1.0f;
    public int handHealth = 10;
    
    private Rigidbody2D handRigidbody;
    private Collider2D crouchHitbox;
    private CircleCollider2D circleHitbox;
    private SpriteRenderer handSprite;
    private float nextJump = 0.0f;
    private float nextCheckpointPress = 0.0f;
    private float nextRestartPress = 0.0f;
    private float nextRespawn = 0.0f;
    private float nextDamageTaken = 0.0f;
    private float nextDamageBySpikes = 0.0f;
    private float nextKick = 0.0f;
    private float nextDamageFlash = 0.0f;
    private float nextHealingFlash = 0.0f;
    private Vector2 respawnPoint = new Vector2(-15f, -2.375548f);
    private int checkPointNum = 0;
    private Collider2D[] completedCheckpoints = new Collider2D[0];

    bool idle = true;
    bool jump = false;
    bool crouch = false;
    bool sprint = false;
    bool moving = false;
    bool underSomething = false;
    bool faceRight = true;
    bool isGrounded = false;
    bool climb = false;
    bool waiting = false;
    bool pressCheckpoint = false;
    bool checkpointRecentlyPressed = false;
    bool restartGame = false;
    bool restartRecentlyPressed = false;
    bool damaged = false;
    bool hitBySpikes = false;
    bool kick = false;
    bool recentlyKicked = false;
    bool kickHeld = false;
    bool recentlyTouchedEnemy = false;
    bool recentlyTouchedCheckpoint = false;
    bool gameCompleted = false;
    int jumpCounter = 0;
    int frameDifference = 0;
    int kickTracker = 0;
    float speedFactor = 1f;
    float angle = 0.0f;

    private void Awake() {
        handRigidbody = GetComponent<Rigidbody2D>();
        crouchHitbox = GetComponent<Collider2D>();
        circleHitbox = GetComponent<CircleCollider2D>();
        handSprite = GetComponent<SpriteRenderer>();
        Cursor.visible = false;
    }

    private void Update() {
        getInput();
        if (!gameCompleted) {
            changeHealthText();
            positionHealthText();
        }
        overlapWithExit();
    }
    
    private void FixedUpdate() {
        if (!gameCompleted) {
            //findHandAngle();
            inBounds();
            inContactWithEnemies();
            checkHandHealth();
            if (!waiting) {
                moveHand(idle, moving, crouch, sprint, jump);
                handAnimation(idle, moving, crouch, sprint, jump, climb, kick);
                createHandKick(); 
            }
            handOnCheckPoint();
            handToCheckpoint();
        }
        RestartTheGame();
    }

    private void OnGUI() {
        if (gameCompleted) {
            GUI.Button(new Rect(810, 387, 100, 50), "Finish Level");
        }
    }

    public void getInput() {

        if (!gameCompleted) {
            if (Input.GetKey("z")) {
                sprint = true;
            }
            else if (Input.GetButtonUp("Sprint")) {
                sprint = false;
            }

            if (Input.GetKey(KeyCode.DownArrow)) {
                crouch = true;
            }
            else if (Input.GetButtonUp("Crouch")) { 
                crouch = false;
            }
            
            if (Input.GetKey(KeyCode.RightArrow)) {
                idle = false;
                moving = true;

                if (!faceRight) {
                    flipHand();
                }

            }
            else if (Input.GetKey(KeyCode.LeftArrow)) {
                idle = false;
                moving = true;

                if (faceRight) {
                    flipHand();
                }

            }

            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                jump = true;
            }
            else if (Input.GetKeyUp("up")) {
                jump = false;
            }

            if (Input.GetButtonUp("Horizontal")) {
                idle = true;
                moving = false;
            }

            if (nextCheckpointPress == 0.0f) {
                if (Input.GetKey(KeyCode.Tab)) {
                    pressCheckpoint = true;
                    checkpointRecentlyPressed = true;
                }
            }
            if (Input.GetKeyUp("tab")) {
                pressCheckpoint = false;
            }

            if (nextKick == 0.0f && !kickHeld) {
                if (Input.GetKey(KeyCode.X)) {
                    kick = true;
                }
            }
            else if (nextKick >= 0.0f) {
                kick = false;
                if (Input.GetKey(KeyCode.X)) {
                    kickHeld = true;
                }
            }
            if (Input.GetKeyUp("x")) {
                kickHeld = false;
            }
        }

        if (nextRestartPress == 0.0f) {
            if (Input.GetKey(KeyCode.R)) {
                restartGame = true;
                restartRecentlyPressed = true;
            }
        }
        if (Input.GetKeyUp("r")) {
            restartGame = false;
        }
    }
    
    public void moveHand(bool idling, bool moving, bool crouching, bool sprinting, bool jumping) {

        handOnGround();
        handUnderSomething(); 

        if (idling) {
            handRigidbody.velocity = new Vector2(0f, handRigidbody.velocity.y);
        }
        else {
            if (moving) {
                speedFactor = 1f;
                if (sprinting) {
                    speedFactor = 2f;
                    if (crouching) {
                        speedFactor = 0.75f;
                    }
                }
                else if (crouching) {
                    speedFactor = 0.75f;
                }
            }
        
            if (faceRight) {
                handRigidbody.velocity = new Vector2(3.14f * speedFactor, handRigidbody.velocity.y);
            }
            else {
                handRigidbody.velocity = new Vector2(-3.14f * speedFactor, handRigidbody.velocity.y);
            }

        }

        if (jumping) {
            if (isGrounded) {
                isGrounded = false;
                if (crouch) {
                    handRigidbody.velocity = new Vector2(0f, 6f);
                }
                else if (sprinting) {
                    if (moving) {
                        handRigidbody.velocity = new Vector2(0f, 9.5f);
                    }
                    else {
                        handRigidbody.velocity = new Vector2(0f, 8f);
                    }
                }
                else {
                    handRigidbody.velocity = new Vector2(0f, 8f);
                }
                jumpCounter = 1;
            }
            else if (nextJump >= jumpRate && jumpCounter == 1 && !crouching) {
                if (moving || idle) {
                    handRigidbody.velocity = new Vector2(0f, 8f);
                    if (sprinting) {
                        handRigidbody.velocity = new Vector2(0f, 9.5f);
                    }
                }
                nextJump = 0.0f;
                jumpCounter = 0;
            }
        }
        else {
            if (!isGrounded && jumpCounter == 1) {
                if (nextJump <= jumpRate) {
                    nextJump = Time.time;
                }
            }
            else {
                nextJump = 0.0f;
            }
        }

        if (handRigidbody.rotation == 0f) {
            if (Physics2D.OverlapCircle(RightCheck.position, 0.05f, Climbable) && crouching) {
                climb = true;
                handRigidbody.velocity = new Vector2(0.0f, 6.28f);
                perpenRotate(true);
            }
        }
        else if (handRigidbody.rotation == 90f || handRigidbody.rotation == -90f) {
            if (Physics2D.IsTouchingLayers(circleHitbox, Climbable) && crouching) {
                if (!faceRight) {
                    handRigidbody.AddForce(new Vector2(-1000f, 0f));
                }   
                else if (faceRight) {
                    handRigidbody.AddForce(new Vector2(1000f, 0f));
                }
                handRigidbody.velocity = new Vector2(0.0f, 6.28f);
            }
            else {
                climb = false;
                if (moving) {
                    flipHand();
                }

                if (!faceRight) {
                    handRigidbody.AddForce(new Vector2(500f, -325f));
                }   
                else if (faceRight) {
                    handRigidbody.AddForce(new Vector2(-500f, -325f));
                }
                perpenRotate(false);
            }
        }

        if (isGrounded && !climb) {
            if (crouch) {
                crouchHitbox.enabled = false;
            }
            else {
                if (underSomething) {
                    crouchHitbox.enabled = false;
                }
                else {
					crouchHitbox.enabled = true;
                }
            }

            if (moving && underSomething) {
                if (faceRight) {
                handRigidbody.velocity = new Vector2(2.25f, handRigidbody.velocity.y);
                }
                else {
                    handRigidbody.velocity = new Vector2(-2.25f, handRigidbody.velocity.y);
                }
            }
        }
        else if (!isGrounded) {
            if (!climb) {
                if (crouch) {
                    crouchHitbox.enabled = false;
                }
                else {
                    crouchHitbox.enabled = true;
                }
            }
            else if (climb) {
                crouchHitbox.enabled = true;
            }    
        }

    }

    public void handAnimation(bool idling, bool moving, bool crouching, bool sprinting, bool jumping, bool climbing, bool kicking) {
        if (!sprinting) {
            animator.SetBool("Is Sprinting", false);
        }

        if (!crouching) {
            animator.SetBool("Is Crouching", false);
        }
            
        if (idle) {
            animator.SetBool("Is Moving", false);
            animator.SetBool("Is Sprinting", false);
            animator.SetBool("Is Crouching", false);
            if (crouching) {
                animator.SetBool("Is Crouching", true);
            }
        }
        else {
            if (moving) {
                animator.SetBool("Is Moving", true);
                if (sprinting) {
                    animator.SetBool("Is Sprinting", true);
                    if (crouching) {
                        animator.SetBool("Is Crouching", true);
                    }
                }
                else if (crouching) {
                    animator.SetBool("Is Crouching", true);
                }
            }
        }

        if (underSomething && isGrounded) {
            animator.SetBool("Is Crouching", true);
            if (moving) {
                animator.SetBool("Is Moving", true);
            }
        }

        if (climbing) {
            animator.SetBool("Is Crouching", false);
            animator.SetBool("Is Sprinting", true);
        }

        if (kicking) {
            animator.SetBool("Is Kicking", true);
        }
        else {
            animator.SetBool("Is Kicking", false);
        }
    }

    public void flipHand() {
        // Switch the way the player is labelled as facing.
		faceRight = !faceRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
    }

    public void perpenRotate(bool doRotate) {
        if (doRotate) {
            if (faceRight) {
		        handRigidbody.transform.Rotate(0, 0, 90);
		    }
		    else if (!faceRight) {
		        handRigidbody.transform.Rotate(0, 0, -90);
		    }
            Physics2D.gravity = new Vector2(-9.81f, 0f);
        }
        else {
		    handRigidbody.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            Physics2D.gravity = new Vector2(0.0f, -9.81f);
        }
    }

    public void handOnGround() {
        if (Physics2D.OverlapCircle(GroundCheck.position, 0.1f, Ground) || Physics2D.OverlapCircle(GroundCheck.position, 0.1f, Climbable)) {
            isGrounded = true;
        }
        else {
            isGrounded = false;
        }
    }

    public void handUnderSomething() {
        if (Physics2D.OverlapCircle(CeilingCheck.position, 0.1f, Ground) || Physics2D.OverlapCircle(CeilingCheck.position, 0.1f, Climbable)) {
            underSomething = true;
        }
        else {
            underSomething = false;
        }
    }

    public void inBounds() {
        if (BLborder.position.x > handRigidbody.position.x || TRborder.position.x < handRigidbody.position.x || BLborder.position.y > handRigidbody.position.y || TRborder.position.y < handRigidbody.position.y) {
            Physics2D.gravity = new Vector2(0f, -9.81f);
            handRigidbody.position = respawnPoint;
            handHealth = 10;
            waiting = true;
		}
        else if (waiting) {
            animator.SetBool("Is Moving", false);
            animator.SetBool("Is Sprinting", false);
            animator.SetBool("Is Crouching", false);
            handRigidbody.position = respawnPoint;
            handRigidbody.velocity = new Vector2(0.0f, 0.0f);
            handRigidbody.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

            if (nextRespawn <= waitRespawnTime) {
                nextRespawn += Time.deltaTime;
            }
            else {                    
                nextRespawn = 0.0f;
                waiting = false;
            }

            if (frameDifference == 0) {
                handSprite.material.color = Color.clear;
                frameDifference = 1;
            }
            else if (frameDifference == 1) {
                handSprite.material.color = Color.white;
                frameDifference = 0;
            }
        }
	}

    public void findHandAngle() { // adjusts the hand to the angle of whatever is below it
        RaycastHit2D[] hits = new RaycastHit2D[2];
        int h = Physics2D.RaycastNonAlloc(handRigidbody.transform.position, Vector2.down, hits); //cast downwards
        if (h > 1) { //if we hit something do stuff
            if ((moving && sprint) || (moving && crouch) || moving) {
                if (faceRight) {
                    if (handRigidbody.velocity.y >= 0) {
                        angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg);
                    }
                    else {
                        angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg) * -1;
                    }
                }
                else if (!faceRight) {
                    if (handRigidbody.velocity.y >= 0) {
                        angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg) * -1;
                    }
                    else {
                        angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg);
                    }
                }
            //Debug.Log(angle);
            }    
            else if (idle) {
                if (faceRight) {
                    if (handRigidbody.velocity.x > 0) {
                        angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg) * -1;
                    }
                    else if (handRigidbody.velocity.x < 0) {
                        angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg);
                    }
                }
                else if (!faceRight) {
                    if (handRigidbody.velocity.x > 0) {
                        angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg) * -1;
                    }
                    else if (handRigidbody.velocity.x < 0) {
                        angle = Mathf.Abs(Mathf.Atan2(hits[1].normal.x, hits[1].normal.y) * Mathf.Rad2Deg);
                    }
                }
            }
        }

        if (!climb) {
            handRigidbody.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            handRigidbody.transform.Rotate(0, 0, angle);
        }
    }

    public void handOnCheckPoint() {
        Vector2 sizeOfHand = new Vector2(0.67f, 1f);
        Collider2D[] colliders = new Collider2D[1];
        int numOfColliders = Physics2D.OverlapBoxNonAlloc(handRigidbody.position, sizeOfHand, 0f, colliders, CheckPoint);
        if (numOfColliders == 1) {
            Collider2D checkpointCollider = colliders[0];
            addToCompletedCheckpoints(checkpointCollider);
            int spareInt;
            int.TryParse(checkpointCollider.tag, out spareInt);
            if (spareInt - checkPointNum == 1) {
                checkPointNum = spareInt;
                respawnPoint = checkpointCollider.transform.position;
                SpriteRenderer checkPointSprite = checkpointCollider.gameObject.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                checkpointCollider.enabled = false;
                checkPointSprite.material.color = Color.red;
                if (handHealth + 2 <= 10) {
                    handHealth += 2;
                    recentlyTouchedCheckpoint = true;
                }
                else {
                    if (handHealth != 10) {
                        recentlyTouchedCheckpoint = true;
                    }
                    handHealth = 10;
                }
            }
        }

        if (recentlyTouchedCheckpoint) {
            handSprite.material.color = new Color(0 + nextHealingFlash, 1, 0 + nextHealingFlash, 1);
            nextHealingFlash += Time.deltaTime;
            if (nextHealingFlash >= waitHealingTime) {
                nextHealingFlash = 0.0f;
                handSprite.material.color = Color.white;
                recentlyTouchedCheckpoint = false;
            }
        }
    }

    public void addToCompletedCheckpoints(Collider2D collider) {
        Collider2D[] tempArray = new Collider2D[completedCheckpoints.Length + 1];
        for (int i = 0; i < completedCheckpoints.Length; i++) {
            tempArray[i] = completedCheckpoints[i];
        }
        tempArray[tempArray.Length - 1] = collider;
        completedCheckpoints = tempArray;
    }

    public void handToCheckpoint() {
        if (pressCheckpoint) {
            Physics2D.gravity = new Vector2(0f, -9.81f);
            handRigidbody.position = respawnPoint;
            pressCheckpoint = false;
        }

        if (checkpointRecentlyPressed) {
            nextCheckpointPress += Time.deltaTime;
            if (nextCheckpointPress >= waitPressTime) {
                nextCheckpointPress = 0.0f;
                checkpointRecentlyPressed = false;
                handSprite.material.color = Color.white;
            }
        }
    }

    public void RestartTheGame() {
        if (restartGame) {
            handSprite.material.color = Color.white;
            handRigidbody.velocity = new Vector2(0f, 0f);
            
            gameCompleted = false;
            handRigidbody.constraints = RigidbodyConstraints2D.None;
            handRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            GameObject[] allCapsuleObjects = GameObject.FindGameObjectsWithTag("2");
            GameObject[] allHexagonObjects = GameObject.FindGameObjectsWithTag("3");
            GameObject[] allDiamondObjects = GameObject.FindGameObjectsWithTag("4");
            for (int i  = 0; i < allCapsuleObjects.Length; i++) {
                if (allCapsuleObjects[i].TryGetComponent(typeof(EnemyMovement), out Component component)) {
                    EnemyMovement enemy1 = allCapsuleObjects[i].GetComponent(typeof(EnemyMovement)) as EnemyMovement;
                    enemy1.handOverlapWithExit(false);
                }
            }
            for (int i  = 0; i < allHexagonObjects.Length; i++) {
                if (allHexagonObjects[i].TryGetComponent(typeof(EnemyMovement), out Component component)) {
                    EnemyMovement enemy2 = allHexagonObjects[i].GetComponent(typeof(EnemyMovement)) as EnemyMovement;
                    enemy2.handOverlapWithExit(false);
                }
            }
            for (int i  = 0; i < allDiamondObjects.Length; i++) {
                if (allDiamondObjects[i].TryGetComponent(typeof(EnemyMovement), out Component component)) {
                    EnemyMovement enemy3 = allDiamondObjects[i].GetComponent(typeof(EnemyMovement)) as EnemyMovement;
                    enemy3.handOverlapWithExit(false);
                }
            }

            idle = true;
            jump = false;
            crouch = false;
            sprint = false;
            moving = false;
            kick = false;
            underSomething = false;
            if (faceRight) {
                faceRight = true;
            }
            else {
                faceRight = false;
            }
            isGrounded = false;
            climb = false;
            waiting = false;
            pressCheckpoint = false;
            checkpointRecentlyPressed = false;
            damaged = false;
            hitBySpikes = false;
            kick = false;
            recentlyKicked = false;
            kickHeld = false;

            handHealth = 10;
            nextJump = 0.0f;
            nextCheckpointPress = 0.0f;
            nextRestartPress = 0.0f;
            nextRespawn = 0.0f;
            nextKick = 0.0f;
            respawnPoint = new Vector2(-15f, -2.375548f);
            checkPointNum = 0;
            kickTracker = 0;

            handRigidbody.position = new Vector2(-15f, 2f);

            for (int i = 0; i < completedCheckpoints.Length; i++) {
                SpriteRenderer colliderSprite = completedCheckpoints[i].gameObject.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                completedCheckpoints[i].enabled = true;
                colliderSprite.material.color = Color.white;
            } 
            completedCheckpoints = new Collider2D[0];

            restartGame = false;
        }
        
        if (restartRecentlyPressed) {
            nextRestartPress += Time.deltaTime;
            if (nextRestartPress >= waitPressTime) {
                nextRestartPress = 0.0f;
                restartRecentlyPressed = false;
            }
        }
    }

    public void inContactWithEnemies() { //damages the hand if it touches any enemies
        int damageTaken = 0;
        Vector2 sizeOfHand = new Vector2(0.67f, 1.4f);
        Collider2D[] colliders = new Collider2D[4];
        int numOfColliders = Physics2D.OverlapBoxNonAlloc(handRigidbody.position, sizeOfHand, 0f, colliders, Damaging);
        if (numOfColliders > 0) {
            float downWardForce = handRigidbody.velocity.y;
            damageTaken = (int)((downWardForce / -9.81f) * 3f);
                for (int i = 0; i < colliders.Length; i++) {
                    if (colliders[i] != null) {
                        if (colliders[i].tag == "Spikes") {
                            if (downWardForce < 0) {
                                if (!hitBySpikes) {
                                    handHealth -= damageTaken;
                                    hitBySpikes = true;
                                    if (damageTaken > 0) {
                                        damaged = true;
                                    } 
                                }
                            }
                            colliders[i].enabled = false;
                        }
                        else if (colliders[i].tag == "2") {
                            CapsuleCollider2D badBody = colliders[i].GetComponent(typeof(CapsuleCollider2D)) as CapsuleCollider2D;
                            if (Physics2D.IsTouching(circleHitbox, badBody) || Physics2D.IsTouching(crouchHitbox, badBody)) {
                                int spareInt;
                                int.TryParse(badBody.tag, out spareInt);
                                if (!damaged) {
                                    handHealth -= spareInt;
                                    damaged = true;
                                }
                            }
                        }
                        else if (colliders[i].tag == "3") {
                            EdgeCollider2D badBody = colliders[i].GetComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
                            if (Physics2D.IsTouching(circleHitbox, badBody) || Physics2D.IsTouching(crouchHitbox, badBody)) {
                                int spareInt;
                                int.TryParse(badBody.tag, out spareInt);
                                if (!damaged) {
                                    handHealth -= spareInt;
                                    damaged = true;
                                }
                            }
                        }
                        else if (colliders[i].tag == "1") {
                            CircleCollider2D badBody = colliders[i].GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
                            Rigidbody2D projectRigidbody = colliders[i].GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                            GameObject projectile = colliders[i].gameObject;
                            if (Physics2D.IsTouching(circleHitbox, badBody) || Physics2D.IsTouching(crouchHitbox, badBody)) {
                                int spareInt;
                                int.TryParse(badBody.tag, out spareInt);
                                if (!damaged) {
                                    handHealth -= spareInt;
                                    Destroy(projectile);
                                    damaged = true;
                                }
                            }
                        }
                        else if (colliders[i].tag == "4") {
                            PolygonCollider2D badBody = colliders[i].GetComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
                            if (Physics2D.IsTouching(circleHitbox, badBody) || Physics2D.IsTouching(crouchHitbox, badBody)) {
                                int spareInt;
                                int.TryParse(badBody.tag, out spareInt);
                                if (!damaged) {
                                    handHealth -= spareInt;
                                    damaged = true;
                                }
                            }
                        }
                    }
                }

            if (damaged) {
                recentlyTouchedEnemy = true;
                nextDamageTaken += Time.deltaTime;
                if (nextDamageTaken >= waitDamageTime) {
                    nextDamageTaken = 0.0f;
                    damaged = false;
                }
            }
        }
        else {
            damaged = false;
            nextDamageTaken = 0.0f;
            GameObject[] allSpikes = GameObject.FindGameObjectsWithTag("Spikes");
            for (int i = 0; i < allSpikes.Length; i++) {
                BoxCollider2D specificCollider = allSpikes[i].GetComponent(typeof(BoxCollider2D)) as BoxCollider2D;
                Rigidbody2D specificRigibody = allSpikes[i].GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                if (handRigidbody.position.x > specificRigibody.position.x + ((specificCollider.size.y / 2) + 0.45f) || handRigidbody.position.x < specificRigibody.position.x - ((specificCollider.size.y / 2) + 0.45f)) {
                    specificCollider.enabled = true;
                    hitBySpikes = false;
                }
                else {
                    if (handRigidbody.position.y <= specificRigibody.position.y + ((specificCollider.size.x / 2) + 0.54f) && handRigidbody.position.y >= specificRigibody.position.y - ((specificCollider.size.x / 2) + 0.54f)) {
                        specificCollider.enabled = false;
                        hitBySpikes = true;
                        if (!idle && !crouch) {
                            if (moving) {
                                nextDamageBySpikes += Time.deltaTime;
                                int randomNumber1 = (int)UnityEngine.Random.Range(1.0f, 200.0f);
                                if (randomNumber1 == 5) {
                                    if (isGrounded && nextDamageBySpikes >= waitDamageBySpikesTime) {
                                        handHealth--;
                                        nextDamageBySpikes = 0.0f;
                                        recentlyTouchedEnemy = true;
                                    }
                                }
                                if (sprint) {
                                    int randomNumber2 = (int)UnityEngine.Random.Range(1.0f, 100.0f);
                                    if (randomNumber2 == 2) {
                                        if (isGrounded && nextDamageBySpikes >= waitDamageBySpikesTime) {
                                            handHealth--;
                                            nextDamageBySpikes = 0.0f;
                                            recentlyTouchedEnemy = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else {
                        specificCollider.enabled = true;
                        hitBySpikes = false;
                    }
                }
            }
        }

        if (recentlyTouchedEnemy) {
            handSprite.material.color = new Color(1, 0 + nextDamageFlash, 0 + nextDamageFlash, 1);
            nextDamageFlash += Time.deltaTime;
            if (nextDamageFlash >= waitDamageTime) {
                nextDamageFlash = 0.0f;
                handSprite.material.color = Color.white;
                recentlyTouchedEnemy = false;
            }
        }
    }

    public void checkHandHealth() {
        if (handHealth <= 0) {
            pressCheckpoint = true;
            checkpointRecentlyPressed = true;
            waiting = true;
            recentlyTouchedEnemy = false;
            nextDamageFlash = 0.0f;
            handSprite.material.color = Color.white;
            handHealth = 10;
        }
    }

    public void changeHealthText() {
        string spareString = handHealth.ToString();
        TRHandHealthText.text = spareString;
        TLHandHealthText.text = spareString;
        SRHandHealthText.text = spareString;
        SLHandHealthText.text = spareString;
    }

    public void positionHealthText() {
        GameObject TRHandHealthTextObject = TRHandHealthText.gameObject;
        GameObject TLHandHealthTextObject = TLHandHealthText.gameObject;
        GameObject SRHandHealthTextObject = SRHandHealthText.gameObject;
        GameObject SLHandHealthTextObject = SLHandHealthText.gameObject;
        
        if (faceRight) {
            if (climb) {
                SRHandHealthTextObject.SetActive(true);
                TRHandHealthTextObject.SetActive(false);
                TLHandHealthTextObject.SetActive(false);
                SLHandHealthTextObject.SetActive(false);
            }
            else {
                TRHandHealthTextObject.SetActive(true);
                TLHandHealthTextObject.SetActive(false);
                SRHandHealthTextObject.SetActive(false);
                SLHandHealthTextObject.SetActive(false);
            }
        }
        else if (!faceRight) {
            if (climb) {
                SLHandHealthTextObject.SetActive(true);
                TRHandHealthTextObject.SetActive(false);
                TLHandHealthTextObject.SetActive(false);
                SRHandHealthTextObject.SetActive(false);
            }
            else {
                TLHandHealthTextObject.SetActive(true);
                TRHandHealthTextObject.SetActive(false);
                SRHandHealthTextObject.SetActive(false);
                SLHandHealthTextObject.SetActive(false);
            }
        }
    }

    public void createHandKick() { //what makes the hand kick work on the kick while falling
        CircleCollider2D kickCollider = kickFromHand.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
        SpriteRenderer kickSprite = kickFromHand.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        if (!crouch && !climb) {
            if (kick) {
                recentlyKicked = true;
                kickCollider.enabled = true;
            }
        }

        if (recentlyKicked) {
            kickTracker++;
            kickFromHand.transform.localPosition = new Vector2(0.475f + ((4 * 1.33f) * kickTracker * Time.deltaTime), 0f + (1.33f * kickTracker * Time.deltaTime));

            nextKick += Time.deltaTime;
            if (nextKick >= waitKickTime) {
                nextKick = 0.0f;
                recentlyKicked = false;
                kickTracker = 0;
                kickFromHand.transform.localPosition = new Vector3(0.475f, 0f, 0f);
                kickCollider.enabled = false;
                kickSprite.material.color = Color.blue;
            }
        }
    }

    public void overlapWithExit() {
        EdgeCollider2D exitCollider = Exit.GetComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
        if (Physics2D.IsTouching(circleHitbox, exitCollider) || Physics2D.IsTouching(crouchHitbox, exitCollider)) {
            handRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            GameObject[] allCapsuleObjects = GameObject.FindGameObjectsWithTag("2");
            GameObject[] allHexagonObjects = GameObject.FindGameObjectsWithTag("3");
            GameObject[] allDiamondObjects = GameObject.FindGameObjectsWithTag("4");
            for (int i  = 0; i < allCapsuleObjects.Length; i++) {
                if (allCapsuleObjects[i].TryGetComponent(typeof(EnemyMovement), out Component component)) {
                    EnemyMovement enemy1 = allCapsuleObjects[i].GetComponent(typeof(EnemyMovement)) as EnemyMovement;
                    enemy1.handOverlapWithExit(true);
                }
            }
            for (int i  = 0; i < allHexagonObjects.Length; i++) {
                if (allHexagonObjects[i].TryGetComponent(typeof(EnemyMovement), out Component component)) {
                    EnemyMovement enemy2 = allHexagonObjects[i].GetComponent(typeof(EnemyMovement)) as EnemyMovement;
                    enemy2.handOverlapWithExit(true);
                }
            }
            for (int i  = 0; i < allDiamondObjects.Length; i++) {
                if (allDiamondObjects[i].TryGetComponent(typeof(EnemyMovement), out Component component)) {
                    EnemyMovement enemy3 = allDiamondObjects[i].GetComponent(typeof(EnemyMovement)) as EnemyMovement;
                    enemy3.handOverlapWithExit(true);
                }
            }
            gameCompleted = true;
        }
    }

}