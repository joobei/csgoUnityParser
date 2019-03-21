using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class csvSaver
{
    //TODO handle illegal chars in path | check symbol encoding 
    public void writeListCSV<T>(List<T> data, string title, string path) where T : IFormattable
    {
        Directory.CreateDirectory(path);
        if (!path.EndsWith("/")) path += "/";

        title = title.CleanInput();
        path = path.CleanInput(true);


        path += title + ".csv";

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("ticks,posX,posY,posZ,viewX,viewY");


        for (int ticksDone = 0; ticksDone < data.Count; ticksDone++)
        {

            T aPos = data[ticksDone];


            string line = (ticksDone + 1) + "," + aPos.ToString("csv", null);
            if (ticksDone < 5)
            {
                Console.WriteLine(aPos.ToString("csv", null));
                Console.WriteLine(line);
            }


            builder.AppendLine(line);

        }

        using (StreamWriter output = new StreamWriter(path, false))
        {
            output.Write(builder.ToString());
        }


    }
}


