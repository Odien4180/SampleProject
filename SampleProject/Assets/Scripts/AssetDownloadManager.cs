using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using UnityEngine.Networking;

public class AssetDownloadManager : Singleton<AssetDownloadManager>
{
    private void Download(string bundleUrl, string localUrl)
    {
        UnityWebRequest uwr = new UnityWebRequest(bundleUrl, UnityWebRequest.kHttpVerbGET);
        uwr.downloadHandler = new DownloadHandlerFile(localUrl);
        uwr.SendWebRequest().completed += _ =>
        {
            string error = uwr.error;

            uwr.downloadHandler.Dispose();
            uwr.Dispose();
            
            if (error?.Length > 0)
            {
                Debug.LogError(error);
            }
        };
    }
}
