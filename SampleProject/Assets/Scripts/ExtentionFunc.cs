using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

    
}

public static class CHString
{
    //스트링빌더 사용해서 문자열 합성
    public static string StringBuild(params string[] str)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < str.Length; ++i)
        {
            sb.Append(sb);
        }
        return sb.ToString();
    }
}

public static class CHDebug
{
    //스트링빌더 사용해서 합성한 문자열로 로그 출력
    public static void Log(params string[] str)
    {
        Debug.Log(CHString.StringBuild(str));
    }
}