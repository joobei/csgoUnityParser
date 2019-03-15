using System;
using System.Collections.Generic;
using UnityEngine;


///<summary> a float intervall representation with a lower and upper bound and a optional smapling rate </summary>
public class Interval
{
    public readonly float LOWERBOUND;
    public readonly float UPPERBOUND;
    public float _samplingRate { get; set; }

    public readonly float LENGTH;








    /// <summary>
    /// Sets up an interval object with lower and upper bound, and an optional sampling rate
    /// </summary>
    /// <remarks>
    /// Default sampling rate is 0.05 and it is ensured that lower bound is always smaller than/equal to the upper bound
    /// </remarks>
    /// <param name="lBound"> the lower bound</param>
    /// <param name="uBound"> the upper bound</param>
    /// <param name="samplingRate"> sampling rate, default 0.05</param>
    public Interval(float lBound, float uBound, float samplingRate = 0.05f)
    {
        //ensure lower bound is smaller then the upper bound
        if (lBound > uBound)
        {
            LOWERBOUND = uBound;
            UPPERBOUND = lBound;
        }
        else
        {
            LOWERBOUND = lBound;
            UPPERBOUND = uBound;
        }

        LENGTH = UPPERBOUND - LOWERBOUND;
        _samplingRate = samplingRate;
    }



    /// <summary>
    /// Checks if the value is inside the interval
    /// </summary>
    /// <param name="value">the testing value</param>
    /// <returns>Whether the value is inside the interval or not</returns>
    public Boolean IsInside(float value)
    {
        return LOWERBOUND <= value && value <= UPPERBOUND;
    }


    /// <summary>
    /// Creates a list in which every value inside the interval is listed by the sampling rate
    /// </summary>
    /// <remarks> Adds the samping rate to the lower bound, then adds the sampling rate to the resulting value and creates a list that way</remarks>
    /// <returns>a list with every value inside given the samplig rate</returns>
    public List<float> getEveryValue()
    {
        List<float> every = new List<float>();
        for (float i = LOWERBOUND; i <= UPPERBOUND;)
        {
            every.Add(i);
            i += _samplingRate;
        }
        return every;
    }


    /// <summary>
    /// Intersects the two intervals
    /// </summary>
    /// <param name="cut">the interval with which to intersect</param>
    /// <returns>the intersection interval or null if there is no intersection</returns>
    public Interval Intersect(Interval cut)
    {
        if (!DoNotIntersect(cut))
        {
            if (cut.LOWERBOUND <= LOWERBOUND)
            {
                if (cut.UPPERBOUND <= UPPERBOUND)
                {
                    return new Interval(LOWERBOUND, cut.UPPERBOUND);
                }
                return this;
            }
            else
            {
                if (cut.UPPERBOUND <= UPPERBOUND)
                {
                    return cut;
                }
                return new Interval(cut.LOWERBOUND, UPPERBOUND);
            }
        }
        return null;
    }


    /// <summary>
    /// Filters through a float array, only keeping the values inside the interval
    /// </summary>
    /// <param name="toFilter">the array wished to be filtered</param>
    /// <returns>the filtered array</returns>
    public float[] filterArray(float[] toFilter)
    {
        List<float> res = new List<float>();
        foreach (float filter in toFilter)
        {
            if (IsInside(filter)) res.Add(filter);
        }
        return res.ToArray();
    }

    /// <summary>
    /// Returns a rand float from inside the interval, with respect to the sampling rate
    /// </summary>
    /// <returns> a random float from inside the interval</returns>
    public float getRandom()
    {

        List<float> values = getEveryValue();

        int key = UnityEngine.Random.Range(0, values.Count - 1);



        return values[key];
    }

    public override string ToString()
    {
        return "[" + LOWERBOUND + ";" + UPPERBOUND + "]";
    }

    public Vector2 ToVector()
    {
        return new Vector2(LOWERBOUND, UPPERBOUND);
    }

    public static Interval fromFloatArray(float[] res)
    {
        return new Interval(Mathf.Max(res), Mathf.Min(res));
    }

    public float[] ToArray()
    {
        List<float> everyValue = getEveryValue();
        return everyValue.ToArray();
    }

    public string[] ToStringArray()
    {
        List<float> everyValue = getEveryValue();
        List<string> everyValueString = new List<string>();

        foreach (float  value in everyValue)
        {
            everyValueString.Add(value + "");
        }

        return everyValueString.ToArray();
    }

    public static Interval Naturals(int end)
    {
        return Naturals(0, end);
    }

    public static Interval Naturals(int start, int end)
    {
        return new Interval(start, end, 1);
    }

    public static Interval operator -(Interval a, Interval b)
    {
        return new Interval(a.LOWERBOUND - b.UPPERBOUND, a.UPPERBOUND - b.LOWERBOUND);
    }

    public static Interval operator *(Interval a, float b)
    {
        return new Interval(a.LOWERBOUND * b, a.UPPERBOUND * b);
    }

    public static Interval operator *(float b, Interval a)
    {
        return new Interval(a.LOWERBOUND * b, a.UPPERBOUND * b);
    }




    //private methods

    private bool DoNotIntersect(Interval second)
    {
        return second == null || second.LOWERBOUND > UPPERBOUND || second.UPPERBOUND < LOWERBOUND;
    }
}
