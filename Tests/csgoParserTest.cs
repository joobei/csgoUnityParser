
using System.Collections.Generic;
using NUnit.Framework;
using DemoInfo;
using System.IO;
using System.Linq;



[TestFixture]
public class csgoParserTests
{
    //TODO relative path
    string pathMirageDemo = @"D:/Benutzer/5haffke/csgoParserProject/Tests/test_demos/mirage.dem";
    string outputPath = @"D:/Benutzer/5haffke/csgoParserProject/Tests/output";
   
    // A Test behaves as an ordinary method
    [Test]
    public void testReplayProperlyLoaded()
    {
        csgoParser parser = new csgoParser(pathMirageDemo);
        parser.parseToMatchStart();



        Assert.AreEqual(parser.Map, "de_mirage");
        Assert.AreEqual(parser.Players.Length, 10);
    }

    [Test]
    public void testReplayProperlyParsed()
    {
        
        csgoParser parser = new csgoParser(pathMirageDemo);
        parser.parseAllRounds();



        Assert.AreEqual(Team.CounterTerrorist, parser.getWinningTeam());
        Assert.AreEqual(25, parser.RoundsPlayed);
        Assert.AreEqual(25, parser.pathInEveryRound.Keys.Count);
        Assert.AreEqual(25, parser.getPlayerPathInAllRounds(parser.Players[0]).Keys.Count);

        foreach (var item in parser.pathInEveryRound)
        {
            var underlyingDictionary = item.Value;

            //for every tick in a round a position is saved
            var ticksPerRound = parser.getTicksPerRound(item.Key);

            List<AdvancedPosition> path = underlyingDictionary.getFirstValue();

            Assert.AreEqual(ticksPerRound, path.Count);
            

            //10 player path are saved each round
            Assert.AreEqual(underlyingDictionary.Keys.Count, 10);
        }
    }

    [Test]
    public void testRefillDictionary()
    {
        
        csgoParser parser = new csgoParser(pathMirageDemo);
        parser.parseToMatchStart();

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
        parser.parseAllRounds();
        Player p = parser.Players[0];
        string header;
        int round = 1;
        Player illegalName = parser.Players[5];

        parser.saveToCSV(p, round, outputPath);
        parser.saveToCSV(illegalName, round, outputPath);

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
        }
        //header is written correctly
        Assert.IsTrue(header.ContainsAll("ticks", "posX", "posY", "posZ"));
    }

    
}

