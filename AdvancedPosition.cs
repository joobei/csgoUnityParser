using System;
using System.Globalization;
using System.Numerics;


[System.Serializable]
public class AdvancedPosition : IFormattable 
{
    private Vector3 position;
    private float orientationX;
    private float orientationY;

    public AdvancedPosition(Vector3 pos, float orientX, float orientY)
    {
        position = pos;
        orientationX = orientX;
        orientationY = orientY;
    }

    public Vector3 GetPosition()
    {
        return position;
    }

    public float GetOrientationX()
    {
        return orientationX;
    }

    public float GetOrientationY()
    {
        return orientationY;
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        if (String.IsNullOrEmpty(format)) format = "All";
        if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

        switch (format.ToUpperInvariant())
        {
            case "ALL":
                return string.Format("Position: {0} \n viewdirection {1}{2}" , position.ToString(), orientationX,orientationY);
            case "CSV":
                return string.Format("{0},{1},{2},{3},{4}", position.X.DotSeparation(), position.Y.DotSeparation(), position.Z.DotSeparation(), orientationX.DotSeparation(), orientationY.DotSeparation());
            default:
                throw new FormatException(String.Format("The {0} format string is not supported.", format));
        }
    }

    public override string ToString()
    {
        return ToString(null , null);
    }

    public string ToString(string format)
    {
        return ToString(format, null);
    }
}
