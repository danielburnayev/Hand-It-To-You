using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuAndGUIDirectories : MonoBehaviour
{

    bool createdPlayableLevel;
    int levelNum = -1;
    int highestLevelCompleted = 0;
    Vector2 levelScrollerPosition;
    Vector2 scrollPosition;

    void Start()
    {
        numOfLevelMenues();
        deleteObjectsBesidesMenues();
    }

    void Update()
    {
        createSpecificPlayableLevel();
    }

    void OnGUI() {
        if (levelNum == -1) {
            GUI.BeginGroup(new Rect(Screen.width / 2 - 400, Screen.height / 2 - 300, 800, 600));
            GUI.Box(new Rect(0, 0, 800, 800), "Hand It To Them");
            if (GUI.Button(new Rect(20, 50, 100, 50), "Play")) {
                levelNum = 0;
            }
            else if (GUI.Button(new Rect(140, 50, 100, 50), "Quit")) {
                Destroy(this);
                Application.Quit();
            }
            GUI.EndGroup();
        }
        else if (levelNum == 0) {
            levelScrollerPosition = GUI.BeginScrollView(new Rect(Screen.width / 2 - 400, Screen.height / 2 - 300, 800, 600), levelScrollerPosition, new Rect(0, 0, 600, 800), false, true);
            GUI.Box(new Rect(0, 0, 800, 800), "Level Menu");
            if (GUI.Button(new Rect(20, 10, 75, 20), "Main Menu")) {
                levelNum = -1;
            }
            else if (GUI.Button(new Rect(20, 50, 100, 50), "Level 1")) {
                levelNum = 1;
            }
            else if (GUI.Button(new Rect(140, 50, 100, 50), "Level 2")) {
                // if (highestLevelCompleted >= 1) {
                     levelNum = 2;
                // }
                // else {
                //     Debug.Log("You need to complete Level " + (highestLevelCompleted + 1) + " first!");
                // }
            }
            else if (GUI.Button(new Rect(260, 50, 100, 50), "Level 3")) {
                // if (highestLevelCompleted >= 2) {
                     levelNum = 3;
                // }
                // else {
                //     Debug.Log("You need to complete Level " + (highestLevelCompleted + 1) + " first!");
                // }
            }
            else if (GUI.Button(new Rect(380, 50, 100, 50), "Level 4")) {
                // if (highestLevelCompleted >= 3) {
                     levelNum = 4;
                // }
                // else {
                //     Debug.Log("You need to complete Level " + (highestLevelCompleted + 1) + " first!");
                // }
            }
            else if (GUI.Button(new Rect(20, 650, 100, 50), "Level 5")) {
                // if (highestLevelCompleted >= 4) {
                     levelNum = 5;
                // }
                // else {
                //     Debug.Log("You need to complete Level " + (highestLevelCompleted + 1) + " first!");
                // }
            }
            GUI.EndScrollView();
        }
    }

    private void numOfLevelMenues() {
        GameObject[] allLevelMenues = GameObject.FindGameObjectsWithTag("LevelMenu");
        if (allLevelMenues.Length > 1) {
            for (int i = 0; i < allLevelMenues.Length; i++) {
                if (i > 0) {
                    Destroy(allLevelMenues[i]);
                }
            }
        }
    }

    private void deleteObjectsBesidesMenues() {
        GameObject[] otherGameObjects = (GameObject[])FindObjectsOfType(typeof(GameObject));
        if (otherGameObjects.Length > 0) {
            for (int i = 0; i < otherGameObjects.Length; i++) {
                if (otherGameObjects[i].name != "Main Camera" && otherGameObjects[i].name != "CM vcam1" && otherGameObjects[i].name != "Level Menu") {
                    Destroy(otherGameObjects[i]);
                }
            }
        } 
    }

    public void setLevelNum(int providedLevelNum) {
        levelNum = providedLevelNum;
    }

    public void createSpecificPlayableLevel() {
        if (!createdPlayableLevel) {
            if (levelNum == 1) {
                SceneManager.LoadScene("Level 1", LoadSceneMode.Single);
            }
            else if (levelNum == 2) {
                SceneManager.LoadScene("Level 2", LoadSceneMode.Single);
            }
            else if (levelNum == 3) {
                SceneManager.LoadScene("Level 3", LoadSceneMode.Single);
            }
            else if (levelNum == 4) {
                SceneManager.LoadScene("Level 4", LoadSceneMode.Single);
            }
            else if (levelNum == 5) {
                SceneManager.LoadScene("Level 5", LoadSceneMode.Single);
            }
            
            if (levelNum > 0) {
                createdPlayableLevel = true;
            } 
        }         
    }

    public void setHighestLevelCompleted(int highestLevelProvided) {
        if (highestLevelProvided > highestLevelCompleted) {
            highestLevelCompleted = highestLevelProvided;
        }
    }

}
