using DemoInfo;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;
using System.Numerics;

public class csgoParser
{
    public Player[] Players = new Player[10];
    public List<Player> Terrorists;
    public List<Player> Counterterrorists;

    public int HighestParsedRound { get; private set; }
    public int RoundsPlayed { get; private set; }

    public string Map { get; private set; }

    public Dictionary<int, Dictionary<Player, List<AdvancedPosition>>> pathInEveryRound;

    private Dictionary<int, int> ticksPerRound;

    private string defaultSaveFolder;
    private csvSaver csv = new csvSaver();
    private string filePath;
    private float parserTickrate = 32;
    private Team winningTeam = Team.Spectate;
    private float matchTime = 0;

    public csgoParser(string fileName)
    {
        Construct(fileName);
        defaultSaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/CSV/";
    }

    public csgoParser(string fileName,string saveFolder)
    {
        Construct(fileName);
        defaultSaveFolder = saveFolder;
    }

    public void Construct(string fileName)
    {
        Reset();
        
        filePath = fileName;

        parseToMatchStart();
    }

    public bool parseToMatchStart()
    {
        using (var fileStream = File.OpenRead(filePath))
        {
            using (DemoParser parser = new DemoParser(fileStream))
            {
                parser.ParseHeader();
                Map = parser.Header.MapName;    

                matchTime = parser.Header.PlaybackTime;

                CancellationTokenSource tokenSource = new CancellationTokenSource();
                CancellationToken token = tokenSource.Token;

                parser.MatchStarted += (sender, e) =>
                {
                    Counterterrorists = parser.PlayingParticipants.Where(a => a.Team == Team.CounterTerrorist).ToList();
                    Terrorists = parser.PlayingParticipants.Where(a => a.Team == Team.Terrorist).ToList();

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

        using (var fileStream = File.OpenRead(filePath))
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
                    winningTeam = roundMVP.Team;                    
                };

                parser.RoundOfficiallyEnd += (sender, e) =>
                {
                    roundsParsed++;
                   

                    //ensure round has not been parsed before
                    if (roundsParsed > HighestParsedRound)
                    {
                        HighestParsedRound = roundsParsed;

                        //index starts at 1
                        Dictionary<Player, List<AdvancedPosition>> temp = new Dictionary<Player, List<AdvancedPosition>>(PlayerPathsRound);
                        pathInEveryRound.Add(roundsParsed, temp);
                        PlayerPathsRound.refillDictionary<Player,List<AdvancedPosition>,AdvancedPosition>();

                        ticksPerRound.Add(roundsParsed, ticksDone);
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

    public List<AdvancedPosition> getPlayerPathInRound(Player player, int round)
    {
        Dictionary<Player, List<AdvancedPosition>> playerPathsInThatRound;
        List<AdvancedPosition> path;

        pathInEveryRound.TryGetValue(round, out playerPathsInThatRound);
        playerPathsInThatRound.TryGetValue(player, out path);

        return path;
    }

    public Dictionary<Player,List<AdvancedPosition>> getAllPlayerPathInRound(int round)
    {
        Dictionary<Player, List<AdvancedPosition>> playerPathsInThatRound;
        pathInEveryRound.TryGetValue(round, out playerPathsInThatRound);

        return playerPathsInThatRound;
    }

    public Dictionary<int , List<AdvancedPosition>> getPlayerPathInAllRounds(Player player)
    {
        Dictionary<int, List<AdvancedPosition>> res = new Dictionary<int, List<AdvancedPosition>>();
        for (int i = 1; i <= RoundsPlayed; i++)
        {
            res.Add(i, getPlayerPathInRound(player, i));
        }

        return res;
    }

    public int getTicksPerRound(int round)
    {
        int ticksDone;
        ticksPerRound.TryGetValue(round, out ticksDone);
        return ticksDone;
    }

    public float getRoundLengthInSeconds(int round)
    {
        int ticksDone = getTicksPerRound(round);
        return ticksDone / parserTickrate;
    }

    public string getFormattedMatchTime()
    {

        int matchTimeMin = (int)matchTime / 60;
        int RemainderMatchTimeSec = (int)matchTime % 60;

        return matchTimeMin + ":" + RemainderMatchTimeSec + " min";
    }

    public Team getWinningTeam()
    {
        return winningTeam;
    }

    public void saveToCSV() => saveToCSV(defaultSaveFolder);
    public void saveToCSV(string path)
    {
        for (int i = 1; i <= RoundsPlayed; i++)
        {
            foreach (Player p in Players)
            {
            saveToCSV(p,i,path);
            }  
        }
    }

    public void saveToCSV(int round)
    {
        foreach (Player p in Players)
        {
            saveToCSV(p, round);
        }
    }

    public void saveToCSV(Player p)
    {
        for (int round = 0; round < RoundsPlayed; round++)
        {
            saveToCSV(p, round,defaultSaveFolder);
        }  
    }

    public void saveToCSV(Player p, int round)
    {
        saveToCSV(p, round, defaultSaveFolder);
    }

    public void saveToCSV(Player p, int round, string path)
    {
        string title = p.Name + "-round_" + round;

        List<AdvancedPosition> data = getPlayerPathInRound(p, round);

        csv.writeListCSV(data, title, path);

    }

    public string GetDefaultSaveFolder()
    {
        return defaultSaveFolder;
    }

    override
    public string ToString()
    {
        string res;
        string intro = string.Format("match on {0}\n match time: {1}", Map,getFormattedMatchTime());

        string terrorists = "Terrorists: ";
        string counterterrorists = "Counterterrorists: ";

        foreach (Player  p in Terrorists)
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
        return string.Format("winning team: {0} \n {1} rounds played", getWinningTeam(), RoundsPlayed);
    }

    public void Reset()
    {

        filePath = "";
        parserTickrate = 1;
        winningTeam = Team.Spectate;
        matchTime = 0;

        Players = new Player[10];
        Terrorists = new List<Player>();
        Counterterrorists = new List<Player>();

        HighestParsedRound = 0;
        RoundsPlayed = 0;
        Map = "";
        

        pathInEveryRound = new Dictionary<int, Dictionary<Player, List<AdvancedPosition>>>();
        ticksPerRound = new Dictionary<int, int>();
    }
}
