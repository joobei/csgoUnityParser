using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DemoInfo;
using System;

/// <summary>
/// Unity wrapper for the csgo parser class
/// </summary>
public class csgoUnity : MonoBehaviour
{
    private csgoParser parser;

    public bool ReplayLoaded { get; private set; } //Marker for the editor
    public bool ReplayParsed { get; private set; } //Marker for the editor

    public csgoUnity(string fileName)
    {
        parser = new csgoParser(fileName);
    }

    public void parseToMatchStart()
    {
        ReplayLoaded = parser.ParseToMatchStart();
    }

    public void parseAllRounds()
    {
        ReplayParsed = parser.parseAllRounds();
    }

    public Player[] getPlayers()
    {
        if(ReplayParsed) throw new System.Exception("Parse replay first");
        return parser.Players;
    }

    public Player[] getTerrorists()
    {
        return parser.Terrorists.ToArray();
    }

    public Player[] getCounterterrorsts()
    {
        return parser.Counterterrorists.ToArray();
    }

    public string getMatchFormattedTime()
    {
        if (!ReplayLoaded) return "00:00";

        return parser.GetFormattedMatchTime();
    }

    public int GetRoundsPlayed()
    {
        return parser.RoundsPlayed;
    }

    public string GetMap()
    {
        return parser.Map;
    }

    public List<AdvancedPosition> getPlayerPathInRound(Player p, int round) => parser.GetPlayerPathInRound(p, round);

    public Dictionary<Player, List<AdvancedPosition>> getAllPlayerPathInRound(int round) => parser.GetAllPlayerPathInRound(round);

    public Dictionary<int, List<AdvancedPosition>> getPlayerPathInAllRounds(Player player) => parser.GetPlayerPathInAllRounds(player);

    public void saveToCSV(int round) => parser.SaveToCSV(round);

    public void saveToCSV(Player p) => parser.SaveToCSV(p);


    public void saveToCSV(Player p, int round) => parser.SaveToCSV(p, round);

    public void saveToCSV(Player p, int round, string path) => parser.SaveToCSV(p, round, path);

    public void Reset()
    {
        ReplayLoaded = false;
        ReplayParsed = false;
        parser.Reset();
    }
}
