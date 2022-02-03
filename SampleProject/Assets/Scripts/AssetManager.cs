using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AssetManager : Singleton<AssetManager>
{
    private Dictionary<string, string> luaScriptDic = new Dictionary<string, string>();

    //Load�� Lua��ũ��Ʈ�� ��� ���⼭ �ε�?
    public void LoadAllLuaScript()
    {
        
    }

    public string LoadLuaScript(string _scriptName)
    {
        string path = Application.dataPath + "/LuaScript/" + _scriptName;

        //��� �������� .Lua�� ���� ���� ��� �߰����ش�
        if (path.TakeLast(4).ToString() != ".Lua")
        {
            path += ".Lua";
        }

        string[] entireText = System.IO.File.ReadAllLines(path);

        string retText = "";

        for (int i = 0; i < entireText.Length; ++i)
        {
            retText += entireText[i];
        }
        
        return retText;
    }
}
