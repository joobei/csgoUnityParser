using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Outsorted class to help with writing data to a csv file
/// </summary>
public class csvSaver
{
    /// <summary>
    /// Creates a new csv file and writes all data from a list in it. Custom headers are possible.
    /// </summary>
    /// <remarks>Uses the IFormattable interface with the csv option - provide that for custom classes</remarks>
    /// <typeparam name="T">Type of the list elements</typeparam>
    /// <param name="data">the list data which to write in the file</param>
    /// <param name="title">the file's title - invalid symbols are getting automatically removed</param>
    /// <param name="path">the file's path - invalid symbols are getting automatically removed</param>
    /// <param name="header">the first line of the file explaining the columns - optional</param>
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

    /// <summary>
    /// Creates a csv file at a certain path with a given title, and an optional header line
    /// </summary>
    /// <remarks> path and title automatically remove invalid chars</remarks> 
    /// <param name="path">the save path</param>
    /// <param name="title">the file's title</param>
    /// <param name="header">the file's header - first line in the file</param>
    /// <returns>the cleaned output path</returns>
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

    /// <summary>
    /// Adds a line to an existing file
    /// </summary>
    /// <param name="path">the existing file</param>
    /// <param name="line">line to add</param>
    public static void addLineToFile(string path, string line)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(line);

        using (StreamWriter output = new StreamWriter(path, true))
        {
            output.Write(builder.ToString());
        }
    }
}


