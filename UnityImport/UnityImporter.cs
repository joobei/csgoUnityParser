using UnityEngine;
using System.Collections.Generic;
using DemoInfo;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;


public static class UnityImporter
{

    public static pathing createPathFromListVector3(List<Vector3> listVectors, string name = "pathholder", Color? color = null)
    {
        GameObject map = GameObject.FindGameObjectsWithTag("map")[0];
        GameObject pathHolder = new GameObject(name);
        pathHolder.tag = "Player";
        pathHolder.AddComponent<pathing>();
        pathHolder.transform.SetParent(map.transform);
        pathHolder.transform.position = listVectors[0];

        int i = 0;
        foreach (Vector3 item in listVectors)
        {
            GameObject waypoint = new GameObject("point: " + i);
            waypoint.transform.position = item;
            waypoint.transform.SetParent(pathHolder.transform);
            i++;
        }

        pathing script = pathHolder.GetComponent<pathing>();

        //ensures color != null
        color.GetValueOrDefault(Color.green);

        script.rayColor = (Color)color;
        return script;
    }

    public static void combinePlayerWithPath(Player p, List<AdvancedPosition> advancedPath, Material mat , int round)
    {
        float scaleFactor = 20;

        List<Vector3> path = new List<Vector3>();
        List<float> viewX = new List<float>();
        List<float> viewY = new List<float>();

        foreach (AdvancedPosition item in advancedPath)
        {
            path.Add(item.GetPosition().castToUnityVector3());
            viewX.Add(item.getOrientationX());
            viewY.Add(item.getOrientationY());
        }

        GameObject map = GameObject.FindGameObjectsWithTag("map")[0];

        //TODO turn model 180 degrees |imported wrong way around | DID NOT FIX
        GameObject player = GameObject.Instantiate(Resources.Load<GameObject>("visualization models/player_char"));

        player.tag = "Player";
        player.name = p.Name;

        foreach (var item in player.transform.GetComponentsInChildren<Renderer>())
        {
             item.material = mat;
        }
       
        player.transform.localScale = Vector3.one * scaleFactor;
        player.transform.SetParent(map.transform);
        player.transform.position = path[0];

        player.AddComponent<followPath>();
        followPath followScript = player.GetComponent<followPath>();

        string pathName = player.name + " path " + "round: " + round;
        followScript.pathToFollow = createPathFromListVector3(path, pathName, mat.color);
        followScript.pathToFollow.viewDirectionX = viewX;
        followScript.pathToFollow.viewDirectionY = viewY;
        followScript.seperateYRotation = player.transform.GetChild(1).gameObject;

        //adjust to tickrate
        followScript.tickRate = 32;
    }
    
    public static void loadCorrespondingMap(string mapName)
    {
        float scaleFactor = 20;

        GameObject map = Object.Instantiate(Resources.Load("maps/" + mapName, typeof(GameObject))) as GameObject;
        map.name = mapName;
        map.tag = "map";
        map.transform.localScale *= scaleFactor;


    }

    public static Material[] loadMaterials()
    {
        string[] colors = { "blue", "green", "yellow", "orange", "purple"};
        Material[] mats = new Material[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            string path = "mats/player_colors/" + colors[i];
            mats[i] = Object.Instantiate(Resources.Load(path, typeof(Material))) as Material; 
        }
        return mats;
    }

    public static void writeCSVFromListVector(string title, List<Vector3> data)
    {
        string path = Application.dataPath + "/CSV/" ;
        Directory.CreateDirectory(path);
        path += title + ".csv";
        

        using (StringWriter writer = new StringWriter())
        {

            writer.WriteLine("ticks,posX,posY,PosZ");

            for (int ticksDone = 0; ticksDone < data.Count; ticksDone++)
            {
                Vector3 current = data[ticksDone];
                writer.WriteLine((ticksDone + 1) + "," + current.x + "," + current.y + "," + current.z);
            }

            using (StreamWriter output = new StreamWriter(path, false))
            {
                output.Write(writer.ToString());
            }
        }
    }


    [DidReloadScripts]
    public static void onReload()
    {
        GameObject toRefresh = GameObject.FindGameObjectsWithTag("refreshOnCompile")[0];
        toRefresh.GetComponent<csgoUnity>().Reset();
        GameObject[] destroy = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] destroy2 = GameObject.FindGameObjectsWithTag("map");
        GameObject[] destroy3 = GameObject.FindGameObjectsWithTag("deleteOnCompile");

        List<GameObject> destroyObjects = new List<GameObject>();

        destroyObjects.AddRange(destroy);
        destroyObjects.AddRange(destroy2);
        destroyObjects.AddRange(destroy3);

        foreach (GameObject item in destroyObjects) Object.DestroyImmediate(item);
    }
}

