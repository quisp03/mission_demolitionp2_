using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    // fields set in the Unity Inspector pane
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;
    public Transform leftArm;  // Left arm of the slingshot
    public Transform rightArm; // Right arm of the slingshot
    public AudioSource rubberBandSnapSound; // Sound to play when the projectile is shot

    // fields set dynamically
    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;
    
    private LineRenderer lineRenderer; // Line Renderer for the rubber band

    void Awake() {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        // Get the Line Renderer component from the slingshot
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 3; // Set 3 points: leftArm, projectile, rightArm

        // Get the AudioSource component if not manually assigned
        if (rubberBandSnapSound == null) {
            rubberBandSnapSound = GetComponent<AudioSource>(); // Try to find the AudioSource on this GameObject
            if (rubberBandSnapSound == null) {
                Debug.LogWarning("No AudioSource found on Slingshot! Please add one.");
            }
        }
    }

    // Do not change!
    void OnMouseEnter()
    {
        // print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        // print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown() {
        // The player has pressed the mouse button while over Slingshot
        aimingMode = true;
        // Instantiate a Projectile
        projectile = Instantiate( projectilePrefab ) as GameObject;
        // Start it at the launchPoint
        projectile.transform.position = launchPos;
        // Set it to isKinematic for now
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        // Enable Line Renderer and set the initial points
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, leftArm.position); // Left arm position
        lineRenderer.SetPosition(1, projectile.transform.position); // Projectile position
        lineRenderer.SetPosition(2, rightArm.position); // Right arm position
    }

    void Update() {
        // If Slingshot is not in aimingMode, don't run this code
        if (!aimingMode) return;
        
        // Get the current mouse position in 2D screen coordinates
        Vector3 mousePos2D = Input.mousePosition;
        
        // Set the Z-axis relative to the slingshot’s position (to keep it aligned with the plane)
        mousePos2D.z = Camera.main.WorldToScreenPoint(launchPos).z;  // Use Z-depth of launchPos
        
        // Convert screen coordinates to world coordinates
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);
        
        // Find the delta from the launchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;

        // Limit mouseDelta to the radius of the Slingshot SphereCollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude) {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        // Move the projectile to this new position (adjusting z-axis to stay on the slingshot's plane)
        Vector3 projPos = launchPos + mouseDelta;
        projPos.z = launchPos.z;  // Ensure the projectile stays on the same Z-plane as the slingshot
        projectile.transform.position = projPos;

        // Update the Line Renderer for the rubber band
        lineRenderer.SetPosition(1, projectile.transform.position); // Update projectile position

        if (Input.GetMouseButtonUp(0)) {  // This is a zero, not a o
            // The mouse has been released
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            // Play the sound when the projectile is released
            if (rubberBandSnapSound != null) {
                Debug.Log("Playing rubber band snap sound");
                rubberBandSnapSound.Play();
            } else {
                Debug.Log("AudioSource not assigned or found");
            }

            // Switch to slingshot view immediately before setting POI
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile; // Set the _MainCamera POI

            // Add a ProjectileLine to the Projectile
            Instantiate<GameObject>(projLinePrefab, projectile.transform);

            // Disable the Line Renderer after releasing the projectile
            lineRenderer.enabled = false;

            projectile = null;
            MissionDemolition.SHOT_FIRED();
        }
    }
}
