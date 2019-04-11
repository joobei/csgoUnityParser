using System;
using System.Collections.Generic;
using UnityEngine;
using DemoInfo;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;

public static class Extensions
{
    /// <summary>
    /// Gets the first key of a dictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="dic">dictionary</param>
    /// <returns>the first key</returns>
    public static V getFirstKey<T, V>(this Dictionary<T, V> dic)
    {
        int count = dic.Keys.Count;

        T[] keysArray = new T[count];

        dic.Keys.CopyTo(keysArray, 0);
        return dic[keysArray[0]];
    }


    /// <summary>
    /// Fills a dictionary, whose values are lists, with new empty lists of the same type
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="TList">type of the list elements</typeparam>
    /// <param name="dic">dictionary</param>
    public static void refillDictionary<K, V, TList>(this Dictionary<K, V> dic) where V : List<TList>
    {
        K[] keys = new K[dic.Keys.Count];
        dic.Keys.CopyTo(keys, 0);

        foreach (K item in keys)
        {
            dic[item] = (V)new List<TList>();
        }
    }

    /// <summary>
    /// Casts a source engine vector to a Numerics.Vector3
    /// <remarks>switch z  and y axis -then invert x and z values</remarks>
    /// </summary>
    /// <param name="sourceVec">source engine vector</param>
    /// <returns>vector3</returns>
    public static System.Numerics.Vector3 FromSourceEngineVector(Vector sourceVec)
    {
        System.Numerics.Vector3 res = new System.Numerics.Vector3(-sourceVec.X, sourceVec.Z, -sourceVec.Y);
        return res;
    }

    /// <summary>
    /// casts Sysem.Numerics.Vector3 to UnityEngine.Vector3
    /// </summary>
    /// <param name="vec">System.Numerics.Vector3</param>
    /// <returns>UnityEngine.Vector3</returns>
    public static Vector3 castToUnityVector3(this System.Numerics.Vector3 vec)
    {
        return new Vector3(vec.X, vec.Y, vec.Z);
    }

    /// <summary>
    /// Adds an element to a list, which is a value of a dictionary 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="TList">type of the list elements</typeparam>
    /// <param name="dic"></param>
    /// <param name="key">key to which list to add</param>
    /// <param name="value">the element to add to the list</param>
    public static void AddValueToExistingList<K, V, TList>(this Dictionary<K, V> dic, K key, TList value) where V : List<TList>
    {
        dic.TryGetValue(key, out V v);
        v.Add(value);
        dic[key] = v;
    }

    /// <summary>
    /// Fills a dictionary at each given key with a clone of the given value
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="dic"></param>
    /// <param name="keys">the keys at which position to fill</param>
    /// <param name="value">the value with which to fill</param>
    public static void fillWithValue<K, V>(this Dictionary<K, V> dic, K[] keys, V value)
    {
        foreach (var k in keys)
        {
            V temp = value.Clone();
            dic[k] = temp;
        }
    }

    /// <summary>
    /// Checks whether a string contains multiple substrings(at least one)
    /// Returns true if it contains all
    /// </summary>
    /// <param name="str">string in which to search</param>
    /// <param name="match">substring to match</param>
    /// <param name="moreMatches">optional, addditional matches</param>
    /// <returns>true if str contains all substrings</returns>
    public static bool ContainsAll(this string str, string match, params string[] moreMatches)
    {
        //copy all to one array
        string[] matches = new string[moreMatches.Length + 1];
        matches[0] = match;
        moreMatches.CopyTo(matches, 1);

        foreach (string item in matches)
        {
            if (!str.Contains(item)) return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if a string contains all given chars
    /// </summary>
    /// <param name="str"></param>
    /// <param name="chars">the chars to check for</param>
    /// <returns></returns>
    public static bool ContainsAll(this string str, char[] chars)
    {
        string[] charsInStrings = new string[chars.Length];
        for (int i = 0; i < charsInStrings.Length; i++)
        {
            charsInStrings[i] = "" + chars[i];
        }

        //Double checks first char in string | doesnt bother me
        return ContainsAll(str, charsInStrings[0], charsInStrings);
    }

    /// <summary>
    /// Checks whether a string contains any of the given substrings
    /// </summary>
    /// <param name="str"></param>
    /// <param name="match">a string to check</param>
    /// <param name="moreOptions">more possible strings</param>
    /// <returns></returns>
    public static bool ContainsAny(this string str, string match, params string[] moreOptions)
    {
        //copy all to one array
        string[] matches = new string[moreOptions.Length + 1];
        matches[0] = match;
        moreOptions.CopyTo(matches, 1);

        foreach (string item in matches)
        {
            if (str.Contains(item)) return true;
        }
        return false;
    }

    /// <summary>
    ///  Checks whether a string contains any of the given chars
    /// </summary>
    /// <param name="str"></param>
    /// <param name="chars">chars to check for</param>
    /// <returns></returns>
    public static bool containsAny(this string str, char[] chars)
    {
        string[] charsInStrings = new string[chars.Length];
        for (int i = 0; i < charsInStrings.Length; i++)
        {
            charsInStrings[i] = "" + chars[i];
        }

        //Double checks first char in string | doesnt bother me
        return ContainsAny(str, charsInStrings[0], charsInStrings);
    }


    /// <summary>
    /// Removes any non-alphanumeric character from a string besides '.','@','-'
    /// </summary>
    /// <param name="strIn"></param>
    /// <returns></returns>
    public static string CleanInput(this string strIn, bool allowPathSeparators = false)
    {
        char separator = Path.PathSeparator;
        char direcSep = Path.DirectorySeparatorChar;
        char altDirecSep = Path.AltDirectorySeparatorChar;
        char volumeSeperator = Path.VolumeSeparatorChar;


        string pattern = string.Format(@"[^\w\.@{0}{1}{1}{2}{3}{3}-]", separator,direcSep,volumeSeperator, altDirecSep);
        strIn = Regex.Replace(strIn, pattern, "");

        if (!allowPathSeparators)strIn = Regex.Replace(strIn, @"[^\w\.@-]", "");


        return strIn;
    }

    
    /// <summary>
    /// Ensures a float's string representation is seperated by a dot instead of a comma
    /// </summary>
    /// <param name="f"></param>
    /// <returns>string with the dot separation</returns>
    public static string DotSeparation(this float f)
    {
        string cultureSpecific = f.ToString();
        return Regex.Replace(cultureSpecific, ",", ".");
    }

    /// <summary>
    /// Returns a string with the important details of a kill, ready for CSV files
    /// <remarks>Order is: Victim,Killer,Assist, Weapon, Headshot </remarks>
    /// </summary>
    /// <param name="args">the kill event</param>
    /// <returns>formatted string</returns>
    public static string ToCSVString(this PlayerKilledEventArgs args)
    {
        string assist = "";
        if (args.Assister != null) assist = args.Assister.Name;
        return String.Format("{0},{1},{2},{3},{4}",args.Victim.Name, args.Killer.Name, assist, args.Weapon.Weapon, args.Headshot);
    }

    private static T Clone<T>(this T source)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new ArgumentException("The type must be serializable.", "source");
        }

        // Don't serialize a null object, simply return the default for that object
        if (object.ReferenceEquals(source, null))
        {
            return default(T);
        }

        IFormatter formatter = new BinaryFormatter();
        using (Stream stream = new MemoryStream())
        {
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }
}
