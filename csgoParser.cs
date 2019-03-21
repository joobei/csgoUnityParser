using DemoInfo;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;
using System.Numerics;

/// <summary>
/// Parses a replay and saves each player's position and view direction,
/// as well as game-specific info like map, match time or participating players
/// </summary>
public class csgoParser
{
    public Player[] Players;
    public Player[] Terrorists;
    public Player[] Counterterrorists;

    public string Map { get; private set; }
    public int RoundsPlayed { get; private set; }


    private const float PARSERTICKRATE = 32;
    private Dictionary<int, Dictionary<Player, List<AdvancedPosition>>> _pathInEveryRound;
    private Dictionary<int, int> _ticksPerRound;

    private string _defaultSaveFolder;
    private csvSaver csv;
    private string _filePath;
    
    private Team _winningTeam = Team.Spectate;
    private float _matchTime = 0;
    private int _highestParsedRound;

    public csgoParser(string fileName)
    {
        construct(fileName);
        _defaultSaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/CSV/";
    }

    public csgoParser(string fileName, string saveFolder)
    {
        construct(fileName);
        _defaultSaveFolder = saveFolder;
    }


    /// <summary>
    /// Parses the replay till the matchHasStarted event occurs
    /// Initializes game-specific info
    /// </summary>
    /// <returns>true if it successully ran through</returns>
    public bool ParseToMatchStart()
    {
        using (var fileStream = File.OpenRead(_filePath))
        {
            using (DemoParser parser = new DemoParser(fileStream))
            {
                parser.ParseHeader();
                Map = parser.Header.MapName;
                _matchTime = parser.Header.PlaybackTime;

                CancellationTokenSource tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;

                parser.MatchStarted += (sender, e) =>
                {
                    Counterterrorists = parser.PlayingParticipants.Where(a => a.Team == Team.CounterTerrorist).ToArray();
                    Terrorists = parser.PlayingParticipants.Where(a => a.Team == Team.Terrorist).ToArray();

                    Terrorists.CopyTo(Players, 0);
                    Counterterrorists.CopyTo(Players, 5);

                    tokenSource.Cancel();
                };
                parser.ParseToEnd(token);
            }
            return true;
        }
    }

    public bool parseAllRounds()
    {
        Player roundMVP = null;
        int roundsParsed = 0;
        int ticksDone = 0;
        bool hasMatchStarted = false;
        bool freezeTimeEnded = false;

        Dictionary<Player, List<AdvancedPosition>> PlayerPathsRound = new Dictionary<Player, List<AdvancedPosition>>();
        Dictionary<Player, List<float>> lastOrientationAlive = new Dictionary<Player, List<float>>();

        using (var fileStream = File.OpenRead(_filePath))
        {
            using (DemoParser parser = new DemoParser(fileStream))
            {
                parser.ParseHeader();

                parser.MatchStarted += (sender, e) =>
                {
                    hasMatchStarted = true;

                    Players = parser.PlayingParticipants.ToArray(); // neccessary? Y but why

                    PlayerPathsRound.fillWithValue(Players, new List<AdvancedPosition>());
                };

                parser.FreezetimeEnded += (sender, e) =>
                {
                    freezeTimeEnded = true;
                };

                parser.TickDone += (sender, e) =>
                {
                    if (hasMatchStarted && freezeTimeEnded)
                    {
                        foreach (Player p in Players)
                        {
                            if (p.IsAlive) lastOrientationAlive[p] = new List<float>(2) { p.ViewDirectionX, p.ViewDirectionY };

                            Vector3 pos = Extensions.FromSourceEngineVector(p.Position);

                            float viewX = p.ViewDirectionX;
                            float viewY = p.ViewDirectionY;

                            //Once the player dies, the reported position and orientation switches to the spectated player
                            if (!p.IsAlive)
                            {
                                pos = Extensions.FromSourceEngineVector(p.LastAlivePosition);
                                viewX = lastOrientationAlive[p].First();
                                viewY = lastOrientationAlive[p][1];
                            }

                            PlayerPathsRound.AddValueToExistingList(p, new AdvancedPosition(pos, viewX, viewY));

                        }
                        ticksDone++;
                    }
                };

                parser.RoundMVP += (sender, e) =>
                {
                    roundMVP = e.Player;
                };

                parser.WinPanelMatch += (sender, e) =>
                {
                    _winningTeam = roundMVP.Team;
                };

                parser.RoundOfficiallyEnd += (sender, e) =>
                {
                    roundsParsed++;


                    //ensure round has not been parsed before
                    if (roundsParsed > _highestParsedRound)
                    {
                        _highestParsedRound = roundsParsed;

                        //index starts at 1
                        Dictionary<Player, List<AdvancedPosition>> temp = new Dictionary<Player, List<AdvancedPosition>>(PlayerPathsRound);
                        _pathInEveryRound.Add(roundsParsed, temp);
                        PlayerPathsRound.refillDictionary<Player, List<AdvancedPosition>, AdvancedPosition>();

                        _ticksPerRound.Add(roundsParsed, ticksDone);
                        ticksDone = 0;
                    }

                    freezeTimeEnded = false;
                };


                parser.ParseToEnd();

                RoundsPlayed = roundsParsed;

                return true;
            }
        }
    }

    public List<AdvancedPosition> GetPlayerPathInRound(Player player, int round)
    {
        Dictionary<Player, List<AdvancedPosition>> playerPathsInThatRound;
        List<AdvancedPosition> path;

        _pathInEveryRound.TryGetValue(round, out playerPathsInThatRound);
        playerPathsInThatRound.TryGetValue(player, out path);

        return path;
    }

    public Dictionary<Player, List<AdvancedPosition>> GetAllPlayerPathInRound(int round)
    {
        Dictionary<Player, List<AdvancedPosition>> playerPathsInThatRound;
        _pathInEveryRound.TryGetValue(round, out playerPathsInThatRound);

        return playerPathsInThatRound;
    }

    public Dictionary<int, List<AdvancedPosition>> GetPlayerPathInAllRounds(Player player)
    {
        Dictionary<int, List<AdvancedPosition>> res = new Dictionary<int, List<AdvancedPosition>>();
        for (int i = 1; i <= RoundsPlayed; i++)
        {
            res.Add(i, GetPlayerPathInRound(player, i));
        }
        return res;
    }

    public int GetTicksPerRound(int round)
    {
        int ticksDone;
        _ticksPerRound.TryGetValue(round, out ticksDone);
        return ticksDone;
    }

    public float GetRoundLengthInSeconds(int round)
    {
        int ticksDone = GetTicksPerRound(round);
        return ticksDone / PARSERTICKRATE;
    }

    public string GetFormattedMatchTime()
    {

        int matchTimeMin = (int)_matchTime / 60;
        int RemainderMatchTimeSec = (int)_matchTime % 60;

        return matchTimeMin + ":" + RemainderMatchTimeSec + " min";
    }

    public Team GetWinningTeam()
    {
        return _winningTeam;
    }

    public void SaveToCSV()
    {
        SaveToCSV(_defaultSaveFolder);
    }

    public void SaveToCSV(string path)
    {
        for (int i = 1; i <= RoundsPlayed; i++)
        {
            foreach (Player p in Players)
            {
                SaveToCSV(p, i, path);
            }
        }
    }

    public void SaveToCSV(int round)
    {
        foreach (Player p in Players)
        {
            SaveToCSV(p, round);
        }
    }

    public void SaveToCSV(Player p)
    {
        for (int round = 0; round < RoundsPlayed; round++)
        {
            SaveToCSV(p, round, _defaultSaveFolder);
        }
    }

    public void SaveToCSV(Player p, int round)
    {
        SaveToCSV(p, round, _defaultSaveFolder);
    }

    public void SaveToCSV(Player p, int round, string path)
    {
        string title = p.Name + "-round_" + round;

        List<AdvancedPosition> data = GetPlayerPathInRound(p, round);

        csv.writeListCSV(data, title, path);

    }

    public string GetDefaultSaveFolder()
    {
        return _defaultSaveFolder;
    }

    private void construct(string fileName)
    {
        Reset();

        _filePath = fileName;
        csv = new csvSaver();

        ParseToMatchStart();
    }

    public override string ToString()
    {
        string res;
        string intro = string.Format("match on {0}\n match time: {1}", Map, GetFormattedMatchTime());

        string terrorists = "Terrorists: ";
        string counterterrorists = "Counterterrorists: ";

        foreach (Player p in Terrorists)
        {
            terrorists += string.Format("\n {0}", p.Name);
        }

        foreach (Player p in Counterterrorists)
        {
            counterterrorists += string.Format("\n {0}", p.Name);
        }

        res = intro + " \n" + terrorists + " \n" + counterterrorists;

        return res;
    }

    public string GetEndGameStatsString()
    {
        return string.Format("winning team: {0} \n {1} rounds played", GetWinningTeam(), RoundsPlayed);
    }

    public void Reset()
    {

        _filePath = "";
        _winningTeam = Team.Spectate;
        _matchTime = 0;

        Players = new Player[10];
        Terrorists = new Player[5];
        Counterterrorists = new Player[5];

        _highestParsedRound = 0;
        RoundsPlayed = 0;
        Map = "";


        _pathInEveryRound = new Dictionary<int, Dictionary<Player, List<AdvancedPosition>>>();
        _ticksPerRound = new Dictionary<int, int>();
    }
}
