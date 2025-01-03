using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyMovement : MonoBehaviour {
    
    public GameObject Enemy;
    public GameObject Hand;
    public Transform LeftBound;
    public Transform RightBound;
    public Transform BLborder;
    public Transform TRborder;
    public LayerMask Damaging;
    public TextMesh EnemyHealthText;
    public int EnemyHealth = 10;
    public float waitPressTime = 1.5f;
    public float waitShootingTime = 2.0f;
    public float waitDamageTime = 1.0f;
    public float waitKickRecieveTime = 0.25f;

    private float nextRestartPress = 0.0f;
    private float nextShooting = 0.0f;
    private float nextDamageTaken = 0.0f;
    private float nextKickRecieved = 0.0f;
    private Rigidbody2D enemyRigidbody;
    private Rigidbody2D handRigidbody;
    private Rigidbody2D leftRigidbody;
    private Rigidbody2D rightRigidbody;
    private GameObject HealthTextObject;
    private SpriteRenderer enemySprite;
    private Vector2 enemySpwanPoint;
    private Vector2 leftAndRightBounds;
    private Collider2D specificEnemyCollider;
    private Collider2D boxEnemyCollider;
    private string enemyType;
    private Camera Cam;

    float amplifier = 1f;
    bool restartGame;
    bool restartRecentlyPressed;
    bool damaged;
    bool dodge;
    bool nearEdge;
    bool kicked;
    bool dead;
    bool gameEnded;

    void Start()
    {
        enemyRigidbody = Enemy.GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        handRigidbody = Hand.GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        enemySprite = Enemy.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        leftRigidbody = LeftBound.GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        rightRigidbody = RightBound.GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        HealthTextObject = EnemyHealthText.gameObject;
        enemySpwanPoint = enemyRigidbody.position;
        leftAndRightBounds = new Vector2(LeftBound.position.x, RightBound.position.x);
        enemyType = Enemy.tag;
        boxEnemyCollider = Enemy.GetComponent(typeof(BoxCollider2D)) as BoxCollider2D;
        if (enemyType == "2") {
            specificEnemyCollider = Enemy.GetComponent(typeof(CapsuleCollider2D)) as CapsuleCollider2D;
        }
        else if (enemyType == "3") {
            specificEnemyCollider = Enemy.GetComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
        }
        else if (enemyType == "4") {
            specificEnemyCollider = Enemy.GetComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
        }
        Camera[] themCameras = Camera.allCameras;
        Cam = themCameras[0];
    }

    void Update() {
        getInput();
        displayEnemyHealth();
    }

    void FixedUpdate()
    {
        enemyInBounds();
        enemyMovement();
        damagedOrNot();
        checkEnemyHealth();
        respawnEnemies();
    }

    public void getInput() {
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

    public void enemyMovement() { 
        if (!gameEnded) {
            if (!dead) {    
                enemyRigidbody.constraints = RigidbodyConstraints2D.None;
                enemyRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                if (!kicked) {
                    if (enemyType == "2") {
                        if (enemyRigidbody.position.x > leftAndRightBounds.y) {
                            amplifier = -1.0f;
                        }
                        if (enemyRigidbody.position.x < leftAndRightBounds.x) {
                            amplifier = 1.0f;
                        }

                        if (Physics2D.gravity == new Vector2(-9.81f, 0f)) {        
                            enemyRigidbody.velocity = new Vector2((2f * amplifier) + .5886f, enemyRigidbody.velocity.y);
                        }
                        else {
                            enemyRigidbody.velocity = new Vector2(2f * amplifier, enemyRigidbody.velocity.y);
                        }
                    }
                    else if (enemyType == "3") {
                        if ((handRigidbody.position.x < leftRigidbody.position.x || handRigidbody.position.x > rightRigidbody.position.x) || (handRigidbody.position.y > leftRigidbody.position.y || handRigidbody.position.y < rightRigidbody.position.y)) {
                            nextShooting = 0.0f;
                            if (enemyRigidbody.position.x > leftAndRightBounds.y) {
                                amplifier = -1.0f;
                            }
                            if (enemyRigidbody.position.x < leftAndRightBounds.x) {
                                amplifier = 1.0f;
                            }

                            if (Physics2D.gravity == new Vector2(-9.81f, 0f)) {        
                                enemyRigidbody.velocity = new Vector2((2f * amplifier) + .5886f, enemyRigidbody.velocity.y);
                            }
                            else {
                                enemyRigidbody.velocity = new Vector2(2f * amplifier, enemyRigidbody.velocity.y);
                            }
                        }
                        else {
                            nextShooting += 0.04f;
                            if (Physics2D.gravity != new Vector2(-9.81f, 0f)) {
                                if (handRigidbody.position.x < enemyRigidbody.position.x - 0.3f) {
                                    enemyRigidbody.velocity = new Vector2(-3f, enemyRigidbody.velocity.y);
                                }
                                else if (handRigidbody.position.x > enemyRigidbody.position.x + 0.3f) {
                                    enemyRigidbody.velocity = new Vector2(3f, enemyRigidbody.velocity.y);
                                }
                                else {
                                    if (enemyRigidbody.velocity == new Vector2(-3f, enemyRigidbody.velocity.y)) {
                                        enemyRigidbody.velocity = new Vector2(-3f, enemyRigidbody.velocity.y);
                                    }
                                    else {
                                        enemyRigidbody.velocity = new Vector2(3f, enemyRigidbody.velocity.y);
                                    }
                                }
                            }
                            else {
                                if (handRigidbody.position.x < enemyRigidbody.position.x - 0.3f) {
                                    enemyRigidbody.velocity = new Vector2(-3f + .5886f, enemyRigidbody.velocity.y);
                                }
                                else if (handRigidbody.position.x > enemyRigidbody.position.x + 0.3f) {
                                    enemyRigidbody.velocity = new Vector2(3f + .5886f, enemyRigidbody.velocity.y);
                                }
                            }
                            throwAtPlayer();
                            checkShotLocation();
                        }
                    }
                    else if (enemyType == "4") {
                        if ((handRigidbody.position.x < leftRigidbody.position.x || handRigidbody.position.x > rightRigidbody.position.x) || (handRigidbody.position.y > leftRigidbody.position.y || handRigidbody.position.y < rightRigidbody.position.y)) {
                            nextShooting = 0.0f;
                            if (enemyRigidbody.position.x > leftAndRightBounds.y) {
                                amplifier = -1.0f;
                            }
                            if (enemyRigidbody.position.x < leftAndRightBounds.x) {
                                amplifier = 1.0f;
                            }

                            if (Physics2D.gravity == new Vector2(-9.81f, 0f)) {        
                                enemyRigidbody.velocity = new Vector2((2f * amplifier) + .5886f, enemyRigidbody.velocity.y);
                            }
                            else {
                                enemyRigidbody.velocity = new Vector2(2f * amplifier, enemyRigidbody.velocity.y);
                            }
                        }
                        else {
                            if (Physics2D.gravity != new Vector2(-9.81f, 0f)) {
                                if (!dodge) { 
                                    if (!nearEdge) {
                                        if (handRigidbody.position.x < enemyRigidbody.position.x - 0.3f) {
                                            enemyRigidbody.velocity = new Vector2(-5.5f, enemyRigidbody.velocity.y);
                                            dodge = false;
                                        }
                                        else if (handRigidbody.position.x > enemyRigidbody.position.x + 0.3f) {
                                            enemyRigidbody.velocity = new Vector2(5.5f, enemyRigidbody.velocity.y);
                                            dodge = false;
                                        }
                                        else {
                                            if (enemyRigidbody.velocity == new Vector2(-5.5f, enemyRigidbody.velocity.y)) {
                                                enemyRigidbody.velocity = new Vector2(10f, enemyRigidbody.velocity.y);
                                            }
                                            else {
                                                enemyRigidbody.velocity = new Vector2(-10f, enemyRigidbody.velocity.y);
                                            }
                                            dodge = true;
                                        }
                                    }
                                    else {
                                        if (enemyRigidbody.position.x < leftAndRightBounds.y - 3f && enemyRigidbody.position.x > leftAndRightBounds.x + 3f) {
                                            nearEdge = false;
                                        }
                                        else {
                                            if (enemyRigidbody.position.x > leftAndRightBounds.y - 3f) {
                                                enemyRigidbody.velocity = new Vector2(-10f, enemyRigidbody.velocity.y);
                                            }
                                            else if (enemyRigidbody.position.x < leftAndRightBounds.x + 3f) {
                                                enemyRigidbody.velocity = new Vector2(10f, enemyRigidbody.velocity.y);
                                            }
                                        }
                                    }
                                }
                                else {
                                    if (enemyRigidbody.position.x > handRigidbody.position.x + 3f || enemyRigidbody.position.x < handRigidbody.position.x - 3f) {
                                        dodge = false;
                                    }
                                    else {
                                        if (enemyRigidbody.position.x >= leftAndRightBounds.y) {
                                            enemyRigidbody.velocity = new Vector2(10f, enemyRigidbody.velocity.y);
                                            dodge = false;
                                            nearEdge = true;
                                        }
                                        else if (enemyRigidbody.position.x <= leftAndRightBounds.x) {
                                            enemyRigidbody.velocity = new Vector2(-10f, enemyRigidbody.velocity.y);
                                            dodge = false;
                                            nearEdge = true;
                                        }
                                    }
                                }
                            }
                            else { //for when hand is climbing
                                if (handRigidbody.position.x < enemyRigidbody.position.x - 0.3f) {
                                    enemyRigidbody.velocity = new Vector2(-5.5f + .5886f, enemyRigidbody.velocity.y);
                                }
                                else if (handRigidbody.position.x > enemyRigidbody.position.x + 0.3f) {
                                    enemyRigidbody.velocity = new Vector2(5.5f + .5886f, enemyRigidbody.velocity.y);
                                }
                            }
                        }
                    }

                    if (enemyRigidbody.position.y > leftRigidbody.position.y || enemyRigidbody.position.y < rightRigidbody.position.y) {
                        bool positionChecker1 = handCamInEnemyBounds();
                        bool positionChecker2 = handCamNearEnemy();
                        if (!positionChecker1 && !positionChecker2) {
                            enemyRigidbody.position = enemySpwanPoint;
                        }
                    }
                }
                else {
                    nextKickRecieved += Time.deltaTime;
                    if (nextKickRecieved == Time.deltaTime) {
                        if (handRigidbody.position.x < enemyRigidbody.position.x) {
                            if (Mathf.Abs(handRigidbody.velocity.x) > 3.14f) {
                                enemyRigidbody.velocity = new Vector2(10f, 5f);
                            }
                            else {
                                enemyRigidbody.velocity = new Vector2(6f, 4f);
                            }
                        }
                        else {
                            if (Mathf.Abs(handRigidbody.velocity.x) > 3.14f) {
                                enemyRigidbody.velocity = new Vector2(-10f, 5f);
                            }
                            else {
                                enemyRigidbody.velocity = new Vector2(-6f, 4f);
                            }
                        }
                    }
                    if (nextKickRecieved >= waitKickRecieveTime) {
                        nextKickRecieved = 0.0f;
                        kicked = false;
                    }
                    
                }
            }
            else {
                enemyRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    public void damagedOrNot() {
        int damageTaken = 0;
        Vector2 sizeOfHand = new Vector2(0.67f, 1.4f);
        Collider2D[] colliders = new Collider2D[1];
        Vector2 sizeOfEnemy = new Vector2(1f, 1f);
        Collider2D[] possibleBullets = new Collider2D[2];
        CircleCollider2D kickCollider = new CircleCollider2D();
        SpriteRenderer kickSprite = new SpriteRenderer();

        int numOfColliders = Physics2D.OverlapBoxNonAlloc(handRigidbody.position, sizeOfHand, 0f, colliders, Damaging);
        if (numOfColliders > 0 && colliders[0].tag == enemyType) {
            float downWardForce = handRigidbody.velocity.y;
            if ((handRigidbody.position.x >= enemyRigidbody.position.x - 0.5f && handRigidbody.position.x <= enemyRigidbody.position.x + 0.5f) && handRigidbody.position.y > enemyRigidbody.position.y + 0.435f) {
                if (downWardForce < 0) {
                    damageTaken = (int)((downWardForce / -9.81f) * 3f);
                    EnemyHealth -= damageTaken;
                    handRigidbody.velocity = new Vector2(handRigidbody.velocity.x, 8f);
                }
            }
        }

        if (enemyType == "3") {
            sizeOfEnemy = new Vector2(1.25f, 1.25f);
        }
        else if (enemyType == "4") {
            sizeOfEnemy = new Vector2(1.25f, 1.5f);
        }

        GameObject staryKick = GameObject.Find("kick");
        if (staryKick != null) {
            kickCollider = staryKick.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
            kickSprite = staryKick.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        }
        
        int numOfShots = Physics2D.OverlapBoxNonAlloc(enemyRigidbody.position, sizeOfEnemy, 0f, possibleBullets, Damaging);
        if (numOfShots > 1) { // when the enemy is alive
            if (possibleBullets[1].tag == "1") {
                if (!damaged) {
                    EnemyHealth -= 1;
                    Destroy(possibleBullets[1].gameObject);
                    damaged = true;
                }
            }

            if (staryKick != null) {
                if (possibleBullets[1].tag == "hand Kick" || Physics2D.IsTouching(kickCollider, boxEnemyCollider) || Physics2D.IsTouching(kickCollider, specificEnemyCollider)) {
                    if (!damaged) {
                        EnemyHealth -= 2;
                        kicked = true;
                        kickCollider.enabled = false;
                        damaged = true;
                        kickSprite.material.color = Color.clear;
                    }
                }
            }

            if (damaged) {
                nextDamageTaken += Time.deltaTime;
                if (nextDamageTaken >= waitDamageTime) {
                    nextDamageTaken = 0.0f;
                    damaged = false;
                }
            }
        }
        else { //if the enemy has died
            damaged = false;
            nextDamageTaken = 0.0f;
        }

    }

    public void checkEnemyHealth() {
        if (EnemyHealth <= 0) {
            dead = true;
            if (enemyType == "2") {
                CapsuleCollider2D enemyCapsuleCollider = Enemy.GetComponent(typeof(CapsuleCollider2D)) as CapsuleCollider2D;
                enemyCapsuleCollider.enabled = false;
            }
            else if (enemyType == "3") {
                EdgeCollider2D enemyEdgeCollider = Enemy.GetComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
                enemyEdgeCollider.enabled = false;
                GameObject[] allShots = GameObject.FindGameObjectsWithTag("1");
                for (int i = 0; i < allShots.Length; i++) {
                    if (allShots[i].layer == 9) {
                        Destroy(allShots[i]);
                    }
                }
            }
            else if (enemyType == "4") {
                PolygonCollider2D enemyPolyCollider = Enemy.GetComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
                enemyPolyCollider.enabled = false;
            }

            BoxCollider2D enemyBoxCollider = Enemy.GetComponent(typeof(BoxCollider2D)) as BoxCollider2D;
            enemyBoxCollider.enabled = false;
            enemySprite.material.color = Color.clear;
            HealthTextObject.SetActive(false);
        }
    }

    public void respawnEnemies() {
        if (restartGame) {
            BoxCollider2D enemyBoxCollider = Enemy.GetComponent(typeof(BoxCollider2D)) as BoxCollider2D;
            enemyBoxCollider.enabled = true;
            enemyRigidbody.position = enemySpwanPoint;
            amplifier = 1f;
            HealthTextObject.SetActive(true);
            nextRestartPress = 0.0f;
            nextShooting = 0.0f;
            GameObject[] allShots = GameObject.FindGameObjectsWithTag("1");
            for (int i = 0; i < allShots.Length; i++) {
                if (allShots[i].layer == 9) {
                    Destroy(allShots[i]);
                }
            }
            damaged = false;
            dodge = false;
            nearEdge = false;
            kicked = false;
            dead = false;

            if (enemyType == "2") {
                CapsuleCollider2D enemyCapsuleCollider = Enemy.GetComponent(typeof(CapsuleCollider2D)) as CapsuleCollider2D;
                EnemyHealth = 3;
                enemyCapsuleCollider.enabled = true;
                enemySprite.material.color = Color.red;
            }
            else if (enemyType == "3") {
                EdgeCollider2D enemyEdgeCollider = Enemy.GetComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;
                EnemyHealth = 5;
                enemyEdgeCollider.enabled = true;
                enemySprite.material.color = Color.yellow;
            }
            else if (enemyType == "4") {
                PolygonCollider2D enemyPolyCollider = Enemy.GetComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
                EnemyHealth = 8;
                enemyPolyCollider.enabled = true;
                enemySprite.material.color = Color.blue;
            }
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

    public void displayEnemyHealth() {
        string spareString = EnemyHealth.ToString();
        EnemyHealthText.text = spareString; 
    }

    public void throwAtPlayer() {
        if (enemyType == "3") {
            if (nextShooting >= waitShootingTime) {
                GameObject projectile = new GameObject("projectile", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D));
                projectile.tag = "1";
                projectile.layer = 9;
                SpriteRenderer projectSprite = projectile.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
                Rigidbody2D projectRigidbody = projectile.GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
                CircleCollider2D projectCollider = projectile.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
                projectCollider.radius = 0.055f;
                projectRigidbody.mass = 10f;
                projectSprite.sprite = Resources.Load<Sprite>("Bullet Circle");
                projectSprite.material.color = Color.black;
                CircleCollider2D handCircleHitbox = Hand.GetComponent(typeof(CircleCollider2D)) as CircleCollider2D;
                float distanceApart = enemyRigidbody.Distance(handCircleHitbox).distance;
                projectRigidbody.position = new Vector2(enemyRigidbody.position.x, enemyRigidbody.position.y + 0.7f);
                if (handRigidbody.position.x < enemyRigidbody.position.x - 0.3f) {
                    projectRigidbody.velocity = new Vector2(-20f, 20f * (Mathf.Sin((handRigidbody.position.y - enemyRigidbody.position.y) / distanceApart)));
                }
                else if (handRigidbody.position.x > enemyRigidbody.position.x + 0.3f) {
                    projectRigidbody.velocity = new Vector2(20f, 20f * (Mathf.Sin((handRigidbody.position.y - enemyRigidbody.position.y) / distanceApart)));
                }
                else {
                    projectRigidbody.velocity = new Vector2(0f, 20f);
                }
                nextShooting = 0.0f;
            }
        }
    }

    public void checkShotLocation() {
        GameObject[] allShots = GameObject.FindGameObjectsWithTag("1");
        Rigidbody2D leftRigidbody = LeftBound.GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        Rigidbody2D rightRigidbody = RightBound.GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
        for (int i = 0; i < allShots.Length; i++) {
            Rigidbody2D projectRigidbody = allShots[i].GetComponent(typeof(Rigidbody2D)) as Rigidbody2D;
            if (allShots[i].layer == 9) {
                if ((projectRigidbody.position.x < leftRigidbody.position.x - 1f || projectRigidbody.position.x > rightRigidbody.position.x) || (projectRigidbody.position.y > leftRigidbody.position.y || projectRigidbody.position.y < rightRigidbody.position.y)) {
                    Destroy(allShots[i]);
                }        
            }
        }
    }

    public void enemyInBounds() {
        if (BLborder.position.x > enemyRigidbody.position.x || TRborder.position.x < enemyRigidbody.position.x || BLborder.position.y > enemyRigidbody.position.y || TRborder.position.y < enemyRigidbody.position.y) {
            Physics2D.gravity = new Vector2(0f, -9.81f);
            EnemyHealth = 0;
		}
    }

    public void handOverlapWithExit(bool decider) {
        if (decider) {    
            gameEnded = true;
            enemyRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else {
            gameEnded = false;
        }
    }

    private bool handCamInEnemyBounds() {
        if (handRigidbody.position.x - (Cam.orthographicSize * 2) > rightRigidbody.position.x || handRigidbody.position.x + (Cam.orthographicSize * 2) < leftRigidbody.position.x) {
            return false;
        }
        else {
            if (handRigidbody.position.y - (Cam.orthographicSize) > leftRigidbody.position.y || handRigidbody.position.y + (Cam.orthographicSize) < rightRigidbody.position.y) {
                return false;
            }
            else {
                return true;
            }
        }
    }

    private bool handCamNearEnemy() {
        if (enemyRigidbody.position.x <= handRigidbody.position.x + (Cam.orthographicSize * 2) && enemyRigidbody.position.x >= handRigidbody.position.x - (Cam.orthographicSize * 2) && enemyRigidbody.position.y <= handRigidbody.position.y + Cam.orthographicSize && enemyRigidbody.position.y >= handRigidbody.position.y - Cam.orthographicSize) {
            return true;
        }
        else {
            return false;
        }
    }

    public void changeRestartStatusAfterButtonPress(bool reset) {
        restartGame = reset;
    }
}
