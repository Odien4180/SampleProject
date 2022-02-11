using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    //��Ʈ�� �迭 ���� ���ϱ�
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
