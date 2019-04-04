using DemoInfo;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(csgoUnity))]
public class csgoParserEditor : Editor
{
    List<string> playerNames = new List<string>();
    int indexPlayer = 0;
    int indexRound = 0;

    Material[] mats; 
    
    bool saveToCSVToggle = false;
    bool colorsChosen = false;

    Dictionary<Player, Material> colors = new Dictionary<Player, Material>();

    public override void OnInspectorGUI()
    {
        var set = target as csgoUnity;

        if (GUILayout.Button("load replay"))
        {
            string fileName = EditorUtility.OpenFilePanel("choose replay", "", "dem");
            set.parseToMatchStart();
            UnityImporter.loadCorrespondingMap(set.GetMap());
            mats = UnityImporter.loadMaterials();
        }
        if (set.ReplayLoaded)
        {
            mats = UnityImporter.loadMaterials();

            EditorGUILayout.LabelField(set.GetMap());

            EditorGUILayout.LabelField(set.getMatchFormattedTime());

            GUILayout.Space(15);

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Counterterrorists", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical();

            int colorIndex = 0;
            foreach (Player p in set.getCounterterrorsts())
            {
                EditorGUILayout.LabelField(p.Name);
                playerNames.Add(p.Name);

                if (!colorsChosen && !colors.ContainsKey(p)) colors.Add(p, mats[colorIndex]);
                colorIndex++;
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(15);

            EditorGUILayout.LabelField("Terrorists", EditorStyles.boldLabel);


            EditorGUILayout.BeginVertical();

            colorIndex = 0;
            foreach (Player p in set.getTerrorists())
            {
                EditorGUILayout.LabelField(p.Name);
                playerNames.Add(p.Name);

                if (!colorsChosen && !colors.ContainsKey(p)) colors.Add(p, mats[colorIndex]);
                colorIndex++;
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            colorsChosen = true;

            if (GUILayout.Button("parse replay"))
            {
                set.parseAllRounds();
            }
            if (set.ReplayParsed)
            {
                indexPlayer = EditorGUILayout.Popup(indexPlayer, playerNames.ToArray());
                Player desiredPlayer = set.getPlayers().Where(p => p.Name == playerNames[indexPlayer]).First();

                string[] stringNaturals = Interval.Naturals(1, set.GetRoundsPlayed()).ToStringArray();

                indexRound = EditorGUILayout.Popup(indexRound, stringNaturals);

                saveToCSVToggle = GUILayout.Toggle(saveToCSVToggle, "save to CSV");

                if (GUILayout.Button("Get player path"))
                {
                    List<AdvancedPosition> playerPath = set.getPlayerPathInRound(desiredPlayer, indexRound);
                    
                    UnityImporter.combinePlayerWithPath(desiredPlayer, playerPath, colors[desiredPlayer], indexRound);

                    if (saveToCSVToggle)
                    {
                        set.saveToCSV(desiredPlayer, indexRound); 
                    }
                }
                if (GUILayout.Button("Get all player paths"))
                {
                    Dictionary <Player,List<AdvancedPosition>> playerPaths = set.getAllPlayerPathInRound(indexRound +1 );

                    foreach (Player p in playerPaths.Keys)
                    {
                      desiredPlayer = p;
                      UnityImporter.combinePlayerWithPath(desiredPlayer, playerPaths[desiredPlayer], colors[desiredPlayer], indexRound + 1);

                    }
                    if (saveToCSVToggle)
                    {
                        set.saveToCSV(indexRound);
                    }
                }
            }
        }
    }
}
