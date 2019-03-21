using System;
using System.IO;

public class StartUp_console
{
    public static void Main(string[] args)
    {
        string saveFolder;
        if (!AreProperArguments(args, out saveFolder)) return;

        string fileReplay = args[0];

        csgoParser parser = new csgoParser(fileReplay);

        Console.Write(parser.ToString());

        parser.parseAllRounds();


        parser.saveToCSV(saveFolder);

        Console.WriteLine("parsing finished");
        Console.Write(parser.GetEndGameStatsString());
    }

    private static bool AreProperArguments(string[] args, out string saveFolder)
    {
        switch (args.Length)
        {
            case 2:
                return checkIntegrity(args, out saveFolder);
            case 1:
                return checkIntegrity(args[0], out saveFolder);
            default:
                Console.WriteLine(string.Format("Illegal Number of arguments " +
                    "\n proper usage is {0} <replayToLoad.dem> <output path - optional>", System.AppDomain.CurrentDomain.FriendlyName));
                saveFolder = "";
                return false;
        }
    }

    private static bool checkIntegrity(string[] args, out string saveFolder)
    {
        saveFolder = saveFolder = args[1];
        bool goodFile = checkFileGoodness(args[0]);
        bool allowedPath = !args[1].containsAny(Path.GetInvalidPathChars());
        if (!allowedPath) Console.WriteLine(string.Format("{0} is an invalid output path", args[1]));
        return goodFile && allowedPath;
    }

    private static bool checkIntegrity(string arg, out string saveFolder)
    {
        saveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/CSV/";

        bool goodFile = checkFileGoodness(arg);
        if (goodFile) Console.WriteLine(string.Format("No savefolder argument is given - will be defaulted to {0}", saveFolder));

        return goodFile;
    }

    private static bool checkFileGoodness(string arg)
    {
        bool goodFile = File.Exists(arg) && arg.EndsWith(".dem");

        if (!goodFile) Console.WriteLine(string.Format("File {0} can not be found or does not match the .dem extension", arg));
        return goodFile;
    }
}
