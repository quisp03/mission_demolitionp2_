using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    static private FollowCam S; // Another private Singleton
    static public GameObject POI; // The static point of interest

    public enum eView {none, slingshot, castle, both };

    [Header("Inscribed")]
    public float easing = 0.05f;
    public Vector2 minXY = Vector2.zero; // Vector2.zero is [0, 0]
    public GameObject viewBothGO;

    [Header("Dynamic")]
    public float camZ; // The desired Z pos of the camera
    public eView nextView = eView.slingshot;

    void Awake() {
        S = this;
        camZ = this.transform.position.z;
    }

    void FixedUpdate() {
        // A single-line if statement doesn't require braces
   //     if (POI == null) return; // if there is no POI, then return
    

        // Get the position of the poi
   //     Vector3 destination = POI.transform.position;
        // Limit the minimum values of destination.x & destination.y

        Vector3 destination = Vector3.zero;

        if (POI != null) {
            // If the POI has a Rigidbody, check to see if it is sleeping
            Rigidbody poiRigid = POI.GetComponent<Rigidbody>();
            if ((poiRigid != null) && poiRigid.IsSleeping()) {
                POI = null;
            }
        }

        if (POI != null) {
            destination = POI.transform.position;
        }

        destination.x = Mathf.Max(minXY.x, destination.x);
        destination.y = Mathf.Max(minXY.y, destination.y);
        // Interpolate from the current Camera position towards the desitnation
        destination = Vector3.Lerp(transform.position, destination, easing);
        // Force destiantion.z to be camZ to keep the camera far enough away
        destination.z = camZ;
        // Set the camera to the destination
        transform.position = destination;
        // Set the orthogaphicSize of the Camera to keep Ground in view
        Camera.main.orthographicSize = destination.y + 10;
    }

    public void SwitchView(eView newView) {
        if (newView == eView.none) {
            newView = nextView;
        }
        switch(newView) {
        case eView.slingshot:
            POI = null;
            nextView = eView.castle;
            break;
        case eView.castle:
            POI = MissionDemolition.GET_CASTLE();
            nextView = eView.both;
            break;
        case eView.both:
            POI = viewBothGO;
            nextView = eView.slingshot;
            break;
        }
    }
    public void SwitchView() {
        SwitchView(eView.none);
    }

    static public void SWITCH_VIEW(eView newView) {
        S.SwitchView(newView);
    }
}