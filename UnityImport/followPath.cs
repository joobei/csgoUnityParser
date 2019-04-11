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


    private void Start()
    { 
        waypoints = pathToFollow.waypoints;
        viewX = pathToFollow.viewDirectionX;
        viewY = pathToFollow.viewDirectionY;
        InvokeRepeating("TickUpdate",0f, 1 / tickRate);
    }

   //TODO  make a coroutine
    void TickUpdate()
    {
        Vector3 waypointPosition = waypoints[waypointID].position;
       
        Quaternion viewDirectionX = Quaternion.Euler(0, viewX[waypointID], 0);
        Quaternion viewDirectionY = Quaternion.Euler(viewY[waypointID], 0, 0);

        if (seperateYRotation != null)
        {
            seperateYRotation.transform.rotation = Quaternion.Euler(viewY[waypointID], viewX[waypointID], 0);
            viewDirectionY = Quaternion.identity;
        }
        Quaternion viewDirection = viewDirectionX * viewDirectionY;

        transform.position = waypointPosition; 
        transform.rotation = viewDirection;

        if ( waypointID < pathToFollow.waypoints.Count-1) waypointID++;
    }
}
