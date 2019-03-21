using System;
using System.Globalization;
using System.Numerics;

/// <summary>
/// A class used to store position and orientation
/// </summary>
[System.Serializable]
public class AdvancedPosition : IFormattable
{
    private Vector3 _position;
    private float _orientationX;
    private float _orientationY;

    public AdvancedPosition(Vector3 pos, float orientX, float orientY)
    {
        _position = pos;
        _orientationX = orientX;
        _orientationY = orientY;
    }

    /// <summary>
    /// Get the position
    /// </summary>
    /// <returns>a vector3</returns>
    public Vector3 GetPosition()
    {
        return _position;
    }

    /// <summary>
    /// Get the rotation in degrees around the right-axis
    /// </summary>
    /// <returns>angle in degrees</returns>
    public float GetOrientationX()
    {
        return _orientationX;
    }

    /// <summary>
    /// Get the rotation in degrees around the upward-axis
    /// </summary>
    /// <returns>the angle  in degrees</returns>
    public float GetOrientationY()
    {
        return _orientationY;
    }

    /// <summary>
    /// Creates a string following a given format
    /// </summary>
    /// <param name="format">format options - "", "all", "csv" </param>
    /// <param name="formatProvider">set null</param>
    /// <returns>a string in the specified format</returns>
    public string ToString(string format, IFormatProvider formatProvider)
    {
        if (String.IsNullOrEmpty(format)) format = "All";
        if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

        switch (format.ToUpperInvariant())
        {
            case "ALL":
                return string.Format("Position: {0} \n viewdirection {1}{2}", _position.ToString(), _orientationX, _orientationY);
            case "CSV":
                return string.Format("{0},{1},{2},{3},{4}", _position.X.DotSeparation(), _position.Y.DotSeparation(), _position.Z.DotSeparation(), _orientationX.DotSeparation(), _orientationY.DotSeparation());
            default:
                throw new FormatException(String.Format("The {0} format string is not supported.", format));
        }
    }

    /// <summary>
    /// Creates a string following a given format
    /// </summary>
    /// <param name="format">format options - "", "all", "csv" </param>
    /// <returns>a string in the specified format</returns>
    public string ToString(string format)
    {
        return ToString(format, null);
    }
    public override string ToString()
    {
        return ToString(null, null);
    }


}
