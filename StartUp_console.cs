using System;

public class StartUp_console 
{
  public static void Main(string[] args)
    {
        //TODO errror handling 
        string fileReplay = args[0];
        string saveFolder = args[1];

        csgoParser parser = new csgoParser(fileReplay);
        parser.parseToMatchStart();

        Console.Write(parser.ToString());

        parser.parseAllRounds();

        parser.saveToCSV(saveFolder);

        Console.WriteLine("parsing finished");
        Console.Write(parser.GetEndGameStatsString());
    }
}
