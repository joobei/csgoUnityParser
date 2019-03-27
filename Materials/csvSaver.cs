using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class csvSaver
{
    public static void writeListCSV<T>(List<T> data, string title, string path, string header = "") where T : IFormattable
    {
        path = createCSVFile(path, title, header);

        StringBuilder builder = new StringBuilder();

        for (int ticksDone = 0; ticksDone < data.Count; ticksDone++)
        {

            T aPos = data[ticksDone];


            string line = (ticksDone + 1) + "," + aPos.ToString("csv", null);


            builder.AppendLine(line);

        }

        using (StreamWriter output = new StreamWriter(path, true))
        {
            output.Write(builder.ToString());
        }


    }

    public static string createCSVFile(string path, string title, string header = "")
    {
        Directory.CreateDirectory(path);
        if (!path.EndsWith("/")) path += "/";

        title = title.CleanInput();
        path = path.CleanInput(true);


        path += title + ".csv";

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(header);

        using (StreamWriter output = new StreamWriter(path, false))
        {
            output.Write(builder.ToString());
        }

        return path;
    }
    
    public static void addLineToFile(string path,string line)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(line);

        using (StreamWriter output = new StreamWriter(path, true))
        {
            output.Write(builder.ToString());
        }
    }
}


