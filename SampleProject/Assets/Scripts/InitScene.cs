using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FileInfoDef;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class InitScene : MonoBehaviour
{
    private string resourceVersionFileUrl;
    private string resourceVersionLocalFileUrl;
    private string localAssetSaveUrl;
    private string assetBundleInfoLocalUrl;

    private string downloadTargetResourceVersionUrl;

    private List<AssetBundleInfo> newAssetBundleList = new List<AssetBundleInfo>();

    public DownloadStream downloadStream;

    public ProgressBar progressBar;
    public TextMeshProUGUI noticeText;

    void Start()
    {
        resourceVersionFileUrl = ConstValue.awsRootUrl + ConstValue.resourceVersionFileName;

        resourceVersionLocalFileUrl = Application.persistentDataPath + "/" + ConstValue.resourceVersionFileName;

        localAssetSaveUrl = Application.persistentDataPath + "/AssetBundles/";

        assetBundleInfoLocalUrl = localAssetSaveUrl + ConstValue.assetBundleInfoFileName;

        StartCoroutine(StartAssetBundleDownload());
    }

    //리소스 버전정보 파일 다운로드 완료 후 해당 리소스 버전 경로의 AssetBundleInfo.json을 받는다.
    //그 후 AssetBundleInfo.json에 기록되 있는 정보를 토대로 에셋번들들을 다운로드 받는다.
    private IEnumerator StartAssetBundleDownload()
    {
        noticeText.gameObject.SetActive(true);
        noticeText.SetText("리소스 버전 확인 중...");
        progressBar.gameObject.SetActive(false);

        yield return StartCoroutine(DownloadResourceVersionInfo());

        noticeText.SetText("리소스 파일 검사중...");

        var downloadAssetInfo = new DownloadInfo();
        downloadAssetInfo.downloadUrl = downloadTargetResourceVersionUrl + "/AssetBundles/" + ConstValue.assetBundleInfoFileName;
        downloadAssetInfo.localSaveUrl = assetBundleInfoLocalUrl;

        downloadStream.SetDownloadStream(null, downloadAssetInfo);

        yield return downloadStream.OnWork();

        noticeText.SetText("리소스 파일 다운로드 중...");
        progressBar.gameObject.SetActive(true);

        yield return StartCoroutine(CrcCheckAndDownloadAssetBundle());

        noticeText.SetText("리소스 다운로드 완료");
        progressBar.gameObject.SetActive(false);

        //새로 다운로드 받은 에셋번들들 모두 로드
        foreach(var newAssetbundle in newAssetBundleList)
        {
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(localAssetSaveUrl + newAssetbundle.bundleName, newAssetbundle.crc);

            yield return bundleLoadRequest;

            if (bundleLoadRequest != null)
            {
                AssetManager.Instance.AddAssetBundle(bundleLoadRequest.assetBundle);
            }
        }

        SceneManager.LoadSceneAsync("LoginScene");
    }

    //리소스 버전정보 파일 다운로드 및 다운받을 리소스 url(resourceVersionDirectory) 지정
    private IEnumerator DownloadResourceVersionInfo()
    {
        var downloadAssetInfo = new DownloadInfo();
        downloadAssetInfo.downloadUrl = resourceVersionFileUrl;
        downloadAssetInfo.localSaveUrl = resourceVersionLocalFileUrl;

        downloadStream.SetDownloadStream(null, downloadAssetInfo);

        yield return downloadStream.OnWork();

        string[] bundleInfoText = File.ReadAllLines(resourceVersionLocalFileUrl);
        var resourceVersionInfo = JsonUtility.FromJson<ResourceVersionInfo>(bundleInfoText.AddStringArray());
        downloadTargetResourceVersionUrl = ConstValue.awsRootUrl + resourceVersionInfo.version;
    }

    //다운받은 AssetBundleInfo.json을 로드해서 현재 가지고있는 에셋들과 crc값 비교
    //달라졋다면 에셋에 변경점이 있다는 것, 새로 다운로드 받음
    private IEnumerator CrcCheckAndDownloadAssetBundle()
    {
        List<DownloadInfo> downloadInfos = new List<DownloadInfo>();
        string[] bundleInfoTextAllLine = File.ReadAllLines(assetBundleInfoLocalUrl);

        var bundleInfos = JsonUtility.FromJson<AssetBundleInfos>(bundleInfoTextAllLine.AddStringArray());

        if (progressBar) progressBar.Init(0, bundleInfos.assetBundleInfos.Count, true);

        foreach(var bundleInfo in bundleInfos.assetBundleInfos)
        {
            FileInfo fileInfo = new FileInfo(localAssetSaveUrl + bundleInfo.bundleName);

            bool needDownload = true;

            //파일 존재하면 crc검증
            if (fileInfo.Exists)
            {
                var bundleLoadRequest = AssetBundle.LoadFromFileAsync(localAssetSaveUrl + bundleInfo.bundleName, bundleInfo.crc);

                yield return bundleLoadRequest;

                //crc 일치하지 않을 때 (null) 새로 다운로드
                needDownload = bundleLoadRequest.assetBundle == null;

                if (needDownload == false)
                {
                    AssetManager.Instance.AddAssetBundle(bundleLoadRequest.assetBundle);
                }
            }

            if (needDownload)
            {
                downloadInfos.Add(new DownloadInfo()
                {
                    downloadUrl = downloadTargetResourceVersionUrl + "/AssetBundles/" + bundleInfo.bundleName,
                    localSaveUrl = localAssetSaveUrl + bundleInfo.bundleName,
                    onComplete = x =>
                    {
                        if (progressBar) progressBar.Init(downloadStream.currentJobIndex, (float)downloadStream.maxJobSize, true);

                        Debug.Log("Download Complete : " + bundleInfo.bundleName);
                    }
                });
                //다운로드 완료 후 에셋 번들 로드를 하기 위해 리스트 추가
                newAssetBundleList.Add(bundleInfo);
                Debug.Log("Reservation Download : " + bundleInfo.bundleName);
            }
            else
            {
                //동일한 에셋 이미 저장되어 있음
                Debug.Log("Already Exist : " + bundleInfo.bundleName);
            }
        }

        downloadStream.SetDownloadStream(x =>
        {
            Debug.Log("Download Stream Complete");
        }, downloadInfos.ToArray());

        yield return downloadStream.OnWork();
    }
}
