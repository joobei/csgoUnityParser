using System.Collections.Generic;
using System.IO;

public class csvSaver
{

    public void writeListCSV(List<AdvancedPosition> data, string title, string path)
    {
        Directory.CreateDirectory(path);
        if (!path.EndsWith("/")) path += "/";
        
        path += title + ".csv";

        

        using (StringWriter writer = new StringWriter())
        {
            writer.WriteLine("ticks,posX,posY,posZ");

            for (int ticksDone = 0; ticksDone < data.Count; ticksDone++)
            {
                //TODO Make generic
                AdvancedPosition aPos =  data[ticksDone];
                System.Numerics.Vector3 current = aPos.GetPosition();
                writer.WriteLine((ticksDone + 1) + "," + current.X + "," + current.Y + "," + current.Z);
            }

            using (StreamWriter output = new StreamWriter(path, false))
            {
                output.Write(writer.ToString());
            }
        }
    }

}
