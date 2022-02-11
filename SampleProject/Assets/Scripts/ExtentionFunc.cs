using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ExtentionFunc
{
    //디렉토리 없으면 생성
    public static void CheckAndCreateDirectory(string directoryName)
    {
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
    }

    //스트링 배열 전부 더하기
    public static string AddStringArray(this string[] stringArray)
    {
        string retString = "";

        for (int i = 0; i < stringArray.Length; ++i)
        {
            retString += stringArray[i];
        }

        return retString;
    }
}
