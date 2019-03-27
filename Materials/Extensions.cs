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
    public static V getFirstValue<T, V>(this Dictionary<T, V> dic)
    {
        int count = dic.Keys.Count;

        T[] keysArray = new T[count];

        dic.Keys.CopyTo(keysArray, 0);
        return dic[keysArray[0]];
    }


    public static void refillDictionary<T, V, TList>(this Dictionary<T, V> dic) where V : List<TList>
    {
        T[] keys = new T[dic.Keys.Count];
        dic.Keys.CopyTo(keys, 0);

        foreach (T item in keys)
        {
            dic[item] = (V)new List<TList>();
        }
    }

    public static System.Numerics.Vector3 FromSourceEngineVector(Vector sourceVec)
    {
        System.Numerics.Vector3 res = new System.Numerics.Vector3(-sourceVec.X, sourceVec.Z, -sourceVec.Y);
        return res;
    }

    public static Vector3 castToUnityVector3(this System.Numerics.Vector3 vec)
    {
        return new Vector3(vec.X, vec.Y, vec.Z);
    }

    public static void AddValueToExistingList<K, V, TList>(this Dictionary<K, V> dic, K key, TList value) where V : List<TList>
    {
        V v = dic.GetValueOrNull(key);
        v.Add(value);
        dic[key] = v;
    }

    public static V GetValueOrNull<K, V>(this Dictionary<K, V> dic, K key)
    {
        V temp;
        dic.TryGetValue(key, out temp);
        return temp;
    }


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

    
    public static string DotSeparation(this float f)
    {
        string cultureSpecific = f.ToString();
        return Regex.Replace(cultureSpecific, ",", ".");
    }

    public static T Clone<T>(this T source)
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
