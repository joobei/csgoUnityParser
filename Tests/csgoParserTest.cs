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
        int round = 1;
        Player illegalName = parser.Players[5];

        parser.SaveToCSV(p, round, outputPath);
        parser.SaveToCSV(illegalName, round, outputPath);

        string[] files = Directory.GetFiles(outputPath);
        string[] filePath = files.Where(f => f.ContainsAll(p.Name,round.ToString())).ToArray();

        //directory is created and file is saved there
        Assert.IsTrue(Directory.Exists(outputPath));
        Assert.IsNotEmpty(files);
        Assert.IsNotEmpty(filePath);


        using (StreamReader reader = new StreamReader(filePath[0]))
        {
            header = reader.ReadLine();
            //file doesnt end after header
            Assert.IsFalse(reader.EndOfStream);
            secondLine = reader.ReadLine();
        }

        int amountOfCommataInValues = secondLine.Count(f => f == ',');
        int amountOfCommataInHeader = header.Count(f => f == ',');

        //header is written correctly
        Assert.IsTrue(header.ContainsAll("ticks", "posX", "posY", "posZ"));
        Assert.AreEqual(amountOfCommataInHeader, amountOfCommataInValues);
    }

    
}

