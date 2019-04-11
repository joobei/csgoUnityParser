using System.Collections.Generic;
using NUnit.Framework;
using DemoInfo;
using System.IO;
using System.Linq;



[TestFixture]
public class csgoParserTests
{
    
    string pathMirageDemo = @"Tests/test_demos/mirage.dem";
    string outputPath = @"Tests/output";
   
    // A Test behaves as an ordinary method
    [Test]
    public void testReplayProperlyLoaded()
    {
        csgoParser parser = new csgoParser(pathMirageDemo);
        



        Assert.AreEqual(parser.Map, "de_mirage");
        Assert.AreEqual(parser.Players.Length, 10);
    }

    [Test]
    public void testReplayProperlyParsed()
    {
        
        csgoParser parser = new csgoParser(pathMirageDemo);
        parser.ParseAllRounds();



        Assert.AreEqual(Team.CounterTerrorist, parser.GetWinningTeam());
        Assert.AreEqual(25, parser.RoundsPlayed);
        Assert.AreEqual(25, parser.GetPlayerPathInAllRounds(parser.Players[0]).Keys.Count);

        for (int i = 0; i < parser.RoundsPlayed; i++)
        {
            var allPlayerPathsInRound = parser.GetAllPlayerPathInRound(i); 

            //for every tick in a round a position is saved
            var ticksPerRound = parser.GetTicksPerRound(i);

            List<AdvancedPosition> path = allPlayerPathsInRound.getFirstKey();

            Assert.AreEqual(ticksPerRound, path.Count);
            

            //10 player path are saved each round
            Assert.AreEqual(allPlayerPathsInRound.Keys.Count, 10);
        }
    }

    [Test]
    public void testRefillDictionary()
    {
        
        csgoParser parser = new csgoParser(pathMirageDemo);

        Dictionary<Player, List<AdvancedPosition>> dic = new Dictionary<Player, List<AdvancedPosition>>();
        Player[] players = parser.Players;

        foreach (Player p in players)
        {
            List<AdvancedPosition> list = new List<AdvancedPosition>();
            list.Add(new AdvancedPosition(Extensions.FromSourceEngineVector(p.Position), p.ViewDirectionX, p.ViewDirectionY));
            dic.Add(p, list);
        }

        dic.refillDictionary<Player, List<AdvancedPosition>, AdvancedPosition>();

        foreach (Player p in players)
        {
            CollectionAssert.AreEqual(new List<AdvancedPosition>(), dic[p]);
        }

    }

    [Test]
    public void testKillfeed()
    {
        csgoParser parser = new csgoParser(pathMirageDemo);
        parser.ParseAllRounds();

        Dictionary<int, Dictionary<int, List<PlayerKilledEventArgs>>> killfeed = parser.GetKillFeed();

        Assert.AreEqual(killfeed.Keys.Count, parser.RoundsPlayed);
        foreach (var item in killfeed.Keys)
        {
            Dictionary<int, List<PlayerKilledEventArgs>> kills =  killfeed[item];
            //Latest kill is still in the round
            Assert.LessOrEqual(kills.Keys.Last(), parser.GetTicksPerRound(item));
            //No more than 10 deaths are possible in a round
            Assert.LessOrEqual(kills.Count,10);
        }
    }

    [Test]  
    public void testMainFunction()
    {
        string[] args = new string[2];
        args[0] = pathMirageDemo;
        args[1] = outputPath;

        TestContext.WriteLine(args[0]);
        TestContext.WriteLine(args[1]);

        StartUp_console.Main(args);
    }

    [Test]
    public void testCSVWriter()
    {
        csgoParser parser = new csgoParser(pathMirageDemo);
        parser.ParseAllRounds();
        Player p = parser.Players[0];
        string header;
        string secondLine;
        string header2;
        string secondLine2;
        int round = 1;
        Player illegalName = parser.Players[5];

        parser.SaveToCSV(p, round, outputPath);
        parser.SaveToCSV(illegalName, round, outputPath);
        parser.saveOnlyKillFeed(outputPath);

        string[] files = Directory.GetFiles(outputPath);
        string[] filePlayerPath = files.Where(f => f.ContainsAll(p.Name, round.ToString())).ToArray();
        string[] fileKillfeedPath = files.Where(f => f.ContainsAll("killFeed", round.ToString())).ToArray();

        //directory is created and file is saved there
        Assert.IsTrue(Directory.Exists(outputPath));
        Assert.IsNotEmpty(files);
        Assert.IsNotEmpty(filePlayerPath);
        Assert.IsNotEmpty(fileKillfeedPath);

        using (StreamReader reader = new StreamReader(filePlayerPath[0]))
        {
            header = reader.ReadLine();
            //file doesnt end after header
            Assert.IsFalse(reader.EndOfStream);
            secondLine = reader.ReadLine();
        }

        using (StreamReader reader = new StreamReader(fileKillfeedPath[0]))
        {
            header2 = reader.ReadLine();
            //file doesnt end after header
            Assert.IsFalse(reader.EndOfStream);
            secondLine2 = reader.ReadLine();
        }

        int amountOfCommataInValues = secondLine.Count(f => f == ',');
        int amountOfCommataInHeader = header.Count(f => f == ',');

        //header is written correctly
        Assert.IsTrue(header.ContainsAll("ticks", "posX", "posY", "posZ"));
      
        Assert.AreEqual(amountOfCommataInHeader, amountOfCommataInValues);
        Assert.AreEqual(header2.Count(f => f == ','), secondLine2.Count(f => f == ','));
    }

    
}

