using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AssetManager : Singleton<AssetManager>
{
    private Dictionary<string, AssetBundle> assetBundleDic = new Dictionary<string, AssetBundle>();
    
    public void AddAssetBundle(AssetBundle assetBundle)
    {
        if (assetBundleDic.ContainsKey(assetBundle.name))
        {
            assetBundleDic[assetBundle.name] = assetBundle;

            Debug.Log("Already added assetbundle : " + assetBundle.name);
        }
        else
        {
            assetBundleDic.Add(assetBundle.name, assetBundle);
        }
    }

    public bool GetAssetBundle(string bundleName, ref AssetBundle assetBundle)
    {
        if (assetBundleDic.ContainsKey(bundleName))
        {
            assetBundle = assetBundleDic[bundleName];

            return true;
        }
        else
        {
            Debug.LogError("Assetbundle not exist : " + bundleName);

            return false;
        }
    }
}
