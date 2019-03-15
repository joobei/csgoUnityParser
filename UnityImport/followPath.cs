using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPath : MonoBehaviour
{

    public pathing pathToFollow;
    private List<Transform> waypoints;
    private List<float> viewX;
    private List<float> viewY;
    public GameObject seperateYRotation;
    
    private int waypointID = 1;

    public float tickRate = 1f;
    private float reachDistance = 0.05f;

    //private Vector3 lastPosition;
    //private Quaternion lastOrientation;

    private void Start()
    { 
        waypoints = pathToFollow.waypoints;
        viewX = pathToFollow.viewDirectionX;
        viewY = pathToFollow.viewDirectionY;
        InvokeRepeating("TickUpdate",0f, 1 / tickRate);
    }

   
    void TickUpdate()
    {
        //lastPosition = transform.position;
        //lastOrientation = transform.rotation;

        Vector3 waypointPosition = waypoints[waypointID].position;
       

        Quaternion viewDirectionX = Quaternion.Euler(0, viewX[waypointID], 0);
        Quaternion viewDirectionY = Quaternion.Euler(viewY[waypointID], 0, 0);

        if (seperateYRotation != null)
        {
            seperateYRotation.transform.rotation = Quaternion.Euler(viewY[waypointID], viewX[waypointID], 0);
            viewDirectionY = Quaternion.identity;
        }

        Quaternion viewDirection = viewDirectionX * viewDirectionY;
        //float distance = Vector3.Distance(waypointPosition, lastPosition);


        transform.position = waypointPosition; //TODO no interpolation needed
        transform.rotation = viewDirection; //TODO interpolate


        //distance < reachDistance &&
        if ( waypointID < pathToFollow.waypoints.Count-1) waypointID++;
    }
}
