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


        //�켱������ AssetBundleInfo.json�� �޴´�.
        var downloadAssetInfo = new DownloadInfo();
        downloadAssetInfo.downloadUrl = rootUrl + "AssetBundleInfo.json";
        downloadAssetInfo.localSaveUrl = assetBundleInfoLocalUrl;

        downloadStream.SetDownloadStream(() =>
        {
            StartCoroutine(CrcCheckAndDownloadAssetBundle());
        }, downloadAssetInfo);

        downloadStream.OnWork();
    }

    //�ٿ���� AssetBundleInfo.json�� �ε��ؼ� ���� �������ִ� ���µ�� crc�� ��
    //�޶󠺴ٸ� ���¿� �������� �ִٴ� ��, ���� �ٿ�ε� ����
    private IEnumerator CrcCheckAndDownloadAssetBundle()
    {
        List<DownloadInfo> downloadInfos = new List<DownloadInfo>();
        
        string[] bundleInfoText = File.ReadAllLines(assetBundleInfoLocalUrl);
        var bundleInfos = JsonUtility.FromJson<AssetBundleInfos>(bundleInfoText[0]);

        foreach(var bundleInfo in bundleInfos.assetBundleInfos)
        {
            FileInfo fileInfo = new FileInfo(localSaveUrl + bundleInfo.bundleName);

            bool needDownload = true;

            //���� �����ϸ� crc����
            if (fileInfo.Exists)
            {
                var bundleLoadRequest = AssetBundle.LoadFromFileAsync(localSaveUrl + bundleInfo.bundleName, bundleInfo.crc);

                yield return bundleLoadRequest;

                //crc ��ġ���� ���� �� (null) ���� �ٿ�ε�
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
                //������ ���� �̹� ����Ǿ� ����
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
