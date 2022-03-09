using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class ExtentionFunc
{
    //���丮 ������ ����
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
    //��Ʈ������ ����ؼ� ���ڿ� �ռ�
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
    //��Ʈ������ ����ؼ� �ռ��� ���ڿ��� �α� ���
    public static void Log(params string[] str)
    {
        Debug.Log(CHString.StringBuild(str));
    }
}