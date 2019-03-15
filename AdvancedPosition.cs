using System.Numerics;

[System.Serializable]
public class AdvancedPosition
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

    public float getOrientationX()
    {
        return orientationX;
    }

    public float getOrientationY()
    {
        return orientationY;
    }


}
