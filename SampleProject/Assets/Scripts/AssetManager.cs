using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AssetManager : Singleton<AssetManager>
{
    private Dictionary<string, string> luaScriptDic = new Dictionary<string, string>();

    //Load할 Lua스크립트들 모두 여기서 로드?
    public void LoadAllLuaScript()
    {
        
    }

    public string LoadLuaScript(string _scriptName)
    {
        string path = Application.dataPath + "/LuaScript/" + _scriptName;

        //경로 마지막에 .Lua가 생략 됬을 경우 추가해준다
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
