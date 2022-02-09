using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class ExtentionInfo
{
    public static string ToJson<T>(this List<T> _list)
    {
        string retJson = "[";

        for (int i = 0; i < _list.Count; ++i)
        {
            retJson += JsonUtility.ToJson(_list[i]);

            if (i < _list.Count - 1)
            {
                retJson += ",\n";
            }
            else
            {
                retJson += "]";
            }
        }
        
        return retJson;
    } 
}
