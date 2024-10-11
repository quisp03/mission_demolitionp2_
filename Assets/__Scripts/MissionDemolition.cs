using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum GameMode {
    idle,
    playing,
    levelEnd
}

public class MissionDemolition : MonoBehaviour
{
    static private MissionDemolition S; // a private Singleton

    [Header("Inscribed")]
    public TextMeshProUGUI uitLevel; // TextMeshPro for the UIText_Level Text
    public TextMeshProUGUI uitShots; // TextMeshPro for the UIText_Shots Text
    public TextMeshProUGUI uitPoints; // TextMeshPro for the UIText_Points Text (New)
    public Vector3 castlePos; // The place to put castles
    public GameObject[] castles; // An array of the castles

    [Header("Dynamic")]
    public int level; // The current level
    public int levelMax; // The number of levels
    public int shotsTaken; // The number of shots taken
    public GameObject castle; // The current castle
    public GameMode mode = GameMode.idle;
    public string showing = "Show Slingshot"; // FollowCam mode
    public int points; // Initialize points to 0 (New)
    public int pointsPerLevel; // Starting points per level (New)

    void Start() {
        S = this; // Define the Singleton

        level = 0;
        shotsTaken = 0;
        points = 0; // Initialize points to 0
        levelMax = castles.Length;
        pointsPerLevel = 1000; // Set points per level
        StartLevel();
    }
    
    void StartLevel() {
        // Get rid of the old castle if one exists
        if (castle != null) {
            Destroy(castle);
        }

        // Destroy old projectiles if they exist (the method is not yet written)
        Projectile.DESTROY_PROJECTILES(); // Placeholder for method

        // Instantiate the new castle
        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;

        // Reset the goal
        Goal.goalMet = false;

        UpdateGUI();

        mode = GameMode.playing;
    }

    void UpdateGUI() {
        // Show the data in the GUITexts
        uitLevel.text = "Level: " + (level + 1) + " of " + levelMax;
        uitShots.text = "Shots Taken: " + shotsTaken;
        uitPoints.text = "Points: " + points; // Update points display
    }

    void Update() {
        UpdateGUI();

        // Check for level end
        if ((mode == GameMode.playing) && Goal.goalMet) {
            // Calculate points based on the number of shots taken
            CalculatePoints();

            // Change mode to stop checking for level end
            mode = GameMode.levelEnd;

            // Start the next level in 2 seconds
            Invoke("NextLevel", 2f);
        }
    }

    void NextLevel() {
        level++;
        if (level == levelMax) {
            level = 0;
            shotsTaken = 0;
            // Don't reset points if you want to accumulate points across levels
        }
        StartLevel();
    }

    // Static method that allows code anywhere to increment shotsTaken
    static public void SHOT_FIRED() {
        S.shotsTaken++;
    }

    // Static method allows code anywhere to get a reference to S.castle
    static public GameObject GET_CASTLE() {
        return S.castle;
    }

    // Method to calculate points based on shots taken
    void CalculatePoints() {
        int pointsForThisLevel = Mathf.Max(pointsPerLevel - (shotsTaken * 100), 0); 
        points += pointsForThisLevel; // Add points for this level to the total points
    }
}
