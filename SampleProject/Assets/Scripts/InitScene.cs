using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class InitScene : MonoBehaviour
{
    [Serializable]
    public class AssetBundleInfo
    {
        public string bundleName;
        public int hash;
        public long size;
        public uint crc;

        public AssetBundleInfo(string bundleName, int hash, long size, uint crc)
        {
            this.bundleName = bundleName;
            this.hash = hash;
            this.size = size;
            this.crc = crc;
        }
    }
    [Serializable]
    public class AssetBundleInfos
    {
        public AssetBundleInfo[] assetBundleInfos;
    }

    private string rootUrl = "";
    private string localSaveUrl = "";
    private string assetBundleInfoLocalUrl = "";
    public DownloadStream downloadStream;

    void Start()
    {
        rootUrl = "https://testproject-cch.s3.ap-northeast-2.amazonaws.com/AssetBundles/";
        localSaveUrl = Application.persistentDataPath + "/AssetBundles/";
        assetBundleInfoLocalUrl = localSaveUrl + "AssetBundleInfo.json";


        //우선적으로 AssetBundleInfo.json을 받는다.
        var downloadAssetInfo = new DownloadInfo();
        downloadAssetInfo.downloadUrl = rootUrl + "AssetBundleInfo.json";
        downloadAssetInfo.localSaveUrl = assetBundleInfoLocalUrl;

        downloadStream.SetDownloadStream(() =>
        {
            StartCoroutine(CrcCheckAndDownloadAssetBundle());
        }, downloadAssetInfo);

        downloadStream.OnWork();
    }

    //다운받은 AssetBundleInfo.json을 로드해서 현재 가지고있는 에셋들과 crc값 비교
    //달라졋다면 에셋에 변경점이 있다는 것, 새로 다운로드 받음
    private IEnumerator CrcCheckAndDownloadAssetBundle()
    {
        List<DownloadInfo> downloadInfos = new List<DownloadInfo>();
        
        string[] bundleInfoText = File.ReadAllLines(assetBundleInfoLocalUrl);
        var bundleInfos = JsonUtility.FromJson<AssetBundleInfos>(bundleInfoText[0]);

        foreach(var bundleInfo in bundleInfos.assetBundleInfos)
        {
            FileInfo fileInfo = new FileInfo(localSaveUrl + bundleInfo.bundleName);

            bool needDownload = true;

            //파일 존재하면 crc검증
            if (fileInfo.Exists)
            {
                var bundleLoadRequest = AssetBundle.LoadFromFileAsync(localSaveUrl + bundleInfo.bundleName, bundleInfo.crc);

                yield return bundleLoadRequest;

                //crc 일치하지 않을 때 (null) 새로 다운로드
                needDownload = bundleLoadRequest.assetBundle == null;
            }

            if (needDownload)
            {
                downloadInfos.Add(new DownloadInfo()
                {
                    downloadUrl = rootUrl + bundleInfo.bundleName,
                    localSaveUrl = localSaveUrl + bundleInfo.bundleName,
                    onComplete = _ =>
                    {
                        Debug.Log("Download Complete : " + bundleInfo.bundleName);
                    }
                });

                Debug.Log("Restore Download : " + bundleInfo.bundleName);
            }
            else
            {
                //동일한 에셋 이미 저장되어 있음
                Debug.Log("Already Exist : " + bundleInfo.bundleName);
            }
        }

        downloadStream.SetDownloadStream(() =>
        {
            Debug.Log("Download Stream Complete");
        }, downloadInfos.ToArray());

        downloadStream.OnWork();
    }
}
