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
/// <remarks>all round indexes start at 1</remarks>
/// </summary>
public class csgoParser
{
    public Player[] Players;
    public Player[] Terrorists;
    public Player[] Counterterrorists;

    public string Map
    {
        get;
        private set;
    }

    public int RoundsPlayed
    {
        get;
        private set;
    }


    private const float PARSERTICKRATE = 32;
    private Dictionary<int, Dictionary<Player, List<AdvancedPosition>>> _pathInEveryRound;
    private Dictionary<int, int> _ticksPerRound;
    private Dictionary<int, Dictionary<int, List<PlayerKilledEventArgs>>> _killFeed;

    private string _defaultSaveFolder;
    private string _filePath;

    private Team _winningTeam;
    private float _matchTime;

    /// <summary>
    /// Constructs a new csgo parser for the given replay at the given file path.
    /// default save folder is MyDocuments\CSV.
    /// </summary>
    /// <param name="fileName">path of the replay file</param>
    public csgoParser(string fileName)
    {
        construct(fileName);
        _defaultSaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/CSV/";
    }

    /// <summary>
    /// Constructs a new csgo parser with a specified savefolder, to which all CSV files get saved,
    /// if not specified otherwise.
    /// </summary>
    /// <param name="fileName">path to the replay file</param>
    /// <param name="saveFolder">the folder to which to save the CSV files</param>
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

    public bool ParseAllRounds()
    {
        Player roundMVP = null;
        int roundsParsed = 0;
        int ticksDone = 0;
        bool hasMatchStarted = false;
        bool freezeTimeEnded = false;

        Dictionary<Player, List<AdvancedPosition>> playerPathsRound = new Dictionary<Player, List<AdvancedPosition>>();
        Dictionary<Player, List<float>> lastOrientationAlive = new Dictionary<Player, List<float>>();
        Dictionary<int, List<PlayerKilledEventArgs>> playersKilledThatRound = new Dictionary<int, List<PlayerKilledEventArgs>>();

        using (var fileStream = File.OpenRead(_filePath))
        {
            using (DemoParser parser = new DemoParser(fileStream))
            {
                parser.ParseHeader();

                parser.MatchStarted += (sender, e) =>
                {
                    hasMatchStarted = true;

                    Players = parser.PlayingParticipants.ToArray();

                    playerPathsRound.fillWithValue(Players, new List<AdvancedPosition>());
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
                            if (p.IsAlive) lastOrientationAlive[p] = new List<float>(2) {
                                p.ViewDirectionX,
                                p.ViewDirectionY
                            };

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
                            playerPathsRound.AddValueToExistingList(p, new AdvancedPosition(pos, viewX, viewY));
                        }
                        ticksDone++;
                    }
                };

                parser.PlayerKilled += (sender, e) =>
                {
                    //TODO simultaneously killed players
                    if (playersKilledThatRound.ContainsKey(ticksDone))
                    {
                        playersKilledThatRound.AddValueToExistingList(ticksDone, e);
                    }
                    else playersKilledThatRound.Add(ticksDone, new List<PlayerKilledEventArgs>());
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

                    //index starts at 1
                    Dictionary<Player, List<AdvancedPosition>> temp = new Dictionary<Player, List<AdvancedPosition>>(playerPathsRound);
                    _pathInEveryRound.Add(roundsParsed, temp);
                    playerPathsRound.refillDictionary<Player, List<AdvancedPosition>, AdvancedPosition>();

                    Dictionary<int, List<PlayerKilledEventArgs>> temp2 = new Dictionary<int, List<PlayerKilledEventArgs>>(playersKilledThatRound);
                    _killFeed.Add(roundsParsed, temp2);
                    playersKilledThatRound = new Dictionary<int, List<PlayerKilledEventArgs>>();

                    _ticksPerRound.Add(roundsParsed, ticksDone);
                    ticksDone = 0;

                    freezeTimeEnded = false;
                };

                parser.ParseToEnd();

                RoundsPlayed = roundsParsed;

                return true;
            }
        }
    }
    /// <summary>
    /// Get the path a player took in a certain round
    /// </summary>
    /// <param name="player">the player</param>
    /// <param name="round">the round</param>
    public List<AdvancedPosition> GetPlayerPathInRound(Player player, int round)
    {
        _pathInEveryRound.TryGetValue(round, out Dictionary<Player, List<AdvancedPosition>> playerPathsInThatRound);
        playerPathsInThatRound.TryGetValue(player, out List<AdvancedPosition> path);

        return path;
    }

    /// <summary>
    /// Get a Dictionary with all players and their respective paths in a specified round
    /// </summary>
    /// <param name="round">the round</param>
    /// <returns>a dictionary,in which players are keys and values their respective path </returns>
    public Dictionary<Player,
    List<AdvancedPosition>> GetAllPlayerPathInRound(int round)
    {
        _pathInEveryRound.TryGetValue(round, out Dictionary<Player, List<AdvancedPosition>> playerPathsInThatRound);

        return playerPathsInThatRound;
    }

    /// <summary>
    /// Get the paths for a certain player over all rounds played
    /// </summary>
    /// <param name="player">the player</param>
    /// <returns>a dictionary, in which the round is the key and the value the player's path</returns>
    public Dictionary<int, List<AdvancedPosition>> GetPlayerPathInAllRounds(Player player)
    {
        Dictionary<int,
        List<AdvancedPosition>> res = new Dictionary<int,
        List<AdvancedPosition>>();
        for (int i = 1;
        i <= RoundsPlayed;

        i++)
        {
            res.Add(i, GetPlayerPathInRound(player, i));
        }

        return res;
    }

    /// <summary>
    /// the number of ticks a round was going on for
    /// </summary>
    /// <param name="round">the round number</param>
    /// <returns>number of ticks</returns>
    public int GetTicksPerRound(int round)
    {
        _ticksPerRound.TryGetValue(round, out int ticksDone);
        return ticksDone;
    }

    /// <summary>
    /// The time a round took, in seconds - corresponding to the parser's tickrate
    /// </summary>
    /// <param name="round">the round</param>
    /// <returns>float with the duration in seconds</returns>
    public float GetRoundLengthInSeconds(int round)
    {
        int ticksDone = GetTicksPerRound(round);
        return ticksDone / PARSERTICKRATE;
    }

    /// <summary>
    /// Get a well formatted string of the total match time
    /// </summary>
    /// <returns>a string ["min:sec min"]</returns>
    public string GetFormattedMatchTime()
    {

        int matchTimeMin = (int)_matchTime / 60;
        int RemainderMatchTimeSec = (int)_matchTime % 60;

        return matchTimeMin + ":" + RemainderMatchTimeSec + " min";
    }

    /// <summary>
    /// Get the team that won the game - call <c> parseAllRounds() </c>  first
    /// Otherwise winning team = spectators
    /// </summary>
    /// <returns>the team, which won the game </returns>
    public Team GetWinningTeam()
    {
        return _winningTeam;
    }

    /// <summary>
    /// Saves all player paths to the default save folder
    /// </summary>
    public void SaveToCSV()
    {
        SaveToCSV(_defaultSaveFolder);
        saveOnlyKillFeed(_defaultSaveFolder);
    }

    /// <summary>
    /// saves all player paths to the specified path
    /// </summary>
    /// <param name="path">save folder</param>
    public void SaveToCSV(string path)
    {
        for (int i = 1; i <= RoundsPlayed; i++)
        {
            foreach (Player p in Players)
            {
                SaveToCSV(p, i, path);
            }
        }
        saveOnlyKillFeed(path);
    }

    /// <summary>
    /// saves all player paths in a round to the default save folder
    /// </summary>
    /// <param name="round">the round</param>
    public void SaveToCSV(int round)
    {
        foreach (Player p in Players)
        {
            SaveToCSV(p, round);
        }
    }

    /// <summary>
    /// saves a players path in each round to the default save folder
    /// </summary>
    /// <param name="p">the player object</param>
    public void SaveToCSV(Player p)
    {
        for (int round = 0; round < RoundsPlayed; round++)
        {
            SaveToCSV(p, round, _defaultSaveFolder);
        }
    }

    /// <summary>
    /// Saves a certain player's path in a certain round to a optional path
    /// </summary>
    /// <param name="p">the player object</param>
    /// <param name="round">the specified round</param>
    /// <param name="path">the optional save path</param>
    public void SaveToCSV(Player p, int round, string path = "")
    {
        if (string.IsNullOrEmpty(path)) path = _defaultSaveFolder;

        string title = p.Name + "-round_" + round;

        List<AdvancedPosition> data = GetPlayerPathInRound(p, round);

        string header = "ticks,posX,posY,posZ,viewX,viewY";

        csvSaver.writeListCSV(data, title, path, header);

    }

    /// <summary>
    /// save the killfeed to a optional directory in a csv format
    /// </summary>
    /// <param name="pathDirectory">the optional directory path</param>
    public void saveOnlyKillFeed(string pathDirectory = "")
    {
        if (string.IsNullOrEmpty(pathDirectory)) pathDirectory = _defaultSaveFolder;
        string header = "victim,killer,assist,weapon,headshot";

        foreach (int item in _killFeed.Keys)
        {
            string pathFile = csvSaver.createCSVFile(pathDirectory, "killFeed_round" + item, header);
            foreach (int tick in GetKillFeedInRound(item).Keys)
            {
                List<PlayerKilledEventArgs> kills = GetKillFeedInRound(item)[tick];
                foreach (PlayerKilledEventArgs args in kills)
                {
                    string csvWritable = string.Format("{0},{1},{2},{3},{4},{5}", tick, args.Victim.Name, args.Killer.Name, args.Assister.Name, args.Weapon.OriginalString, args.Headshot);
                    csvSaver.addLineToFile(pathFile, csvWritable);
                }
            }
        }
    }

    /// <summary>
    /// Gets the default save folder
    /// </summary>
    /// <returns>default save folder</returns>
    public string GetDefaultSaveFolder()
    {
        return _defaultSaveFolder;
    }

    /// <summary>
    /// Gets the kill feed
    /// </summary>
    /// <returns>a dictionary , in which the rounds are keys to a second dictionary with all ticks at which a player died</returns>
    public Dictionary<int, Dictionary<int, List<PlayerKilledEventArgs>>> GetKillFeed()
    {
        return _killFeed;
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

    /// <summary>
    /// Gets a string with the winnig team and the rounds played
    /// </summary>
    /// <returns>a well formatted string with the winning team and the number of rounds played</returns>
    public string GetEndGameStatsString()
    {
        return string.Format("winning team: {0} \n {1} rounds played", GetWinningTeam(), RoundsPlayed);
    }

    /// <summary>
    /// Gets the kill feed in a specified round
    /// </summary>
    /// <param name="round">the round</param>
    /// <returns>the dictionary, where ticks are keys and PlayerKilledEventArgs are values</returns>
    public Dictionary<int, List<PlayerKilledEventArgs>> GetKillFeedInRound(int round)
    {
        _killFeed.TryGetValue(round, out Dictionary<int, List<PlayerKilledEventArgs>> killFeedInRound);

        return killFeedInRound;
    }

    public void Reset()
    {
        _winningTeam = Team.Spectate;
        _matchTime = 0;

        Players = new Player[10];
        Terrorists = new Player[5];
        Counterterrorists = new Player[5];

        RoundsPlayed = 0;
        Map = "";


        _pathInEveryRound = new Dictionary<int, Dictionary<Player, List<AdvancedPosition>>>();
        _ticksPerRound = new Dictionary<int, int>();
        _killFeed = new Dictionary<int, Dictionary<int, List<PlayerKilledEventArgs>>>();
    }


    ////private methods
    ///
    private void construct(string fileName)
    {
        Reset();

        _filePath = fileName;

        ParseToMatchStart();
    }
}