using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class pathing : MonoBehaviour
{
    public Color rayColor;

    public List<Transform> waypoints = new List<Transform>();

    public List<float> viewDirectionX;
    public List<float> viewDirectionY;

    private Transform[] waypointArray;
    
    

    private void OnDrawGizmos()
    {
        Gizmos.color = rayColor;
        waypointArray = GetComponentsInChildren<Transform>();
        waypoints.Clear();

        foreach (Transform path in waypointArray)
        {
            if (path != this.transform)
            {
                
                waypoints.Add(path);
            }
        }

        
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 position = waypoints[i].position;
            Gizmos.DrawWireSphere(position, .1f);

            if (i>0)
            {
                Vector3 previousPos = waypoints[i - 1].position;
                Gizmos.DrawLine(previousPos, position);
                
            }
        }
    }
}
